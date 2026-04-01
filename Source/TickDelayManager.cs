using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source;

public class ScheduledAction
{
    public int ExecuteTick;
    public int Interval;
    public Action Action;
    public bool Repeat;
    public bool Cancelled;
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
            if (a.Cancelled)
            {
                toRemove.Add(a);
                continue;
            }

            if (currentTick >= a.ExecuteTick)
            {
                try
                {
                    a.Action?.Invoke();
                }
                catch (Exception e)
                {
                    Log.Error($"Error while executing action {a.Action}, {e}");
                }
                finally
                {
                    toRemove.Add(a);

                    if (a.Repeat && !a.Cancelled)
                    {
                        a.ExecuteTick = currentTick + a.Interval;
                        toReinsert.Add(a);
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

    public static void Delay(int ticks, Action action)
    {
        _instance.InsertSorted(new ScheduledAction
        {
            ExecuteTick = Find.TickManager.TicksGame + ticks,
            Action = action,
            Repeat = false
        });
    }

    public static ScheduledAction Interval(int interval, Action action)
    {
        var a = new ScheduledAction
        {
            ExecuteTick = Find.TickManager.TicksGame + interval,
            Interval = interval,
            Action = action,
            Repeat = true
        };

        _instance.InsertSorted(a);
        return a;
    }

    public static void Cancel(ScheduledAction action)
    {
        action?.Cancelled = true;
    }
}
