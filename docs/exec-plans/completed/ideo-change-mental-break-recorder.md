# Ideology-Change Mental Break Recorder

When a pawn suffers the Ideology expansion's crisis-of-belief mental break, the base game explains three player-facing outcomes that matter to history: whether the pawn actually converted, how much certainty remained if they did not, and whether the change cost them an ideological role. Without a dedicated recorder path, PawnHistory either missed that moment entirely or reduced it to a generic sad-wander entry.

## Summary
Added explicit history support for the `IdeoChange` mental break within the existing mental-break recorder flow. The recorder now preserves the original ideology-change state before RimWorld silently transitions it into `Wander_OwnRoom` or `Wander_Sad`, then writes a history entry that records conversion vs certainty loss, the aftermath wander variant, and optional role loss using past-tense history wording.

## Shipped Scope
- Preserved the original `MentalState_IdeoChange` instance through the mental-break event pipeline.
- Added reflected accessors for ideology-change-only fields: old ideology, new ideology, lost role, converted flag, and resulting certainty.
- Implemented a dedicated `MentalBreakComp_IdeoChange` rather than widening the generic recorder logic.
- Extended `PH_MentalBreak` with an `IdeoChange` branch for the new text variants.
- Added recorder-local Ideology tests for:
  - converted with own-room aftermath
  - converted with sad-wander aftermath
  - converted with role loss
  - not converted, recording reduced certainty

## Design
The normal `MentalStateHandler.TryStartMentalState` postfix is too late for `MentalState_IdeoChange`, because that mental state immediately performs a silent transition into another state during `PostStart()`. Relying on `pawn.MentalState` at recorder time therefore loses the ideology-change payload.

To keep the design local and avoid a recorder-specific patch, the event layer now publishes `IdeoChange` from a Harmony postfix on `MentalState_IdeoChange.PostStart()`. The existing mental-break event record gained an optional `MentalStateInstance` property so recorder code can prefer the original state object when available while leaving all other mental breaks unchanged.

The recorder-specific logic lives in a new `MentalBreakComp_IdeoChange`, which matches the repo's existing mental-break extension pattern and keeps ideology-only reflection and grammar setup out of `MentalBreakRecorder`.

## Rules
- Converted crises record both old and new ideology.
- Non-converted crises record old ideology and the new certainty value instead of inventing a conversion.
- The aftermath sentence reflects the real fallback RimWorld chose: own-room hiding or sad wandering.
- Role-loss text appears only when the pawn actually converted away from an ideology role.
- The entry remains a `MentalBreak` record instead of introducing a parallel history-def family for one special case.

## Verification
- Added recorder-local tests that exercise all meaningful description branches through the real mental-break path.
- Built the mod in Debug after the recorder, event, accessor, and rulepack changes.
