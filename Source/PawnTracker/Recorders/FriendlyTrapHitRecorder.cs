using PawnHistory.Source.DebugTools;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class FriendlyTrapHitRecorder : RecorderBase<FriendlyTrapHitEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<FriendlyTrapHitEvent>(CreateRecord);
    }

    public override void CreateRecord(FriendlyTrapHitEvent e)
    {
        // This is just an intermediate record to store the crushed position so the Death record can reference it later.
        var recordDef = HistoryRecordDefOf.FriendlyTrapHit;
        var pawn = e.Pawn;
        var desc = recordDef.Description(pawn).Format();
        var location = RecordLocation.Of(pawn.SpawnedParentOrMe);

        AddRecord(recordDef, pawn, desc, location: location);
    }

    public Action Test(TestScenario scenario)
    {
        NearDebugSettings.ForceSpringTrap = true;
        var pawn = scenario.Pawn()
            .Colonist()
            .DiesOnNextHit()
            .CreateSingle();
        var spouse = scenario.Pawn().SetRelation(pawn, PawnRelationDefOf.Spouse).CreateSingle();
        
        scenario.Thing(ThingDefOf.TrapSpike, ThingDefOf.Plasteel).At(pawn.Position).PlaceMode(ThingPlaceMode.Direct).Create();

        Expect.That(pawn).ToHaveHistoryRecordOf(HistoryRecordDefOf.FriendlyTrapHit, -2);
        Expect.That(pawn).ToHaveHistoryRecordOf(HistoryRecordDefOf.Death, -1);
        Expect.That(pawn).ToHaveHistoryRecordPosition(pawn.Position, HistoryRecordDefOf.Death);

        Expect.That(spouse).ToHaveHistoryRecordPosition(pawn.Position, HistoryRecordDefOf.RelativeDeath);
        
        return () => NearDebugSettings.ForceSpringTrap = false;
    }
}