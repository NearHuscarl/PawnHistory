using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class QuestPawnArrivedComp_IntroWimp : QuestPawnArrivedComp
{
    public override bool Match(Quest quest) => quest.root.defName == nameof(Extra.QuestScriptDefOf.Intro_Wimp);

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, Quest quest, Pawn pawn, List<Pawn> questPawns)
    {
        var animalKind = Accessor.QuestPart_Incident.IncidentParms(quest.GetFirstPartOfType<QuestPart_Incident>()).pawnKind;
        return builder.AddRule("AnimalKind", animalKind.label);
    }
    
    public override IEnumerable<Thing> GetConcerns(Quest quest, List<Pawn> questPawns)
    {
        var chasingAnimal = questPawns.FirstOrDefault(p => p.IsAnimal); // only exists if animal is not mad
        if (chasingAnimal == null)
            yield break;
        
        yield return chasingAnimal;
    }

    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.Intro_Wimp).Execute();
        scenario.ForwardTicks(1800); // QuestNode_Delay before pawn arrives
        
        QuestPawnArrivedRecorder.AssertArrived(quest, "[PAWN] arrived at the colony looking for protection while being followed by a manhunting [Animal].");
    }
}
