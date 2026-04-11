using LudeonTK;
using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PawnHistory.Source.PawnTracker.Recorders;
using UnityEngine.Profiling;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public static class RecorderManager
{
    public static bool ShouldRecord(ThingDef thingDef) => thingDef.race?.intelligence == Intelligence.Humanlike;
    public static bool ShouldRecord(Pawn pawn) => pawn != null && pawn.RaceProps.Humanlike;

    private static readonly List<RecorderBase> ActiveRecorders = [];
    private static readonly TestScenario TestScenario = new();

    public static void Initialize()
    {
        ActiveRecorders.Clear();

        foreach (var type in typeof(RecorderBase).AllSubclassesNonAbstract())
        {
            var recorder = (RecorderBase)Activator.CreateInstance(type);
            
            recorder.Register();
            ActiveRecorders.Add(recorder);
        }
    }

    [NearDebugOutput]
    public static void ListRecorderTests()
    {
        var dataSource = GetTestMethods();
        var totalLabels = dataSource.Select(x => x.Label).Distinct().Count();
        var totalSkip = dataSource.Count(x => x.SkipTest);

        DebugTables.MakeTablesDialog(dataSource,
            new TableDataGetter<TestMethodInfo>($"Label ({totalLabels})", d => d.Label),
            new TableDataGetter<TestMethodInfo>("Method", d => d.Method.Name),
            new TableDataGetter<TestMethodInfo>($"Skip ({totalSkip})", d => d.SkipTest.ToStringCheckBlank()),
            new TableDataGetter<TestMethodInfo>("DebugValues", d => d.DebugValues.JoinToString())
        );
    }

    [NearDebugAction]
    public static void RunAllTests()
    {
        foreach (var testMethodInfo in GetTestMethods())
        {
            var (label, recorder, method, debugValues, skipTest) = testMethodInfo;

            if (debugValues != null)
                continue;

            if (skipTest)
                continue;

            TestManager.EnqueueTest(() => method.Invoke(recorder, [TestScenario]), label);
        }
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

        foreach (var testMethodInfo in GetTestMethods())
        {
            var (label, recorder, method, debugValues, skipTest) = testMethodInfo;
            var parameters = method.GetParameters();

            if (debugValues == null)
            {
                actionNodes.Add(new DebugActionNode(label, DebugActionType.Action, () =>
                {
                    try
                    {
                        TestManager.ExecuteTestMethod(label, () => method.Invoke(recorder, [TestScenario]));
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

                    foreach (var count in debugValues)
                    {
                        options.Add(new DebugMenuOption($"{parameters[1].Name}: {count}", DebugMenuOptionMode.Action, () =>
                        {
                            try
                            {
                                TestManager.ExecuteTestMethod(label, () => method.Invoke(recorder, [TestScenario, count]));
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

        return actionNodes;
    }

    private record TestMethodInfo(string Label, RecorderBase Recorder, MethodInfo Method, int[] DebugValues, bool SkipTest);

    private static List<TestMethodInfo> GetTestMethods()
    {
        var testMethods = new List<TestMethodInfo>();

        foreach (var recorder in ActiveRecorders)
        {
            var type = recorder.GetType();
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

                testMethods.Add(new TestMethodInfo(label, recorder, method, debugValues, skipTest != null));
            }
        }

        return testMethods;
    }

    [NearDebugOutput]
    public static void RecorderLogs()
    {
        var options = new List<DebugMenuOption>();

        foreach (var recorder in ActiveRecorders)
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
