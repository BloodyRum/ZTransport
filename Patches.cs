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
    }
}
