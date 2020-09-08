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
    public class LiquidRecverConfig : IBuildingConfig
    {
        public const string ID = "ZLiquidRecver";
        private const ConduitType CONDUIT_TYPE = ConduitType.Liquid;

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 1, "zliquidrecver_kanim", 100, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.ALL_METALS, 800f, BuildLocationRule.Anywhere, TUNING.BUILDINGS.DECOR.PENALTY.TIER1, TUNING.NOISE_POLLUTION.NOISY.TIER0, 0.2f);
            buildingDef.OutputConduitType = ConduitType.Liquid;
            buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
            buildingDef.AudioCategory = "HollowMetal";
            buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
            buildingDef.Floodable = false;
            buildingDef.Overheatable = false;
            buildingDef.Entombable = false;

            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, ID);
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<ZTransporter>();
            go.AddOrGet<ZConfigButton>();

            MatPacketRecver matpacketrecver = go.AddOrGet<MatPacketRecver>();
            matpacketrecver.conduitType = ConduitType.Liquid;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayBehindConduits, false);
        }
    }
}
