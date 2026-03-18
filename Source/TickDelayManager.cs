using System;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source;

#pragma warning disable CS9113 // Parameter is unread.
public class TickDelayManager(Game game) : GameComponent
#pragma warning restore CS9113 // Parameter is unread.
{
    public class ScheduledAction
    {
        public int ExecuteTick;
        public int Interval;
        public Action Action;
        public bool Repeat;
        public bool Cancelled;
    }

    private static readonly List<ScheduledAction> actions = [];

    public override void GameComponentTick()
    {
        var currentTick = Find.TickManager.TicksGame;

        for (var i = actions.Count - 1; i >= 0; i--)
        {
            var a = actions[i];

            if (a.Cancelled)
            {
                actions.RemoveAt(i);
                continue;
            }

            if (currentTick >= a.ExecuteTick)
            {
                a.Action?.Invoke();

                if (a.Repeat && !a.Cancelled)
                    a.ExecuteTick = currentTick + a.Interval;
                else
                    actions.RemoveAt(i);
            }
        }
    }

    public static void Delay(int ticks, Action action)
    {
        actions.Add(new ScheduledAction
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

        actions.Add(a);
        return a;
    }

    public static void Cancel(ScheduledAction action)
    {
        action?.Cancelled = true;
    }
}
