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
    public class ZTransporter : KMonoBehaviour {
        #pragma warning disable 414
        private static readonly EventSystem.IntraObjectHandler<ZTransporter> RefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<ZTransporter>((System.Action<ZTransporter, object>) ((component, data) => component.RefreshUserMenu(data)));
        #pragma warning restore 414

        private static string address;
        private static ushort port;
        
        private void RefreshUserMenu(object data = null) {
            KIconButtonMenu.ButtonInfo info;
            info = new KIconButtonMenu
                .ButtonInfo("action_repair",
                            (string)STRINGS.ZTRANSPORT.SETTINGS_BUTTON,
                            new System.Action(this.PressItBaby),
                            Action.NumActions,
                            (System.Action<GameObject>) null,
                            (System.Action<KIconButtonMenu.ButtonInfo>) null,
                            (Texture) null,
                            (string)STRINGS.ZTRANSPORT.SETTINGS_TOOLTIP,
                            true);
            Game.Instance.userMenu.AddButton(this.gameObject, info, 1000f);
        }
        protected override void OnSpawn() {
            base.OnSpawn();
            this.Subscribe<ZTransporter>(493375141, ZTransporter.RefreshUserMenuDelegate);
        }

        private void PressItBaby() {
            address = Z.address;
            port = Z.port;

            var dialog = new PDialog("ZTransportOptions") {
                Title = "ZTransport Options",
                Size = new Vector2(320f, 200f),
                DialogBackColor = PUITuning.Colors.OptionsBackground,
                DialogClosed = OnDialogClosed,
                MaxSize = new Vector2(320f, 200f),
            };
            dialog
                .AddButton("ok", STRINGS.UI.CONFIRMDIALOG.OK,
                           "Apply changes immediately.", // TODO: LocString
                           PUITuning.Colors.ButtonPinkStyle)
                .AddButton(PDialog.DIALOG_KEY_CLOSE,
                           STRINGS.UI.CONFIRMDIALOG.CANCEL,
                           PUIStrings.TOOLTIP_CANCEL,
                           PUITuning.Colors.ButtonBlueStyle);
            var body = dialog.Body;
            var panel = new PPanel("ServerAddressAndPortPanel") {
                Direction = PanelDirection.Vertical,
                Alignment = TextAnchor.UpperLeft,
                FlexSize = Vector2.right,
            };
            panel.AddChild(new PLabel("ServerAddressLabel") {
                    TextAlignment = TextAnchor.UpperLeft,
                    Text = "Server Address", // TODO: LocString
                    FlexSize = Vector2.right,
                    Margin = new RectOffset(0, 10, 0, 10),
                });
            panel.AddChild(new PTextField("ServerAddressField") {
                    Text = Z.address,
                    TextStyle = PUITuning.Fonts.TextDarkStyle,
                    FlexSize = Vector2.right,
                    ToolTip = "Address of onizd server. Leave empty to "
                    +"disable ZTransport on this world.", //TODO: LocString
                    OnTextChanged = ServerAddressChanged,
                });
            panel.AddChild(new PLabel("ServerPortLabel") {
                    TextAlignment = TextAnchor.UpperLeft,
                    Text = "Server Port", // TODO: LocString
                    FlexSize = Vector2.right,
                    Margin = new RectOffset(0, 10, 0, 10),
                });
            panel.AddChild(new PTextField("ServerPortField") {
                    Text = Z.port.ToString(),
                    TextStyle = PUITuning.Fonts.TextDarkStyle,
                    FlexSize = Vector2.right,
                    ToolTip = "Port number of onizd server. Leave empty to "
                    +"use the default port (5496).", //TODO: LocString
                    OnTextChanged = ServerPortChanged,
                    OnValidate = ServerPortValidate,
                });
            body.AddChild(panel);
            var built = dialog.Build();
            var screen = built.GetComponent<KScreen>();
            screen.Activate();
        }

        private void ServerAddressChanged(GameObject widget, string neu) {
            if (!neu.Equals("")) {
                address = neu;
            } else {
                address = null;
            }
        }

        private void ServerPortChanged(GameObject widget, string neu) {
            if (!neu.Equals("")) {
                port = ushort.Parse(neu);
            } else {
                port = 5496;
            }
        }

        private char ServerPortValidate(string text, int i, char c) {
            // Ports can't be longer than 5 characters
            if(text.Length >= 5) return (char)0;
            // Ports can't contain non-digit characters
            else if(!(c >= '0' && c <= '9')) return (char)0;
            // Ports must be <= 65535
            string neu = text.Insert(i, c.ToString());
            int value = int.Parse(neu);
            if(value > 65535) return (char)0;
            // This seems legit -SB
            return c;
        }

        private void OnDialogClosed(string action) {
            if(action == "ok") {
                Z.address = address;
                Z.port = port;

                Z.net.connect(Z.address, Z.port);
            }
        }
    }
}
