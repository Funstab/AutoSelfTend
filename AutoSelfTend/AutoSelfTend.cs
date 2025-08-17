// AutoSelfTend.cs
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace AutoSelfTend
{

    public class Settings : ModSettings
    {
        public static int MinimumSkillLevel = 6;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref MinimumSkillLevel, "MinimumSkillLevel", 6);
        }
    }


    public class AutoSelfTendMod : Mod
    {
        public AutoSelfTendMod(ModContentPack content) : base(content)
        {
            GetSettings<Settings>();
        }

        public override string SettingsCategory() => "Auto Self-Tend";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label($"Minimum Medical skill for self-tend: {Settings.MinimumSkillLevel}");

            // Slider (signature compatible with Verse)
            var r = listing.GetRect(28f);
            Settings.MinimumSkillLevel = Mathf.RoundToInt(
                Widgets.HorizontalSlider(
                    r,
                    Settings.MinimumSkillLevel,
                    0f, 20f,
                    false,
                    Settings.MinimumSkillLevel.ToString(),
                    "0",
                    "20",
                    1f
                )
            );

            listing.GapLine();
            listing.Label("Colonists with Medical ≥ threshold will have Self-tend enabled; others disabled.");
            listing.End();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            AutoSelfTendGameComp.ApplyAllNow();
        }
    }

 
    public class AutoSelfTendGameComp : GameComponent
    {
        private const int ApplyIntervalTicks = 1200;
        private static readonly List<Pawn> _buffer = new List<Pawn>(64);

        public AutoSelfTendGameComp() { }
        public AutoSelfTendGameComp(Game _) { }

        public override void FinalizeInit()
        {
            ApplyAllNow();
        }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % ApplyIntervalTicks == 0)
                ApplyAllNow();
        }

        public static void ApplyAllNow()
        {
            if (Current.Game == null) return;

            _buffer.Clear();
            CollectPlayerColonists(_buffer);

            for (int i = 0; i < _buffer.Count; i++)
                ApplyToPawn(_buffer[i]);
        }

        private static void CollectPlayerColonists(List<Pawn> outPawns)
        {

            var maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                var col = map?.mapPawns?.FreeColonists;
                if (col == null) continue;
                for (int j = 0; j < col.Count; j++)
                    if (col[j] != null) outPawns.Add(col[j]);
            }

            var caravans = Find.WorldObjects?.Caravans;
            if (caravans != null)
            {
                for (int i = 0; i < caravans.Count; i++)
                {
                    var caravan = caravans[i];
                    if (caravan?.Faction != Faction.OfPlayer) continue;

                    var pawns = caravan.PawnsListForReading;
                    for (int j = 0; j < pawns.Count; j++)
                    {
                        var p = pawns[j];
                        if (p != null && p.IsColonist) outPawns.Add(p);
                    }
                }
            }
        }

        private static void ApplyToPawn(Pawn p)
        {
            if (p == null) return;
            if (p.playerSettings == null) return;
            if (p.skills == null) return;

            var med = p.skills.GetSkill(SkillDefOf.Medicine);
            int level = med?.Level ?? 0;

            bool shouldEnable = level >= Settings.MinimumSkillLevel;
            if (p.playerSettings.selfTend != shouldEnable)
                p.playerSettings.selfTend = shouldEnable;
        }
    }
}
