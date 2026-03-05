using HarmonyLib;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.PawnTracker;

// Events:
// Mental breaks, reason
// Down event, reason
// Kill event, save & use combat log text
// Ideology convert, believe reduced
// Pawn.Notify_PassedToWorld() event?

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
    public static bool ShouldTrack(Pawn pawn) => ShouldTrack(pawn.def);

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
        GameEventListener.Subscribe<RaidEvent>(e =>
        {
            foreach (var pawn in e.Enemies)
            {
                var comp = CompHistoryManager.GetComp(pawn);

                CompHistoryManager.GetComp(pawn).records.Add(new HistoryRecord(PawnEventDefOf.Raid, pawn));
            }
        });

        GameEventListener.Subscribe<GameEvent>(e =>
        {
            CompHistoryManager.GetComp(e.Pawn).records.Add(new HistoryRecord(e.eventDef, e.Pawn, e.combatLogText));
        });
    }
}
