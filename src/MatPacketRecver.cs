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
using System; // Needed for math
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZTransport {
    
    [SerializationConfig(KSerialization.MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/scripts/MatPacketRecver")]
    public class MatPacketRecver : KMonoBehaviour, ISaveLoadable
    {
        private static readonly Operational.Flag outputConduitFlag = new Operational.Flag("output_conduit", Operational.Flag.Type.Functional);
        private int utilityCell = -1;
        [SerializeField]
        public ConduitType conduitType;
        [SerializeField]
        public SimHashes[] elementFilter;
        [SerializeField]
        public bool invertElementFilter;
        [SerializeField]
        public bool blocked;
        [MyCmpReq]
#pragma warning disable 0649
        private Operational operational;
#pragma warning restore 0649
        private HandleVector<int>.Handle partitionerEntry;

        public ConduitType TypeOfConduit
        {
            get
            {
                return this.conduitType;
            }
        }

        public ConduitFlow.ConduitContents ConduitContents
        {
            get
            {
                return this.GetConduitManager().GetContents(this.utilityCell);
            }
        }

        public void SetConduitType(ConduitType type)
        {
            this.conduitType = type;
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

        private void OnConduitConnectionChanged(object data)
        {
            this.Trigger(-2094018600, (object) this.IsConnected);
        }

        SubstanceChunk dave;
        PrimaryElement steve;
        
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

        private void ConduitUpdate(float dt)
        {
            this.operational.SetFlag(MatPacketRecver.outputConduitFlag, this.IsConnected);
            this.blocked = false;
            this.Dispense(dt);
        }

        private void Dispense(float dt)
        {
            PrimaryElement suitableElement = this.FindSuitableElement();
            if (!((UnityEngine.Object) suitableElement != (UnityEngine.Object) null))
                return;
            suitableElement.KeepZeroMassObject = true;
            float num1 = this.GetConduitManager().AddElement(this.utilityCell, suitableElement.ElementID, suitableElement.Mass, suitableElement.Temperature, suitableElement.DiseaseIdx, suitableElement.DiseaseCount);
            if ((double) num1 > 0.0)
            {
                int num2 = (int) ((double) num1 / (double) suitableElement.Mass * (double) suitableElement.DiseaseCount);
                suitableElement.ModifyDiseaseCount(-num2, "MatPacketRecver.ConduitUpdate");
                suitableElement.Mass -= num1;
                this.Trigger(-1697596308, (object) suitableElement.gameObject);
            }
            else
                this.blocked = true;
        }

        bool outstanding = false;

        private PrimaryElement FindSuitableElement() {
            PrimaryElement ret = null;
            // Maybe make it buffer an extra message, that might help?

            // better version: grab stuff from our ass, I mean network - BR

            int x, y;
            Grid.CellToXY(Grid.PosToCell(this), out x, out y);

            JObject message = Z.net.get_message_for("got_packet", x, y);
            if (outstanding && message != null) {
                outstanding = false;
                // We have a response from the server, finally ya lazy basterd
                if (message["packet"] != null
                    && message["packet"].Type == JTokenType.Object) {
                    
                    JObject mat_packet = (JObject) message["packet"];
                    JObject germ_packet = null;
                    if (mat_packet["germs"] != null
                        && mat_packet["germs"].Type == JTokenType.Object) {
                        germ_packet = (JObject) mat_packet["germs"];
                    }
                    if (dave == null) {
                        dave = GasSourceManager.Instance.CreateChunk(
                            (SimHashes) (-1528777920), 0f, 456f,
                            255, 0, this.transform.GetPosition());
                        steve = dave.GetComponent<PrimaryElement>();
                        steve.KeepZeroMassObject = true;
                    }
                    steve.SetElement((SimHashes)((int) mat_packet["element"]));
                    steve.SetMassTemperature((float) mat_packet["mass"],
                                            (float) mat_packet["temperature"]);
                    string reason = conduitType == ConduitType.Liquid
                        ? "Storage.AddLiquid" : "Storage.AddGasChunk";
                    steve.ModifyDiseaseCount(-steve.DiseaseCount, reason);
                    if (germ_packet != null) {
                        steve.AddDisease((byte) germ_packet["id"],
                                         (int) germ_packet["count"],
                                         reason);
                    }
                    
                    message = null;

                    ret = steve;
                }
            }
            if (!outstanding) {
                // Send a message to the server asking for goodies

                message = Network.make_message("recv_packet", x, y);
                message.Add("phase", this.conduitType == ConduitType.Liquid ?
                            "Liquid" : "Gas");
                Z.net.send_message(message);
                outstanding = true;
            }
            return ret;
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
    }
}
