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

    public static Action SubscribeOnce<T>(Action<T> listener) where T : GameEventBase
    {
        Subscribe((Action<T>)Wrapper);

        return Unsub;

        void Wrapper(T evt)
        {
            try
            {
                listener(evt);
            }
            finally
            {
                Unsub();
            }
        }

        void Unsub()
        {
            Unsubscribe((Action<T>)Wrapper);
        }
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

        // TODO: for record during world/map generation, add to a queue to process in Playing state
        // so information are available. TODO: Simulate past event
        if (Current.ProgramState != ProgramState.Playing)
        {
            if (NearDebugSettings.LogDebug)
                Log.Message($"[PawnHistory] {nameof(GameEventBus)} skipped firing {DebugUtility.Format(evt)} during {Current.ProgramState} state");
            return;
        }

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
