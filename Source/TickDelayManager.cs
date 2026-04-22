using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source;

public class ScheduledActionData
{
    public bool Cancelled;
}

public class ScheduledAction
{
    public int ExecuteTick;
    public int Interval;
    public int EndTick = -1;
    public Action<ScheduledActionData> Action;
    public bool Repeat;
    public readonly ScheduledActionData Data = new();
}

public class TickDelayManager : GameComponent
{

    private readonly List<ScheduledAction> actions = [];
    private static TickDelayManager _instance;

    public TickDelayManager(Game _)
    {
        _instance = this;
    }

    public override void GameComponentTick()
    {
        var currentTick = Find.TickManager.TicksGame;
        var toRemove = new List<ScheduledAction>();
        var toReinsert = new List<ScheduledAction>();

        foreach (var a in actions.ToList())
        {
            if (a.Data.Cancelled)
            {
                toRemove.Add(a);
                continue;
            }

            if (currentTick >= a.ExecuteTick)
            {
                try
                {
                    a.Action?.Invoke(a.Data);
                }
                catch (Exception e)
                {
                    Log.Error($"[{nameof(TickDelayManager)}] ScheduledAction failed: {e}");
                }
                finally
                {
                    toRemove.Add(a);

                    if (a.Repeat && !a.Data.Cancelled)
                    {
                        var nextExecuteTick = currentTick + a.Interval;

                        if (a.EndTick != -1 && nextExecuteTick > a.EndTick)
                            a.Data.Cancelled = true;
                        else
                        {
                            a.ExecuteTick = nextExecuteTick;
                            toReinsert.Add(a);
                        }
                    }
                }
            }
        }

        foreach (var a in toRemove) actions.Remove(a);
        foreach (var a in toReinsert) InsertSorted(a);
    }

    private static readonly IComparer<ScheduledAction> Comparer = Comparer<ScheduledAction>.Create((a, b) => a.ExecuteTick.CompareTo(b.ExecuteTick));

    private void InsertSorted(ScheduledAction action)
    {
        var i = actions.BinarySearch(action, Comparer);

        if (i < 0)
            actions.Insert(~i, action);
        else
        {
            while (i < actions.Count && actions[i].ExecuteTick == action.ExecuteTick)
                i++;
            actions.Insert(i, action);
        }
    }

    public static void Delay(int ticks, Action action) => Delay(ticks, a => action());

    public static void Delay(int ticks, Action<ScheduledActionData> action)
    {
        _instance.InsertSorted(new ScheduledAction
        {
            ExecuteTick = Find.TickManager.TicksGame + ticks,
            Action = action,
            Repeat = false
        });
    }

    public static ScheduledAction Interval(int interval, Action action) => Interval(interval, -1, _ => action());
    public static ScheduledAction Interval(int interval, Action<ScheduledActionData> action) => Interval(interval, -1, action);
    public static ScheduledAction Interval(int interval, int timeout, Action action) => Interval(interval, timeout, _ => action());
    public static ScheduledAction Interval(int interval, int timeout, Action<ScheduledActionData> action)
    {
        var currentTick = Find.TickManager.TicksGame;
        var a = new ScheduledAction
        {
            ExecuteTick = currentTick + interval,
            Interval = interval,
            EndTick = timeout > 0 ? (currentTick + timeout) : -1,
            Action = action,
            Repeat = true
        };

        _instance.InsertSorted(a);
        return a;
    }
}
