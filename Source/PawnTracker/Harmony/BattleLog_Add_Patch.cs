using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnHistory.Source.PawnTracker
{
    [HarmonyPatch(typeof(BattleLog), "Add")]
    public static class BattleLog_Add_Patch
    {
        public static void Postfix(BattleLog __instance, LogEntry entry)
        {
            if (entry is not BattleLogEntry_StateTransition transitionEntry) return;

            // Right after the log is added, DamageWorker.AssociateWithLog() is not called yet so we will miss the
            // information about affected body part if we publish the event now.
            TickDelayManager.Delay(1, () =>
            {
                try
                {
                    var battle = __instance.Battles.FirstOrDefault(b => b.Entries.Contains(transitionEntry));
                    var transitionIndex = battle.Entries.IndexOf(transitionEntry);

                    var concerns = transitionEntry.GetConcerns();
                    var initiator = concerns.FirstOrDefault();
                    var subject = concerns.LastOrDefault();
                    var isKillLog = transitionEntry.IconFromPOV(null) == LogEntry.Skull;
                    var killOrDownEntry = battle.Entries.First(e => e is LogEntry_DamageResult && e.Concerns(initiator) && e.Concerns(subject));
                    var combatLogText = (subject as Pawn).health.hediffSet.hediffs.FirstOrDefault(h => h.combatLogEntry?.Target?.LogID == killOrDownEntry.LogID)?.combatLogText;
                    Log.Message($"{transitionIndex} {battle.Entries.Count} subject={subject} initiator={initiator} logId={killOrDownEntry.LogID}");

                    if (combatLogText == null) return;

                    if (initiator is Pawn initiatorPawn && isKillLog && PawnTracker.ShouldTrack(initiatorPawn))
                    {
                        GameEventListener.Publish(new GameEvent(initiatorPawn, PawnEventDefOf.Kill, combatLogText));
                    }
                    if (subject is Pawn subjectPawn && PawnTracker.ShouldTrack(subjectPawn))
                    {
                        var eventDef = isKillLog ? PawnEventDefOf.Death : PawnEventDefOf.Downed;
                        GameEventListener.Publish(new GameEvent(subjectPawn, eventDef, combatLogText));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[PawnHistory] {ex.Message}");
                }
            });
        }
    }
}
