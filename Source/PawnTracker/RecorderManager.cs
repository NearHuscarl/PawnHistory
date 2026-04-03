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
                if (!method.Name.StartsWith("Test"))
                    continue;

                var parameters = method.GetParameters();
                var label = (method.Name == "Test")
                    ? buttonName
                    : $"{buttonName}_{method.Name.ReplaceFirst("Test", "")}";

                // CASE 1: Standard Test(TestScenario scenario)
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(TestScenario))
                {
                    actionNodes.Add(new DebugActionNode(label, DebugActionType.Action, () =>
                    {
                        try
                        {
                            TestScenario.ClearAll();
                            TestManager.Current = new TestContext(label);

                            method.Invoke(recorder, [testScenario]);

                            WaitForTestCompletion(TestManager.Current);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"[PawnHistory] Failed to run recorder test {label}\n\n{ex}");
                        }
                    }));
                }
                // CASE 2: Parameterized Test(TestScenario scenario, int count)
                else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(TestScenario) && parameters[1].ParameterType == typeof(int))
                {
                    var parentNode = new DebugActionNode(label, DebugActionType.Action, () =>
                    {
                        var attr = method.GetCustomAttribute<DebugValuesAttribute>()
                           ?? parameters[1].GetCustomAttribute<DebugValuesAttribute>();

                        int[] presets = attr?.Values ?? [1, 2, 3, 5, 10];
                        var options = new List<DebugMenuOption>();

                        foreach (int count in presets)
                        {
                            options.Add(new DebugMenuOption($"{parameters[1].Name}: {count}", DebugMenuOptionMode.Action, () =>
                            {
                                try
                                {
                                    TestScenario.ClearAll();
                                    TestManager.Current = new TestContext(label);
                                    method.Invoke(recorder, [testScenario, count]);

                                    WaitForTestCompletion(TestManager.Current);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error($"[PawnHistory] Failed to run recorder test {label}\n\n{ex}");
                                }
                            }));
                        }
                        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
                    });

                    actionNodes.Add(parentNode);
                }
            }
        }

        return actionNodes;
    }

    private static void WaitForTestCompletion(TestContext ctx)
    {
        var start = Find.TickManager.TicksGame;

        TickDelayManager.Interval(1, (a) =>
        {
            if (ctx.PendingEventually == 0)
            {
                if (!ctx.Failed)
                    ctx.Pass();
                a.Cancelled = true;

                return;
            }

            if (Find.TickManager.TicksGame - start > TestManager.Timeout)
            {
                ctx.Fail("Timeout waiting for test assertions.");
                a.Cancelled = true;
            }
        });
    }

    [NearDebugOutput]
    public static void RecorderLogs()
    {
        var options = new List<DebugMenuOption>();

        foreach (var recorder in activeRecorders)
        {
            var type = recorder.GetType();
            var recorderName = type.Name.Replace("Recorder", "");
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                if (!method.Name.StartsWith("Log"))
                    continue;

                var parameters = method.GetParameters();
                var labelSuffix = method.Name.Replace("Log", "");
                var label = $"{recorderName}_{labelSuffix}";

                if (parameters.Length == 0)
                {
                    options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () =>
                    {
                        try
                        {
                            method.Invoke(recorder, null);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"[PawnHistory] Failed to run log method {label}\n\n{ex}");
                        }
                    }));
                }
                else
                    Log.Warning($"[PawnHistory] Skipping {type.Name}.{method.Name} - unsupported parameters");
            }
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }
}
