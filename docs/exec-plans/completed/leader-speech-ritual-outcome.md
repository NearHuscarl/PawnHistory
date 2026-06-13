# Leader Speech Ritual Outcome

Leader speeches are ideology leadership moments: one pawn addresses the colony, and the outcome can shape listener mood, respect, inspiration, and occasional conversion. PawnHistory should remember the speaker's public leadership act without spraying duplicate ritual records onto every listener.

## Summary
Added explicit `LeaderSpeech` text support to the shared ritual-outcome recorder. The event path remains the same as throne speech because both rituals use RimWorld's speech outcome worker and publish through `RitualOutcomeCompletedEvent`.

## Shipped Scope
- Recorded leader speeches on the speaker/leader only.
- Reused the existing `RitualOutcome` history def and `PH_RitualOutcome` rulepack.
- Added test support for starting a leader speech through the real ability and ritual dialog path.
- Preserved runtime ritual labels by continuing to use `RitualOutcomeCompletedEvent.RitualLabel`.

Explicitly excluded:
- No listener-side `RitualOutcome` records.
- No new `RitualOutcomeComp`, because leader speech does not need special concern attachment, participant-wide recording, or custom subject extraction.
- No custom ritual-name handling beyond the existing RimWorld `Precept_Ritual.Label` value.

## Design
`LeaderSpeech` uses `RitualOutcomeEffectWorker_Speech`, which is already patched by `RitualOutcomeCompletedEvent`. RimWorld's `RitualBehaviorWorker_Speech` resolves the actual speaker from the assigned `speaker` role and stores that pawn as the ritual lord job organizer. The existing event and recorder therefore already identify the correct pawn.

The only recorder behavior needed was a specific rulepack branch so the history sentence mirrors throne speech:

`[PAWN] delivered [Outcome_indefinite] [Ritual] to [Others].`

The test helper mirrors `ThroneSpeech`, but uses the Ideology `LeaderSpeech` ability and requires an ideology ritual focus such as an ideogram or ritual spot.

## Verification
Added recorder-local coverage that:
- creates an ideology with the `LeaderSpeech` precept
- assigns the organizer as ideology leader
- starts the ritual through the real ability/dialog path
- forces the best `AttendedSpeech` outcome
- asserts the leader receives the expected ritual outcome record
- asserts spectators do not receive ritual outcome records

Ran the Debug MSBuild build successfully after the implementation.
