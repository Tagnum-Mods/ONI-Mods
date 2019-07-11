using TUNING;
using UnityEngine;

namespace AdvanceElectrolyzer
{
    public class AdvancedElectrolyzerConfig : IBuildingConfig
    {
        public const string ID = "AdvacnedElectrolyzer";

        // This is litre per second
        public const float WATERCONSUMPTIONRATE = 1f;
        // This is gram per second
        public const float WATER2OXYGEN_RATIO = 1f;
        public const float WATER2HYDROGEN_RATIO = 2f;

        //Defines the effieceny of the conversion
        public const float EFFIECENY = 50f;

        public const float OXYGEN_TEMPERATURE = 343.15f;

        private ConduitPortInfo secondaryPort = new ConduitPortInfo(ConduitType.Gas, new CellOffset(0, 1));

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
            def.EnergyConsumptionWhenActive = 400f;
            def.ExhaustKilowattsWhenActive = 0f;
            def.SelfHeatKilowattsWhenActive = 4f;
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
            conduitConsumer.consumptionRate = WATERCONSUMPTIONRATE;
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
