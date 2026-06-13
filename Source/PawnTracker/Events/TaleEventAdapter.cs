using System;
using System.Collections.Generic;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

internal abstract class TaleDispatcher
{
    public abstract TaleDef TaleDef { get; }
    public abstract void Dispatch(TaleRecordedEvent e);
}

internal static class TaleEventAdapter
{
    public static void Publish(TaleDef def, object[] args)
    {
        if (args.Length == 0 || args[0] is not Pawn pawn)
            return;

        var rawEvent = new TaleRecordedEvent(def, pawn, [.. args.Skip(1)]);
        TaleDispatcherManager.Dispatch(rawEvent);
    }
}

[StaticConstructorOnStartup]
internal static class TaleDispatcherManager
{
    private static readonly Dictionary<TaleDef, List<TaleDispatcher>> Dispatchers = [];

    static TaleDispatcherManager()
    {
        foreach (var type in typeof(TaleDispatcher).AllSubclassesNonAbstract())
        {
            var dispatcher = (TaleDispatcher)Activator.CreateInstance(type);
            
            if (dispatcher.TaleDef == null) // DLC gated
                continue;

            if (!Dispatchers.TryGetValue(dispatcher.TaleDef, out var list))
                Dispatchers[dispatcher.TaleDef] = list = [];

            list.Add(dispatcher);
        }
    }

    public static void Dispatch(TaleRecordedEvent e)
    {
        if (!Dispatchers.TryGetValue(e.Tale, out var list))
            return;

        foreach (var dispatcher in list)
            dispatcher.Dispatch(e);
    }
}
