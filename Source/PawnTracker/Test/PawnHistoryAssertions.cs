using PawnHistory.Source.Helper;
using System;
using System.Linq;
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
                TestManager.Ctx.Fail($"Expected record of type '{def.defName}' for {pawn} but none found.");
        });

        return this;
    }

    public PawnHistoryAssertions ToHaveHistoryRecordCount(int expected)
    {
        RunAssertion(() =>
        {
            var actual = pawn.GetHistoryRecords().Count;

            if (actual != expected)
                TestManager.Ctx.Fail($"Expected {expected} number of records but got {actual}.");
        });

        return this;
    }

    public PawnHistoryAssertions ToHaveHistoryRecord(string descriptionTemplate, int index = -1, bool exactMatch = false)
    {
        RunAssertion(() =>
        {
            var lastRecord = pawn.GetHistoryRecords().At(index);
            var actual = lastRecord.description.StripTags();

            if (!LangUtility.IsStructurallyTheSame(descriptionTemplate, actual, exactMatch))
            {
                TestManager.Ctx.Fail(
                    $"Expected template [exactMatch={exactMatch}]:",
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
        var ctx = TestManager.Ctx;

        if (!isEventually)
        {
            try
            {
                assertion();
                ctx.AssertionsPassed++;
            }
            catch
            {
                ctx.AssertionsFailed++;
            }
            return;
        }

        var tickStart = Find.TickManager.TicksGame;
        Exception lastException = null;

        ctx.PendingEventually++;

        var action = TickDelayManager.Interval(eventuallyPollIntervalTicks, (a) =>
        {
            if (Find.TickManager.TicksGame - tickStart > eventuallyTimeoutTicks)
            {
                ctx.PendingEventually--;
                ctx.AssertionsFailed++;
                ctx.Fail(lastException, $"Eventually failed after {eventuallyTimeoutTicks} ticks.");
                a.Cancelled = true;
                return;
            }

            try
            {
                assertion();
                ctx.AssertionsPassed++;
                ctx.PendingEventually--;
                a.Cancelled = true;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        });

        ctx.OnCleanup(() => action.Data.Cancelled = true);
        // reset mode so next assertions are normal unless re-enabled
        isEventually = false;
    }
}