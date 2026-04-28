using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PlayedGameRecorder : HistoryTaleRecorder<PlayedGameRecorder.Input>
{
    public record Input(Pawn Pawn, ThingDef ObjectDef);

    protected override float DaysToRecordAgain => 12f;

    public override void Register()
    {
        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale != Extra.TaleDefOf.PlayedGame)
                return;
            if (e.Params[0] is not ThingDef objectDef)
                return;

            CreateRecord(new Input(e.Pawn, objectDef));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (pawn, objectDef) = input;
        var recordDef = HistoryRecordDefOf.PlayedGame;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Object", objectDef, addSubsymbols: true)
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc);
    }

    public void Test(TestScenario scenario)
    {
        var joyThings = new List<ThingDef>
        {
            Extra.ThingDefOf.HorseshoesPin,
            Extra.ThingDefOf.HoopstoneRing,
            Extra.ThingDefOf.GameOfUrBoard,
            Extra.ThingDefOf.ChessTable,
            Extra.ThingDefOf.PokerTable,
            ThingDefOf.BilliardsTable,
        };
        var pawns = scenario.Pawn(joyThings.Count).Colonist()
            .Do((p, i) => TaleRecorder.RecordTale(Extra.TaleDefOf.PlayedGame, p, joyThings[i]))
            .Execute();

        Expect.ThatAll(pawns).ToHaveHistoryRecordOf(HistoryRecordDefOf.PlayedGame);
    }
}
