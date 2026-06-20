using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RareCatchRecorder : RecorderBase<RareCatchEvent>
{
    public override void Register()
    {
        if (!ModsConfig.OdysseyActive)
            return;

        GameEventBus.Subscribe<RareCatchEvent>(CreateRecord);
    }

    public override void CreateRecord(RareCatchEvent input)
    {
        var (pawn, catches) = input;

        if (!ShouldRecord(pawn))
            return;

        var recordDef = HistoryRecordDefOf.RareCatch;
        var desc = recordDef.Description(pawn)
            .AddRule("Catches", LangUtility.FormatList(catches, thing => thing.Label.Colorize(ColoredText.SubtleGrayColor)))
            .Resolve();

        AddRecord(recordDef, pawn, desc, catches);
    }

    [RequiresOdyssey]
    public void Test(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var rareCatch = scenario.Thing(ThingDefOf.Silver).Stack(50).CreateSingle();
        scenario.ForcedRareCatch = rareCatch;

        FishingUtility.GetCatchesFor(pawn, pawn.Position, false, out _);

        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RareCatch,
            Description = "While fishing, [PAWN] made an unusual catch: silver x50.",
            Concerns = [rareCatch],
        });
    }
}
