using System.Collections.Generic;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class BabyAdoptedRecorder : RecorderBase<BabyAdoptedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<BabyAdoptedEvent>(CreateRecord);
    }

    public override void CreateRecord(BabyAdoptedEvent e)
    {
        var parents = ResolveParents(e.Baby);
        var desc = HistoryRecordDefOf.BabyAdopted.Description(e.Baby)
            .WithPlayerFaction()
            .AddRule("FormerFaction", e.FormerFaction)
            .AddConstant("hasFaction", e.FormerFaction != null)
            .AddConstant("pov", "Baby")
            .Resolve();

        if (ShouldRecord(e.Baby))
            AddRecord(HistoryRecordDefOf.BabyAdopted, e.Baby, desc, parents);

        CreateParentRecords(e, parents);
    }

    private void CreateParentRecords(BabyAdoptedEvent e, List<Pawn> parents)
    {
        foreach (var parent in parents)
        {
            if (!ShouldRecordParent(parent))
                continue;
            
            var desc = HistoryRecordDefOf.BabyAdopted.Description(e.Baby)
                .WithPlayerFaction()
                .AddRule("ChildRelation", PawnRelationDefOf.Child.GetGenderSpecificLabel(e.Baby))
                .AddRule("Child", e.Baby)
                .AddConstant("pov", "Parent")
                .Resolve();
            AddRecord(HistoryRecordDefOf.BabyAdopted, parent, desc, [e.Baby]);
        }
    }

    [RequiresBiotech]
    public void Test(TestScenario scenario)
    {
        var prisoners = new List<Pawn>();
        scenario.Map()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(2, prisoners: prisoners)
            .Execute();
        scenario.Pawn(prisoners).SetFaction(Faction.OfHostile).Execute();

        var baby = prisoners[0].GiveBirth(prisoners[1]);
        new Designator_Adopt().DesignateThing(baby);

        Expect.That(baby).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BabyAdopted,
            Description = "[PAWN], a baby from [FormerFaction], was adopted by the colony.",
            Concerns = [..prisoners]
        });
        Expect.ThatAll(prisoners).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BabyAdopted,
            Description = "[PAWN]'s [ChildRelation], [Child], was adopted by the colony.",
            Concerns = [baby]
        });
    }
    
    private static List<Pawn> ResolveParents(Pawn baby)
    {
        var parents = new List<Pawn>();
        var mother = baby.GetMother();
        var father = baby.GetFather();

        if (mother != null)
            parents.Add(mother);
        if (father != null && father != mother)
            parents.Add(father);

        return parents;
    }

    private static bool ShouldRecordParent(Pawn parent)
    {
        if (parent.IsPrisonerOfColony || parent.IsSlaveOfColony)
            return true;
        
        if (parent.Faction != Faction.OfPlayer)
            return true;

        return false;
    }
}
