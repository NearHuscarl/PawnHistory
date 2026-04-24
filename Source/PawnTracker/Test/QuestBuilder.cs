using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class QuestBuilder
{
    private readonly List<Action<Quest>> processors = [];
    private readonly QuestScriptDef questScriptDef;
    private readonly float points;
    private Pawn pawn;
    private Quest quest;
    
    public QuestBuilder(QuestScriptDef def = null, float points = 500)
    {
        questScriptDef = def;
        this.points = points;
    }
    
    public QuestBuilder WithQuest(Quest quest1)
    {
        this.quest = quest1;
        return this;
    }
    
    public QuestBuilder Pawn(Pawn pawn1)
    {
        this.pawn = pawn1;
        return this;
    }
    
    public QuestBuilder Do(Action<Quest> processor)
    {
        processors.Add(processor);
        return this;
    }

    public QuestBuilder ChooseReward(Func<QuestPart_Choice.Choice, bool> filter)
    {
        return Do(quest =>
        {
            var partChoice = quest.GetFirstPartOfType<QuestPart_Choice>();

            if (partChoice == null)
            {
                Log.Warning($"Quest {quest.root.defName} has no choice defined.");
                return;
            }
            
            var choicePart = quest.PartsListForReading.OfType<QuestPart_Choice>().First(part => part.choices.Any(filter));
            var rewardChoice = choicePart.choices.First(choice => choice.rewards.OfType<Reward_Pawn>().Any());
            choicePart.Choose(rewardChoice);
        });
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

    private Quest GenerateQuest()
    {
        return QuestUtility.GenerateQuestAndMakeAvailable(questScriptDef, points);
    }

    public Quest Execute()
    {
        var effectiveQuest = quest ?? GenerateQuest();
        AcceptInstantly(effectiveQuest);
        processors.ForEach(processor => processor(effectiveQuest));
        return effectiveQuest;
    }
}