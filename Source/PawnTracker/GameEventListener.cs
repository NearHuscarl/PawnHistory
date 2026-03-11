using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker;

public class GameEventBase() { }

public class GameEvent(Pawn pawn, PawnEventDef eventDef, TaggedString resolvedDesc) : GameEventBase
{
    public PawnEventDef eventDef { get; } = eventDef;
    public Pawn Pawn { get; } = pawn;
    public List<Pawn> relatedPawns { get; set; } = [];
    public TaggedString resolvedDesc = resolvedDesc;
}

public class GroupEvent(List<Pawn> pawns, Faction faction, PawnEventDef eventDef, Func<Pawn, TaggedString> resolveDesc) : GameEventBase
{
    public PawnEventDef eventDef { get; } = eventDef;
    public Func<Pawn, TaggedString> resolveDesc { get; private set; } = resolveDesc;
    public List<Pawn> Pawns { get; } = pawns;
    public Faction Faction { get; } = faction;
}

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
        if (listeners.TryGetValue(typeof(T), out var list))
        {
            foreach (var listener in list.Cast<Action<T>>())
            {
                listener(evt);
            }
        }
    }
}
