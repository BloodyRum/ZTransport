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
using System.Runtime.Serialization;

namespace ZTransport {

    [SerializationConfig(KSerialization.MemberSerialization.OptIn)]
    public class ZServerInfoSaver : KMonoBehaviour, ISaveLoadable {


        [Serialize]
        public string address = null;

        [Serialize]
        public ushort port = Z.DEFAULT_PORT;

        [Serialize]
        public int ping_interval = Z.DEFAULT_PING_INTERVAL;

        [System.Runtime.Serialization.OnSerializing]
        private void OnSerializing() { // Are you happy now?
            address = Z.address;
            port = Z.port;
            ping_interval = Z.ping_interval;
        }

        [System.Runtime.Serialization.OnDeserialized]
        private void OnDeserialized() {
            Z.address = address;
            Z.port = port;
            Z.ping_interval = ping_interval;
            Z.net.connect(Z.address, Z.port);
        }
    }
}
