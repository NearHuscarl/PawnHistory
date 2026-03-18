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
            var buttonName = type.Name.Replace("Recorder", "");
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                // Check if method starts with "Test" and accepts exactly one TestScenario parameter
                if (method.Name.StartsWith("Test"))
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(TestScenario))
                    {
                        var label = (method.Name == "Test")
                            ? buttonName
                            : $"{buttonName}_{method.Name.ReplaceFirst("Test", "")}";

                        actionNodes.Add(new DebugActionNode(label, DebugActionType.Action, () =>
                        {
                            TestScenario.ClearAll();
                            try
                            {
                                method.Invoke(recorder, [testScenario]);
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"[PawnHistory] Failed to run recorder test {label}\n\n{ex}");
                            }
                        }));
                    }
                }
            }
        }

        return actionNodes;
    }
}
