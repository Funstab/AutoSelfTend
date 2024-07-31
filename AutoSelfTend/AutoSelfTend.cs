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
            var harmony = new Harmony("com.funstab.autoselftend");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void UpdateAllColonists()
        {
            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
            {
                if (pawn.IsColonist)
                {
                    if (pawn.skills.GetSkill(SkillDefOf.Medicine).Level >= Settings.MinimumSkillLevel)
                    {
                        pawn.playerSettings.selfTend = true;
                    }
                    else
                    {
                        pawn.playerSettings.selfTend = false;
                    }
                }
            }
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
                if (__instance.skills.GetSkill(SkillDefOf.Medicine).Level >= Settings.MinimumSkillLevel)
                {
                    __instance.playerSettings.selfTend = true;
                }
                else
                {
                    __instance.playerSettings.selfTend = false;
                }
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
        }
    }

    public class AutoSelfTendMod : Mod
    {
        private Settings settings;

        public AutoSelfTendMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label($"Minimum skill level to enable self-tend: {Settings.MinimumSkillLevel}");
            int newSkillLevel = (int)listingStandard.Slider(Settings.MinimumSkillLevel, 0, 20);
            if (newSkillLevel != Settings.MinimumSkillLevel)
            {
                Settings.MinimumSkillLevel = newSkillLevel;
                AutoSelfTend.UpdateAllColonists();
            }
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Auto Self-Tend";
        }
    }
}
