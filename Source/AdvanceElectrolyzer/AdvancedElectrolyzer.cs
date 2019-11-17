using KSerialization;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace AdvanceElectrolyzer
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class AdvancedElectrolyzer : KMonoBehaviour, ISaveLoadable, ISecondaryOutput, IEffectDescriptor
    {
        [MyCmpGet]
        private Operational operational;

        [SerializeField]
        public ConduitPortInfo portInfo;

        [MyCmpReq]
        private Building building;

        [MyCmpReq]
        private KSelectable selectable;

        [MyCmpReq]
        public Storage storage;

        //public Tag waterTag = GameTags.Water;
        //public Tag oxygenTag = GameTags.Oxygen;
        public Tag hydrogenTag = GameTags.Hydrogen;

        private Guid needsConduitStatusItemGuid;

        private Guid conduitBlockedStatusItemGuid;

        //private static StatusItem electrolyzerStatusItem = null;

        private HandleVector<int>.Handle partitionerEntry;

        public List<Descriptor> GetDescriptors(BuildingDef def) {
            //List<Descriptor> list = new List<Descriptor>();
            //if (false) { return list; }
            //Tag water = GameTags.Water;
            //Tag oxygen = GameTags.Oxygen;
            //Tag hydrogen = GameTags.Hydrogen;
            //Descriptor item = default(Descriptor);
            //item.SetupDescriptor(string.Format(UI.BUILDINGEFFECTS.ELEMENTCONSUMED, water.Name, GameUtil.GetFormattedMass(water, GameUtil.TimeSlice.PerSecond))
            //return null;
            Descriptor item = default;
            item.SetupDescriptor("Test", "Testy Test");
            return new List<Descriptor>() { item };
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            KBatchedAnimController component = base.GetComponent<KBatchedAnimController>();
            component.randomiseLoopedOffset = true;
            InitializeStatusItems();
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();

            //this.liquidInputCell = this.building.GetUtilityInputCell();
            this.oxygenOutputCell = this.building.GetUtilityOutputCell();
            int cell = Grid.PosToCell(base.transform.GetPosition());
            CellOffset rotatedOffset = this.building.GetRotatedOffset(this.portInfo.offset);
            this.hydrogenOutputCell = Grid.OffsetCell(cell, rotatedOffset);
            IUtilityNetworkMgr networkManager = Conduit.GetNetworkManager(this.portInfo.conduitType);
            this.hydrogenOutputItem = new FlowUtilityNetwork.NetworkItem(this.portInfo.conduitType, Endpoint.Source, this.hydrogenOutputCell, base.gameObject);
            networkManager.AddToNetworks(this.hydrogenOutputCell, this.hydrogenOutputItem, true);
            ConduitFlow flowManager = Conduit.GetFlowManager(this.portInfo.conduitType);
            flowManager.AddConduitUpdater(this.OnConduitTick, ConduitFlowPriority.Default);
            //base.GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, ElementFilter.filterStatusItem, this);
            //base.GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, electrolyzerStatusItem, this);
            UpdateConduitExistsStatus();
            UpdateConduitBlockedStatus();
            //ScenePartitionerLayer scenePartitionerLayer = GameScenePartitioner.Instance.gasConduitsLayer;
            //if (scenePartitionerLayer != null) {
            //    partitionerEntry = GameScenePartitioner.Instance.Add("AdvElectrolyzerConduitExists", base.gameObject, hydrogenOutputCell, scenePartitionerLayer, delegate { UpdateConduitExistsStatus(); });
            //}
            Tutorial.Instance.oxygenGenerators.Add(base.gameObject);
        }

        protected override void OnCleanUp()
        {
            Tutorial.Instance.oxygenGenerators.Remove(base.gameObject);
            IUtilityNetworkMgr networkManager = Conduit.GetNetworkManager(this.portInfo.conduitType);
            networkManager.RemoveFromNetworks(this.hydrogenOutputCell, this.hydrogenOutputItem, true);
            ConduitFlow flowManager = Conduit.GetFlowManager(this.portInfo.conduitType);
            flowManager.RemoveConduitUpdater(this.OnConduitTick);
            if (partitionerEntry != null && partitionerEntry.IsValid() && GameScenePartitioner.Instance != null)
            {
                GameScenePartitioner.Instance.Free(ref partitionerEntry);
            }
            base.OnCleanUp();
        }

        //private int liquidInputCell = -1;

        private int oxygenOutputCell = -1;

        private int hydrogenOutputCell = -1;

        //private FlowUtilityNetwork.NetworkItem oxygenOutputItem;

        private FlowUtilityNetwork.NetworkItem hydrogenOutputItem;

        //private static StatusItem ElectrolyzerWaterInput;
        //private static StatusItem ElectrolyzerOxygenOutput;
        //private static StatusItem ElectrolyzerHydrogenOutput;

        private void OnConduitTick(float dt) {
            bool value = false;
            UpdateConduitExistsStatus();
            UpdateConduitBlockedStatus();
            if (this.operational.IsOperational) {
                PrimaryElement water = storage.FindFirstWithMass(GameTags.AnyWater);
                ConduitFlow gasFlowManager = Conduit.GetFlowManager(this.portInfo.conduitType);
                ConduitFlow.ConduitContents oxygen = gasFlowManager.GetContents(this.oxygenOutputCell);
                ConduitFlow.ConduitContents hydrogen = gasFlowManager.GetContents(this.hydrogenOutputCell);
                if (water != null && water.Mass >= LiquidRatio && hydrogen.mass <= 0f && oxygen.mass <= 0f) {
                    //Debug.Log(String.Format("Current Pipe Contents: Water({0}), Oxygen({1}) Hydrogen({2})", water.Mass, oxygen.mass, hydrogen.mass));
                    value = true;
                    SimHashes oxygenHash = (water.ElementID == SimHashes.Water) ? SimHashes.Oxygen : SimHashes.ContaminatedOxygen;
                    //Debug.Log(String.Format("Output Nums: ({0}, {1})", num2, num3));
                    float oxygenGenerated = gasFlowManager.AddElement(this.oxygenOutputCell, oxygenHash, OxygenRatio, 300f, water.DiseaseIdx, 0);
                    float hydrogenGenerated = gasFlowManager.AddElement(this.hydrogenOutputCell, SimHashes.Hydrogen, HydrogenRatio, 300f, water.DiseaseIdx, 0);
                    if (oxygenGenerated > 0f && hydrogenGenerated > 0f) {
                        water.Mass -= LiquidRatio;
                        ReportManager.Instance.ReportValue(ReportManager.ReportType.OxygenCreated, OxygenRatio, base.gameObject.GetProperName());
                    } else if (oxygenGenerated > 0f || hydrogenGenerated > 0f) {
                        value = false;
                        gasFlowManager.RemoveElement(this.oxygenOutputCell, oxygenGenerated);
                        gasFlowManager.RemoveElement(this.hydrogenOutputCell, hydrogenGenerated);
                    }
                }
            }
            this.operational.SetActive(value, false);
        }

        private void UpdateConduitExistsStatus() {
            bool flag = RequireOutputs.IsConnected(hydrogenOutputCell, portInfo.conduitType);
            bool flag2 = needsConduitStatusItemGuid != Guid.Empty;
            if (flag == flag2) {
                needsConduitStatusItemGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NeedGasOut, needsConduitStatusItemGuid, !flag);
            }
        }

        private void UpdateConduitBlockedStatus()
        {
            ConduitFlow flowManager = Conduit.GetFlowManager(portInfo.conduitType);
            bool flag = flowManager.IsConduitEmpty(hydrogenOutputCell);
            StatusItem conduitBlockedMultiples = Db.Get().BuildingStatusItems.ConduitBlockedMultiples;
            bool flag2 = conduitBlockedStatusItemGuid != Guid.Empty;
            if (flag == flag2)
            {
                conduitBlockedStatusItemGuid = selectable.ToggleStatusItem(conduitBlockedMultiples, conduitBlockedStatusItemGuid, !flag);
            }
        } 
        private void InitializeStatusItems() {/*
            if (ElectrolyzerWaterInput == null) {
                ElectrolyzerWaterInput = new StatusItem("AdvancedElectrolyzer", "BUILDING", string.Empty, StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: true, OverlayModes.None.ID).SetResolveStringCallback(delegate (string str, object data)
                {
                    Electrolyzer electrolyzer = (Electrolyzer)data;
                    //str = str.Replace("{ElementTypes}", GameTags.Water.Name);
                    //str = str.Replace("{FlowRate}", GameUtil.GetFormattedByTag(GameTags.Water.Name, LiquidRatio, GameUtil.TimeSlice.PerSecond));
                    return str + " WATER INPUT";
                });
            }
            if (ElectrolyzerOxygenOutput == null) {
                ElectrolyzerOxygenOutput = new StatusItem("AdvancedElectrolyzer", "BUILDING", string.Empty, StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: true, OverlayModes.None.ID).SetResolveStringCallback(delegate (string str, object data)
                {
                    Electrolyzer electrolyzer = (Electrolyzer)data;
                    //str = str.Replace("{ElementTypes}", GameTags.Oxygen.Name);
                    //str = str.Replace("{FlowRate}", GameUtil.GetFormattedMass(OxygenRatio, GameUtil.TimeSlice.PerSecond));
                    return str + "OXYGEN OUTPUT";
                });
            }
            if (ElectrolyzerHydrogenOutput == null) {
                ElectrolyzerHydrogenOutput = new StatusItem("AdvancedElectrolyzer", "BUILDING", string.Empty, StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: true, OverlayModes.None.ID).SetResolveStringCallback(delegate (string str, object data)
                {
                    Electrolyzer electrolyzer = (Electrolyzer)data;
                    //str = str.Replace("{ElementTypes}", hydrogenTag.Name);
                    //str = str.Replace("{FlowRate}", GameUtil.GetFormattedMass(HydrogenRatio, GameUtil.TimeSlice.PerSecond));
                    return str + "HYDROGEN OUTPUT";
                });
            }
            if (electrolyzerStatusItem == null)
            {
                //electrolyzerStatusItem = new StatusItem("AdvancedElectrolyzer", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.GasConduits.ID);
                //electrolyzerStatusItem.resolveStringCallback = delegate (string str, object data)
                //{
                    //AdvancedElectrolyzer electrolyzer = (AdvancedElectrolyzer)data;
                    /*if (electrolyzer.filterTag == SimHashes.Void)
                    {
                        str = string.Format(BUILDINGS.PREFABS.GASFILTER.STATUS_ITEM, BUILDINGS.PREFABS.GASFILTER.ELEMENT_NOT_SPECIFIED);
                    }
                    else
                    {
                        Element element = ElementLoader.FindElementByHash(electrolyzer.filteredElem);
                        str = string.Format(BUILDINGS.PREFABS.GASFILTER.STATUS_ITEM, element.name);
                    }
                    return str;*/
                    //return "TEST";
                //};
                //electrolyzerStatusItem.conditionalOverlayCallback = ShowInUtilityOverlay;
            //}
        }

        public CellOffset GetSecondaryConduitOffset()
        {
            return this.portInfo.offset;
        }

        public ConduitType GetSecondaryConduitType()
        {
            return this.portInfo.conduitType;
        }

        public float LiquidRatio {
            get {
                return AdvancedElectrolyzerConfig.config.waterConsumptionRate;
            }
        }

        public float OxygenRatio {
            get {
                return AdvancedElectrolyzerConfig.config.water2OxygenRatio;
            }
        }

        public float HydrogenRatio {
            get {
                return AdvancedElectrolyzerConfig.config.water2HydrogenRatio;
            }
        }
    }
}
