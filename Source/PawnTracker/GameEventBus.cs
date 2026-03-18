using PawnHistory.Source.DebugTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public class GameEventBase() { }

public class GameEventBus
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
        if (!listeners.TryGetValue(typeof(T), out var list))
            return;

        foreach (var listener in list.Cast<Action<T>>())
        {
            try
            {
                listener(evt);
            }
            catch (Exception ex)
            {
                Log.Error($"[PawnHistory] Failed after firing {DebugUtility.Format(evt)}\n{ex}");
            }
        }
    }
}
