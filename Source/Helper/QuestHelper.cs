using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System.Linq;
using RimWorld.Planet;

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
    
    public static Pawn GetPawnReward(Quest quest)
    {
        return quest.GetFirstPartOfType<QuestPart_Choice>().choices.SelectMany(c => c.rewards.OfType<Reward_Pawn>().Select(r => r.pawn)).FirstOrDefault();
    }

    public static IEnumerable<Pawn> GetQuestPawns(Quest quest = null)
    {
        quest ??= Find.QuestManager.QuestsListForReading.Last();
        
        return quest.QuestLookTargets.Where(t => t.Pawn != null).Select(p => p.Pawn);
    }

    public static List<Pawn> GetArrivalPawns(Quest quest = null)
    {
        quest ??= Find.QuestManager.QuestsListForReading.Last();
        
        var source1 = quest.PartsListForReading.OfType<QuestPart_PawnsArrive>().SelectMany(part => part.pawns);
        var source2 = quest.PartsListForReading.OfType<QuestPart_DropPods>().SelectMany(part => part.Things).OfType<Pawn>();
        var source3 = quest.PartsListForReading.OfType<QuestPart_GiveToCaravan>()
            .Where(part => part.caravan.Spawned)
            .SelectMany(part => part.Things).OfType<Pawn>();
        var source4 = quest.PartsListForReading.OfType<QuestPart_SetupTransportShip>()
            .Where(part => part.transportShip.ShipExistsAndIsSpawned)
            .SelectMany(part => part.transportShip.TransporterComp.innerContainer.OfType<Pawn>());
        
        return source1.Concat(source2).Where(p => p.MapHeld != null).Concat(source3).Concat(source4).ToList();
    }
    
    public static T GetWorldObject<T>(Quest quest) where T : WorldObject
    {
        return (T)quest.PartsListForReading.OfType<QuestPart_SpawnWorldObject>().FirstOrDefault(p => p.worldObject is T)?.worldObject;
    }
    
    public static bool TryGetRelatedQuestFrom(WorldObject worldObject, out Quest quest)
    {
        quest = null;

        foreach (var q in Find.QuestManager.QuestsListForReading)
        {
            if (q.hidden)
                continue;

            if (!q.QuestLookTargets.Contains(worldObject))
                continue;

            quest = q;
            return true;
        }

        return false;
    }
    
    public static bool TryGetRelatedQuestFrom(Pawn pawn, out Quest quest)
    {
        quest = null;

        foreach (var q in Find.QuestManager.QuestsListForReading)
        {
            if (q.hidden)
                continue;
            if (!q.QuestReserves(pawn))
                continue;
            
            quest = q;
            return true;
        }

        return false;
    }
    
    public static bool TryGetRelatedQuestFrom(TransportShip ship, out Quest quest)
    {
        quest = null;

        foreach (var q in Find.QuestManager.QuestsListForReading)
        {
            if (q.hidden)
                continue;

            if (!q.QuestReserves(ship))
                continue;

            quest = q;
            return true;
        }

        return false;
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
