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
using System.IO.Compression;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;


namespace ZTransport {

    [SerializationConfig(KSerialization.MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/scripts/ObjectSender")]
    public class ObjectSender : KMonoBehaviour, ISaveLoadable, ISim1000ms {

        private const int MAX_MESSAGE_SIZE = 4096;

        [MyCmpReq]
        public Storage storage;

        #pragma warning disable 0649
        [MyCmpGet]
        private ZTransporter ztransporter;
        #pragma warning restore 0649

        public bool autoSend = false;

        protected override void OnSpawn()
        {
            base.OnSpawn();
        }

        protected override void OnCleanUp()
        {
            base.OnCleanUp();
        }

        bool outstanding = false;

        [SerializeField]
        List<String> objects = new List<String>();

        public int get_objects_count() {
            return objects.Count;
        }

        public void drop_item(GameObject go) {
            // Some weird voodoo magic is needed to drop items on the ground
            // so lets just add the object back into storage and make
            // that handle it

            storage.Store(go);
            storage.Drop(go);
        }

        public bool add_to_object_list() {
            GameObject suitableObject
                = this.PopObject();

            // No suitable objects to send, so we didnt add anything
            if (suitableObject == null) return false;

            KSerialization.Manager.Clear();

            MemoryStream stream = new MemoryStream(MAX_MESSAGE_SIZE);
            BinaryWriter writer = new BinaryWriter(stream);

            suitableObject.AddOrGet<SaveLoadRoot>().SaveWithoutTransform(writer);

            byte[] serialized_object = stream.ToArray();
            // oh boy... -SB
            stream = new MemoryStream(MAX_MESSAGE_SIZE);
            using(DeflateStream deflated = new DeflateStream(stream, CompressionMode.Compress)) {
                writer = new BinaryWriter(deflated);
                string name = suitableObject.GetComponent<KPrefabID>().GetSaveLoadTag().Name;
                writer.WriteKleiString(name);
                // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA -SB
                KSerialization.Manager.SerializeDirectory(writer);
                writer.Write(serialized_object);
                // IS THIS ENOUGH? WHAT MORE DO YOU WANT, O GREAT AND CAP-
                // RICIOUS GODS OF SERIALIZATION MAGIC?! -SB
            }

            var message = stream.ToArray();
            if (message.Length > MAX_MESSAGE_SIZE) {
                Debug.Log("Object Buffered exceded the max bytes so, I THREW IT ON THE GROUND");
                drop_item(suitableObject);
            } else {
                objects.Add(System.Convert.ToBase64String(message));
                Util.KDestroyGameObject(suitableObject);
            }

            return true;
        }

        public void Sim1000ms(float dt) {
            int x, y;
            Grid.CellToXY(Grid.PosToCell(this), out x, out y);

            // Buffer five objects to send, that way you dont only get
            // 20g (or whatever) per second
            if (autoSend && (objects.Count <= 5)) add_to_object_list();

            JObject message = Z.net.get_message_for("sent_object", x, y);
            if(outstanding && message != null) {
                // We got a response from the server, deal with it
                if ((bool) message["accepted"]) {
                    objects.RemoveAt(0);
                }
                outstanding = false;
            }
            // Only send message when enabled, you know the rest
            if(!outstanding && objects.Count != 0 && ztransporter.is_enabled_and_linked()) {
                // We are not currently waiting for a message
                // so make a message, and then wait for response
                message = Network.make_message("send_object", x, y);
                String object_to_send = objects[0];
                message["object"] = object_to_send;
                // We need to (re)send the current message to the server
                Z.net.send_message(message);
                outstanding = true;
            }
        }

        private GameObject PopObject()
        {
            List<GameObject> items = this.storage.items;

            if (items.Count == 0) return null;

            GameObject item = items[0];
            storage.Remove(item, false);
            return item;
        }
    }
}
