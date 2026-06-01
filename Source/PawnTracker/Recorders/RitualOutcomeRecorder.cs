using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

// TODO: more testing
public class RitualOutcomeRecorder : RecorderBase<RitualOutcomeCompletedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<RitualOutcomeCompletedEvent>(CreateRecord);
    }

    public override void CreateRecord(RitualOutcomeCompletedEvent e)
    {
        if (!ShouldRecord(e.Host))
            return;

        var recordDef = HistoryRecordDefOf.RitualOutcome;
        var spectatorsAndHost = e.Spectators.ToList();
        if (e.Host != null)
            spectatorsAndHost.Add(e.Host);
        
        var builder = recordDef
            .Description(e.Host, "Host")
            .IncludePawnGrammar()
            .AddRule("Ritual", e.RitualLabel)
            .AddRule("Outcome", e.OutcomeLabel.ToLowerInvariant(), addSubsymbols: true)
            .WithOthers(spectatorsAndHost)
            .AddConstant("ritual", e.RitualDef.defName);
        var concerns = new List<Thing>();
        var input = new RitualOutcomeComp.BuildInput(e);

        foreach (var comp in Comps.OfType<RitualOutcomeComp>())
        {
            if (!comp.Match(input))
                continue;

            builder = comp.BuildGrammarRequest(builder, input);
            concerns.AddRange(comp.GetConcerns(input));
        }

        AddRecord(recordDef, e.Host, builder.Resolve(), concerns);
    }

    [RequiresRoyalty]
    public void TestSpeech(TestScenario scenario)
    {
        scenario.SpeedUp();
        
        var organizer = scenario.Pawn().FullHeal().Colonist().SetRoyalTitle(Extra.RoyalTitleDefOf.Praetor).CreateSingle();
        var spectators = scenario.Pawn(4).Colonist().Execute();

        scenario.Map()
            .BuildRoom(10, 10, floorDef: TerrainDefOf.MetalTile)
            .AsThroneRoom(organizer)
            .Execute();
        
        scenario
            .Ritual(organizer)
            .Outcome(Extra.RitualOutcomeEffectDefOf.AttendedSpeech.BestOutcome)
            .ThroneSpeech(spectators)
            .Execute();

        Expect.That(organizer).ToHaveHistoryRecord(HistoryRecordDefOf.RitualOutcome, "[PAWN] delivered an inspirational throne speech to 4 others.");
    }
}
