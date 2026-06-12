# Party Attended Event

Parties are only useful history when a pawn actually experienced the gathering. The previous recorder watched broad lord toil transitions and produced start, join, successful finish, and cancellation records. That made party history noisy and tied the recorder to `LordToilChangeEvent`, even though the desired player-facing record is the successful attendance moment.

## Summary

Party history now records a single `PartyAttended` entry when a normal party reaches its timeout finish transition. The organizer receives host wording, while other partygoers receive attended wording with the organizer as a concern.

## Shipped Scope

- Added `PartyAttendedEvent`, published from the successful timeout transition on `LordJob_Joinable_Party`.
- Removed `PartyRecorder` handling for party start, party join, and cancellation.
- Replaced `PartyStarted`, `PartyJoined`, and `PartyFinished` with the single `PartyAttended` history def and rulepack.
- Kept concerts excluded from party attendance records.

## Design

`LordJob_Joinable_Party.CreateGraph()` builds separate transitions for cancellation and timeout completion. The event patch attaches a custom pre-action only to the timeout transition, so the event fires at party completion without subscribing to every lord toil change.

The event payload is per pawn and includes the organizer plus the complete partygoer list. The recorder filters each pawn with `ShouldRecord(...)`, resolves host versus attendee wording through `isOrganizer`, and only attaches the organizer as a concern for non-host records.

## Rules

- Cancelled parties produce no party history record.
- Party start and join moments produce no party history record.
- Concerts do not produce `PartyAttended`.
- Partygoers are pawns present for at least one tick plus the organizer when available.

## Verification

- Updated party tests to cover successful organizer/attendee records.
- Updated cancellation coverage to assert no `PartyAttended` record is written.
- Updated concert coverage to assert no `PartyAttended` record is written for concerts.
