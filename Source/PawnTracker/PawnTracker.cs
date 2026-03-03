using HarmonyLib;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.PawnTracker
{
    [StaticConstructorOnStartup]
    internal class PawnTracker
    {
        static PawnTracker()
        {
            new Harmony("rimworld.mod.nearhuscarl.pawnhistory").PatchAllUncategorized(Assembly.GetExecutingAssembly());

            AddCompToHumanlikes();
            SetupEvenListeners();
        }

        public static void AddCompToHumanlikes()
        {
            var defsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;

            for (var i = 0; i < defsListForReading.Count; ++i)
            {
                var thingDef = defsListForReading[i];
                var race = thingDef.race;
                if (race != null && race.intelligence == Intelligence.Humanlike && !thingDef.IsCorpse)
                {
                    thingDef.comps.Add(new CompProperties_History());
                    CompHistoryManager.TrackingDefHash.Add(thingDef.shortHash);
                    var type = typeof(ITab_Pawn_History);
                    var sharedInstance = InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_History));

                    thingDef.inspectorTabs?.AddDistinct(type);
                    thingDef.inspectorTabsResolved?.AddDistinct(sharedInstance);

                    if (thingDef.race?.corpseDef == null)
                    {
                        Log.Warning("[ModName] thingDef.race?.corpseDef == null for thingDef = " + thingDef.defName);
                    }
                    else
                    {
                        thingDef.race.corpseDef.inspectorTabs?.AddDistinct(type);
                        thingDef.race.corpseDef.inspectorTabsResolved?.AddDistinct(sharedInstance);
                    }
                }
            }
        }

        private static void SetupEvenListeners()
        {
            GameEventListener.Subscribe<RaidEvent>(e =>
            {
                foreach (var pawn in e.Enemies)
                {
                    CompHistoryManager.GetComp(pawn).records.Add(new HistoryRecord(PawnEventDefOf.Raid));
                }
            });
        }
    }
}
