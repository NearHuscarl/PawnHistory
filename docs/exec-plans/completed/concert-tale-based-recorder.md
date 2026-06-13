# Concert Tale-Based Recorder

Royalty concerts already have two tale concepts: attending a performance and holding one. Recording those tales directly keeps concert history tied to the successful game outcome and lets the history log distinguish the performer from the audience without a custom party completion event.

## Summary

Concert history now uses tale-based dispatch. `ConcertAttended` writes records on audience members with the organizer as a concern, while `ConcertHeld` writes a separate performer record on the organizer. The old shared party-attended event path was removed.

## Shipped Scope

- Added `ConcertAttendedEvent` and `ConcertHeldEvent` dispatchers for `TaleDefOf.ConcertAttended` and `TaleDefOf.ConcertHeld`.
- Converted `ConcertRecorder` to `HistoryTaleRecorder<ConcertAttendedEvent>`.
- Added `ConcertHeldRecorder` as the performer-side recorder.
- Replaced the single concert history def with `ConcertAttended` and `ConcertHeld`.
- Added `PH_ConcertAttended` and `PH_ConcertHeld` rulepacks based on Royalty's concert tales.
- Removed the custom party-attended completion event and its party-versus-concert type tag.

## Design

The tale adapter receives the tale pawn separately from the remaining tale arguments. For `ConcertAttended`, the tale pawn is the attendee and the first remaining argument is the organizer. For `ConcertHeld`, the tale pawn is the organizer.

Both recorders keep the Royalty gate in `Register()`. Both use `ShouldRecordTale(...)` so repeated same-description tales follow the existing tale recorder overlap and date checks. Attendance is not recomputed from the concert lord.

## Rule Text

The concert rulepacks follow the updated party rulepack style:

- text is past tense for history entries
- Royalty's uppercase `ATTENDER` and `ORGANIZER` subsymbols are not used
- symbols use PascalCase names such as `[Attender]` and `[Organizer]`
- `circumstance_group` is represented as `circumstance_phrase`
- richer tale-style descriptions are weighted with `p=1` against the concise `p=9` entry
- randomized quantity claims are replaced with scene details that do not assert false crowd size or quality

## Verification

- Updated the recorder-local concert test to assert `ConcertHeld` for the organizer.
- Updated the same test to assert `ConcertAttended` for an attendee with the organizer concern.
- Kept the assertion that concert attendees do not receive `PartyAttended`.
- Ran the Debug MSBuild build successfully after the migration.
- Did not run the in-game recorder test from the shell; it remains the runtime check for actual concert tale emission.
