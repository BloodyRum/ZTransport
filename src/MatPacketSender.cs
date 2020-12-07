/*
 * This file is part of ZTransport, copyright ©2020 BloodyRum.
 *
 * ZTransport is free software: you can redistribute it and/or modify it under
 * the terms of the GNU General Public License as published by the Free Soft-
 * ware Foundation, either version 3 of the License, or (at your option) any
 * later version.
 *
 * ZTransport is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FIT-
 * NESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 * details.
 *
 * You should have received a copy of the GNU General Public License along with
 * ZTransport. If not, see <https://www.gnu.org/licenses/>.
 */

using KSerialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZTransport {

    [SerializationConfig(KSerialization.MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/scripts/MatPacketSender")]
    public class MatPacketSender : KMonoBehaviour, ISaveLoadable
    {
        private static readonly Operational.Flag outputConduitFlag = new Operational.Flag("output_conduit", Operational.Flag.Type.Functional);

        [MyCmpReq]
        public Storage storage;

        [SerializeField]
        public bool isOn = true;

#pragma warning disable 0649
        [MyCmpReq]
        private Operational operational;
        [MyCmpReq]
        private ZTransporter ztransporter;
#pragma warning restore 0649

        [SerializeField]
        public ConduitType conduitType;

        private HandleVector<int>.Handle partitionerEntry;
        private int utilityCell = -1;

        private int elementOutputOffset = 0;

        private const float GASCHUNKMAX = 1f; // In kg I believe
        private const float LIQUIDCHUNKMAX = 10f;

        public ConduitType TypeOfConduit
        {
            get
            {
                return this.conduitType;
            }
        }

        public bool isGas {
            get {
                return (this.conduitType == ConduitType.Gas);
            }
        }

        public void SetConduitType(ConduitType type)
        {
            this.conduitType = type;
        }

        private void OnConduitConnectionChanged(object data)
        {
            this.Trigger(-2094018600, (object) this.IsConnected);
        }

        private JObject convert_from_primary(PrimaryElement pri_element) {
            JObject chunk = new JObject();
            JObject disease_chunk = new JObject();

            chunk.Add("element", (int) pri_element.Element.id);
            float mass_to_add = Math.Min(pri_element.Mass,
                      (this.isGas ? GASCHUNKMAX : LIQUIDCHUNKMAX));
            pri_element.Mass -= mass_to_add;
            chunk.Add("mass", mass_to_add); // yo mama            
            chunk.Add("temperature", pri_element.Temperature);

            if ((int)pri_element.DiseaseIdx != 255) { // 255 = no disease
                //Klei.AI.Disease disease = Db.Get().Diseases[(int) pri_element.DiseaseIdx];

                // TODO: Need to handle germs better
                disease_chunk.Add("id", (int) pri_element.DiseaseIdx);
                disease_chunk.Add("count", pri_element.DiseaseCount);

                chunk.Add("germs", disease_chunk);
            }
            return chunk;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            GameScheduler.Instance.Schedule("PlumbingTutorial", 2f, (System.Action<object>) (obj => Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_Plumbing, true)), (object) null, (SchedulerGroup) null);
            this.utilityCell = this.GetComponent<Building>().GetUtilityOutputCell();
            this.partitionerEntry = GameScenePartitioner.Instance.Add("ConduitConsumer.OnSpawn", (object) this.gameObject, this.utilityCell, GameScenePartitioner.Instance.objectLayers[this.conduitType == ConduitType.Gas ? 12 : 16], new System.Action<object>(this.OnConduitConnectionChanged));
            this.GetConduitManager().AddConduitUpdater(new System.Action<float>(this.ConduitUpdate), ConduitFlowPriority.Dispense);
            this.OnConduitConnectionChanged((object) null);
        }

        protected override void OnCleanUp()
        {
            this.GetConduitManager().RemoveConduitUpdater(new System.Action<float>(this.ConduitUpdate));
            GameScenePartitioner.Instance.Free(ref this.partitionerEntry);
            base.OnCleanUp();
        }


        JObject buffered = null;
        bool outstanding = false;

        private void ConduitUpdate(float dt)
        {
            this.operational.SetFlag(MatPacketSender.outputConduitFlag, this.IsConnected);
            if (!this.isOn)
                return;
            this.Dispense(dt);
        }


        public void Dispense(float dt)
        {
            // Because of how complex materials, we are NOT going to only
            // use the internal buffer of the tank/whatever.
            // As soon as we get a suitable element to send, we are going to
            // cache it in a special buffer, and that buffer has these rules:
            //  1. There can only EVER be one thing buffered at a time
            //  2. Any thing that enters the buffer MUST exit
            //     by being sent to the server, it cannot use any other way
            // using these two simple rules, that should prevent a lot of
            // possible exploits from happening, like asking the server to
            // take 500g of 21 degree temperture, and then having it rise by
            // 5 degrees while waiting for a response, and this having
            // the 5 degrees destroyed
            int x, y;
            Grid.CellToXY(Grid.PosToCell(this), out x, out y);

            JObject message = Z.net.get_message_for("sent_packet", x, y);
            if(outstanding && message != null) {
                // We got a response from the server, deal with it
                if ((bool) message["accepted"]) {
                    buffered = null; // Message sent and received, KILL IT
                }
                outstanding = false;
            }
            // Only make and send a message if we are enabled
            if(!outstanding && ztransporter.is_enabled_and_linked()) {
                // We are not currently waiting for a message
                // so make a message (if necessary), and then wait for response

                if(buffered == null) {
                    // We need to remove some of the primary element, make a
                    // message out of it and then send that to the sever

                    PrimaryElement suitableElement
                        = this.FindSuitableElement();
                    // If the suitableElement returned is null, then we
                    // don't have any suitable elements in our storage,
                    // so skip this whole section
                    if (suitableElement != null) {
                        suitableElement.KeepZeroMassObject = false;
                        message = Network.make_message("send_packet", x, y);
                        message["phase"] = (this.isGas ? "Gas" : "Liquid");
                        message["packet"]
                            = convert_from_primary(suitableElement);

                        buffered = message;
                    }
                }
                if (buffered != null) {
                    // We need to (re)send the current message to the server
                    Z.net.send_message(buffered);
                    outstanding = true;
                }
            }
        }

        public ConduitFlow GetConduitManager()
        {
            switch (this.conduitType)
            {
                case ConduitType.Gas:
                    return Game.Instance.gasConduitFlow;
                case ConduitType.Liquid:
                    return Game.Instance.liquidConduitFlow;
                default:
                    return (ConduitFlow) null;
            }
        }

        public bool IsConnected
        {
            get
            {
                GameObject gameObject = Grid.Objects[this.utilityCell, this.conduitType == ConduitType.Gas ? 12 : 16];
                return gameObject != null
                    && gameObject.GetComponent<BuildingComplete>() != null;
            }
        }


        private PrimaryElement FindSuitableElement()
        {
            // Need to set this items storage to be the same as the senders?
            List<GameObject> items = this.storage.items;
            int count = items.Count;
            for (int index1 = 0; index1 < count; ++index1)
            {
                int index2 = (index1 + this.elementOutputOffset) % count;
                PrimaryElement component = items[index2].GetComponent<PrimaryElement>();
                if ((UnityEngine.Object) component != (UnityEngine.Object) null) {
                    if ((double) component.Mass > 0.0) {
                        this.elementOutputOffset = (this.elementOutputOffset
                                                    + 1) % count;
                        return component;
                    }
                }
            }
            return (PrimaryElement) null;
        }
    }
}
