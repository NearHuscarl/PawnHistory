using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class DivorceByIdeoRecorder : RecorderBase<DivorceByIdeoEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<DivorceByIdeoEvent>(CreateRecord);
    }

    public override void CreateRecord(DivorceByIdeoEvent e)
    {
        var recordDef = HistoryRecordDefOf.DivorceByIdeo;

        if (ShouldRecord(e.DivorcingPawn))
        {
            var desc = recordDef.Description(e.DivorcingPawn, "Divorcer")
                .IncludePawnGrammar()
                .AddRule("Spouses", LangUtility.FormatList(e.FormerSpouses))
                .AddConstant("pov", "Divorcer")
                .Resolve();
            AddRecord(recordDef, e.DivorcingPawn, desc, e.FormerSpouses);
        }

        foreach (var spouse in e.FormerSpouses)
        {
            if (!ShouldRecord(spouse))
                continue;
            
            var desc = recordDef.Description(spouse)
                .AddRule("Divorcer", e.DivorcingPawn, addSubsymbols: true)
                .AddConstant("pov", "FormerSpouse")
                .Resolve();
            AddRecord(recordDef, spouse, desc, [e.DivorcingPawn]);
        }
    }

    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        var haremIdeo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.SpouseCount_Female_Unlimited).Execute();
        var farcistIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
        var divorcingPawn = scenario.Pawn()
            .Colonist()
            .SetGender(Gender.Male)
            .SetIdeo(haremIdeo)
            .CreateSingle();
        var formerSpouses = scenario.Pawn(2)
            .Colonist()
            .SetGender(Gender.Female)
            .Do(p => divorcingPawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KilledMyFriend, p)) // pawn pick spouses with low opinion to divorce first
            .SetRelation(divorcingPawn, PawnRelationDefOf.Spouse)
            .Execute();
        var otherSpouse = scenario.Pawn()
            .Colonist()
            .SetGender(Gender.Female)
            .SetRelation(divorcingPawn, PawnRelationDefOf.Spouse)
            .CreateSingle();
        var otherLover = scenario.Pawn()
            .Colonist()
            .SetRelation(divorcingPawn, PawnRelationDefOf.Lover)
            .CreateSingle();
        
        scenario.Pawn(divorcingPawn).SetIdeo(farcistIdeo).CreateSingle();

        Expect.That(divorcingPawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.DivorceByIdeo,
            Description = "[Divorcer] divorced [A] and [B] after [His] new ideoligion forbade having too many spouses.",
            Concerns = [..formerSpouses],
        });
        Expect.ThatAll(formerSpouses).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.DivorceByIdeo,
            Description = "[Divorcer] divorced [PAWN] after [His] new ideoligion forbade having too many spouses.",
            Concerns = [divorcingPawn],
        });
        Expect.That(divorcingPawn.HistoryRecords.TakeLast(2).Select(r => r.def)).SequenceEqual([HistoryRecordDefOf.IdeoChanged, HistoryRecordDefOf.DivorceByIdeo]);
        Expect.That(otherSpouse).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.DivorceByIdeo);
        Expect.That(otherLover).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.DivorceByIdeo);
        Expect.That(divorcingPawn).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.Breakup);
        Expect.ThatAll(formerSpouses).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.Breakup);
    }
}
