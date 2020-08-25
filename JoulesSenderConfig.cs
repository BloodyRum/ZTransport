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

using TUNING;
using UnityEngine;

namespace ZTransport {
    public class JoulesSenderConfig : IBuildingConfig
    {
        public const string ID = "ZJoulesSender";
        public const float MAX_STORED_JOULES = 20000f;

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 1, "zpowersender_1kw_kanim", 30, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.RAW_METALS, 800f, BuildLocationRule.Anywhere, BUILDINGS.DECOR.PENALTY.TIER1, NOISE_POLLUTION.NONE, 0.2f);
            buildingDef.RequiresPowerInput = true;
            buildingDef.PowerInputOffset = new CellOffset(0, 0);
            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.AudioCategory = "Metal";
            buildingDef.ExhaustKilowattsWhenActive = 0f;
            buildingDef.SelfHeatKilowattsWhenActive = 0f;
            buildingDef.Floodable = false;
            buildingDef.Overheatable = false;
            buildingDef.Entombable = false;

            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            ZTransporter ztransporter = go.AddOrGet<ZTransporter>();

            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery, false);
            go.AddComponent<RequireInputs>();
            BuildingDef def = go.GetComponent<Building>().Def;
            Battery battery = go.AddOrGet<Battery>();
            battery.capacity = MAX_STORED_JOULES;
            battery.joulesLostPerSecond = 0f;
            go.AddComponent<JoulesSender>();
        }

        public override void DoPostConfigureComplete(GameObject go) {
            go.AddOrGet<Battery>().powerSortOrder = 1000;
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}
