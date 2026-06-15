using LudeonTK;
using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PawnHistory.Source.PawnTracker.HistoryBackfill;
using PawnHistory.Source.PawnTracker.Recorders;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public static class RecorderManager
{
    public static bool ShouldRecord(ThingDef thingDef) => thingDef.race?.intelligence >= Intelligence.Animal;
    public static bool ShouldRecord(Pawn pawn)
    {
        return pawn != null && (pawn.RaceProps.Humanlike || (pawn.RaceProps.Animal && pawn.relations?.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) != null));
    }

    private static readonly List<RecorderBase> ActiveRecorders = [];

    public static void Initialize()
    {
        ActiveRecorders.Clear();
        HistoryBackfillRegistry.Clear();
        
        var compTypes = GenTypes.AllTypes
            .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IRecordComp).IsAssignableFrom(t))
            .ToList();
        
        foreach (var recorderType in typeof(RecorderBase).AllSubclassesNonAbstract())
        {
            var recorder = (RecorderBase)Activator.CreateInstance(recorderType);

            foreach (var compType in compTypes)
            {
                var comp = (IRecordComp)Activator.CreateInstance(compType);
                if (comp.RecorderType == recorderType)
                    recorder.AddComp(comp);
            }
            
            HistoryBackfillRegistry.Register(recorder.GetBackfillDefinitions());
            recorder.Register();
            ActiveRecorders.Add(recorder);
        }
    }

    [NearDebugOutput]
    public static void ListRecorderTests()
    {
        var dataSource = ScanTestMethods();
        var testReportEntries = TestReportManager.LastReport?.Entries.ToDictionary(x => x.TestId, x => x) ?? [];
        var totalIds = dataSource.Select(x => x.Id).Distinct().Count();
        var totalSkip = dataSource.Count(x => x.Attributes.SkipTest);
        var totalFailed = dataSource.Count(x => testReportEntries.TryGetValue(x.Id)?.TestFailures.Any() ?? false);

        DebugTables.MakeTablesDialog(dataSource,
            new TableDataGetter<TestMethodInfo>($"Id ({totalIds})", d => d.Id),
            new TableDataGetter<TestMethodInfo>("Requires", d => d.Attributes.ModActiveById.Select(r => r.Key).JoinToString()),
            new TableDataGetter<TestMethodInfo>($"Skip ({totalSkip})", d => d.Attributes.SkipTest.ToStringCheckBlank()),
            new TableDataGetter<TestMethodInfo>("DebugValues", d => d.Attributes.DebugValues.JoinToString()),
            new TableDataGetter<TestMethodInfo>("Tags", d => d.Attributes.Tags?.JoinToString() ?? ""),
            new TableDataGetter<TestMethodInfo>("Passed/Total", d =>
            {
                var entry = testReportEntries.TryGetValue(d.Id);
                return entry == null ? "?/?" : $"{entry.AssertionsPassed}/{entry.AssertionsPassed + entry.TestFailures.Count}";
            }),
            new TableDataGetter<TestMethodInfo>($"Failed Message ({totalFailed})", d =>
            {
                var entry = testReportEntries.TryGetValue(d.Id);
                return entry?.TestFailures.FirstOrDefault()?.message;
            })
        );
    }

    [NearDebugAction]
    public static void RunAllTests()
    {
        var methodInfos = ScanTestMethods()
            .Where(t => t.Attributes.DebugValues == null && !t.Attributes.SkipTest)
            .ToList();
        
        foreach (var t in methodInfos)
        {
            TestManager.EnqueueTest(t.Id, () => InvokeTest(t), debugMapSize: t.Attributes.DebugMapSize);
        }
        TestManager.Run();
    }

    [NearDebugAction]
    public static List<DebugActionNode> RunTaggedTests()
    {
        var actionNodes = new List<DebugActionNode>();
        var testMethodInfos = ScanTestMethods();
        var userDefinedTags = testMethodInfos.SelectMany(t => t.Attributes.Tags)
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct()
            .OrderBy(t => t);
        var dlcTags = testMethodInfos.SelectMany(t => t.Attributes.ModActiveById.Keys).Distinct();
        List<string> allTags = [..userDefinedTags, ..dlcTags];

        foreach (var tag in allTags)
        {
            var taggedMethodInfos = testMethodInfos
                .Where(t => t.Attributes.DebugValues == null && !t.Attributes.SkipTest && (t.Attributes.Tags.Contains(tag) || t.Attributes.ModActiveById.GetValueOrDefault(tag)))
                .ToList();
            actionNodes.Add(new DebugActionNode($"{tag} ({taggedMethodInfos.Count})", DebugActionType.Action, () =>
            {
                foreach (var t in taggedMethodInfos)
                {
                    TestManager.EnqueueTest(t.Id, () => InvokeTest(t), debugMapSize: t.Attributes.DebugMapSize);
                }
                TestManager.Run();
            }));
        }

        return actionNodes;
    }

    [NearDebugAction]
    public static void RunLastFailedTests()
    {
        var lastRunReport = TestReportManager.LastReport;
        if (lastRunReport == null)
            return;
        
        var failedTests = new HashSet<string>(lastRunReport.Entries.Where(x => x.TestFailures.Count > 0).Select(x => x.TestId));
        var methodInfos = ScanTestMethods()
            .Where(t => t.Attributes.DebugValues == null && !t.Attributes.SkipTest && failedTests.Contains(t.Id))
            .ToList();
        
        foreach (var t in methodInfos)
        {
            TestManager.EnqueueTest(t.Id, () => InvokeTest(t), debugMapSize: t.Attributes.DebugMapSize);
        }
        TestManager.Run();
    }

    [NearDebugAction]
    public static List<DebugActionNode> RunSpecificTest()
    {
        var actionNodes = new List<DebugActionNode>();

        foreach (var testMethodInfo in ScanTestMethods())
        {
            var (id, label, _, method, attributes) = testMethodInfo;
            var parameters = method.GetParameters();

            if (attributes.DebugValues == null)
            {
                actionNodes.Add(new DebugActionNode(label, DebugActionType.Action, () =>
                {
                    try
                    {
                        TestManager.EnqueueTest(id, () => InvokeTest(testMethodInfo), true, attributes.DebugMapSize);
                        TestManager.Run();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[PawnHistory] Failed to run recorder test {id}\n\n{ex}");
                    }
                }));
            }
            else
            {
                var parentNode = new DebugActionNode(label, DebugActionType.Action, () =>
                {
                    var options = new List<DebugMenuOption>();

                    foreach (var count in attributes.DebugValues)
                    {
                        options.Add(new DebugMenuOption($"{parameters[1].Name}: {count}", DebugMenuOptionMode.Action, () =>
                        {
                            try
                            {
                                TestManager.EnqueueTest(id, () => InvokeTest(testMethodInfo, [count]), true, attributes.DebugMapSize);
                                TestManager.Run();
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"[PawnHistory] Failed to run recorder test {id}\n\n{ex}");
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

    [NearDebugAction]
    public static void StopTestRun()
    {
        TestManager.StopTestRun();
    }

    private record TestAttributes(int[] DebugValues, bool SkipTest, int? DebugMapSize, HashSet<string> Tags, Dictionary<string, bool> ModActiveById);
    private record TestMethodInfo(string Id, string Label, object Target, MethodInfo Method, TestAttributes Attributes);
    
    private static string GetTestId(string ownerName, string targetName, string methodName)
    {
        var suffix = methodName == "Test"
            ? null
            : methodName.ReplaceFirst("Test", "");

        var baseId = targetName.NullOrEmpty()
            ? ownerName
            : $"{ownerName}.{targetName}";

        return suffix.NullOrEmpty()
            ? baseId
            : $"{baseId}_{suffix}";
    }
    
    private static IEnumerable<TestMethodInfo> ScanTestMethodsForTarget(object target, string ownerName, string targetName)
    {
        var type = target.GetType();
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (var method in methods)
        {
            if (!method.Name.StartsWith("Test"))
                continue;

            var parameters = method.GetParameters();
            int[] debugValues = null;
            // CASE 1: Standard Test(TestScenario scenario)
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(TestScenario))
            {
            }
            // CASE 2: Parameterized Test(TestScenario scenario, int count)
            else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(TestScenario) && parameters[1].ParameterType == typeof(int))
            {
                var attr = method.GetCustomAttribute<DebugValuesAttribute>() ?? parameters[1].GetCustomAttribute<DebugValuesAttribute>();
                debugValues = attr?.Values ?? [1, 2, 3, 5, 10];
            }
            else
            {
                throw new ArgumentException($"Unsupported test method signature for {type.Name}.{method.Name}. Expected either {method.Name}(TestScenario) or {method.Name}(TestScenario, int).");
            }

            var id = GetTestId(ownerName, targetName, method.Name);
            var requires = method.GetCustomAttributes<RequiresAttribute>().ToList();
            var requiredMods = requires.Select(r => r.ModName).JoinToString();
            var label = requiredMods.NullOrEmpty() ? id : $"{id} [{requiredMods}]";

            var skipTest = method.GetCustomAttribute<SkipTestAttribute>();
            var debugMapSize = method.GetCustomAttribute<DebugMapSizeAttribute>()?.Size;
            var tags = method.GetCustomAttributes<TestTagAttribute>().Select(a => a.Tag).ToHashSet();
            var modActiveById = requires.ToDictionary(a => a.ModId, a => a.IsActive);
            var testAttributes = new TestAttributes(debugValues, skipTest != null, debugMapSize, tags, modActiveById);

            yield return new TestMethodInfo(id, label, target, method, testAttributes);
        }
    }
    
    private static List<TestMethodInfo> ScanTestMethods()
    {
        var testMethods = new List<TestMethodInfo>();

        foreach (var recorder in ActiveRecorders)
        {
            var recordType = recorder.GetType();
            var recorderName = recordType.Name.Replace("Recorder", "");

            // Recorder methods
            testMethods.AddRange(ScanTestMethodsForTarget(
                target: recorder,
                ownerName: recorderName,
                targetName: null
            ));

            // Comp methods
            foreach (var comp in recorder.Comps)
            {
                var parts = comp.GetType().Name.Split("_");
                var compName = parts.Last();

                testMethods.AddRange(ScanTestMethodsForTarget(
                    target: comp,
                    ownerName: recorderName,
                    targetName: compName
                ));
            }
        }

        return testMethods;
    }

    private static object InvokeTest(TestMethodInfo info, object[] parameters = null)
    {
        var (_, _, target, method, attributes) = info;

        foreach (var kv in attributes.ModActiveById)
        {
            if (kv.Value) continue;
            Log.Warning($"[PawnHistory] Skipping '{info.Id}' test because a required mod '{kv.Key}' is not active.");
            return null;
        }
        
        if (parameters == null)
            return method.Invoke(target, [TestManager.Scenario]);
        
        return method.Invoke(target, [TestManager.Scenario, ..parameters]);
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
