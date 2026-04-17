using PawnHistory.Source.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public enum MatchCondition
{
    Any,
    All,
}

public sealed class PawnHistoryAssertions(IEnumerable<Pawn> pawns, MatchCondition matchCondition = MatchCondition.Any)
{
    private readonly List<Pawn> pawns = pawns?.ToList() ?? throw new ArgumentNullException(nameof(pawns));

    private bool isEventually;
    private int eventuallyTimeoutTicks;
    private int eventuallyPollIntervalTicks;
    private bool negate = false;

    public PawnHistoryAssertions Not()
    {
        negate = !negate;
        return this;
    }

    private void Assert<T>(T expected, T actual, string positiveMessage, string negativeMessage, Dictionary<string, string> testParams = null, Func<T, T, bool> comparator = null)
    {
        var result = comparator?.Invoke(expected, actual) ?? EqualityComparer<T>.Default.Equals(expected, actual);
        var comparatorFn = comparator?.Method.Name ?? $"{typeof(T).Name}.Equals";
        var finalParams = testParams != null
            ? new Dictionary<string, string>(testParams)
            : new Dictionary<string, string>();

        if (negate ? !result : result)
            return;
        
        finalParams.Add("comparatorFn", comparatorFn);
        
        var ctx = TestManager.Ctx;
        var message = negate ? negativeMessage : positiveMessage;
        var failure = new TestAssertionFailure(
            ctx.Name,
            message,
            expected?.ToString() ?? "null",
            actual?.ToString() ?? "null",
            negate,
            finalParams
        );
        throw new TestException(failure);
    }

    private record AssertionData<T>(T Actual, string PositiveMessage, string NegativeMessage);
    private void AssertCollection<T>(T expected, Func<Pawn, AssertionData<T>> getAssertionData, Dictionary<string, string> testParams = null, Func<T, T, bool> comparator = null)
    {
        if (matchCondition == MatchCondition.All)
        {
            foreach (var pawn in pawns)
            {
                var (actual, positiveMessage, negativeMessage) = getAssertionData(pawn);
                Assert(expected, actual, positiveMessage, negativeMessage, testParams, comparator);
            }
        }

        if (matchCondition == MatchCondition.Any)
        {
            Exception lastException = null;
            foreach (var pawn in pawns)
            {
                var (actual, positiveMessage, negativeMessage) = getAssertionData(pawn);

                try
                {
                    Assert(expected, actual, positiveMessage, negativeMessage, testParams, comparator);
                    return;
                }
                catch (TestException ex)
                {
                    lastException = ex;
                }
            }

            if (lastException != null)
                throw lastException;
        }
    }

    public void ToHaveHistoryRecordOf(HistoryRecordDef def, int index = -1)
    {
        RunAssertion(() =>
        {
            var testParams =  new Dictionary<string, string> 
            { 
                { "index", index.ToString() } 
            };

            AssertCollection(
                def,
                pawn => new AssertionData<HistoryRecordDef>(
                    !pawn.HistoryRecords.TryAt(index, out var record) ? null : record.def,
                    $"Expect HistoryRecordDef to exist for {pawn} {ToString(testParams)}.\nExpected:\n{def}\nActual:\n{record?.def}.",
                    $"Expect HistoryRecordDef NOT to exist for {pawn} {ToString(testParams)}.\nExpected:\n{def}\nActual:\n{record?.def}."
                    ),
                testParams
                );
        });
    }

    public void ToHaveHistoryRecordCount(int expected)
    {
        RunAssertion(() =>
        {
            AssertCollection(
                expected,
                pawn => new AssertionData<int>(
                    pawn.HistoryRecords.Count,
                    $"Expect correct number of HistoryRecord for {pawn}.\nExpected:\n{expected}\nActual:\n{pawn.HistoryRecords.Count}.",
                    $"Expect HistoryRecordDef NOT to match for {pawn}.\nExpected:\n{expected}\nActual:\n{pawn.HistoryRecords.Count}."
                )
            );
        });
    }

    private static string ToString(Dictionary<string, string> testParams)
    {
        return "[" + testParams.Select(p => $"{p.Key}={p.Value}").JoinToString() + "]";
    }

    public void ToHaveHistoryRecord(string descriptionTemplate, int index, bool exactMatch = false)
    {
        RunAssertion(() =>
        {
            var testParams =  new Dictionary<string, string> 
            { 
                { "index", index.ToString() },
                { "exactMatch", exactMatch.ToString() },
            };
            
            AssertCollection(
                descriptionTemplate,
                pawn =>
                {
                    pawn.HistoryRecords.TryAt(index, out var record);
                    var actual = record?.description.StripTags();
                    
                    return new AssertionData<string>(
                        actual,
                        $"Expect description to match template {ToString(testParams)}\nExpected template:\n{descriptionTemplate}\nActual resolved description:\n{actual}",
                        $"Expect description NOT to match template {ToString(testParams)}\nExpected template:\n{descriptionTemplate}\nActual resolved description:\n{actual}."
                        );
                },
                testParams, 
                (a, b) => LangUtility.IsStructurallyTheSame(a, b, exactMatch)
            );
        });
    }

