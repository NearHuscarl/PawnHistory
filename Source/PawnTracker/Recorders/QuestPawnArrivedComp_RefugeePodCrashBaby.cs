using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class QuestPawnArrivedComp_RefugeePodCrashBaby : QuestPawnArrivedComp
{
    public override bool Match(Quest quest) => quest.root == Extra.QuestScriptDefOf.RefugeePodCrash_Baby;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var corpse = FindDeceasedParentCorpse(input.Pawn, GetDropPodContents(input.Quest));
        return builder
            .AddRule("BabyFaction", input.Pawn.Faction)
            .AddRule("Relation", corpse?.InnerPawn != null ? input.Pawn.GetMostImportantRelation(corpse.InnerPawn).GetGenderSpecificLabel(corpse.InnerPawn) : null)
            .AddConstant("hasDeadParent", corpse != null);
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        var corpse = FindDeceasedParentCorpse(input.Pawn, GetDropPodContents(input.Quest));
        if (corpse != null)
            yield return corpse;
    }

    private static Corpse FindDeceasedParentCorpse(Pawn pawn, IEnumerable<Thing> podContents)
    {
        var parents = new List<Pawn> { pawn.GetMother(), pawn.GetFather() }.Where(parent => parent != null);
        return podContents.OfType<Corpse>().FirstOrDefault(corpse => parents.Contains(corpse.InnerPawn));
    }

    private static IEnumerable<Thing> GetDropPodContents(Quest quest)
    {
        return quest.PartsListForReading.OfType<QuestPart_DropPods>().SelectMany(part => Accessor.QuestPart_DropPods.TmpThingsToDrop(part));
    }
    
    private const int SeedWithoutParent = 20;
    private const int SeedWithParent = 1;

    [RequiresBiotech]
    public void Test(TestScenario scenario)
    {
        var quest = ExecuteWithSeed(SeedWithoutParent, () => scenario.Quest(Extra.QuestScriptDefOf.RefugeePodCrash_Baby).Execute());
        var baby = QuestHelper.GetArrivalPawns(quest).Single();

        Expect.That(baby).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestPawnArrived,
            Description = "A baby named [PAWN] from [Faction] crashed nearby in a transport pod.",
            Concerns = [],
        });
    }

    [RequiresBiotech]
    public void TestWithParent(TestScenario scenario)
    {
        var quest = ExecuteWithSeed(SeedWithParent, () => scenario.Quest(Extra.QuestScriptDefOf.RefugeePodCrash_Baby).Execute());
        var baby = QuestHelper.GetArrivalPawns(quest).Single();
        var corpse = GetDropPodContents(quest).OfType<Corpse>().Single();

        Expect.That(baby).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestPawnArrived,
            Description = "A baby named [PAWN] from [Faction] crashed nearby in a transport pod. [His] [Mother]'s body was in the pod as well.",
            Concerns = [corpse]
        });
    }

    private static T ExecuteWithSeed<T>(int seed, System.Func<T> action)
    {
        Rand.PushState(seed);
        try
        {
            return action();
        }
        finally
        {
            Rand.PopState();
        }
    }
}
