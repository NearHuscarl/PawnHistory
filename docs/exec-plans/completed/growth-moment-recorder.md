# Growth Moment Recorder

Implemented a Biotech-gated `GrowthMomentRecorder` that records the completed outcome of child growth-moment letters.

## Notes

- The Harmony hook is `ChoiceLetter_GrowthMoment.MakeChoices(...)`, which is the literal game moment when the selected passions and trait are confirmed.
- The event publishes only for live growth letters that actually complete a choice, and normalizes the special `NoTrait` selection to `null`.
- The history record only describes gained passions and/or the gained trait. It intentionally does not include indirect skill-level bonuses caused by the trait.
- Extended `LetterAction<TLetter>` with `CompleteGrowthMoment(...)` so recorder tests can complete real growth letters without driving the UI dialog.
- Added recorder-local Biotech tests for trait-only, trait plus one passion, trait plus many passions, and passion-only outcomes.
