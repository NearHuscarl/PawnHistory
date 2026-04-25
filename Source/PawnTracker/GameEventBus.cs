using PawnHistory.Source.DebugTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public record GameEventBase;

public class GameEventBus
{
    private static readonly Dictionary<Type, List<Delegate>> Listeners = [];

    public static void Subscribe<T>(Action<T> listener) where T : GameEventBase
    {
        var type = typeof(T);

        if (!Listeners.TryGetValue(type, out var list))
        {
            list = [];
            Listeners[type] = list;
        }

        list.Add(listener);
    }

    public static void SubscribeOnce<T>(Action<T> listener) where T : GameEventBase
    {
        void wrapper(T evt)
        {
            try
            {
                listener(evt);
            }
            finally
            {
                Unsubscribe((Action<T>)wrapper);
            }
        }
        Subscribe((Action<T>)wrapper);
    }

    public static void Unsubscribe<T>(Action<T> listener) where T : GameEventBase
    {
        var type = typeof(T);

        if (!Listeners.TryGetValue(type, out var list))
            return;

        list.Remove(listener);

        if (list.Count == 0)
            Listeners.Remove(type);
    }

    public static void Publish<T>(T evt) where T : GameEventBase
    {
        if (!Listeners.TryGetValue(typeof(T), out var list))
            return;

        foreach (var listener in list.ToArray().Cast<Action<T>>())
        {
            try
            {
                listener(evt);
            }
            catch (Exception ex)
            {
                Log.Error($"[PawnHistory] {nameof(GameEventBus)} failed after firing {DebugUtility.Format(evt)}\n{ex}");
            }
        }
    }
}
