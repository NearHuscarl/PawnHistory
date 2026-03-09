using LudeonTK;
using PawnHistory.Source.PawnTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source;

/// <summary>
/// https://github.com/pardeike/Rimworld-Doorstop
/// </summary>
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method)]
public class ReloadableAttribute : Attribute { }

class NearDebugActionAttribute : DebugActionAttribute
{
    public NearDebugActionAttribute(DebugActionType actionType = DebugActionType.Action) : base(
            category: "Pawn History",
            name: null,
            requiresRoyalty: false,
            requiresIdeology: false,
            requiresBiotech: false,
            requiresAnomaly: false,
            requiresOdyssey: false,
            displayPriority: 0,
            hideInSubMenu: false
        )
    {
        this.actionType = actionType;
    }
}

class NearDebugOutputAttribute : DebugOutputAttribute
{
    public NearDebugOutputAttribute() : base(category: "Pawn History", true) { }
}

public static class DebugTools
{
    // Hot reload feature does not work with generic type
    // https://github.com/pardeike/Rimworld-Doorstop/issues/5
    public static Pawn[] AllPawns()
    {
        return Find.CurrentMap.mapPawns.AllPawns.ToArray();
    }
    public static Corpse[] AllCorpses()
    {
        return Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).Cast<Corpse>().ToArray();
    }

    [NearDebugOutput]
    public static void PawnHistoryRecords()
    {
        var options = new List<DebugMenuOption>();
        var alivePawns = Find.CurrentMap.mapPawns.AllPawns;
        var deadPawns = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).Cast<Corpse>().Select(c => c.InnerPawn);
        var pawns = alivePawns.Concat(deadPawns).Where(x => x.HasComp<CompHistory>()).OrderByDescending(x => CompHistoryManager.GetComp(x).records.Count);

        foreach (var pawn in pawns)
        {
            var compHistory = CompHistoryManager.GetComp(pawn);
            var label = $"{pawn.Name} ({compHistory.records.Count})";

            options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () =>
            {
                DebugTables.MakeTablesDialog(compHistory.records,
                    new TableDataGetter<HistoryRecord>("Timestamp", r => r.date),
                    new TableDataGetter<HistoryRecord>("Date", r => compHistory.GetShortDate(r)),
                    new TableDataGetter<HistoryRecord>("label", r => r.eventDef.label),
                    new TableDataGetter<HistoryRecord>("description", r => r.GetDescription()),
                    new TableDataGetter<HistoryRecord>("concerns", r => string.Join(", ", r.concerns.Select(c => c.NameShortColored))),
                    new TableDataGetter<HistoryRecord>("currentPawnToJumpTo", r => r.currentPawnToJumpTo)
                );
            }));
        }
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }

    [NearDebugAction(DebugActionType.ToolMapForPawns)]
    public static void DisplayLordGraph(Pawn pawn)
    {
        if (pawn?.lord == null)
        {
            Log.Error($"Please select a pawn that has an assigned lord (raider, caravan trader, refugee...)");
            return;
        }
        Log.Message(ExportGraphviz(pawn.lord));
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
            sb.AppendLine($"\"{ToilName(toil)}\";");

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
