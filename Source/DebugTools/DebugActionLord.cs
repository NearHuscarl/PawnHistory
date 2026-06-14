using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.DebugTools;

public static class DebugActionLord
{
    [NearDebugAction(DebugActionType.ToolMapForPawns)]
    public static void DisplayLordGraph(Pawn pawn)
    {
        if (pawn?.lord == null)
        {
            Log.Error("Please select a pawn that has an assigned lord (raider, caravan trader, refugee...)");
            return;
        }

        var graphviz = ExportGraphviz(pawn.lord);

        Log.Message(graphviz);
        GUIUtility.systemCopyBuffer = graphviz;
        Messages.Message("Lord graph is copied to clipboard.", MessageTypeDefOf.NeutralEvent);
        DebugActionsUtility.DustPuffFrom(pawn);
    }

    public static string ExportGraphviz(Lord lord)
    {
        if (lord?.Graph == null)
            return "digraph G {}";

        var graph = lord.Graph;
        var sb = new StringBuilder();

        sb.AppendLine("digraph LordGraph {");
        sb.AppendLine("rankdir=LR;");
        sb.AppendLine("node [shape=box];");

        // nodes
        foreach (var toil in graph.lordToils)
        {
            var name = ToilName(toil);
            if (toil == graph.StartingToil)
                sb.AppendLine($"\"{name}\" [shape=box style=filled fillcolor=lightgreen];");
            else
                sb.AppendLine($"\"{name}\";");
        }

        var existingEdges = new HashSet<string>();

        // edges
        foreach (var transition in graph.transitions)
        {
            var dst = ToilName(transition.target);
            var triggers = transition.triggers?.Select(TriggerName);
            var triggerLabel = string.Join("\\n", triggers);

            foreach (var srcToil in transition.sources)
            {
                var src = ToilName(srcToil);
                var edgeId = $"{src}_{string.Join("|", triggers)}_{dst}";
                if (existingEdges.Contains(edgeId))
                    continue;
                existingEdges.Add(edgeId);
                sb.AppendLine($"\"{src}\" -> \"{dst}\" [label=\"{triggerLabel}\"];");
            }
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string TriggerName(Trigger trigger)
    {
        var type = trigger.GetType();
        var name = type.Name.ReplaceFirst("Trigger_", "");
        var ctor = type.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (ctor == null)
            return name;

        static string FormatArg(object val)
        {
            if (val == null)
                return "null";

            if (val is Delegate)
                return "[Delegate]";

            if (val is float f)
                return f.ToString("0.0");

            if (val is double d)
                return d.ToString("0.0");

            return val.ToString();
        }
        var args = new List<string>();

        foreach (var param in ctor.GetParameters())
        {
            var field = type.GetField(param.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field == null)
                continue;

            var val = field.GetValue(trigger);
            args.Add(FormatArg(val));
        }

        if (args.Count == 0)
            return name;

        return $"{name}({string.Join(", ", args)})";
    }

    private static string ToilName(LordToil toil) => toil.GetType().Name.ReplaceFirst("LordToil_", "");
}
