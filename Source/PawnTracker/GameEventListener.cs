using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnHistory.Source.PawnTracker
{
    public class GameEventBase
    {
    }

    public class RaidEvent(IEnumerable<Pawn> enemies, Faction faction) : GameEventBase
    {
        public IEnumerable<Pawn> Enemies { get; } = enemies;
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
}
