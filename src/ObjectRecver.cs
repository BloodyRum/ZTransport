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
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZTransport {

    [SerializationConfig(KSerialization.MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/scripts/ObjectRecver")]
    public class ObjectRecver : KMonoBehaviour, ISaveLoadable, ISim1000ms
    {
#pragma warning disable 0649
        [MyCmpReq]
        private Storage storage;
        [MyCmpGet]
        private ZTransporter ztransporter;
#pragma warning restore 0649

        bool outstanding = false;

        GameObject convert_to_gameobject(string base64) {
            byte[] compression = System.Convert.FromBase64String(base64);
            MemoryStream stream = new MemoryStream(compression);
            MemoryStream other_stream = new MemoryStream(4096); // ughhhh
            using(DeflateStream inflater = new DeflateStream(stream, CompressionMode.Decompress)) {
                inflater.CopyTo(other_stream);
            }
            byte[] object_info = other_stream.ToArray();
            // Magic voodoo
            IReader reader = new FastReader(object_info);
            Tag tag = TagManager.Create(reader.ReadKleiString());

            // Blood sacrifice -SB
            KSerialization.Manager.Clear();
            KSerialization.Manager.DeserializeDirectory(reader);

            Transform transform = this.transform;

            // Even more magic voodoo
            // The first argument gets us a blank prefab for an object defined
            //   in the base64 string, put there by the sender
            // Arugments 2-4 get the position, rotation, and localScale of the
            //   current 'this' kmonobehavior
            // Lastly is the Reader which is above
            // And finally, because its a SaveLoadRoot, that just loaded an
            //   object, we need to get that object
            return SaveLoadRoot.Load(SaveLoader.Instance.saveManager.GetPrefab(tag),
                                     transform.GetPosition(),
                                     transform.rotation,
                                     transform.localScale,
                                     reader).gameObject;
        }

        public void Sim1000ms(float dt) {
            int x, y;
            Grid.CellToXY(Grid.PosToCell(this), out x, out y);

            JObject message = Z.net.get_message_for("got_object", x, y);
            if (outstanding && message != null) {
                outstanding = false;
                // We have a response from the server, finally ya lazy basterd
                if (message["object"] != null
                    && message["object"].Type == JTokenType.String) {

                    GameObject opaque_object = convert_to_gameobject((string)message["object"]);
                    storage.Store(opaque_object); // Note: We are not *adding*
                                                  // we are *storing*
                }
            }
            // Only send when we are enabled
            if(!outstanding && !storage.IsFull() && ztransporter.is_enabled()) {
                // Send a message to the server asking for an object

                message = Network.make_message("recv_object", x, y);
                Z.net.send_message(message);
                outstanding = true;
            }

            return;
        }

    }
}
