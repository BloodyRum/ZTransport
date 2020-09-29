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

using UnityEngine;

namespace ZTransport {
    public class ZTransporter : KMonoBehaviour, ISim1000ms {

        #pragma warning disable 649
        [MyCmpReq]
        private KSelectable selectable;
        [MyCmpGet]
        private Operational operational;
        #pragma warning restore 649

        private static Operational.Flag remote_connected = new Operational.Flag("remote_connected", Operational.Flag.Type.Requirement);

        [SerializeField]
        string local_id;
        [SerializeField]
        string expected_id;

        int x, y;

        public void Sim1000ms(float dt) {
            this.operational.SetFlag(remote_connected,
                                     should_be_operational());
        }

        protected override void OnSpawn() {
            base.OnSpawn();
            selectable.SetStatusItem(Db.Get().StatusItemCategories.Main,
                                     Z.coordinates, (object)this);

            Grid.CellToXY(Grid.PosToCell(this), out x, out y);

            Z.net.register_local_device(x, y, local_id);
        }

        protected override void OnCleanUp() {
            base.OnCleanUp();

            Z.net.unregister_local_device(x, y, local_id);
        }

        public void SetLocalAndExpectedID(string local_id,
                                          string expected_id) {
            this.local_id = local_id;
            this.expected_id = expected_id;
        }

        private bool should_be_operational() {
            return Z.net.remote_device_exists(x, y, expected_id);
        }
    }
}


// Senders always operational, recvers not
