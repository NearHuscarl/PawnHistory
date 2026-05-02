using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MentalBreakComp_Jailbreaker : MentalBreakComp
{
    public override bool Match(BuildInput input) => input.MentalState is MentalState_Jailbreaker;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var prisoners = GetPrisoners(input);
        return builder.AddRule("Prisoners", LangUtility.FormatList(prisoners));
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        return GetPrisoners(input);
    }

    private static List<Pawn> GetPrisoners(BuildInput input)
    {
        return input.Target.GetRoom().ContainedThings<Pawn>().Where(p => p.IsPrisoner).ToList();
    }

    public Action Test(TestScenario scenario)
    {
        scenario.SpeedUp();
        var prisoners = new List<Pawn>();
        scenario.Map()
            .BuildRoom(6, 6, tag: "Prison")
            .AsPrison(prisonerCount: 2, prisoners: prisoners)
            .Execute();

        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .Position(scenario.OutsideOf("Prison"))
            .Do((p, i) => p.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.Jailbreaker))
            .CreateSingle();

        Expect.That(pawn).Eventually().ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.MentalBreak,
            Description = $"[PAWN] had a mental breakdown and was going to induce [Prisoners] to escape. {MentalBreakRecorder.MoodReasonTemplate}",
            Concerns = prisoners.Cast<Thing>().ToList(),
        });

        return () => scenario.SlowDown();
    }
}