    public void ToHaveHistoryRecord(string descriptionTemplate, HistoryRecordDef recordDef = null, bool exactMatch = false, int ticksAgo = 0)
    {
        RunAssertion(() =>
        {
            var testParams =  new Dictionary<string, string> 
            { 
                { "recordDef", recordDef?.ToString() ?? "null" },
                { "exactMatch", exactMatch.ToString() },
                { "ticksAgo", ticksAgo.ToString() },
            };
            
            AssertCollection(
                descriptionTemplate,
                pawn =>
                {
                    var record = pawn.HistoryRecords.LastOrDefault(r => (recordDef == null || r.def == recordDef) && r.date >= Find.TickManager.TicksGame - ticksAgo);
                    var actual = record?.description.StripTags();
                    
                    return new AssertionData<string>(
                        actual,
                        $"Expect description to match template {ToString(testParams)}\nExpected template:\n{descriptionTemplate}\nActual resolved description:\n{actual}",
                        $"Expect description NOT to match template {ToString(testParams)}\nExpected template:\n{descriptionTemplate}\nActual resolved description:\n{actual}."
                    );
                },
                testParams, 
                (a, b) => LangUtility.IsStructurallyTheSame(a, b, exactMatch)
            );
        });
    }

    public void ToHaveHistoryRecordPosition(IntVec3 position, HistoryRecordDef recordDef, int ticksAgo = 0)
    {
        RunAssertion(() =>
        {
            var testParams =  new Dictionary<string, string> 
            { 
                { "recordDef", recordDef?.ToString() ?? "null" },
                { "ticksAgo", ticksAgo.ToString() },
            };
            
            AssertCollection(
                position,
                pawn =>
                {
                    var record = pawn.HistoryRecords.LastOrDefault(r => (recordDef == null || r.def == recordDef) && r.date >= Find.TickManager.TicksGame - ticksAgo);
                    var actual = record?.location?.position;
                    
                    return new AssertionData<IntVec3?>(
                        actual,
                        $"Expect position to match for {pawn} {ToString(testParams)}\nExpected:\n{position}\nActual:\n{actual}",
                        $"Expect position NOT to match for {pawn} {ToString(testParams)}\nExpected:\n{position}\nActual:\n{actual}."
                    );
                },
                testParams
            );
        });
    }

    public void ToHaveHistoryRecordConcern(Thing concern, HistoryRecordDef recordDef, int ticksAgo = 0)
    {
        RunAssertion(() =>
        {
            var testParams = new Dictionary<string, string>
            {
                { "recordDef", recordDef?.ToString() ?? "null" },
                { "ticksAgo", ticksAgo.ToString() },
            };

            AssertCollection(
                concern,
                pawn =>
                {
                    var record = pawn.HistoryRecords.LastOrDefault(r => (recordDef == null || r.def == recordDef) && r.date >= Find.TickManager.TicksGame - ticksAgo);
                    var actual = record?.concerns?.FirstOrDefault(c => c == concern);

                    return new AssertionData<Thing>(
                        actual,
                        $"Expect concern to match for {pawn} {ToString(testParams)}\nExpected:\n{concern}\nActual:\n{actual}",
                        $"Expect concern NOT to match for {pawn} {ToString(testParams)}\nExpected:\n{concern}\nActual:\n{actual}."
                    );
                },
                testParams
            );
        });
    }

    public PawnHistoryAssertions Eventually(int timeoutTicks = 3000, int pollIntervalTicks = 25)
    {
        isEventually = true;
        eventuallyTimeoutTicks = timeoutTicks;
        eventuallyPollIntervalTicks = pollIntervalTicks;
        return this;
    }

    private void RunAssertion(Action assertion)
    {
        TestManager.Ctx.PendingEventually++;
        // don't run immediately, so Test method can return cleanup action even if synchronous test call failed.
        TickDelayManager.Delay(0, () => DoRunAssertion(assertion));
    }

    private void DoRunAssertion(Action assertion)
    {
        var ctx = TestManager.Ctx;
        var tickStart = Find.TickManager.TicksGame;
        Exception lastException = null;

        var action = TickDelayManager.Interval(eventuallyPollIntervalTicks, (a) =>
        {
            if (!isEventually)
            {
                try
                {
                    assertion();
                    ctx.Pass();
                }
                catch (Exception ex)
                {
                    ctx.Fail(ex);
                } 
                a.Cancelled = true;
                return;
            }

            if (Find.TickManager.TicksGame - tickStart > eventuallyTimeoutTicks)
            { 
                var failure = new TimeoutFailure(ctx.Name, $"Test assertion failed after {eventuallyTimeoutTicks} ticks.");
                ctx.Fail(new TestException(failure, lastException));
                a.Cancelled = true;
            }

            try
            {
                assertion();
                ctx.Pass();
                a.Cancelled = true;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        });

        ctx.OnCleanup(() => action.Data.Cancelled = true);
    }
}
