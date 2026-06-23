using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class EatenRecorder : RecorderBase<EatenEvent>
{
    private const string EatenPov = "Eaten";
    private const string EaterPov = "Eater";

    public override void Register()
    {
        GameEventBus.Subscribe<EatenEvent>(CreateRecord);
    }

    public override void CreateRecord(EatenEvent e)
    {
        var recordDef = HistoryRecordDefOf.Eaten;

        if (ShouldRecord(e.Eaten))
            AddRecord(HistoryRecordDefOf.Eaten, e.Eaten, CreateDescription(e, e.Eaten, EatenPov), [e.Eater]);
        if (ShouldRecord(e.Eater))
            AddRecord(HistoryRecordDefOf.AtePawn, e.Eater, CreateDescription(e, e.Eater, EaterPov), [e.Eaten]);
    }

    private static string CreateDescription(EatenEvent e, Pawn pawn, string pov)
    {
        return HistoryRecordDefOf.Eaten.Description(pawn)
            .AddRule("Eaten", e.Eaten, addSubsymbols: true)
            .AddRule("Eater", e.Eater, addSubsymbols: true)
            .AddConstant("pov", pov)
            .AddConstant("eaterHumanlike", e.Eater.RaceProps.Humanlike)
            .Resolve();
    }

    public void TestByHuman(TestScenario scenario)
    {
        var eaten = scenario.Pawn()
            .Enemy()
            .Corpse()
            .CreateSingle();
        var eater = scenario.Pawn()
            .Colonist()
            .CreateSingle();

        var corpse = eaten.Corpse;
        corpse.Ingested(eater, FoodUtility.GetBodyPartNutrition(corpse, eaten.RaceProps.body.corePart));

        Expect.That(eaten).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.Eaten,
            Description = "[PAWN] was eaten by [Eater].",
            Concerns = [eater],
        });
        Expect.That(eater).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.AtePawn,
            Description = "[PAWN] ate [Eaten]'s corpse.",
            Concerns = [eaten],
        });
    }

    public void TestByAnimal(TestScenario scenario)
    {
        var eaten = scenario.Pawn()
            .Enemy()
            .Corpse()
            .CreateSingle();
        var eater = scenario.Pawn()
            .Animal(Extra.PawnKindDefOf.Cougar)
            .CreateSingle();

        var corpse = eaten.Corpse;
        corpse.Ingested(eater, FoodUtility.GetBodyPartNutrition(corpse, eaten.RaceProps.body.corePart));

        Expect.That(eaten).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.Eaten,
            Description = "[PAWN] was eaten by a cougar.",
            Concerns = [eater],
        });
        Expect.That(eater).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.AtePawn);
    }
}
