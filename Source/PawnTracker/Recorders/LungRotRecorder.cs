using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class LungRotRecorder : RecorderBase<LungRotRecorder.Input>
{
    public record Input(Pawn Pawn, Hediff Hediff);
    
    public override void Register()
    {
        GameEventBus.Subscribe<HediffAddEvent>(e =>
        {
            if (e.Hediff.def != HediffDefOf.LungRot)
                return;
            
            // event fires twice for each lung affected.
            if (e.Pawn.health.hediffSet.HasHediff(HediffDefOf.LungRot))
                return;
            
            CreateRecord(new  Input(e.Pawn, e.Hediff));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (pawn, hediff) = input;

        if (!ShouldRecord(pawn))
            return;

        var recordDef = HistoryRecordDefOf.LungRot;
        var desc = recordDef.Description(pawn)
            .AddRule("Disease", hediff)
            .Resolve();
        var nearbyCorpse = GetNearbyRottenCorpses(pawn).FirstOrDefault();

        AddRecord(recordDef, pawn, desc, [nearbyCorpse]);
    }
    
    private static IEnumerable<Corpse> GetNearbyRottenCorpses(Pawn pawn)
    {
        var map = pawn.Map;

        foreach (var cell in GenRadial.RadialCellsAround(pawn.Position, 4, true))
        {
            if (!cell.InBounds(map))
                continue;

            var things = cell.GetThingList(map);
            foreach (var t in things)
            {
                if (t is not Corpse corpse)
                    continue;

                var rot = corpse.GetComp<CompRottable>();
                if (rot is { Stage: RotStage.Rotting })
                    yield return corpse;
            }
        }
    }

    public void Test(TestScenario scenario)
    {
        var hediff = (Hediff)null;
        var pawn = scenario.Pawn()
            .Colonist()
            .AddHediff(HediffDefOf.LungRotExposure, BodyPartDefOf.Lung, h => hediff = h)
            .CreateSingle();
        var corpse = scenario.Pawn()
            .WithPosition(pawn.Position, 1)
            .Enemy()
            .Corpse(true)
            .CreateSingle()
            .Corpse;

        pawn.HistoryRecords.Clear();
        hediff.Severity = 1f;

        for (var i = 0; i < 500; i++)
        {
            hediff.PostTickInterval(int.MaxValue);
            if (pawn.health.hediffSet.HasHediff(HediffDefOf.LungRot))
                break;
        }

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] got sick from lung rot due to long-term exposure to rot stink gas, which is given off by rotting corpses.", HistoryRecordDefOf.LungRot);
        Expect.That(pawn).ToHaveHistoryRecordConcern(corpse, HistoryRecordDefOf.LungRot);
        Expect.That(pawn).ToHaveHistoryRecordCount(1);
    }
}
