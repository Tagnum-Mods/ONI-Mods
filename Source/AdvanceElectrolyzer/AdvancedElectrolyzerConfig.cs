using Newtonsoft.Json;
using TUNING;
using UnityEngine;

namespace AdvanceElectrolyzer
{
    public class AdvancedElectrolyzerConfig : IBuildingConfig
    {
        public const string ID = "AdvacnedElectrolyzer"; //Woops, typo. Can't fix this because save game compatibility.

        private ConduitPortInfo secondaryPort = new ConduitPortInfo(ConduitType.Gas, new CellOffset(0, 1));

        public static Config config = new Config();

        public class Config
        {
            // This is litre per second
            [JsonProperty("Water Consumption Rate")]
            public float waterConsumptionRate = 1f;
            // This is gram per second
            [JsonProperty("Oxygen Output Amount")]
            public float water2OxygenRatio = 0.888f;
            // This is gram per second
            [JsonProperty("Hydrogen Output Amount")]
            public float water2HydrogenRatio = 0.111999989f;
            [JsonProperty("Exhaust Heat Amount")]
            public float heatExhaust = 0f;
            [JsonProperty("Self Heat Amount")]
            public float heatSelf = 4f;
            [JsonProperty("Energy Consumption")]
            public float energyConsumption = 400f;
        }

        public override BuildingDef CreateBuildingDef()
        {
            string id = ID;
            int width = 2;
            int height = 2;
            string anim = "electrolyzer_kanim";
            int hitpoints = 100;
            float construction_time = 30f;
            float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;
            string[] all_metals = MATERIALS.ALL_METALS;
            float melting_point = 800f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            EffectorValues tier2 = NOISE_POLLUTION.NOISY.TIER3;

            BuildingDef def = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tier, all_metals, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER1, tier2, 0.2f);
            def.RequiresPowerInput = true;
            def.PowerInputOffset = new CellOffset(1, 0);
            def.EnergyConsumptionWhenActive = config.energyConsumption;
            def.ExhaustKilowattsWhenActive = config.heatExhaust;
            def.SelfHeatKilowattsWhenActive = config.heatSelf;
            def.ViewMode = OverlayModes.GasConduits.ID;
            def.MaterialCategory = MATERIALS.REFINED_METALS;
            def.AudioCategory = "HollowMetal";
            def.InputConduitType = ConduitType.Liquid;
            def.UtilityInputOffset = new CellOffset(0, 0);
            def.OutputConduitType = ConduitType.Gas;
            def.UtilityOutputOffset = new CellOffset(1, 1);
            def.PermittedRotations = PermittedRotations.FlipH;
            return def;
        }

        private void AttachPort(GameObject go) {
            ConduitSecondaryOutput conduitSecondaryOutput = go.AddComponent<ConduitSecondaryOutput>();
            conduitSecondaryOutput.portInfo = secondaryPort;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();

            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);

            Storage storage = BuildingTemplates.CreateDefaultStorage(go, false);
            storage.showInUI = true;
            storage.capacityKg = 6f;
            storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);

            ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
            conduitConsumer.conduitType = ConduitType.Liquid;
            conduitConsumer.capacityTag = GameTags.AnyWater;
            conduitConsumer.capacityKG = storage.capacityKg;
            conduitConsumer.forceAlwaysSatisfied = true;
            conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;

            AdvancedElectrolyzer electrolyzer = go.AddOrGet<AdvancedElectrolyzer>();
            electrolyzer.portInfo = secondaryPort;
            electrolyzer.storage = storage;
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
            AttachPort(go);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
            AttachPort(go);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            BuildingTemplates.DoPostConfigure(go);
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}
