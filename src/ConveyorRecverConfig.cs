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
    public class ConveyorRecverConfig : IBuildingConfig
    {
        // I like how they are all called 'recver', mainly because I don't
        // remember how to spell reciever -BR
        public const string ID = "ZConveyorRecver";

        public override BuildingDef CreateBuildingDef() {
            BuildingDef buildingDef = BuildingTemplates.
                CreateBuildingDef(// building ID, width, height, .kanim ID,
                                  ID, 1, 1, "zconveyorrecver_kanim",
                                  // hitpoints, construction time,
                                  100, 120f,
                                  // build mass, build materials,
                                  TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER1,
                                  MATERIALS.ALL_METALS,
                                  // melting point (??), build location rule,
                                  800f, BuildLocationRule.Anywhere,
                                  // decor and noise
                                  TUNING.BUILDINGS.DECOR.PENALTY.TIER1,
                                  TUNING.NOISE_POLLUTION.NOISY.TIER0);

            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 120f;
            buildingDef.ExhaustKilowattsWhenActive = 0.0f;
            buildingDef.SelfHeatKilowattsWhenActive = 2f;
            buildingDef.Floodable = false;
            buildingDef.ViewMode = OverlayModes.SolidConveyor.ID;
            buildingDef.AudioCategory = "Metal";
            buildingDef.OutputConduitType = ConduitType.Solid;
            buildingDef.PowerInputOffset = new CellOffset(0, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.SolidConveyorIDs, "ZConveyorRecver");

            buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));

            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag) {
            go.AddOrGet<ZTransporter>().SetLocalAndExpectedID("SolidRecver",
                                                              "SolidSender");
            go.AddOrGet<ZConfigButton>();
            go.AddOrGet<OperationalGlueShimThing>();

            List<Tag> tagList = new List<Tag>();
            tagList.AddRange((IEnumerable<Tag>) STORAGEFILTERS.NOT_EDIBLE_SOLIDS);
            tagList.AddRange((IEnumerable<Tag>) STORAGEFILTERS.FOOD);
            Storage storage = go.AddOrGet<Storage>();
            storage.capacityKg = 100f;
            storage.showInUI = true;
            storage.showDescriptor = true;
            storage.storageFilters = tagList;
            storage.allowItemRemoval = false;

            GeneratedBuildings.MakeBuildingAlwaysOperational(go);

            go.AddOrGet<ObjectRecver>();
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            base.DoPostConfigureUnderConstruction(go);
            go.GetComponent<Constructable>().requiredSkillPerk = Db.Get().SkillPerks.ConveyorBuild.Id;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery, false);
            go.AddOrGet<EnergyConsumer>();
            go.AddOrGet<SolidConduitDispenser>();
        }
    }
}
