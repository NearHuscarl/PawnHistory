using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using PawnHistory.Source.Helper;
using Verse;
using System.Linq;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class VisitorLeftGiftRecorder : RecorderBase<VisitorLeftGiftEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<VisitorLeftGiftEvent>(CreateRecord);
    }

    public override void CreateRecord(VisitorLeftGiftEvent input)
    {
        var (giver, faction, gifts) = input;
        
        if (!ShouldRecord(giver))
            return;

        var recordDef = HistoryRecordDefOf.VisitorLeftGift;
        var desc = recordDef.Description(giver)
            .AddRule("Faction", faction)
            .AddRule("Gifts", LangUtility.FormatList(gifts, t => t.Label.Colorize(ColoredText.SubtleGrayColor)))
            .Resolve();

        AddRecord(recordDef, giver, desc, gifts);
    }

    public void Test(TestScenario scenario)
    {
        var visitors = scenario.Incident(IncidentDefOf.VisitorGroup).Point(200).Execute();
        var faction = visitors[0].Faction;
        var giver = Accessor.VisitorGiftForPlayerUtility.GetGiftGiver(visitors, faction);
        var gift = scenario.Thing(ThingDefOf.Silver).Stack(50).CreateAndPutInto(giver).FirstOrDefault();
        
        GameUtility.ClearUpMap(); // leave some space so the visitor can drop the loot
        VisitorGiftForPlayerUtility.GiveGift(visitors, faction, [gift]);

        Expect.That(giver).ToHaveHistoryRecord("[PAWN] from [Faction] left a gift for the colony: silver x50.", HistoryRecordDefOf.VisitorLeftGift);
        Expect.That(giver).ToHaveHistoryRecordConcern(gift, HistoryRecordDefOf.VisitorLeftGift);
    }
}
