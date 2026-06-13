# Concert Recorder

Royalty concerts are social gatherings with a dedicated performer and attendees. They are worth recording because a successful concert is a visible colony-life event, and Royalty already treats it as distinct from a normal party through `HeldConcert` and `AttendedConcert` tales.

## Summary

The implementation adds a Royalty-gated concert history record that is written only after a concert finishes successfully. It reuses the shared party-attendance completion event and does not record concert start, join, or cancellation events.

## Shipped Scope

- Added `ConcertRecorder`, discovered through the existing recorder reflection path.
- Reused `PartyAttendedEvent` with `PartyType.Concert` as the single successful-completion source.
- Added `ConcertAttended` as a Royalty-gated history record def and `HistoryRecordDefOf` entry.
- Added `PH_ConcertAttended` with organizer and attendee text.
- Filtered `PartyRecorder` to `PartyType.Party` so concerts do not also produce party records.

## Design

The recorder listens for `PartyAttendedEvent` and filters to `PartyType.Concert`. The shared event is attached to the successful timeout transition of `LordJob_Joinable_Party`; Royalty concerts inherit that job and are tagged as concerts by the event publisher.

Attendee records are based on the event's partygoer list. The organizer is supplied separately on the event and used for host/attendee wording plus concern links.

## Rule Names

The new rulepack uses `[PAWN]`, `[Others]`, and `[Organizer]`. `[Organizer]` is the only extra recorder-supplied symbol needed beyond the usual pawn and group symbols. Royalty's tale defs use `ATTENDER` and `ORGANIZER`, but this mod's `HistoryDescriptionBuilder.Description(pawn)` only supplies `[PAWN]` unless the recorder adds more rules.

## Verification

- Added recorder-local Royalty test coverage for successful concert completion.
- The test asserts the organizer concert record, an attendee concert record with organizer concern, and absence of a `PartyAttended` history record on concert attendees.
