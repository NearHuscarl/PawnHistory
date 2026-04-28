using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using System.Linq;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class BirthdayRecorder : RecorderBase<BirthdayEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<BirthdayEvent>(CreateRecord);
    }

    public override void CreateRecord(BirthdayEvent e)
    {
        var (pawn, agingHediffs) = e;

        if (!ShouldRecord(pawn))
            return;

        var recordDef = HistoryRecordDefOf.Birthday;
        var agingHediffSet = agingHediffs.ToHashSet();
        var hediffs = pawn.health.hediffSet.hediffs.Where(h => h.ageTicks == 0 && agingHediffSet.Contains(h.def))
            .DistinctBy(h => h.def.defName) // reporting 2x hearing loss in both ears is unnecessary
            .ToList();
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Hediffs", LangUtility.FormatList(hediffs, h => h.LabelNoun()))
            .AddRule("Part", hediffs.FirstOrDefault()?.Part)
            .AddConstant("isCancer", hediffs.Count == 1 && hediffs[0].def.defName == "Carcinoma")
            .Resolve();

        AddRecord(recordDef, pawn, desc);
    }

    public void Test(TestScenario scenario)
    {
        var victim = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .CreateSingle();

        for (var i = 0; i < 500; i++)
        {
            scenario.Pawn(victim).ForceBirthday().Execute();
        }

        Expect.That(victim).ToHaveHistoryRecord(HistoryRecordDefOf.Birthday, "[PAWN] turned [Age] and began suffering from [Hediffs] due to aging.");
    }

    public void TestCancer(TestScenario scenario)
    {
        scenario.AlwaysHaveCancerOnBirthday = true;
        var victim = scenario.Pawn().ThatMatches(ShouldRecord).ForceBirthday().CreateSingle();

        Expect.That(victim).ToHaveHistoryRecord(HistoryRecordDefOf.Birthday, "[PAWN] turned [Age] and began suffering from a carcinoma in [His] [Part] due to aging.");
    }
}
