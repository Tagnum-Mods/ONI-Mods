using Database;
using Harmony;
using System.Collections.Generic;

namespace AdvanceElectrolyzer
{
    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    internal class AdvancedElectrolyzerMod
    {
        public static void Prefix()
        {
            Debug.Log(" === GeneratedBuildings Prefix === " + AdvancedElectrolyzerConfig.ID);

            string prefix = "STRINGS.BUILDINGS.PREFABS." + AdvancedElectrolyzerConfig.ID.ToUpper();
            Strings.Add(prefix + ".NAME", "Advanced Electrolyzer");
            Strings.Add(prefix + ".DESC", "Water goes in one end. life sustaining oxygen comes out the other");
            Strings.Add(prefix + ".EFFECT", string.Format("Converts {0} to {1} and {2}. Also converts {3} to {4} and {2}.c5",
                STRINGS.UI.FormatAsLink("Water", "WATER"),
                STRINGS.UI.FormatAsLink("Oxygen", "OXYGEN"),
                STRINGS.UI.FormatAsLink("Hydrogen", "HYDROGEN"),
                STRINGS.UI.FormatAsLink("Polluted Water", "DIRTYWATER"),
                STRINGS.UI.FormatAsLink("Polluted Oxygen", "CONTAMINATEDOXYGEN")));
            ModUtil.AddBuildingToPlanScreen("Oxygen", AdvancedElectrolyzerConfig.ID);
            UnityEngine.TextAsset anim_file = new UnityEngine.TextAsset();
            UnityEngine.TextAsset build_file = new UnityEngine.TextAsset();
            UnityEngine.Texture2D texture = new UnityEngine.Texture2D(32, 32);
            ModUtil.AddKAnim("advanced_electrolyzer_kanim", anim_file, build_file, texture);

            string status_prefix = "STRINGS.BUILDINGS.STATUSITEMS.{0}.{1}";
            Strings.Add(string.Format(status_prefix, "ADVANCEDELECTROLYZER", "NAME"), "Advanced Electrolyzer");
            Strings.Add(string.Format(status_prefix, "ADVANCEDELECTROLYZER", "TOOLTOP"), "TEST");
        }
    }
    [HarmonyPatch(typeof(Db), "Initialize")]
    public static class InitAdvacnedElectrolyzerMod
    {
        public static void Prefix(Db __instance)
        {
            Debug.Log(" === Database.Techs loaded === " + AdvancedElectrolyzerConfig.ID);
            List<string> list = new List<string>(Techs.TECH_GROUPING["ImprovedOxygen"]);
            list.Add(AdvancedElectrolyzerConfig.ID);
            Techs.TECH_GROUPING["ImprovedOxygen"] = list.ToArray();
        }
    }
}
