using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System.Linq;
using RimWorld.Planet;

namespace PawnHistory.Source.Helper;

public enum QuestPawnKind
{
    Lodger,
    Helper,
    Joiner,
    Raider,
    Guest,
}

public static class QuestHelper
{
    public static QuestPawnKind GetQuestPawnKind(Quest quest, Pawn pawn)
    {
        var kind = QuestPawnKind.Guest;
        
        if (pawn.HostileTo(Faction.OfPlayer) && !pawn.Downed)
            kind = QuestPawnKind.Raider;
        else if (IsReward(quest, pawn) || pawn.HomeFaction == Faction.OfPlayer && pawn.records.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal) == 0)
            kind = QuestPawnKind.Joiner;
        else if (IsHelper(quest, pawn))
            kind = QuestPawnKind.Helper;
        else if (pawn.IsQuestLodger())
            kind = QuestPawnKind.Lodger;

        return kind;
    }
    
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
        return GetArrivalPawns(quest, true);
    }

    public static List<Pawn> GetArrivalPawns(Quest quest = null, bool includesWorldPawns = false)
    {
        quest ??= Find.QuestManager.QuestsListForReading.Last();
        
        var source1 = quest.PartsListForReading.OfType<QuestPart_PawnsArrive>().SelectMany(part => part.pawns);
        var source2 = quest.PartsListForReading.OfType<QuestPart_DropPods>().SelectMany(part => part.Things).OfType<Pawn>();
        var source3 = quest.PartsListForReading.OfType<QuestPart_GiveToCaravan>()
            .Where(part => includesWorldPawns || part.caravan.Spawned)
            .SelectMany(part => part.Things).OfType<Pawn>();
        var source4 = quest.PartsListForReading.OfType<QuestPart_SetupTransportShip>()
            .Where(part => includesWorldPawns || part.transportShip.ShipExistsAndIsSpawned)
            .SelectMany(part => part.transportShip.TransporterComp.innerContainer.OfType<Pawn>());
        
        return source1.Concat(source2).Where(p => includesWorldPawns || p.MapHeld != null).Concat(source3).Concat(source4).ToList();
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
        if (quest == null)
            return null;

        var questPart_PawnHistory = quest.GetFirstPartOfType<QuestPart_PawnHistory>();
        
        if (quest.root == Extra.QuestScriptDefOf.ThreatReward_Raid_Joiner)
            return questPart_PawnHistory?.Joiner;

        return questPart_PawnHistory?.Asker;
    }

    private static readonly HashSet<PawnKindDef> CombatKinds = new List<PawnKindDef>
    {
        // from Util_ChooseRandomQuestHelperKind
        PawnKindDefOf.Empire_Fighter_Trooper,
        PawnKindDefOf.Empire_Fighter_Janissary,
        Extra.PawnKindDefOf.Empire_Fighter_Champion,
        PawnKindDefOf.Empire_Fighter_Cataphract,
        Extra.PawnKindDefOf.Tribal_Archer,
        Extra.PawnKindDefOf.Tribal_Berserker,
        Extra.PawnKindDefOf.Tribal_HeavyArcher,
        Extra.PawnKindDefOf.Tribal_Warrior,
        Extra.PawnKindDefOf.Mercenary_Elite_Acidifier,
        Extra.PawnKindDefOf.Mercenary_Slasher_Acidifier,
        Extra.PawnKindDefOf.Mercenary_Gunner_Acidifier,
        Extra.PawnKindDefOf.Mercenary_Sniper_Acidifier,
        // EndGame_RoyalAscent quest
        Extra.PawnKindDefOf.Empire_Fighter_StellicGuardRanged,
        Extra.PawnKindDefOf.Empire_Fighter_StellicGuardMelee,
    }.Where(k => k != null).ToHashSet();

    public static bool IsHelper(Quest quest, Pawn pawn)
    {
        if (quest == null)
            return false;
        
        var isDesignatedHelper = quest.GetFirstPartOfType<QuestPart_PawnHistory>()?.Helpers.Contains(pawn) ?? false;
        return isDesignatedHelper || CombatKinds.Contains(pawn.kindDef) && !pawn.HostileTo(Faction.OfPlayer);
    }
}
