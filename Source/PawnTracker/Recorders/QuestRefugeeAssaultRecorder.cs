using System.Linq;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class QuestRefugeeAssaultRecorder : RecorderBase<QuestRefugeeAssaultEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<QuestRefugeeAssaultEvent>(CreateRecord);
    }

    public override void CreateRecord(QuestRefugeeAssaultEvent input)
    {
        var (refugees, quest, reason, victim) = input;
        var recordDef = HistoryRecordDefOf.QuestRefugeeAssault;

        foreach (var refugee in refugees)
        {
            if (!ShouldRecord(refugee))
                continue;

            var desc = recordDef.Description(refugee)
                .WithPlayerFaction()
                .AddRule("OtherCount", refugees.Count - 1)
                .AddRule("Victim", victim)
                .AddConstant("reason", reason)
                .AddConstant("otherCount", refugees.Count - 1)
                .Resolve();

            AddRecord(recordDef, refugee, desc, [..refugees, victim], quest: quest);
        }
    }

    [RequiresRoyalty]
    public void TestBetrayal(TestScenario scenario)
    {
        scenario.RefugeeAlwaysAssaultOnViolation = true;
        var quest = scenario.Quest(QuestScriptDefOf.Hospitality_Refugee).Execute();
        var refugeeInteractions = quest.GetFirstPartOfType<QuestPart_RefugeeInteractions>();
        var refugees = refugeeInteractions.pawns.ToList();
        refugeeInteractions.Notify_QuestSignalReceived(new Signal(refugeeInteractions.inSignalAssaultColony));

        foreach (var refugee in refugees)
        {
            Expect.That(refugee).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.QuestRefugeeAssault,
                Description = "[PAWN][AndOthers] betrayed the colony after being given shelter.",
                Concerns = [..refugees.Where(p => p != refugee)],
                Quest = quest,
            });
        }
    }

    [RequiresRoyalty]
    public void TestArrested(TestScenario scenario)
    {
        scenario.RefugeeAlwaysAssaultOnViolation = true;
        scenario.Pawn(8).Colonist().Execute(); // refugee count based on the colony population. must be >1 for the test to pass

        var quest = scenario.Quest(QuestScriptDefOf.Hospitality_Refugee).Execute();
        var refugeeInteractions = quest.GetFirstPartOfType<QuestPart_RefugeeInteractions>();
        var victim = refugeeInteractions.pawns.Last();
        var refugees = refugeeInteractions.pawns.Except(victim).ToList();
        refugeeInteractions.Notify_QuestSignalReceived(new Signal(refugeeInteractions.inSignalArrested, victim.Named(SignalArgsNames.Subject)));

        foreach (var refugee in refugees)
        {
            Expect.That(refugee).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.QuestRefugeeAssault,
                Description = "[PAWN][AndOthers] turned violently against the colony after [Victim] was arrested.",
                Concerns = [..refugees.Where(p => p != refugee), victim],
                Quest = quest,
            });
        }
    }
}
