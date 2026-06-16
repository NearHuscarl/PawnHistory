# Blinding Ceremony Ritual Outcome

Blinding ceremonies are identity-shaping Ideology rituals. The blinded pawn is the subject of the event, and the doer is the pawn who made it happen. Recording the same ceremony on both pawns makes the history useful from either pawn's page without creating two competing descriptions of the same moment.

## Summary
Added BlindingCeremony support to the existing ritual outcome recorder. The record uses the shared `RitualOutcome` history definition and a BlindingCeremony-specific ritual outcome comp, mirroring Trial's two-pawn recording shape.

## Shipped Scope
- Records one ritual outcome on the doer and one on the target.
- Uses one shared description for both records: the target was blinded by the doer during the outcome-quality blinding ceremony.
- Stores the other pawn as the concern on each record through the existing `HistoryRecord` self-filtering behavior.
- Reuses `PH_RitualOutcome` and does not add a new `HistoryRecordDef`.

## Design
`RitualOutcomeComp_BlindingCeremony` matches the Ideology `BlindingCeremony` precept, resolves the plain ritual role ids `doer` and `target`, and supplies both pawns to `RitualOutcomeRecorder`.

The existing ritual outcome Harmony patches already cover `RitualOutcomeEffectWorker_Blinding` because it inherits `RitualOutcomeEffectWorker_FromQuality` without overriding `Apply` or `GetOutcome`. No additional patch target was added.

The event organizer mapping now treats the blinding `doer` as the ceremony host so common ritual grammar remains consistent with the role that led the ceremony.

## Rules
- Rulepack text is specific to BlindingCeremony and does not depend on the ritual label, because the base game label is "blinding" while the player-facing event reads better as "blinding ceremony."
- The recorder test forces the worst outcome to avoid Royalty psylink extra-outcome handling, which expects `LordJob_Ritual_Mutilation.mutilatedPawns` to be populated by the live ritual job.

## Verification
- Added a recorder-local Ideology-gated test that runs BlindingCeremony through the ritual builder and asserts both doer and target records with reciprocal concerns.
- Ran Debug MSBuild successfully:
  `MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`
