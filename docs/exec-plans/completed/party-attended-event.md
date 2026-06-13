# Party Attended Event

Parties are only useful history when a pawn actually experienced the gathering. The previous recorder watched broad lord toil transitions and produced start, join, successful finish, and cancellation records. That made party history noisy and tied the recorder to `LordToilChangeEvent`, even though the desired player-facing record is the successful attendance moment.

## Summary

Party history now records a single `PartyAttended` entry when a normal party reaches its timeout finish transition. The shared completion event also identifies concerts, allowing `ConcertRecorder` to reuse the same successful-gathering signal while writing `ConcertAttended`.

## Shipped Scope

- Added `PartyAttendedEvent`, published from the successful timeout transition on `LordJob_Joinable_Party` and tagged with party versus concert type.
- Removed `PartyRecorder` handling for party start, party join, and cancellation.
- Replaced `PartyStarted`, `PartyJoined`, and `PartyFinished` with the single `PartyAttended` history def and rulepack.
- Kept `PartyRecorder` scoped to `PartyType.Party`, leaving concerts to `ConcertRecorder`.

## Design

`LordJob_Joinable_Party.CreateGraph()` builds separate transitions for cancellation and timeout completion. The event patch attaches a custom pre-action only to the timeout transition, so the event fires at party completion without subscribing to every lord toil change.

The event payload includes the organizer, the complete partygoer list, and the party type. Recorders filter by type, then filter each pawn with `ShouldRecord(...)`, resolve host versus attendee wording through `isOrganizer`, and only attach the organizer as a concern for non-host records.

## Rules

- Cancelled parties produce no party history record.
- Party start and join moments produce no party history record.
- Concerts publish the shared completion event with `PartyType.Concert`, but do not produce `PartyAttended` history records.
- Partygoers come from the shared event payload, which is the lord's owned pawns at successful finish time.

## Verification

- Updated party tests to cover successful organizer/attendee records.
- Updated cancellation coverage to assert no `PartyAttended` record is written.
- Updated concert coverage to assert concerts write `ConcertAttended` and no `PartyAttended` history record.
