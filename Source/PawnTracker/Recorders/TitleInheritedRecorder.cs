using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class TitleInheritedRecorder : RecorderBase<TitleInheritanceEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<TitleInheritanceEvent>(CreateRecord);
    }

    public override void CreateRecord(TitleInheritanceEvent e)
    {
        if (!ShouldRecord(e.Heir))
            return;

        var recordDef = HistoryRecordDefOf.TitleInherited;
        var desc = recordDef.Description(e.Heir)
            .IncludePawnGrammar()
            .AddRule("Deceased", e.Deceased)
            .AddRule("Faction", e.Faction)
            .AddRule("ReplacedTitle", e.HeirCurrentTitle)
            .AddRule("Title", e.Title)
            .AddConstant("outcome", e.Outcome)
            .Resolve();

        // TODO: fix the order of this, it should be below the Death record. 
        AddRecord(recordDef, e.Heir, desc, [e.Deceased]);
    }

    public static (Pawn, Pawn) SetupInheritance(TestScenario scenario, RoyalTitleDef deceasedTitle, RoyalTitleDef heirTitle = null)
    {
        var deceased = scenario.Pawn().Colonist().SetRoyalTitle(deceasedTitle).CreateSingle();
        var heir = scenario.Pawn(deceased.royalty.GetHeir(Faction.OfEmpire)).Colonist().SetRoyalTitle(heirTitle).CreateSingle();

        HealthUtility.DamageUntilDead(deceased);
        
        return (heir, deceased);
    }

    [RequiresRoyalty]
    public void TestWasInherited(TestScenario scenario)
    {
        var (heir, deceased) = SetupInheritance(scenario, DefLookup.RoyalTitle.Praetor);

        Expect.That(heir).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.TitleInherited,
            Description = "[PAWN] was set to inherit the Praetor title from [Deceased] according to the succession laws of [Faction] upon completion of a bestowing ceremony.",
            Concerns = [deceased]
        });
    }

    [RequiresRoyalty]
    public void TestAsReplacement(TestScenario scenario)
    {
        var (heir, deceased) = SetupInheritance(scenario, RoyalTitleDefOf.Count, DefLookup.RoyalTitle.Praetor);

        Expect.That(heir).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.TitleInherited,
            Description = "[PAWN] was set to inherit the Archon title from [Deceased] according to the succession laws of [Faction], replacing [His] Praetor title upon completion of a bestowing ceremony.",
            Concerns = [deceased]
        });
    }
}
