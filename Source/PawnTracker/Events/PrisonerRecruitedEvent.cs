using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PrisonerRecruitedEvent(Pawn Prisoner, Pawn Recruiter, string LogEntryText = null) : GameEventBase;

public static class PrisonerRecruitedContext
{
    public static PrisonerRecruitedEvent PendingEvent;
}

// Call order:
// Pawn_InteractionsTracker.TryInteractWith() prefix
// InteractionWorker_RecruitAttempt.Interacted()
// - InteractionWorker_RecruitAttempt.DoRecruit()
// Find.PlayLog.Add(entry)
// Pawn_InteractionsTracker.TryInteractWith() postfix

[HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
public static class Pawn_InteractionsTracker_TryInteractWith_Patch_2
{
    public static void Postfix(Pawn recipient)
    {
        if (PrisonerRecruitedContext.PendingEvent == null)
            return;

        var entries = Find.PlayLog.AllEntries;
        var logEntry = entries.FirstOrDefault(entry => entry is PlayLogEntry_Interaction il && Accessor.PlayLogEntry_Interaction.InteractionDef(il).defName == "RecruitAttempt");
        var e = PrisonerRecruitedContext.PendingEvent with { LogEntryText = logEntry.ToGameStringFromPOV(recipient) };
        GameEventBus.Publish(e);
        PrisonerRecruitedContext.PendingEvent = null;
    }
}

[HarmonyPatch(
    typeof(InteractionWorker_RecruitAttempt),
    nameof(InteractionWorker_RecruitAttempt.DoRecruit),
    [typeof(Pawn), typeof(Pawn), typeof(string), typeof(string), typeof(bool), typeof(bool)],
    [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal]
)]
public static class InteractionWorker_RecruitAttempt_DoRecruit_Patch
{
    public static void Postfix(Pawn recruiter, Pawn recruitee)
    {
        PrisonerRecruitedContext.PendingEvent = new PrisonerRecruitedEvent(recruitee, recruiter);
    }
}
