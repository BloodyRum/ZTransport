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
    public class DropBoxSenderStatusDisplayer : KMonoBehaviour, ISim200ms {

        #pragma warning disable 649
        [MyCmpReq]
        private KSelectable selectable;

        [MyCmpReq]
        private ObjectSender object_sender;
        #pragma warning restore 649


        public void Sim200ms(float dt) {
            int object_count = object_sender.get_objects_count();

            if (object_count != 0) {
                selectable.SetStatusItem(Z.dropBoxStatus,
                                         Z.dropBoxSending,
                                         object_count);
            } else {
                selectable.SetStatusItem(Z.dropBoxStatus, null, null);
            }
        }
    }
}
