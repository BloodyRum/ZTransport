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
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZTransport {
    [SerializationConfig(KSerialization.MemberSerialization.OptIn)]    
    [DebuggerDisplay("{name}")]
    public class JoulesSender : KMonoBehaviour, ISaveLoadable, ISim200ms
    {
        [MyCmpGet]
        protected Battery battery;
        bool outstanding = false;
        int outstanding_joules = 0;

        protected override void OnSpawn() {
            base.OnSpawn();
            this.battery = this.GetComponent<Battery>();
        }

        public void Sim200ms(float dt)
        {            
            int x, y;
            Grid.CellToXY(Grid.PosToCell(this), out x, out y);

            JObject message = Z.net.get_message_for("sent_joules", x, y);
            if(outstanding && message != null) {
                // We got a response from the server, deal with it
                int spare = (int)message["spare"];
                if ((outstanding_joules - spare) > 0) {
                    this.battery.ConsumeEnergy(outstanding_joules - spare);
                }
                outstanding = false;
                outstanding_joules = 0;
            }
            if(!outstanding) {
                // We are not currently waiting for a message
                // so make a message, and then wait for response
                int int_joules_to_send = (int)this.battery.JoulesAvailable;
                if(int_joules_to_send > 0) {
                    outstanding_joules = int_joules_to_send;
                    message = Network.make_message("send_joules", x, y);
                    message["joules"] = int_joules_to_send;
                    Z.net.send_message(message);
                    outstanding = true;
                }
            }
        }
    }
}
