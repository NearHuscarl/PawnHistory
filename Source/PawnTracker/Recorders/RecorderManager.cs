using LudeonTK;
using PawnHistory.Source.DebugTools;
using PawnHistory.Source.PawnTracker.Test;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public static class RecorderManager
{
    public static bool ShouldRecord(ThingDef thingDef) => thingDef.race?.intelligence == Intelligence.Humanlike;
    public static bool ShouldRecord(Pawn pawn) => pawn != null && pawn.RaceProps.Humanlike;

    private static readonly List<RecorderBase> activeRecorders = [];
    private static readonly TestScenario testScenario = new();

    public static void Initialize()
    {
        activeRecorders.Clear();

        foreach (var type in GenTypes.AllSubclassesNonAbstract(typeof(RecorderBase)))
        {
            var recorder = (RecorderBase)Activator.CreateInstance(type);
            
            recorder.Register();
            activeRecorders.Add(recorder);
        }
    }

    [NearDebugAction]
    public static List<DebugActionNode> RecorderTests()
    {
        var actionNodes = new List<DebugActionNode>();

        foreach (var recorder in activeRecorders)
        {
            var type = recorder.GetType();
            var testMethod = type.GetMethod("Test", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (testMethod != null)
            {
                actionNodes.Add(new DebugActionNode(type.Name.Replace("Recorder", ""), DebugActionType.Action, () =>
                {
                    TestScenario.ClearAll();
                    recorder.Test(testScenario);
                }));
            }
        }

        return actionNodes;
    }
}
