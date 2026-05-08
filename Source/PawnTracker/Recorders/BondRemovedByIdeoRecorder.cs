using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class BondRemovedByIdeoRecorder : RecorderBase<BondRemovedByIdeoEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<BondRemovedByIdeoEvent>(CreateRecord);
    }

    public override void CreateRecord(BondRemovedByIdeoEvent e)
    {
        var recordDef = HistoryRecordDefOf.BondRemovedByIdeo;

        if (ShouldRecord(e.Pawn))
        {
            var desc = recordDef.Description(e.Pawn, "Human")
                .IncludePawnGrammar()
                .AddRule("BondedAnimals", LangUtility.FormatList(e.FormerBondedAnimals))
                .AddConstant("pov", "Human")
                .Resolve();
            AddRecord(recordDef, e.Pawn, desc, e.FormerBondedAnimals);
        }

        foreach (var bondedAnimal in e.FormerBondedAnimals)
        {
            var desc = recordDef.Description(bondedAnimal)
                .AddRule("Human", e.Pawn, addSubsymbols: true)
                .AddConstant("pov", "BondedAnimal")
                .Resolve();
            AddRecord(recordDef, bondedAnimal, desc, [e.Pawn]);
        }
    }

    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        var bondedAnimal = scenario.Pawn().Animal(Extra.PawnKindDefOf.Husky).CreateSingle();
        var ideo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.Bonding_Disapproved).Execute();
        var human = scenario.Pawn()
            .Colonist()
            .SetRelation(bondedAnimal, PawnRelationDefOf.Bond)
            .SetIdeo(ideo)
            .CreateSingle();

        Expect.That(human).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BondRemovedByIdeo,
            Description = "[Human] gave up [His] bond with the husky after [His] new ideoligion disapproved of bonding with animals.",
            Concerns = [bondedAnimal],
        });
        Expect.That(bondedAnimal).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BondRemovedByIdeo,
            Description = "[Human] gave up [His] bond with the husky after [His] new ideoligion disapproved of bonding with animals.",
            Concerns = [human],
        });
        Expect.That(human.HistoryRecords.TakeLast(2).Select(r => r.def)).SequenceEqual([HistoryRecordDefOf.IdeoChanged, HistoryRecordDefOf.BondRemovedByIdeo]);
    }
}
