using HarmonyLib;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum CasualtyType
{
    Killed,
    Downed,
}

public class CasualtyLogAddedEvent(Battle battle, BattleLogEntry_StateTransition transitionEntry, LogEntry_DamageResult lastDamageEntry, Pawn initiator, Pawn subject, CasualtyType casualty, HediffDef culpritHediff) : GameEventBase
{
    public Battle Battle { get; } = battle;
    public BattleLogEntry_StateTransition TransitionEntry { get; } = transitionEntry;
    public LogEntry_DamageResult LastDamageEntry { get; } = lastDamageEntry;
    public Pawn Initiator { get; } = initiator;
    public Pawn Subject { get; } = subject;
    public CasualtyType Casualty { get; } = casualty;
    public HediffDef CulpritHediff { get; } = culpritHediff;
}

[HarmonyPatch(typeof(BattleLog), nameof(BattleLog.Add))]
public static class BattleLog_Add_Patch
{
    static readonly AccessTools.FieldRef<BattleLogEntry_StateTransition, HediffDef> CulpritHediffRef =
        AccessTools.FieldRefAccess<BattleLogEntry_StateTransition, HediffDef>("culpritHediffDef");
    static readonly AccessTools.FieldRef<BattleLogEntry_StateTransition, BodyPartRecord> CulpritHediffTargetPartRef =
        AccessTools.FieldRefAccess<BattleLogEntry_StateTransition, BodyPartRecord>("culpritHediffTargetPart");

    static readonly AccessTools.FieldRef<BattleLogEntry_StateTransition, Pawn> SubjectPawnRef =
        AccessTools.FieldRefAccess<BattleLogEntry_StateTransition, Pawn>("subjectPawn");
    static readonly AccessTools.FieldRef<BattleLogEntry_StateTransition, Pawn> InitiatorRef =
        AccessTools.FieldRefAccess<BattleLogEntry_StateTransition, Pawn>("initiator");

    public static void Postfix(BattleLog __instance, LogEntry entry)
    {
        if (entry is not BattleLogEntry_StateTransition transitionEntry) return;

        var battle = __instance.Battles.FirstOrDefault(b => b.Entries.Contains(transitionEntry));
        var transitionIndex = battle.Entries.IndexOf(transitionEntry);
        var initiator = InitiatorRef(transitionEntry);
        var subject = SubjectPawnRef(transitionEntry);
        var casualtyType = transitionEntry.IconFromPOV(null) == LogEntry.Skull ? CasualtyType.Killed : CasualtyType.Downed;
        var damageResultEntry = battle.Entries.Skip(transitionIndex + 1).FirstOrDefault(e => e is LogEntry_DamageResult && e.Concerns(subject)) as LogEntry_DamageResult;
        var culpritHediff = CulpritHediffRef(transitionEntry);

        GameEventBus.Publish(new CasualtyLogAddedEvent(battle, transitionEntry, damageResultEntry, initiator, subject, casualtyType, culpritHediff));
    }
}
