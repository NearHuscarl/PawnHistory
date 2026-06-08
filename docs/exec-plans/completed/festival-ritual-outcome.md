# Festival Ritual Outcome

Social festivals are outcome-driven Ideology gatherings. Their result matters to every pawn who took part, not just the leader who started the ritual, so the history entry should read like a shared colony event while still showing each participant's own point of view.

## Summary
Festival ritual outcomes now write a `RitualOutcome` history record for the leader and every assigned joiner. The existing generic ritual recorder writes the leader record, while a Festival-specific ritual outcome comp writes attendee-facing records that name the leader as a concern.

## Shipped Scope
- Added `Festival` and `CelebratedDate` entries to the local `Extra` DefOf fallbacks.
- Added a Festival-specific ritual outcome comp that fans attendee records out from the standard ritual spectator assignment state.
- Kept existing non-Festival rituals host-only.
- Added a `RitualBuilder.Festival(...)` helper for recorder tests.
- Added Festival-specific grammar rules to `PH_RitualOutcome`.

## Design
The existing ritual outcome patch already covers Festival because the `CelebratedDate` outcome uses `RitualOutcomeEffectWorker_FromQuality`. The event keeps the same ritual assignment state as other rituals: host, spectators, roles, target, and outcome. `RitualOutcomeRecorder` keeps its host-only behavior. `RitualOutcomeComp_Festival` matches only Festival events and writes joiner records from the comp hook.

Festival descriptions use `PAWN` as the record owner and `Host` as the leader:
- leader: led an outcome-rated ritual with the other participants
- joiner: attended the leader's outcome-rated ritual with the other participants

## Verification
Added a recorder-local Ideology test that starts a Festival through the new ritual builder, forces the best `CelebratedDate` outcome, and asserts records for the leader and all joiners.

Ran the Debug MSBuild build successfully after the recorder, builder, rulepack, and docs changes.
