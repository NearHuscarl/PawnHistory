using RimWorld;
using System;
using System.Text;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public sealed class TestContext(string name)
{
    public string Name = name;
    public bool Failed;
    public int PendingEventually;
    private static string Red(object obj) => obj.ToString().ApplyTag(TagType.Red).Resolve();
    private static string Green(object obj) => obj.ToString().Colorize(ColoredText.FactionColor_Ally);

    public void Fail(params string[] message)
    {
        Fail(null, message);
    }

    public void Pass()
    {
        Log.Message(Green("[PASS] " + Name));
        Messages.Message("[PASS] " + Name, MessageTypeDefOf.PositiveEvent);
    }

    public void Fail(Exception ex = null, params string[] msgs)
    {
        Failed = true;

        var stringBuilder = new StringBuilder();

        for (var i = 0; i < msgs.Length; i++)
        {
            stringBuilder.AppendLine(msgs[i]);
        }

        if (ex != null)
            stringBuilder.Append("\n" + ex.ToString());

        Log.Error(Red("[FAIL] " + Name) + "\n" + stringBuilder);
    }
}
