using PawnHistory.Source.Helper;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public List<Thing> ConcernAtLeast { get; init; }
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
            ConcernAtLeast = other.ConcernAtLeast ?? ConcernAtLeast,
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

    private void AssertPassed(AssertionResult assertionResult)
    {
        var (passed, positiveMessage, negativeMessage, expected, actual) = assertionResult;
        
        if (negate ? !passed : passed)
            return;
        
        var ctx = TestManager.Ctx;
        var message = negate ? negativeMessage : positiveMessage;
        var failure = new TestAssertionFailure(
            ctx.TestId,
            message,
            expected ?? "null",
            actual ?? "null",
            negate
        );
        throw new TestException(failure);
    }

    private record AssertionResult(bool Passed, string PositiveMessage, string NegativeMessage, string Expected, string Actual);
    private record HistoryRecordMatch(HistoryRecord Record, List<string> MismatchedFields)
    {
        public bool Matches => MismatchedFields.Count == 0;
    }
    private void AssertCollection(Func<Pawn, AssertionResult> getResult)
    {
        if (matchCondition == MatchCondition.All)
        {
            foreach (var pawn in pawns)
            {
                AssertPassed(getResult(pawn));
            }
        }

        if (matchCondition == MatchCondition.Any)
        {
            Exception lastException = null;
            foreach (var pawn in pawns)
            {
                try
                {
                    AssertPassed(getResult(pawn));
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

    public void ToHaveHistoryRecordOf(HistoryRecordDef def, int? index = null)
    {
        RunAssertion(() =>
        {
            var testParams = DebugUtility.FormatDict(new Dictionary<string, string>
            {
                { "index", index?.ToString() },
            });

            AssertCollection(
                pawn =>
                {
                    HistoryRecord record;
                    if (index.HasValue)
                        pawn.HistoryRecords.TryAt(index.Value, out record);
                    else
                        record = pawn.HistoryRecords.FirstOrDefault(r => r.def == def) ?? pawn.HistoryRecords.LastOrDefault();
                    
                    return new AssertionResult(
                        record?.def == def,
                        $"Expect HistoryRecordDef to exist for {pawn} {testParams}.",
                        $"Expect HistoryRecordDef NOT to exist for {pawn} {testParams}.",
                        def?.defName,
                        record?.def?.defName
                    );
                });
        });
    }

    public void ToHaveHistoryRecordCount(int expected)
    {
        RunAssertion(() =>
        {
            AssertCollection(
                pawn => new AssertionResult(
                    pawn.HistoryRecords.Count == expected,
                    $"Expect correct number of HistoryRecord for {pawn}.",
                    $"Expect HistoryRecordDef NOT to match for {pawn}.",
                    expected.ToString(),
                    pawn.HistoryRecords.Count.ToString()
                )
            );
        });
    }
    
    public void ToHaveHistoryRecord(HistoryRecordDef recordDef, string descriptionTemplate, bool exactMatch = false, int? index = null)
    {
        ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = recordDef,
            Description = descriptionTemplate
        }, exactMatch, index);
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
                pawn =>
                {
                    var match = BestHistoryRecordMatch(pawn, expected, exactMatch, index);
                    var actual = match.Record;
                    var expectedSummary = FormatExpectedHistoryRecord(expected);
                    var actualSummary = FormatActualHistoryRecord(actual);
                    var mismatchedFields = match.MismatchedFields.JoinToString();

                    return new AssertionResult(
                        match.Matches,
                        $"Expect HistoryRecord to match for {pawn} {testParams}.\nMismatched fields: {mismatchedFields}",
                        $"Expect HistoryRecord NOT to match for {pawn} {testParams}.\nMismatched fields: {mismatchedFields}",
                        expectedSummary,
                        actualSummary
                    );
                }
            );
        });
    }

    private static HistoryRecordMatch BestHistoryRecordMatch(Pawn pawn, ExpectedHistoryRecord expected, bool exactMatch = false, int? index = null)
    {
        if (index.HasValue)
        {
            if (pawn.HistoryRecords.TryAt(index.Value, out var record))
                return Match(expected, record, exactMatch);
            else
                return Match(expected, null, exactMatch);
        }

        return pawn.HistoryRecords
            .Select(r => Match(expected, r, exactMatch))
            .OrderBy(match => match.MismatchedFields.Count)
            .ThenByDescending(match => match.Record.date)
            .FirstOrDefault() ?? Match(expected, pawn.HistoryRecords.LastOrDefault(), exactMatch);
    }
    
    private static HistoryRecordMatch Match(ExpectedHistoryRecord expected, HistoryRecord actual, bool exactMatch)
    {
        return new HistoryRecordMatch(actual, GetMismatchedHistoryRecordFields(expected, actual, exactMatch));
    }

    private static List<string> GetMismatchedHistoryRecordFields(ExpectedHistoryRecord expected, HistoryRecord actual, bool exactMatch)
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
        if (expected.Description != null && !LangUtility.IsStructurallyTheSame(expected.Description, actual.description?.StripTags(), exactMatch))
            mismatchedFields.Add(nameof(expected.Description));
        if (expected.Concerns != null && !ThingListsEqual(expected.Concerns, actual.concerns))
            mismatchedFields.Add(nameof(expected.Concerns));
        if (expected.ConcernAtLeast != null && !ThingListsEqual(expected.ConcernAtLeast, actual.concerns.Intersect(expected.ConcernAtLeast).ToList()))
            mismatchedFields.Add(nameof(expected.ConcernAtLeast));
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
            $"  Concerns: {FormatExpectedThingList(expected.Concerns ?? expected.ConcernAtLeast)}",
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

    private void RunAssertion(Action assertion, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var ctx = TestManager.Ctx;
        var source = new AssertionSource(memberName, filePath, lineNumber);
        
        if (pawns.Count == 0)
        {
            ctx.Fail(new TestException(new TestExecutionFailure(ctx.TestId, "No pawns to assert.")));
            return;
        }

        AssertionRunner.RunAssertion(assertion, source, new AssertionRunOptions(isEventually, eventuallyTimeoutTicks, eventuallyPollIntervalTicks));
    }
}
