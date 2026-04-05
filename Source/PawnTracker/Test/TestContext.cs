using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

internal sealed class TestContext(string name)
{
    public string Name = name;
    public int AssertionsPassed;
    public int AssertionsFailed;
    public int PendingEventually;
    public static string Red(object obj) => obj.ToString().ApplyTag(TagType.Red).Resolve();
    public static string Green(object obj) => obj.ToString().Colorize(ColoredText.FactionColor_Ally);

    public void Pass()
    {
        Log.Message(Green($"[PawnHistory] [Passed] {Name}: {AssertionsPassed} passed"));
        Messages.Message("[Passed] " + Name, MessageTypeDefOf.PositiveEvent);
    }

    public void Fail(params string[] message)
    {
        Fail(null, message);
    }

    public void Fail(Exception ex = null, params string[] msgs)
    {
        var stringBuilder = new StringBuilder();

        for (var i = 0; i < msgs.Length; i++)
        {
            stringBuilder.AppendLine(msgs[i]);
        }

        if (ex != null)
            stringBuilder.Append("\n" + ex.ToString());

        throw new Exception("[PawnHistory] [Failed] " + Name + "\n" + stringBuilder);
    }

    private readonly List<Action> cleanupCallbacks = [];

    public void OnCleanup(Action callback)
    {
        cleanupCallbacks.Add(callback);
    }

    public void Cleanup()
    {
        cleanupCallbacks.ForEach(c => c());
        cleanupCallbacks.Clear();
    }
}
