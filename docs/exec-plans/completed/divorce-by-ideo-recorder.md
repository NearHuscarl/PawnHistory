# Divorce By Ideo Recorder

Ideology-forced divorce is a real social turning point, not background churn. When a pawn changes ideoligion and that new belief system forbids their current spouse count, the game silently converts one marriage at a time into `ExSpouse` relations and only explains it through a notification letter. Those divorces belong in pawn history because they materially change both pawns' biographies.

## Summary

Shipped an Ideology-gated `DivorceByIdeo` recorder backed by the literal ideology divorce flow. The mod now records one history entry per actual spouse pair divorced by `RemoveSpousesAsForbiddenByIdeo`, writes the record only on the two pawns whose marriage ended, and keeps the generic `Breakup` recorder scoped to the existing social-interaction breakup path.

## Shipped Scope

- Added `DivorceByIdeoEvent`.
- Patched `SpouseRelationUtility.RemoveSpousesAsForbiddenByIdeo(...)` to mark the ideology-divorce flow.
- Patched `SpouseRelationUtility.DoDivorce(...)` to publish one event per actual divorce while that flow is active.
- Added `DivorceByIdeoRecorder`, `HistoryRecordDefOf.DivorceByIdeo`, the XML def, and the `PH_DivorceByIdeo` rulepack.
- Added an Ideology-gated recorder test covering the involved-pawns-only rule and the no-overlap-with-`Breakup` rule.

## Design

### Literal patch point

The implementation follows the same base-game path that produces the `LetterIdeoChangedDivorcedPawns` notification:

1. `Pawn_IdeoTracker.SetIdeo(...)`
2. `SpouseRelationUtility.RemoveSpousesAsForbiddenByIdeo(...)`
3. `SpouseRelationUtility.DoDivorce(...)`

The recorder does not hook the letter itself. It hooks the real divorce method, but only while the ideology-removal helper is active. That keeps the event tied to the real relationship mutation instead of to UI output.

### Why this is separate from `Breakup`

`BreakupRecorder` is driven by the `InteractionWorker_Breakup` social interaction and already covers:

- ordinary lover breakup
- fiance breakup
- spouse breakup initiated through the breakup interaction

The ideology divorce path does not use that interaction or its play-log entry. It calls `DoDivorce(...)` directly, so broadening `BreakupRecorder` would have mixed two distinct causes into one record family.

## Rules

- Record only the two pawns whose marriage was actually dissolved.
- Each record concerns only the other spouse from that divorce pair.
- Do not attach surviving spouses or unrelated lovers as extra concerns.
- Do not create a generic `Breakup` record for ideology-forced divorce.
- Keep the naming literal: this recorder is specific to the ideology-forced divorce flow present in the inspected RimWorld source.

## Other Relationship Flows Checked

Checked the current RimWorld source tree for other divorce/breakup paths:

- `InteractionWorker_Breakup.Interacted(...)` handles normal breakup and spouse divorce through the social interaction. Already covered by `BreakupRecorder`.
- `InteractionWorker_MarriageProposal.Interacted(...)` can reject a proposal and directly turn `Lover` into `ExLover`. Already covered as a `MarriageProposal` record, not a `Breakup` record.
- `InteractionWorker_RomanceAttempt.BreakLoverAndFianceRelations(...)` can dissolve lover/fiance relations when a new romance succeeds. That feeds the mod's `NewAffair` coverage rather than `Breakup`.

For divorce specifically, the inspected RimWorld source only calls `SpouseRelationUtility.DoDivorce(...)` from `RemoveSpousesAsForbiddenByIdeo(...)`.

## Verification

Added recorder-local coverage for:

- one ideology-forced divorce being recorded on the divorcing pawn
- the mirrored record being recorded on the former spouse
- unrelated spouse and lover pawns not receiving the new record
- no `Breakup` record being emitted for the same event

Shell verification:

- `MSBuild` debug build for `PawnHistory.csproj`

Limitations:

- The repository does not expose a shell-runnable in-game recorder test harness, so the new recorder test was added but not executed from the shell.
