using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using System.Linq;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class BirthdayRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<BirthdayEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn))
                return;

            HandleBirthdayEvent(e);
        });
    }

    private void HandleBirthdayEvent(BirthdayEvent e)
    {
        var recordDef = HistoryRecordDefOf.Birthday;
        var agingHediffSet = e.AgingHediffs.ToHashSet();
        var hediffs = e.Pawn.health.hediffSet.hediffs.Where(h => h.ageTicks == 0 && agingHediffSet.Contains(h.def))
            .DistinctBy(h => h.def.defName) // reporting 2x hearing loss in both ears is unnecessary
            .ToList();
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("Hediffs", LangUtility.FormatList(hediffs, h => h.LabelNoun()))
            .AddRule("Part", hediffs.FirstOrDefault()?.Part)
            .AddConstant("isCancer", hediffs.Count == 1 && hediffs[0].def.defName == "Carcinoma")
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc);
    }

    public override void Test(TestScenario scenario)
    {
        var victim = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .CreateSingle();

        for (var i = 0; i < 500; i++)
        {
            scenario.Pawn(victim).ForceBirthday().Execute();
        }

        scenario.OpenHistoryRecordTab(victim);
    }
}
