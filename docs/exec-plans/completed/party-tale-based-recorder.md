# Party Tale-Based Recorder

Normal parties already emit RimWorld's Core `AttendedParty` tale from `LordJob_Joinable_Party.ApplyOutcome()`. Using that tale keeps the party recorder on the same event source as other tale-backed history records. Concerts were later moved to their Royalty `ConcertAttended` and `ConcertHeld` tales, so the shared party-completion event path is no longer part of the current design.

## Summary

`PartyRecorder` now consumes a typed `AttendedPartyEvent` dispatched from `TaleDefOf.AttendedParty`. It records the same `PartyAttended` history def, but the description can resolve either to the concise attendee wording or to a richer tale-style description through rulepack weights.

## Shipped Scope

- Added a typed tale dispatcher for `TaleDefOf.AttendedParty` with attender and organizer pawns.
- Converted `PartyRecorder` to `HistoryTaleRecorder<AttendedPartyEvent>`.
- Removed the need for the old party-versus-concert completion event path once concerts moved to Royalty tale dispatch.
- Expanded `PH_PartyAttended` with past-tense tale-style grammar based on Core `AttendedParty`.

## Design

The dispatcher receives the first tale argument as the attender and the second as the organizer, matching vanilla `ApplyOutcome()` where both organizer and attendees are emitted with the organizer supplied as the second pawn. `PartyRecorder` resolves `[Attender]` and `[Organizer]` directly without using vanilla uppercase tale subsymbols.

The rulepack uses weighted entries for attendee descriptions: the concise sentence has higher weight, while the richer tale-style text remains a rarer variant. Organizer records keep the concise host sentence.

## Rules

- Cancelled parties do not run `ApplyOutcome()`, so they do not produce `PartyAttended`.
- Party attendance records pass the organizer as a concern.
- Tale-style text avoids randomized quantity claims such as `[Quantity_adjphrase]`.
- Concert recording remains separate and writes `ConcertAttended` plus `ConcertHeld`.

## Verification

- Confirmed `TaleDefOf.AttendedParty` exists in RimWorld's runtime `TaleDefOf`.
- Confirmed via IL inspection that `LordJob_Joinable_Party.ApplyOutcome()` emits both attendee and organizer tales with the organizer as the second pawn.
- Ran Debug MSBuild after the event and recorder migration.
