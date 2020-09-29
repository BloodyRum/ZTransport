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

using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZTransport {
    [DebuggerDisplay("{name}")]
    public class JoulesRecver : Generator
    {
        private bool outstanding = false;

        public override void EnergySim200ms(float dt)
        {
            base.EnergySim200ms(dt);
            //this.AssignJoulesAvailable(Math.Min(this.JoulesAvailable, this.WattageRating * dt));

            int x, y;
            Grid.CellToXY(Grid.PosToCell(this), out x, out y);

            JObject message = Z.net.get_message_for("got_joules", x, y);
            if(outstanding && message != null) {
                // We got a response from the server, check it out

                // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA -SB
                int amount_to_add = (int)message["joules"];
                if(amount_to_add > 0) {
                    this.GenerateJoules(amount_to_add, true);
                }
                // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAa. -SB
                outstanding = false;
            }
            if(!outstanding) {
                // Time to make a message to send to the server
                int amount_to_request = (int)(this.Capacity - this.JoulesAvailable);
                if (amount_to_request > 0) {
                    message = Network.make_message("recv_joules", x, y);
                    message["max_joules"] = amount_to_request;
                    Z.net.send_message(message);
                    outstanding = true;
                }
            }
        }
    }

}
