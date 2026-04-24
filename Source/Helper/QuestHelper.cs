using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System.Linq;

namespace PawnHistory.Source.Helper;

public static class QuestHelper
{
    public static bool IsReward(Quest quest, Pawn pawn)
    {
        var partChoice = quest.GetFirstPartOfType<QuestPart_Choice>();

        if (partChoice == null)
            return false;
        
        return partChoice.choices.Any(c => c.rewards.OfType<Reward_Pawn>().Any(r => r.pawn == pawn));
    }

    public static List<Pawn> GetQuestPawns(Quest quest = null)
    {
        quest ??= Find.QuestManager.QuestsListForReading.Last();
        
        return quest.QuestLookTargets.Where(t => t.Pawn != null).Select(p => p.Pawn).ToList();
    }

    public static List<Pawn> GetArrivalPawns(Quest quest = null)
    {
        quest ??= Find.QuestManager.QuestsListForReading.Last();
        
        var source1 = quest.PartsListForReading.OfType<QuestPart_PawnsArrive>().SelectMany(part => part.pawns);
        var source2 = quest.PartsListForReading.OfType<QuestPart_DropPods>().SelectMany(part => part.Things).OfType<Pawn>();
        var source3 = quest.PartsListForReading.OfType<QuestPart_GiveToCaravan>().SelectMany(part => part.Things).OfType<Pawn>();
        var source4 = quest.PartsListForReading.OfType<QuestPart_GiveNearPawn>().SelectMany(part => Accessor.QuestPart_GiveNearPawn.Pawns(part));
        var source5 = quest.PartsListForReading.OfType<QuestPart_SetupTransportShip>().SelectMany(part => part.pawns ?? []);
        
        return source1.Concat(source2).Concat(source3).Concat(source4).Concat(source5).Where(p => p.MapHeld == Find.CurrentMap).ToList();
    }
    
    public static Pawn GetAsker(Quest quest)
    {
        foreach (var part in quest?.PartsListForReading ?? [])
        {
            switch (part)
            {
                case QuestPart_PawnsArrive pawnsArrive:
                {
                    var asker = pawnsArrive.pawns.FirstOrDefault(IsAskerCandidate);
                    if (asker != null)
                        return asker;
                    break;
                }
            }
        }

        return null;
    }
    
    private static bool IsAskerCandidate(Pawn pawn)
    {
        return pawn != null && pawn.RaceProps.Humanlike;
    }
}
