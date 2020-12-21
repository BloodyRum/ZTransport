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

using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace ZTransport {
    public class DropBoxRecverConfig : IBuildingConfig
    {

        public const string ID = "ZDropBoxRecver";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef buildingDef = BuildingTemplates.
                CreateBuildingDef(// building ID, width, height, .kanim ID,
                                  ID, 1, 2, "zdropboxrecver_kanim",
                                  // hitpoints, construction time,
                                  100, 120f,
                                  // build mass, build materials,
                                  TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER1,
                                  MATERIALS.ALL_METALS,
                                  // melting point (??), build location rule,
                                  800f, BuildLocationRule.OnFloor,
                                  // decor and noise
                                  TUNING.BUILDINGS.DECOR.PENALTY.TIER1,
                                  TUNING.NOISE_POLLUTION.NOISY.TIER0);
            buildingDef.AudioCategory = "HollowMetal";
            buildingDef.Floodable = false;
            buildingDef.Overheatable = false;
            buildingDef.Entombable = true;

            buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));

            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<ZTransporter>().SetLocalAndExpectedID("SolidRecver",
                                                              "SolidSender");
            go.AddOrGet<ZConfigButton>();

            go.AddOrGet<OperationalGlueShimThing>();

            GeneratedBuildings.MakeBuildingAlwaysOperational(go);

            Storage storage = go.AddOrGet<Storage>();
            storage.capacityKg = 1000f;
            storage.showInUI = true;
            storage.showDescriptor = true;
            storage.onlyTransferFromLowerPriority = true;
            storage.allowItemRemoval = true;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            Prioritizable.AddRef(go);

            go.AddOrGet<BuildingEnabledButton>();

            go.AddOrGet<ObjectRecver>();
        }
    }
}
