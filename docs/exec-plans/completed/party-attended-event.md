# Party Attended Event

Parties are only useful history when a pawn actually experienced the gathering. The previous recorder watched broad lord toil transitions and produced start, join, successful finish, and cancellation records. That made party history noisy and tied the recorder to `LordToilChangeEvent`, even though the desired player-facing record is the successful attendance moment.

## Summary

Party history was collapsed to a single `PartyAttended` entry, removing start, join, and cancellation records. A later follow-up moved normal party records onto RimWorld's `AttendedParty` tale while keeping `PartyAttendedEvent` as the successful-completion source for concerts.

## Shipped Scope

- Added `PartyAttendedEvent`, published from the successful timeout transition on `LordJob_Joinable_Party` and tagged with party versus concert type.
- Removed `PartyRecorder` handling for party start, party join, and cancellation.
- Replaced `PartyStarted`, `PartyJoined`, and `PartyFinished` with the single `PartyAttended` history def and rulepack.
- Moved normal parties to the Core `AttendedParty` tale path, leaving `PartyAttendedEvent` for `ConcertRecorder`.

## Design

`LordJob_Joinable_Party.CreateGraph()` builds separate transitions for cancellation and timeout completion. The event patch attaches a custom pre-action only to the timeout transition, so the event fires at gathering completion without subscribing to every lord toil change.

Normal parties now use vanilla `TaleDefOf.AttendedParty`, which is emitted from `LordJob_Joinable_Party.ApplyOutcome()` for pawns present at the successful outcome. Concerts still use the shared completion event because their Royalty tale defs and history wording are separate.

## Rules

- Cancelled parties produce no party history record.
- Party start and join moments produce no party history record.
- Concerts publish the shared completion event with `PartyType.Concert`, but do not produce `PartyAttended` history records.
- Normal party history comes from `AttendedParty` tale dispatch rather than recomputing attendance in `PartyRecorder`.

## Verification

- Updated party tests to cover successful organizer/attendee records.
- Updated cancellation coverage to assert no `PartyAttended` record is written.
- Updated concert coverage to assert concerts write `ConcertAttended` and no `PartyAttended` history record.
