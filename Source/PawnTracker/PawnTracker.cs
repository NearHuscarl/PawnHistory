using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Verse;

namespace PawnHistory.Source.PawnTracker;

// Features:
// - Display history record
//   - Colorize names and important information
//   - Tooltip
//   - Click to jump to related pawns
//   - Some icons

// Icon References (AssetRipper)
// - Assets\Resources\textures\things\mote\thoughtsymbol
// - Assets\Resources\textures\things\mote\speechsymbols
// - Assets\Resources\textures\things\mote\battlesymbols
// - Assets\Resources\textures\ui

// Events:
// Mental breaks, reason
// Ideology convert, belief reduced
// Pawn.Notify_PassedToWorld() event?
// More record icons?

// Create a filter in WorldPawn window (All/Alive/Dead)
// - Add a column to see the history of dead AND destroyed pawn. Concerned dead&destoryed pawn will open a dedicated history window.

// TODO: handle another related pawn in BattleLogEntry_RangedImpact

[StaticConstructorOnStartup]
internal class PawnTracker
{
    static PawnTracker()
    {
        new Harmony("rimworld.mod.nearhuscarl.pawnhistory").PatchAllUncategorized(Assembly.GetExecutingAssembly());

        AddCompToHumanlikes();
        SetupEvenListeners();
    }

    public static bool ShouldTrack(ThingDef thingDef) => thingDef.race?.intelligence == Intelligence.Humanlike;
    public static bool ShouldTrack(Pawn pawn) => pawn != null && ShouldTrack(pawn.def);

    private static void AddCompToHumanlikes()
    {
        var defsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;

        for (var i = 0; i < defsListForReading.Count; ++i)
        {
            var thingDef = defsListForReading[i];
            if (ShouldTrack(thingDef) && !thingDef.IsCorpse)
            {
                thingDef.comps.Add(new CompProperties_History());
                CompHistoryManager.TrackingDefHash.Add(thingDef.shortHash);
                var type = typeof(ITab_Pawn_History);
                var sharedInstance = InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_History));

                thingDef.inspectorTabs?.AddDistinct(type);
                thingDef.inspectorTabsResolved?.AddDistinct(sharedInstance);

                if (thingDef.race?.corpseDef != null)
                {
                    thingDef.race.corpseDef.inspectorTabs?.AddDistinct(type);
                    thingDef.race.corpseDef.inspectorTabsResolved?.AddDistinct(sharedInstance);
                }
                else Log.Warning("[ModName] thingDef.race?.corpseDef == null for thingDef = " + thingDef.defName);
            }
        }
    }

    private static void SetupEvenListeners()
    {
        GameEventListener.Subscribe<GroupEvent>(e =>
        {
            foreach (var pawn in e.Pawns)
            {
                var comp = CompHistoryManager.GetComp(pawn);
                CompHistoryManager.GetComp(pawn).records.Add(new HistoryRecord(e.eventDef, pawn, e.resolveDesc(pawn)));
            }
        });

        GameEventListener.Subscribe<GameEvent>(e =>
        {
            CompHistoryManager.GetComp(e.Pawn).records.Add(new HistoryRecord(e.eventDef, e.Pawn, e.resolvedDesc, e.relatedPawns));
        });
    }
}
