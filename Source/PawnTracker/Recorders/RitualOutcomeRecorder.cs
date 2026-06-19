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
        var recordDef = HistoryRecordDefOf.RitualOutcome;
        var input = new RitualOutcomeComp.BuildInput(e);
        var comp = Comps.OfType<RitualOutcomeComp>().FirstOrDefault(c => c.Match(input));

        var recordPawns = comp?.GetRecordPawns(input) ?? [e.Host];
        foreach (var pawn in recordPawns)
        {
            if (!ShouldRecord(pawn))
                continue;

            var builder = BuildCommon(pawn, e);
            var concerns = new List<Thing>();

            if (comp != null)
            {
                builder = comp.BuildGrammarRequest(builder, input);
                concerns.AddRange(comp.GetConcerns(input));
            }
            AddRecord(recordDef, pawn, builder.Resolve(), concerns);
        }
    }

    private static HistoryDescriptionBuilder BuildCommon(Pawn pawn, RitualOutcomeCompletedEvent e)
    {
        var spectatorsAndHost = e.Spectators.ToList();
        if (e.Host != null)
            spectatorsAndHost.Add(e.Host);

        return HistoryRecordDefOf.RitualOutcome.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Ritual", e.RitualLabel)
            .AddRule("Outcome", e.OutcomeLabel?.ToLowerInvariant(), addSubsymbols: true)
            .WithOthers(spectatorsAndHost)
            .AddConstant("ritual", e.RitualDef.defName);
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

    [RequiresIdeology]
    public void TestLeaderSpeech(TestScenario scenario)
    {
        scenario.SpeedUp();

        var leaderIdeo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.LeaderSpeech).Execute();
        var organizer = scenario.Pawn()
            .FullHeal()
            .Colonist()
            .SetIdeo(leaderIdeo, role: PreceptDefOf.IdeoRole_Leader)
            .CreateSingle(false);
        var spectators = scenario.Pawn(4)
            .Colonist()
            .SetIdeo(leaderIdeo)
            .Execute();

        scenario.Map()
            .BuildRoom(10, 10, floorDef: TerrainDefOf.MetalTile)
            .AsShrine(leaderIdeo)
            .Execute();

        scenario
            .Ritual(organizer)
            .Outcome(Extra.RitualOutcomeEffectDefOf.AttendedSpeech.BestOutcome)
            .LeaderSpeech(spectators)
            .Execute();

        Expect.That(organizer).ToHaveHistoryRecord(HistoryRecordDefOf.RitualOutcome, "[PAWN] delivered an inspirational leader speech to 4 others.");
    }
}
