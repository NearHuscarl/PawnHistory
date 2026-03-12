using System;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public static class RecorderManager
{
    public static void Initialize()
    {
        foreach (var type in GenTypes.AllSubclassesNonAbstract(typeof(RecorderBase)))
        {
            var recorder = (RecorderBase)Activator.CreateInstance(type);
            recorder.Register();
        }
    }
    public static bool ShouldRecord(ThingDef thingDef) => thingDef.race?.intelligence == Intelligence.Humanlike;
    public static bool ShouldRecord(Pawn pawn) => pawn != null && pawn.RaceProps.Humanlike;
}
