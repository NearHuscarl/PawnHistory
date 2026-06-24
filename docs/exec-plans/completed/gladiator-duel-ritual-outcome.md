# Gladiator Duel Ritual Outcome

Gladiator duels are ritual outcomes centered on three pawns: the leader who organizes the spectacle and the two prisoners or slaves forced to fight. The generic ritual outcome recorder only understands host-focused rituals or broad participant attendance, so it could not preserve the duel's actual cast without either missing the fighters or spraying records onto escorts and spectators.

## Summary
Added a Gladiator Duel ritual outcome comp that records the event on the organizer and both duelists. Each record points at the other two central pawns as concerns, while escorts and spectators remain unrecorded.

## Shipped Scope
- Recognizes the Ideology `GladiatorDuel` precept.
- Records exactly the leader organizer, `duelist1`, and `duelist2`.
- Uses one shared description naming both duelists and the organizer.
- Adds test DSL support for starting a Gladiator Duel through the real ritual dialog path.

## Design
The implementation follows the existing ritual comp pattern used by trials, blinding, and scarification. The comp resolves required vanilla ritual roles directly and does not add fallbacks for missing organizer or fighter roles, because the vanilla ritual requires those roles before it can start.

Concerns are produced as the same three central pawns. `HistoryRecord` already removes the owning pawn from its concerns, so each saved record naturally points to the other two pawns.

## Rules
- Spectators and escorts are attendees, not record owners.
- The organizer is the `leader` role, matching the vanilla Gladiator Duel behavior.
- The record uses the existing `RitualOutcome` history def and `PH_RitualOutcome` rulepack rather than adding a new history record def.

## Verification
- Added a comp-local Ideology test that starts a real Gladiator Duel from `RitualBuilder`, forces a good outcome, and asserts records on the organizer and both duelists.
- The test asserts each record's concerns and verifies escorts and spectators do not receive a ritual outcome record.
