using System;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source;

public class TickDelayManager(Game game) : GameComponent
{
    private readonly List<(int tick, Action action)> queue = [];

    public static void Delay(int ticks, Action action)
    {
        var comp = Current.Game.GetComponent<TickDelayManager>();
        comp.queue.Add((Find.TickManager.TicksGame + ticks, action));
    }

    public override void GameComponentTick()
    {
        var current = Find.TickManager.TicksGame;

        for (var i = queue.Count - 1; i >= 0; i--)
        {
            if (queue[i].tick <= current)
            {
                queue[i].action?.Invoke();
                queue.RemoveAt(i);
            }
        }
    }
}
