# Party Attended Event (Superseded)

Parties are only useful history when a pawn actually experienced the gathering. The previous recorder watched broad lord toil transitions and produced start, join, successful finish, and cancellation records. That made party history noisy and tied the recorder to `LordToilChangeEvent`, even though the desired player-facing record is the successful attendance moment.

## Summary

Party history was collapsed to a single `PartyAttended` entry, removing start, join, and cancellation records. This implementation was later superseded by tale-based dispatch: normal parties use RimWorld's Core `AttendedParty` tale, and concerts use Royalty's `ConcertAttended` and `ConcertHeld` tales. The old shared party-completion event was removed.

## Shipped Scope

- Originally added a successful timeout event on `LordJob_Joinable_Party` tagged with party versus concert type.
- Removed `PartyRecorder` handling for party start, party join, and cancellation.
- Replaced `PartyStarted`, `PartyJoined`, and `PartyFinished` with the single `PartyAttended` history def and rulepack.
- Later moved normal parties to the Core `AttendedParty` tale path and concerts to Royalty concert tales, removing the shared event entirely.

## Design

The original lord-graph design attached a custom pre-action only to the timeout transition, so the event fired at gathering completion without subscribing to every lord toil change. That design is preserved here as historical context, but it is not the current implementation.

Current party and concert recording use vanilla tale emission from `LordJob_Joinable_Party.ApplyOutcome()`. Normal parties dispatch `AttendedParty`; concerts dispatch `ConcertAttended` for attendees and `ConcertHeld` for the performer.

## Rules

- Cancelled parties produce no party history record.
- Party start and join moments produce no party history record.
- Concerts do not produce `PartyAttended` history records.
- Normal party history comes from `AttendedParty` tale dispatch rather than recomputing attendance in `PartyRecorder`.

## Verification

- Updated party tests to cover successful organizer/attendee records.
- Updated cancellation coverage to assert no `PartyAttended` record is written.
- Updated concert coverage to assert concerts write concert-specific history records and no `PartyAttended` history record.
