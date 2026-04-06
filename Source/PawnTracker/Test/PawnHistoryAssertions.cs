using PawnHistory.Source.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public sealed class PawnHistoryAssertions(IEnumerable<Pawn> pawns)
{
    private readonly IEnumerable<Pawn> pawns = pawns ?? throw new ArgumentNullException(nameof(pawns));

    private bool isEventually;
    private int eventuallyTimeoutTicks;
    private int eventuallyPollIntervalTicks;
    private bool negate = false;

    public PawnHistoryAssertions Not()
    {
        negate = !negate;
        return this;
    }

    private void AssertCondition(bool condition, string positiveMessage, string negativeMessage)
    {
        if (negate ? condition : !condition)
            TestManager.Ctx.Fail(negate ? negativeMessage : positiveMessage);
    }

    public void ToHaveHistoryRecordOf(HistoryRecordDef def)
    {
        RunAssertion(() =>
        {
            var hasRecord = pawns.Any(p => p.GetHistoryRecords().Any(r => r.def == def));
            var pawn = pawns.Count() == 1 ? pawns.First() : null;
            var forPawn = pawn == null ? "" : $"for {pawn} ";
            AssertCondition(
                hasRecord,
                $"Expected record of type '{def.defName}' {forPawn}but none found.",
                $"Expected NO record of type '{def.defName}' {forPawn}but one was found."
            );
        });
    }

    public void ToHaveHistoryRecordCount(int expected)
    {
        RunAssertion(() =>
        {
            var pawn = pawns.First();
            var actual = pawn.GetHistoryRecords().Count;
            var result = actual != expected;

            AssertCondition(
                result,
                $"Expected {expected} number of records but got {actual}.",
                $"Expected NOT {expected} number of records but got {actual}."
            );
        });
    }

    public void ToHaveHistoryRecord(string descriptionTemplate, int index = -1, bool exactMatch = false)
    {
        RunAssertion(() =>
        {
            string actual = "";
            var result = pawns.Any(p =>
            {
                if (!p.GetHistoryRecords().TryAt(index, out HistoryRecord record))
                    return false;
                actual = record.description.StripTags();
                return LangUtility.IsStructurallyTheSame(descriptionTemplate, actual, exactMatch);
            });

            AssertCondition(
                result,
                $"Expected description to match template\nExpected template [exactMatch={exactMatch}]:\n{descriptionTemplate}\nActual resolved description:\n{actual}",
                $"Expected description NOT to match template\nExpected template [exactMatch={exactMatch}]:\n{descriptionTemplate}\nActual resolved description:\n{actual}."
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
        // don't run immediately, so Test method can return cleanup action if synchronous test call failed.
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
                    ctx.PendingEventually--;
                    ctx.AssertionsPassed++;
                    a.Cancelled = true;
                }
                catch (Exception ex)
                {
                    ctx.PendingEventually--;
                    ctx.AssertionsFailed++;
                    a.Cancelled = true;
                    ctx.LogFailed(ex);
                }
                return;
            }

            if (Find.TickManager.TicksGame - tickStart > eventuallyTimeoutTicks)
            {
                ctx.PendingEventually--;
                ctx.AssertionsFailed++;
                a.Cancelled = true;
                ctx.LogFailed(lastException, $"Test assertion failed after {eventuallyTimeoutTicks} ticks.");
            }

            try
            {
                assertion();
                ctx.PendingEventually--;
                ctx.AssertionsPassed++;
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