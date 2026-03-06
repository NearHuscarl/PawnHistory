using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public class GameEventBase(PawnEventDef eventDef)
{
    public PawnEventDef eventDef { get; } = eventDef;
}

public class GameEvent(Pawn pawn, PawnEventDef eventDef, TaggedString resolvedDesc) : GameEventBase(eventDef)
{
    public Pawn Pawn { get; } = pawn;
    public List<Pawn> relatedPawns { get; set; } = [];
    public TaggedString resolvedDesc = resolvedDesc;
}

public class GroupEvent(List<Pawn> pawns, Faction faction, PawnEventDef eventDef, Func<Pawn, TaggedString> resolveDesc) : GameEventBase(eventDef)
{
    public Func<Pawn, TaggedString> resolveDesc { get; private set; } = resolveDesc;
    public List<Pawn> Pawns { get; } = pawns;
    public Faction Faction { get; } = faction;
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
