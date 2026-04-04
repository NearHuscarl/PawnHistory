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
    public static void RunAllTests()
    {
        ForEachTestMethod((label, recorder, method, debugValues, skipTest) =>
        {
            if (debugValues != null)
                return;

            if (skipTest)
                return;

            TestManager.EnqueueTest(() => method.Invoke(recorder, [testScenario]), label);
        });

        TestManager.Run();
    }


    [NearDebugAction]
    public static void StopTestRun()
    {
        TestManager.StopTestRun();
    }

    [NearDebugAction]
    public static List<DebugActionNode> RecorderTests()
    {
        var actionNodes = new List<DebugActionNode>();

        ForEachTestMethod((label, recorder, method, debugValues, skipTest) =>
        {
            var parameters = method.GetParameters();

            if (debugValues == null)
            {
                actionNodes.Add(new DebugActionNode(label, DebugActionType.Action, () =>
                {
                    try
                    {
                        TestManager.ResetBeforeTest(label);
                        method.Invoke(recorder, [testScenario]);
                        TestManager.WaitForTestCompletion(TestManager.Ctx);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[PawnHistory] Failed to run recorder test {label}\n\n{ex}");
                    }
                }));
            }
            else
            {
                var parentNode = new DebugActionNode(label, DebugActionType.Action, () =>
                {
                    var options = new List<DebugMenuOption>();

                    foreach (int count in debugValues)
                    {
                        options.Add(new DebugMenuOption($"{parameters[1].Name}: {count}", DebugMenuOptionMode.Action, () =>
                        {
                            try
                            {
                                TestManager.ResetBeforeTest(label);
                                method.Invoke(recorder, [testScenario, count]);
                                TestManager.WaitForTestCompletion(TestManager.Ctx);
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
        });

        return actionNodes;
    }

    private static void ForEachTestMethod(Action<string, RecorderBase, MethodInfo, int[], bool> action)
    {
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
                int[] debugValues = null;

                // CASE 1: Standard Test(TestScenario scenario)
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(TestScenario))
                {
                }
                // CASE 2: Parameterized Test(TestScenario scenario, int count)
                else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(TestScenario) && parameters[1].ParameterType == typeof(int))
                {
                    var attr = method.GetCustomAttribute<DebugValuesAttribute>()
                       ?? parameters[1].GetCustomAttribute<DebugValuesAttribute>();

                    debugValues = attr?.Values ?? [1, 2, 3, 5, 10];
                }
                else
                {
                    throw new ArgumentException($"Unsupported test method signature for {type.Name}.{method.Name}. Expected either {method.Name}(TestScenario) or {method.Name}(TestScenario, int).");
                }

                var skipTest = method.GetCustomAttribute<SkipTestAttribute>();

                action(label, recorder, method, debugValues, skipTest != null);
            }
        }
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
