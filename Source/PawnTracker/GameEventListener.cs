using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker;

public class GameEventBase() { }

public class RaidEvent(List<Pawn> pawns, Faction faction, RaidStrategyDef raidStrategy, PawnsArrivalModeDef raidArrivalMode, bool isFriendly = false) : GameEventBase
{
    public List<Pawn> Pawns { get; } = pawns;
    public Faction Faction { get; } = faction;
    public RaidStrategyDef RaidStrategy { get; } = raidStrategy;
    public PawnsArrivalModeDef RaidArrivalMode { get; } = raidArrivalMode;
    public bool IsFriendly { get; } = isFriendly;
}

public class LordToilChangeEvent(LordToil currentToil, LordToil nextToil, Trigger trigger, Lord lord) : GameEventBase
{
    public LordToil CurrentToil { get; } = currentToil;
    public LordToil NextToil { get; } = nextToil;
    public Trigger Trigger { get; } = trigger;
    public Lord Lord { get; } = lord;
}

public class HediffPreAddEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public Hediff Hediff { get; } = hediff;
    public BodyPartRecord Part { get; } = part;
    public DamageInfo? Dinfo { get; } = dinfo;
}

public class HediffPostAddEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public Hediff Hediff { get; } = hediff;
    public BodyPartRecord Part { get; } = part;
    public DamageInfo? Dinfo { get; } = dinfo;
}

public enum CasualtyType
{
    Killed,
    Downed,
}

public class CasualtyLogAddedEvent(Battle battle, BattleLogEntry_StateTransition transitionEntry, LogEntry_DamageResult lastDamageEntry, Pawn initiator, Pawn subject, CasualtyType casualty, HediffDef culpritHediff) : GameEventBase
{
    public Battle Battle { get; } = battle;
    public BattleLogEntry_StateTransition TransitionEntry { get; } = transitionEntry;
    public LogEntry_DamageResult LastDamageEntry { get; } = lastDamageEntry;
    public Pawn Initiator { get; } = initiator;
    public Pawn Subject { get; } = subject;
    public CasualtyType Casualty { get; } = casualty;
    public HediffDef CulpritHediff { get; } = culpritHediff;
}

public class LightningStrikeEvent(IntVec3 strikeLoc, Map map, float radius) : GameEventBase
{
    public IntVec3 StrikeLoc { get; } = strikeLoc;
    public Map Map { get; } = map;
    public float Radius { get; } = radius;
}

public class GameEventListener
{
    private static readonly Dictionary<Type, List<Delegate>> listeners = [];

    public static void Subscribe<T>(Action<T> listener) where T : GameEventBase
    {
        var type = typeof(T);

        if (!listeners.TryGetValue(type, out var list))
        {
            list = [];
            listeners[type] = list;
        }

        list.Add(listener);
    }

    public static void Publish<T>(T evt) where T : GameEventBase
    {
        try
        {
            if (listeners.TryGetValue(typeof(T), out var list))
            {
                foreach (var listener in list.Cast<Action<T>>())
                {
                    listener(evt);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[PawnHistory] Failed after firing {evt.GetType().Name}\n\n{ex}");
        }
    }
}
