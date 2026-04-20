using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class QuestBuilder
{
    private readonly List<Action> processors = [];
    
    private readonly QuestScriptDef questScriptDef;
    private float points;
    
    public QuestBuilder(QuestScriptDef def, float points = 500)
    {
        questScriptDef = def;
        this.points = points;
    }

    private static void AcceptInstantly(Quest quest)
    {
        if (quest.root.autoAccept)
            return;
        
        var choice = quest.PartsListForReading.OfType<QuestPart_Choice>().FirstOrDefault();
        if (choice != null && choice.choices.Any())
        {
            choice.Choose(choice.choices.RandomElement());
        }
        quest.Accept(PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended.Where(p => QuestUtility.CanPawnAcceptQuest(p, quest)).RandomElementWithFallback());
        quest.dismissed = false;
    }

    public Quest Execute()
    {
        var quest = QuestUtility.GenerateQuestAndMakeAvailable(questScriptDef, points);

        AcceptInstantly(quest);

        processors.ForEach(processor => processor());
        
        return quest;
    }
}