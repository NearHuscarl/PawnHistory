using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MentalBreakComp_IdeoChange : MentalBreakComp
{
    public override bool Match(BuildInput input) => input.MentalState is MentalState_IdeoChange;

    // TODO: add concerned ideos in HistoryRecord(?)
    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var mentalState = (MentalState_IdeoChange)input.MentalState;
        var oldIdeo = Accessor.MentalState_IdeoChange.OldIdeo(mentalState);
        var newIdeo = Accessor.MentalState_IdeoChange.NewIdeo(mentalState);
        var converted = Accessor.MentalState_IdeoChange.ChangedIdeo(mentalState);
        var newCertainty = Accessor.MentalState_IdeoChange.NewCertainty(mentalState);

        return builder
            .AddRule("OldIdeo", oldIdeo)
            .AddRule("NewIdeo", newIdeo)
            .AddRule("NewCertainty", newCertainty.ToStringPercent())
            .AddConstant("converted", converted)
            .AddConstant("wanderState", input.Pawn.MentalStateDef?.defName);
    }

    [RequiresIdeology]
    public void TestOwnRoom(TestScenario scenario)
    {
        var pawn = CreatePawn(scenario, ownsRoom: true, 1f);
        pawn.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.IdeoChange);

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.MentalBreak, "[PAWN] hid in [His] room to come to terms with [His] beliefs.");
    }

    [RequiresIdeology]
    public void TestSadWander(TestScenario scenario)
    {
        var pawn = CreatePawn(scenario, ownsRoom: false, 1f);
        pawn.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.IdeoChange);

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.MentalBreak, "[PAWN] wandered in shock while grappling with [His] beliefs.");
    }

    [RequiresIdeology]
    public void TestConverted(TestScenario scenario)
    {
        var pawn = CreatePawn(scenario, ownsRoom: false, 0.1f);
        pawn.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.IdeoChange);

        Expect.That(pawn).ToHaveHistoryRecord(
            HistoryRecordDefOf.MentalBreak,
            "[PAWN] decided that [His] belief in [OldIdeo] no longer made any sense. [He] now believed in [NewIdeo]. [PAWN] wandered in shock while grappling with [His] beliefs. [Reason].");
    }

    [RequiresIdeology]
    public void TestReducedCertainty(TestScenario scenario)
    {
        var pawn = CreatePawn(scenario, ownsRoom: true, 1f);
        pawn.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.IdeoChange);

        Expect.That(pawn).ToHaveHistoryRecord(
            HistoryRecordDefOf.MentalBreak,
            "[PAWN] had a crisis of belief, and [His] certainty in [OldIdeo] fell to [NewCertainty]. [PAWN] hid in [His] room to come to terms with [His] beliefs. [Reason].");
    }

    private static Pawn CreatePawn(TestScenario scenario, bool ownsRoom, float ideoCertainty)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .SetIdeo(certainty: ideoCertainty)
            .CreateSingle();

        if (ownsRoom)
        {
            scenario.Map().BuildRoom(7, 7, "Bedroom").AsBarrack(bedCount: 1).Execute();
            pawn.ownership.ClaimBedIfNonMedical(RestUtility.FindBedFor(pawn));
        }

        return pawn;
    }
}
