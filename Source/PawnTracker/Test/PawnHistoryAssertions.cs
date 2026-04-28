using PawnHistory.Source.Helper;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.DebugTools;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public record ExpectedHistoryRecord
{
    public HistoryRecordDef Def { get; init; }
    public int? Date { get; init; }
    public Pawn Pawn { get; init; }
    public string Description { get; init; }
    public List<Thing> Concerns { get; init; }
    public int? TileId { get; init; }
    public RecordLocation Location { get; init; }
    public IntVec3? Position { get; init; }
    public Map Map { get; init; }
    public Quest Quest { get; init; }

    public ExpectedHistoryRecord With(ExpectedHistoryRecord other)
    {
        return new ExpectedHistoryRecord
        {
            Def = other.Def ?? Def,
            Date = other.Date ?? Date,
            Pawn = other.Pawn ?? Pawn,
            Description = other.Description ?? Description,
            Concerns = other.Concerns ?? Concerns,
            TileId = other.TileId ?? TileId,
            Location = other.Location ?? Location,
            Position = other.Position ?? Position,
            Map = other.Map ?? Map,
            Quest = other.Quest ?? Quest,
        };
    }
}

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

    private void Assert<T>(T expected, T actual, string positiveMessage, string negativeMessage, Func<T, T, bool> comparator = null)
    {
        var result = comparator?.Invoke(expected, actual) ?? EqualityComparer<T>.Default.Equals(expected, actual);

        if (negate ? !result : result)
            return;
        
        var ctx = TestManager.Ctx;
        var message = negate ? negativeMessage : positiveMessage;
        var failure = new TestAssertionFailure(
            ctx.TestId,
            message,
            expected?.ToString() ?? "null",
            actual?.ToString() ?? "null",
            negate
        );
        throw new TestException(failure);
    }

    private record AssertionData<T>(T Actual, string PositiveMessage, string NegativeMessage);
    private record HistoryRecordMatch(HistoryRecord Record, List<string> MismatchedFields)
    {
        public bool Matches => MismatchedFields.Count == 0;
    }
    private void AssertCollection<T>(T expected, Func<Pawn, AssertionData<T>> getAssertionData, Func<T, T, bool> comparator = null)
    {
        if (matchCondition == MatchCondition.All)
        {
            foreach (var pawn in pawns)
            {
                var (actual, positiveMessage, negativeMessage) = getAssertionData(pawn);
                Assert(expected, actual, positiveMessage, negativeMessage, comparator);
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
                    Assert(expected, actual, positiveMessage, negativeMessage, comparator);
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
            var testParams = DebugUtility.FormatDict(new Dictionary<string, string>
            {
                { "index", index.ToString() },
            });

            AssertCollection(
                def,
                pawn => new AssertionData<HistoryRecordDef>(
                    !pawn.HistoryRecords.TryAt(index, out var record) ? null : record.def,
                    $"Expect HistoryRecordDef to exist for {pawn} {testParams}.\nExpected:\n{def}\nActual:\n{record?.def}.",
                    $"Expect HistoryRecordDef NOT to exist for {pawn} {testParams}.\nExpected:\n{def}\nActual:\n{record?.def}."
                    )
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
    
    public void ToHaveHistoryRecord(string descriptionTemplate, HistoryRecordDef recordDef, bool exactMatch = false, int index = -1)
    {
        ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = recordDef,
            Description = descriptionTemplate
        });
    }
    public void ToHaveHistoryRecord(HistoryRecordDef recordDef, string descriptionTemplate, bool exactMatch = false, int index = -1)
    {
        ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = recordDef,
            Description = descriptionTemplate
        });
    }

    public void ToHaveHistoryRecord(ExpectedHistoryRecord expected, bool exactMatch = false, int? index = null)
    {
        RunAssertion(() =>
        {
            var testParams = DebugUtility.FormatDict(new Dictionary<string, string>
            {
                { "exactMatch", exactMatch.ToString() },
                { "index", index.ToString() },
            });
            
            AssertCollection(
                true,
                pawn =>
                {
                    var match = BestHistoryRecordMatch(pawn, expected, index);
                    var actual = match.Record;
                    var expectedSummary = FormatExpectedHistoryRecord(expected);
                    var actualSummary = FormatActualHistoryRecord(actual);
                    var mismatchedFields = match.MismatchedFields.JoinToString();

                    return new AssertionData<bool>(
                        match.Matches,
                        $"Expect HistoryRecord to match for {pawn} {testParams}.\nMismatched fields: {mismatchedFields}\nExpected:\n{expectedSummary}\nActual:\n{actualSummary}",
                        $"Expect HistoryRecord NOT to match for {pawn} {testParams}.\nMatched record:\n{actualSummary}"
                    );
                }
            );
        });
    }

    private static HistoryRecordMatch BestHistoryRecordMatch(Pawn pawn, ExpectedHistoryRecord expected, int? index = null)
    {
        if (pawn.HistoryRecords.Count == 0)
            return new HistoryRecordMatch(null, GetMismatchedHistoryRecordFields(expected, null));

        if (index.HasValue && pawn.HistoryRecords.TryAt(index.Value, out var record))
            return new HistoryRecordMatch(record, GetMismatchedHistoryRecordFields(expected, record));

        return pawn.HistoryRecords
            .Select(r => new HistoryRecordMatch(r, GetMismatchedHistoryRecordFields(expected, r)))
            .OrderByDescending(match => match.Matches)
            .ThenBy(match => match.MismatchedFields.Count)
            .ThenByDescending(match => match.Record.date)
            .FirstOrDefault() ?? new HistoryRecordMatch(pawn.HistoryRecords.LastOrDefault(), GetMismatchedHistoryRecordFields(expected, pawn.HistoryRecords.LastOrDefault()));
    }

    private static List<string> GetMismatchedHistoryRecordFields(ExpectedHistoryRecord expected, HistoryRecord actual)
    {
        if (expected == null)
            return [nameof(ExpectedHistoryRecord)];
        if (actual == null)
            return [nameof(HistoryRecord)];

        var mismatchedFields = new List<string>();
        if (expected.Def != null && actual.def != expected.Def)
            mismatchedFields.Add(nameof(expected.Def));
        if (expected.Date.HasValue && actual.date != expected.Date.Value)
            mismatchedFields.Add(nameof(expected.Date));
        if (expected.Pawn != null && actual.pawn != expected.Pawn)
            mismatchedFields.Add(nameof(expected.Pawn));
        if (expected.Description != null && !LangUtility.IsStructurallyTheSame(expected.Description, actual.description?.StripTags(), false))
            mismatchedFields.Add(nameof(expected.Description));
        if (expected.Concerns != null && !ThingListsEqual(expected.Concerns, actual.concerns))
            mismatchedFields.Add(nameof(expected.Concerns));
        if (expected.TileId.HasValue && actual.tileId != expected.TileId.Value)
            mismatchedFields.Add(nameof(expected.TileId));
        if (expected.Location != null && !LocationsEqual(expected.Location, actual.location))
            mismatchedFields.Add(nameof(expected.Location));
        if (expected.Position.HasValue && actual.location?.position != expected.Position.Value)
            mismatchedFields.Add(nameof(expected.Position));
        if (expected.Map != null && actual.location?.map != expected.Map)
            mismatchedFields.Add(nameof(expected.Map));
        if (expected.Quest != null && actual.quest?.id != expected.Quest.id)
            mismatchedFields.Add(nameof(expected.Quest));

        return mismatchedFields;
    }

    private static bool ThingListsEqual(List<Thing> expected, List<Thing> actual)
    {
        if (actual == null)
            return expected.Count == 0;

        return expected.SetsEqual(actual);
    }

    private static bool LocationsEqual(RecordLocation expected, RecordLocation actual)
    {
        return actual != null
            && actual.position == expected.position
            && actual.map == expected.map;
    }

    private static string FormatExpectedHistoryRecord(ExpectedHistoryRecord expected)
    {
        if (expected == null)
            return "  <null ExpectedHistoryRecord>";

        return new List<string>
        {
            $"  Def: {FormatExpectedValue(expected.Def)}",
            $"  Date: {FormatExpectedValue(expected.Date)}",
            $"  Pawn: {FormatExpectedValue(expected.Pawn)}",
            $"  Description: {FormatExpectedValue(expected.Description)}",
            $"  Concerns: {FormatExpectedThingList(expected.Concerns)}",
            $"  TileId: {FormatExpectedValue(expected.TileId)}",
            $"  Location: {FormatExpectedLocation(expected.Location)}",
            $"  Position: {FormatExpectedValue(expected.Position)}",
            $"  Map: {FormatExpectedValue(expected.Map)}",
            $"  Quest: {FormatExpectedQuest(expected.Quest)}",
        }.JoinToString("\n");
    }

    private static string FormatActualHistoryRecord(HistoryRecord actual)
    {
        if (actual == null)
            return "  <no history record>";

        return new List<string>
        {
            $"  Def: {FormatActualValue(actual.def)}",
            $"  Date: {FormatActualValue(actual.date)}",
            $"  Pawn: {FormatActualValue(actual.pawn)}",
            $"  Description: {FormatActualValue(actual.description?.StripTags())}",
            $"  Concerns: {FormatActualThingList(actual.concerns)}",
            $"  TileId: {FormatActualValue(actual.tileId)}",
            $"  Location: {FormatActualLocation(actual.location)}",
            $"  Position: {FormatActualValue(actual.location?.position)}",
            $"  Map: {FormatActualValue(actual.location?.map)}",
            $"  Quest: {FormatActualQuest(actual.quest)}",
        }.JoinToString("\n");
    }

    private static string FormatExpectedValue<T>(T value) => value?.ToString() ?? "<not asserted>";
    private static string FormatActualValue<T>(T value) => value?.ToString() ?? "null";
    private static string FormatExpectedThingList(List<Thing> things) => things == null ? "<not asserted>" : FormatThingList(things);
    private static string FormatActualThingList(List<Thing> things) => things == null ? "null" : FormatThingList(things);
    private static string FormatThingList(List<Thing> things) => "[" + things.Select(thing => thing?.ToString() ?? "null").JoinToString() + "]";
    private static string FormatExpectedLocation(RecordLocation location) => location == null ? "<not asserted>" : FormatLocation(location);
    private static string FormatActualLocation(RecordLocation location) => location == null ? "null" : FormatLocation(location);
    private static string FormatLocation(RecordLocation location) => $"position={location.position}, map={location.map}";
    private static string FormatExpectedQuest(Quest quest) => quest == null ? "<not asserted>" : FormatQuest(quest);
    private static string FormatActualQuest(Quest quest) => quest == null ? "null" : FormatQuest(quest);
    private static string FormatQuest(Quest quest) => $"{quest.name} ({quest.id})";

    public PawnHistoryAssertions Eventually(int timeoutTicks = 3000, int pollIntervalTicks = 25)
    {
        isEventually = true;
        eventuallyTimeoutTicks = timeoutTicks;
        eventuallyPollIntervalTicks = pollIntervalTicks;
        return this;
    }

    private void RunAssertion(Action assertion)
    {
        var ctx = TestManager.Ctx;
        if (!ctx.TryRegisterAssertion())
            return;
        
        if (pawns.Count == 0)
        {
            // TODO: test stacktrace
            ctx.Fail(new TestException(new TestExecutionFailure(ctx.TestId, "No pawns to assert.")));
            return;
        }

        ctx.PendingEventually++;
        // don't run immediately, so Test method can return cleanup action even if synchronous test call failed.
        TickDelayManager.Delay(0, () => DoRunAssertion(assertion));
    }

    private void DoRunAssertion(Action assertion)
    {
        var ctx = TestManager.Ctx;
        var tickStart = Find.TickManager.TicksGame;
        Exception lastException = null;

        var action = TickDelayManager.Interval(eventuallyPollIntervalTicks, a =>
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
                var failure = new TimeoutFailure(ctx.TestId, $"Test assertion failed after waiting for {eventuallyTimeoutTicks} ticks.");
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
