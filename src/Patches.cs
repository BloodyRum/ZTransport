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

using Harmony;
using System;
using System.Collections.Generic;
using Database;
using UnityEngine;
using PeterHan.PLib;
using PeterHan.PLib.Datafiles;

namespace ZTransport
{
    public class Patches
    {
        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                PUtil.InitLibrary(true);
                PLocalization.Register();
                Z.net = new Network();
            }
        }

        [HarmonyPatch(typeof(BuildingStatusItems), "CreateStatusItems")]
        public class CreateStatusItems {
            public static void Postfix(BuildingStatusItems __instance) {
                // Building cords
                Z.coordinates = new StatusItem("ZCoordinates", "ZTRANSPORT","",
                                               StatusItem.IconType.Info,
                                               NotificationType.Neutral,
                                               false,
                                               OverlayModes.None.ID);
                Z.coordinates.resolveStringCallback = (str, data) => {
                    int x, y;
                    ZTransporter transporter = (ZTransporter)data;
                    Grid.CellToXY(Grid.PosToCell(transporter), out x, out y);
                    return str.Replace("{X}", x.ToString())
                        .Replace("{Y}", y.ToString());
                };
                __instance.Add(Z.coordinates);


                // Connected status
                Z.notConnected = new StatusItem("ZNotConnected",
                                                "ZTRANSPORT", "",
                                               StatusItem.IconType.Exclamation,
                                                NotificationType.BadMinor,
                                                false,
                                                OverlayModes.None.ID);
                Z.notConnected.resolveStringCallback = (str, data) => {
                    string ererrroroor = (string)data;
                    return str.Replace("{REASON}", ererrroroor);
                };
                Z.notConnected.AddNotification();
                __instance.Add(Z.notConnected);

            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public class LoadGeneratedBuildings {
            public static void Postfix() {
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESRECVER1KW.NAME",
                            "Z Power Receiver 1kW");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESRECVER1KW.DESC",
                            "Takes up to 1kW power from another Z level.");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESRECVER1KW.EFFECT",
                            "Takes up to 1kW power from another Z level");
                ModUtil.AddBuildingToPlanScreen("Power", JoulesRecver1kWConfig.ID);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESRECVER2KW.NAME",
                            "Z Power Receiver 2kW");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESRECVER2KW.DESC",
                            "Takes up to 2kW power from another Z level.");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESRECVER2KW.EFFECT",
                            "Takes up to 2kW power from another Z level");
                ModUtil.AddBuildingToPlanScreen("Power", JoulesRecver2kWConfig.ID);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESRECVER20KW.NAME",
                            "Z Power Receiver 20kW");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESRECVER20KW.DESC",
                            "Takes up to 20kW power from another Z level.");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESRECVER20KW.EFFECT",
                            "Takes up to 20kW power from another Z level");
                ModUtil.AddBuildingToPlanScreen("Power", JoulesRecver20kWConfig.ID);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESRECVER50KW.NAME",
                            "Z Power Receiver 50kW");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESRECVER50KW.DESC",
                            "Takes up to 50kW power from another Z level.");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESRECVER50KW.EFFECT",
                            "Takes up to 50kW power from another Z level");
                ModUtil.AddBuildingToPlanScreen("Power", JoulesRecver50kWConfig.ID);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESSENDER.NAME",
                            "Z Power Sender");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESSENDER.DESC",
                            "Sends and buffers power to another Z level.");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZJOULESSENDER.EFFECT",
                            "Sends and buffers power to another Z level");
                ModUtil.AddBuildingToPlanScreen("Power", JoulesSenderConfig.ID);

                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZGASSENDER.NAME",
                            "Z Gas Sender");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZGASSENDER.DESC",
                            "Sends and buffers gas to another Z level.");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZGASSENDER.EFFECT",
                            "Sends and buffers gas to another Z level");
                ModUtil.AddBuildingToPlanScreen("HVAC", GasSenderConfig.ID);

                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZGASRECVER.NAME",
                            "Z Gas Receiver");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZGASRECVER.DESC",
                            "Takes gas from another Z level");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZGASRECVER.EFFECT",
                            "Takes gas from another Z level");
                ModUtil.AddBuildingToPlanScreen("HVAC", GasRecverConfig.ID);

                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZLIQUIDSENDER.NAME",
                            "Z Liquid Sender");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZLIQUIDSENDER.DESC",
                            "Sends and buffers liquid to another Z level.");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZLIQUIDSENDER.EFFECT",
                            "Sends and buffers liquid to another Z level");
                ModUtil.AddBuildingToPlanScreen("Plumbing", LiquidSenderConfig.ID);

                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZLIQUIDRECVER.NAME",
                            "Z Liquid Receiver");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZLIQUIDRECVER.DESC",
                            "Takes liquid from another Z level");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.ZLIQUIDRECVER.EFFECT",
                            "Takes liquid from another Z level");
                ModUtil.AddBuildingToPlanScreen("Plumbing", LiquidRecverConfig.ID);

            }
        }

        [HarmonyPatch(typeof(HoverTextConfiguration), "DrawInstructions")]
        public class HoverTextConfigurationPatch {
            public static void Postfix(HoverTextConfiguration __instance,
                                       HoverTextDrawer drawer) {
                if(__instance == null) return;
                if(!(__instance is BuildToolHoverTextCard)) return;
                BuildToolHoverTextCard hover_text_card
                    = (BuildToolHoverTextCard) __instance;
                if(hover_text_card.currentDef == null) return;
                if(hover_text_card.currentDef.BuildingComplete == null) return;

                ZTransporter possible_ztransport = hover_text_card.currentDef.BuildingComplete.GetComponent<ZTransporter>();
                if (possible_ztransport != null) {
                    drawer.NewLine(26);  // All the cool kids are doing it -SB
                    drawer.AddIndent(8); // I LEARNED IT FROM WATCHING [Klei] -SB
                    int x, y;

                    var pos = KInputManager.GetMousePos();
                    var point = Camera.main.ScreenToWorldPoint(pos);
                    var cell = Grid.PosToCell(point);
                    Grid.CellToXY(cell, out x, out y);

                    var coords_string = STRINGS.ZTRANSPORT.STATUSITEMS.ZCOORDINATES.NAME.Replace("{X}", x.ToString()).Replace("{Y}", y.ToString());
                    drawer.DrawText(coords_string,
                                  hover_text_card.Styles_Instruction.Standard);
                }
            }
        }

        [HarmonyPatch(typeof(SaveGame), "OnPrefabInit")]
        public class SaveGameComponentPatch {
            public static void Postfix(SaveGame __instance) {
                __instance.gameObject.AddComponent<ZServerInfoSaver>();
            }
        }
        
        [HarmonyPatch(typeof(SaveLoader), "Load", new Type[]{typeof(IReader)})]
        public class DontLoadThatRoad {
            public static void Prefix() {
                Z.address = null;
                Z.port = Z.DEFAULT_PORT;
                Z.ping_interval = Z.DEFAULT_PING_INTERVAL;
                Z.net.connect(Z.address, Z.port);
            }
        }

        [HarmonyPatch(typeof(MainMenu), "OnSpawn")]
        public class SeriouslyDontLoadThatRoad {
            public static void Prefix() {
                Z.address = null;
                Z.port = Z.DEFAULT_PORT;
                Z.ping_interval = Z.DEFAULT_PING_INTERVAL;
                Z.net.connect(Z.address, Z.port);
            }
        }

        [HarmonyPatch(typeof(Telepad), "OnSpawn")]
        public class ConfigPod {
            public static void Postfix(Telepad __instance) {
                __instance.gameObject.AddComponent<ZConfigButton>();
                __instance.gameObject.AddComponent<ZConnectionStatusDisplayer>();
            }
        }


        [HarmonyPatch(typeof(StatusItemCategories), MethodType.Constructor,
                      new Type[] { typeof(ResourceSet) })]
        public class StatusItemCategoriesPatch {
            public static void Postfix(StatusItemCategories __instance) {
                Z.serverStatus = new StatusItemCategory(nameof (Z.serverStatus), (ResourceSet) __instance, nameof (Z.serverStatus));
            }
        }

    }
}
