using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RitualOutcomeComp_Funeral : RitualOutcomeComp
{
    public override bool Match(BuildInput input)
    {
        return input.Event.RitualDef == PreceptDefOf.Funeral || input.Event.RitualDef == PreceptDefOf.FuneralNoCorpse;
    }

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder.AddRule("DeadPawn", GetDeadPawn(input), addSubsymbols: true);
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield return GetDeadPawn(input);
    }

    private static Pawn GetDeadPawn(BuildInput input)
    {
        if (input.Event.TargetA.Thing is Pawn pawn)
            return pawn;
        return null;
    }

    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();

        var funeralIdeo = scenario.Ideo().AddPrecept(PreceptDefOf.Funeral).Execute();
        var organizer = scenario.Pawn()
            .Colonist()
            .SetIdeo(funeralIdeo, role: PreceptDefOf.IdeoRole_Moralist)
            .CreateSingle();
        var deceased = scenario.Pawn()
            .Colonist()
            .SetIdeo(funeralIdeo) // RitualObligationTrigger_MemberDiedProperties
            .CreateSingle(false);
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(funeralIdeo)
            .Execute();

        scenario.Map()
            .BuildRoom(7, 7)
            .WithCasket(ThingDefOf.Grave, pawn: deceased)
            .Execute();

        scenario
            .Ritual(organizer)
            .Outcome(Extra.RitualOutcomeEffectDefOf.AttendedFuneral.BestOutcome)
            .Funeral(deceased, spectators)
            .Execute();

        Expect.That(organizer).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = "[Host] held a heartwarming funeral for [DeadPawn] in front of 2 others.",
            Concerns = [deceased],
        });
    }

    [RequiresIdeology]
    public void TestNoCorpse(TestScenario scenario)
    {
        scenario.SpeedUp();

        var funeralIdeo = scenario.Ideo()
            .AddPrecept(PreceptDefOf.Funeral)
            .AddPrecept(PreceptDefOf.FuneralNoCorpse)
            .Execute();
        var organizer = scenario.Pawn()
            .Colonist()
            .SetIdeo(funeralIdeo, role: PreceptDefOf.IdeoRole_Moralist)
            .CreateSingle();
        var deceased = scenario.Pawn()
            .Colonist()
            .SetIdeo(funeralIdeo)
            .CreateSingle(false);
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(funeralIdeo)
            .Execute();

        scenario.Map()
            .BuildRoom(7, 7)
            .WithCasket(ThingDefOf.Grave, occupied: false)
            .Execute();

        deceased.Kill(null);
        deceased.Corpse.Destroy();

        scenario
            .Ritual(organizer)
            .Outcome(Extra.RitualOutcomeEffectDefOf.AttendedFuneralNoCorpse.WorstOutcome)
            .Funeral(deceased, spectators, true)
            .Execute();

        Expect.That(organizer).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = "[Host] held a terrible funeral for [DeadPawn] in front of 2 others.",
            Concerns = [deceased],
        });
    }
}
