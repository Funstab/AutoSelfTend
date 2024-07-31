using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AutoSelfTend
{
    [StaticConstructorOnStartup]
    public static class AutoSelfTend
    {
        static AutoSelfTend()
        {
            Log.Message("[AutoSelfTend] Initializing Harmony patches.");
            var harmony = new Harmony("com.funstab.autoselftend");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("SpawnSetup")]
    public static class Pawn_SpawnSetup_Patch
    {
        [HarmonyPostfix]
        public static void AutoEnableSelfTend(Pawn __instance, bool respawningAfterLoad)
        {
            if (__instance.IsColonist)
            {
                Log.Message($"[AutoSelfTend] {__instance.Name} is a colonist.");
                if (__instance.skills.GetSkill(SkillDefOf.Medicine).Level >= Settings.MinimumSkillLevel)
                {
                    __instance.playerSettings.selfTend = true;
                    Log.Message($"[AutoSelfTend] Enabled self-tend for {__instance.Name} (Medicine skill: {__instance.skills.GetSkill(SkillDefOf.Medicine).Level}).");
                }
                else
                {
                    Log.Message($"[AutoSelfTend] Did not enable self-tend for {__instance.Name} (Medicine skill: {__instance.skills.GetSkill(SkillDefOf.Medicine).Level} is below threshold).");
                }
            }
            else
            {
                Log.Message($"[AutoSelfTend] {__instance.Name} is not a colonist.");
            }
        }
    }

    public class Settings : ModSettings
    {
        public static int MinimumSkillLevel = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MinimumSkillLevel, "MinimumSkillLevel", 0);
            Log.Message($"[AutoSelfTend] Loaded settings. MinimumSkillLevel: {MinimumSkillLevel}");
        }
    }

    public class AutoSelfTendMod : Mod
    {
        private Settings settings;

        public AutoSelfTendMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<Settings>();
            Log.Message("[AutoSelfTend] Mod constructor called.");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label($"Minimum skill level to enable self-tend: {Settings.MinimumSkillLevel}");
            Settings.MinimumSkillLevel = (int)listingStandard.Slider(Settings.MinimumSkillLevel, 0, 20);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
            Log.Message($"[AutoSelfTend] Settings window updated. MinimumSkillLevel: {Settings.MinimumSkillLevel}");
        }

        public override string SettingsCategory()
        {
            return "Auto Self-Tend";
        }
    }
}
