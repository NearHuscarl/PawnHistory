using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PsychicBondedRecorder : RecorderBase<PsychicBondedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PsychicBondedEvent>(CreateRecord);
    }

    public override void CreateRecord(PsychicBondedEvent e)
    {
        CreateRecord(e.Initiator, e.Recipient);
        CreateRecord(e.Recipient, e.Initiator);
    }

    private void CreateRecord(Pawn pawn, Pawn bondedPawn)
    {
        if (!ShouldRecord(pawn))
            return;

        var recordDef = HistoryRecordDefOf.PsychicBonded;
        var desc = recordDef.Description(pawn)
            .AddRule("BondedPawn", bondedPawn)
            .Resolve();

        AddRecord(recordDef, pawn, desc, [bondedPawn]);
    }

    // TODO: add background reason: romance attempt/making love
    [RequiresBiotech]
    public void Test(TestScenario scenario)
    {
        var initiator = scenario.Pawn()
            .Colonist()
            .Do(p => p.genes.AddGene(Extra.GeneDefOf.PsychicBonding, xenogene: false))
            .CreateSingle();
        var recipient = scenario.Pawn().Colonist().CreateSingle();

        InteractionWorker_RomanceAttempt.TryCreatePsychicBondBetween(initiator, recipient);

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.PsychicBonded,
            Description = "[PAWN] and [BondedPawn] formed a psychic bond.",
        };

        Expect.That(initiator).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [recipient] }));
        Expect.That(recipient).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [initiator] }));
    }
}
