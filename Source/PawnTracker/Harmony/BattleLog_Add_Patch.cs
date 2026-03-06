using HarmonyLib;
using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker;

[HarmonyPatch(typeof(BattleLog), "Add")]
public static class BattleLog_Add_Patch
{
    static readonly AccessTools.FieldRef<BattleLogEntry_StateTransition, HediffDef> CulpritHediffRef =
        AccessTools.FieldRefAccess<BattleLogEntry_StateTransition, HediffDef>("culpritHediffDef");
    static readonly AccessTools.FieldRef<BattleLogEntry_StateTransition, BodyPartRecord> CulpritHediffTargetPartRef =
        AccessTools.FieldRefAccess<BattleLogEntry_StateTransition, BodyPartRecord>("culpritHediffTargetPart");

    public static void Postfix(BattleLog __instance, LogEntry entry)
    {
        if (entry is not BattleLogEntry_StateTransition transitionEntry) return;

        // Right after the log is added, DamageWorker.AssociateWithLog() is not called yet so we will miss the
        // information about affected body part if we publish the event now.
        TickDelayManager.Delay(0, () =>
        {
            var battle = __instance.Battles.FirstOrDefault(b => b.Entries.Contains(transitionEntry));
            var transitionIndex = battle.Entries.IndexOf(transitionEntry);

            var concerns = transitionEntry.GetConcerns().ToList();
            var initiator = concerns.Count == 1 ? null : concerns[0];
            var subject = concerns.Count == 1 ? concerns[0] : concerns[1];
            var isKillLog = transitionEntry.IconFromPOV(null) == LogEntry.Skull;
            var killOrDownEntry = battle.Entries.Skip(transitionIndex + 1).FirstOrDefault(e => e is LogEntry_DamageResult && e.Concerns(subject));
            var subjectPawn = subject as Pawn;
            var combatLogText = (subject as Pawn).health.hediffSet.hediffs.FirstOrDefault(h => h.combatLogEntry?.Target?.LogID == killOrDownEntry?.LogID)?.combatLogText;
            var initiatorPawn = initiator as Pawn;
            var transitionText = transitionEntry.ToGameStringFromPOV(null);

            if (initiatorPawn != null && isKillLog && PawnTracker.ShouldTrack(initiatorPawn))
            {
                GameEventListener.Publish(new GameEvent(initiatorPawn, PawnEventDefOf.Kill, $"{combatLogText} {transitionText}")
                {
                    relatedPawns = subjectPawn != null ? [subjectPawn] : [],
                });
            }
            if (subjectPawn != null && PawnTracker.ShouldTrack(subjectPawn))
            {
                var eventDef = isKillLog ? PawnEventDefOf.Death : PawnEventDefOf.Downed;
                if (eventDef == PawnEventDefOf.Downed && CompHistoryManager.GetComp(subjectPawn).records.Last()?.eventDef == PawnEventDefOf.Death)
                {
                    Log.Warning($"[PawnHistory] Received downed transition from {subjectPawn.NameShortColored}, but they were already dead. Skipping..");
                    return;
                }

                TaggedString resolvedDesc;
                // log entry is not associated with any active battle. Non-combat dead needs to be handled manually (e.g. BloodLoss, ToxicBuildup...)
                if (killOrDownEntry == null)
                {
                    var culpritHediff = CulpritHediffRef(transitionEntry);
                    resolvedDesc = eventDef.description.Formatted(subjectPawn.NameShortColored.Named("PAWN"), culpritHediff.label.Colorize(culpritHediff.defaultLabelColor).Named("REASON"));
                }
                else
                    resolvedDesc = $"{combatLogText} {transitionText}";

                GameEventListener.Publish(new GameEvent(subjectPawn, eventDef, resolvedDesc)
                {
                    relatedPawns = initiatorPawn != null ? [initiatorPawn] : [],
                });
            }
        });
    }
}
