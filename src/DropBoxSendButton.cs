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
using TMPro;
using PeterHan.PLib.UI;

namespace ZTransport {
    public class DropBoxSendButton : KMonoBehaviour {
        #pragma warning disable 414, 649
        private static readonly EventSystem.IntraObjectHandler<DropBoxSendButton> RefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<DropBoxSendButton>((System.Action<DropBoxSendButton, object>) ((component, data) => component.RefreshUserMenu(data)));
        #pragma warning restore 414, 649

        private void RefreshUserMenu(object data = null) {
            KIconButtonMenu.ButtonInfo info;
            info = new KIconButtonMenu
                .ButtonInfo("action_repair",
                            (string)STRINGS.ZTRANSPORT.DROP_BOX.SEND,
                            new System.Action(this.ButtonPressed),
                            Action.NumActions,
                            (System.Action<GameObject>) null,
                            (System.Action<KIconButtonMenu.ButtonInfo>) null,
                            (Texture) null,
                            (string)STRINGS.ZTRANSPORT.DROP_BOX.TOOLTIP,
                            true);
            Game.Instance.userMenu.AddButton(this.gameObject, info, 1000f);
        }
        protected override void OnSpawn() {
            base.OnSpawn();
            this.Subscribe<DropBoxSendButton>(493375141, DropBoxSendButton.RefreshUserMenuDelegate);
        }

        private void ButtonPressed() {
            if (this.gameObject.AddOrGet<ObjectSender>().get_objects_count() > 0) {
                return;
            }
            while (this.gameObject.AddOrGet<ObjectSender>().add_to_object_list() != false) {};
        }
    }
}
