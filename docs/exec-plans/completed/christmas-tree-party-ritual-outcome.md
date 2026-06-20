Christmas tree party is a date-triggered social ritual with the same player-facing story shape as festivals: several pawns spend time together around a focal celebration object, and what matters in history is that each participant took part in a memorable communal event. Before this change, PawnHistory only recognized a few festival-style rituals, so christmas tree party fell through the generic ritual wording and did not fan out to every participant.

## Summary
Christmas tree party now records as a festival-style ritual outcome for every participant. The recorder reuses the existing `RitualOutcomeComp_Festival` path, matches the specific `DateRitualConsumable` ritual, and writes attendee wording instead of the generic fallback ritual sentence.

## Shipped Scope
- Added `Extra.PreceptDefOf.DateRitualConsumable` for the ritual itself.
- Added `Extra.ThingDefOf.ChristmasTree` for the test ritual target.
- Extended the festival ritual outcome matcher to include christmas tree party.
- Added a dedicated ritual builder helper and a recorder-local test that drives the real ritual dialog and outcome path.

Explicitly excluded:
- a dedicated `RitualOutcomeComp_ChristmasTreeParty`
- broader support for other consumable celebration rituals
- attaching the destroyed christmas tree as a concern

## Design
The implementation stays inside the existing festival specialization because christmas tree party has the same recording semantics:
- record on all participants
- use attendee wording
- keep concerns empty

Only the recognition and text contract changed:
- `RitualOutcomeComp_Festival.Match(...)` now accepts `DateRitualConsumable`
- `PH_RitualOutcome` gained a `ritual==DateRitualConsumable` attendee rule so the record does not fall back to the generic "delivered ..." text

The test DSL gained `RitualBuilder.ChristmasTreeParty(...)`, which mirrors the drum/dance-party helpers and uses the no-role `CelebrationTree` ritual behavior through the real begin-dialog flow.

## Rules
- Every participant in the christmas tree party receives a `RitualOutcome` history record.
- The record description uses the same attendee wording shape as other festival-style rituals.
- The record carries no concerns, even though the ritual target is destroyed and could technically be saved.
- The change is intentionally limited to christmas tree party and does not generalize to other consumable date rituals.

## Verification
- Added a recorder-local Ideology test that:
  - creates a player-owned christmas tree target
  - advances time for the date ritual obligation
  - forces a best outcome
  - runs the real ritual begin-dialog and outcome path
  - asserts that all participants receive the attendee-form `RitualOutcome` record with empty concerns

- Ran the approved Debug MSBuild build successfully after the recorder, rulepack, test DSL, and plan-note changes.
- Did not run the in-game test harness in this session.
