using PawnHistory.Source.Helper;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public sealed class PawnHistoryAssertions(Pawn pawn)
{
    private readonly Pawn pawn = pawn ?? throw new ArgumentNullException(nameof(pawn));

    private bool isEventually;
    private int eventuallyTimeoutTicks;
    private int eventuallyPollIntervalTicks;

    public PawnHistoryAssertions ToHaveHistoryRecordOf(HistoryRecordDef def)
    {
        RunAssertion(() =>
        {
            if (!pawn.GetHistoryRecords().Any(r => r.def == def))
                TestManager.Current.Fail($"Expected record of type '{def.defName}' for {pawn} but none found.");
        });

        return this;
    }

    public PawnHistoryAssertions ToHaveHistoryRecordCount(int expected)
    {
        RunAssertion(() =>
        {
            var actual = pawn.GetHistoryRecords().Count;

            if (actual != expected)
                TestManager.Current.Fail($"Expected {expected} number of records but got {actual}.");
        });

        return this;
    }

    public PawnHistoryAssertions ToHaveHistoryRecord(string descriptionTemplate)
    {
        RunAssertion(() =>
        {
            var lastRecord = pawn.GetHistoryRecords().LastOrDefault();
            var actual = lastRecord.description.StripTags();

            if (!IsStructurallyTheSame(descriptionTemplate, actual))
            {
                TestManager.Current.Fail(
                    "Expected template:",
                    descriptionTemplate,
                    "Actual resolved description:",
                    actual
                );
            }
        });

        return this;
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
        if (!isEventually)
        {
            assertion();
            return;
        }

        var tickStart = Find.TickManager.TicksGame;
        Exception lastException = null;

        var ctx = TestManager.Current;
        ctx.PendingEventually++;

        TickDelayManager.Interval(eventuallyPollIntervalTicks, (a) =>
        {
            if (Find.TickManager.TicksGame - tickStart > eventuallyTimeoutTicks)
            {
                ctx.PendingEventually--;
                ctx.Fail(lastException, $"Eventually failed after {eventuallyTimeoutTicks} ticks.");
                a.Cancelled = true;
                return;
            }

            try
            {
                assertion();
                ctx.PendingEventually--;
                a.Cancelled = true;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        });

        // reset mode so next assertions are normal unless re-enabled
        isEventually = false;
    }

    public static bool IsStructurallyTheSame(string template, string actual)
    {
        var segments = Regex.Matches(template, @"(?<rule>\[[^\]]+\])|(?<literal>[^\[]+)", RegexOptions.Compiled);
        int searchFrom = 0;
        var expectingContent = false;

        foreach (Match m in segments)
        {
            if (m.Groups["rule"].Success)
            {
                expectingContent = true;
                continue;
            }

            var literal = m.Groups["literal"].Value;
            var index = actual.IndexOf(literal, searchFrom, StringComparison.OrdinalIgnoreCase);
            
            // The text following a rule wasn't found
            if (index == -1)
                return false;

            // The placeholder was empty (literal found immediately at current position)
            if (expectingContent && index == searchFrom)
                return false;

            searchFrom = index + literal.Length;
            expectingContent = false;
        }

        // Handle case where template ends with a placeholder [Rule]
        if (expectingContent && searchFrom >= actual.Length)
            return false;

        return true;
    }
}