# Concert Recorder

Royalty concerts are social gatherings with a dedicated performer and attendees. They are worth recording because a successful concert is a visible colony-life event, and Royalty already treats it as distinct from a normal party through `ConcertHeld` and `ConcertAttended` tales.

## Summary

Concert history is written from Royalty's own concert tales after a concert finishes successfully. Attendees use `ConcertAttended`; the performer uses `ConcertHeld`. This replaces the earlier shared party-completion event path, so concerts no longer depend on lord toil transitions or party-versus-concert event tags.

## Shipped Scope

- Added `ConcertRecorder`, discovered through the existing recorder reflection path, for `ConcertAttendedEvent`.
- Added `ConcertHeldRecorder` for the performer's `ConcertHeldEvent`.
- Added typed tale dispatchers for `TaleDefOf.ConcertAttended` and `TaleDefOf.ConcertHeld`.
- Added `ConcertAttended` and `ConcertHeld` as Royalty-gated history record defs and `HistoryRecordDefOf` entries.
- Added `PH_ConcertAttended` and `PH_ConcertHeld` rulepacks with separate attendee and performer wording.
- Kept concerts separate from party records; concert attendees do not receive `PartyAttended`.

## Design

Royalty concert completion emits `ConcertAttended` and `ConcertHeld` tales from the successful outcome path. `ConcertAttendedDispatcher` maps the tale pawn to the attendee and the first tale parameter to the organizer. `ConcertHeldDispatcher` maps the tale pawn directly to the organizer.

The attendee recorder writes `ConcertAttended` on the attending pawn and attaches the organizer as a concern. The performer recorder writes `ConcertHeld` on the organizer without recomputing attendance from the lord.

## Rule Names

The rulepacks use PascalCase symbols such as `[Attender]` and `[Organizer]` instead of Royalty's uppercase tale symbols. They rename the vanilla `circumstance_group` concept to `circumstance_phrase`, convert concert tale text to past tense, and avoid randomized quantity claims.

## Verification

- Added recorder-local Royalty test coverage for successful concert completion.
- The test asserts the organizer `ConcertHeld` record, an attendee `ConcertAttended` record with organizer concern, and absence of a `PartyAttended` history record on concert attendees.
- Ran the Debug MSBuild build successfully after the tale-based concert migration.
- The in-game recorder test remains the runtime verification path for actual concert tale emission.
