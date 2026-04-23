using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class QuestPawnArrivedComp_IntroWimp : QuestPawnArrivedComp
{
    public override bool Match(Quest quest) => quest.root.defName == nameof(DefLookup.QuestScript.Intro_Wimp);

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, Quest quest, Pawn pawn, List<Pawn> questPawns)
    {
        var chasingAnimal = questPawns.FirstOrDefault(p => p.IsAnimal);
        return builder.AddRule("AnimalKind", chasingAnimal.kindDef.label);
    }
    
    public override IEnumerable<Thing> GetConcerns(List<Pawn> questPawns)
    {
        var chasingAnimal = questPawns.FirstOrDefault(p => p.IsAnimal);
        if (chasingAnimal == null)
            yield break;
        
        yield return chasingAnimal;
    }

    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        var quest = scenario.Quest(DefLookup.QuestScript.Intro_Wimp).Execute();
        scenario.ForwardTicks(1800);
        
        QuestPawnArrivedRecorder.AssertArrived(quest, "[PAWN] arrived at the colony looking for protection while being followed by a manhunting [Animal].");
    }
}
