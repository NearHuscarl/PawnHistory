# Rare Catch Recorder

Rare fishing catches are intentionally uncommon, player-visible moments in Odyssey fishing. RimWorld already calls them out with the `LetterLabelRareCatch` and `LetterTextRareCatch` letter, so PawnHistory records the same event as a work-history milestone for the pawn who made the catch.

## Summary

Added an Odyssey-gated `RareCatchRecorder` that records a history entry when human fishing returns a true rare catch. The recorder only subscribes while Odyssey is active, the defs are also `MayRequire`-gated, and the entry follows vanilla rare-catch wording while attaching the exact caught `Thing` objects as record concerns so the history entry points back to what was pulled from the water.

## Shipped Scope

- Added `RareCatchEvent` from `FishingUtility.GetCatchesFor(...)`, publishing only when `rare` is true, the call is not animal fishing, and the returned catch list is non-empty.
- Added `RareCatchRecorder`, `HistoryRecordDefOf.RareCatch`, a `RareCatch` history def, and `PH_RareCatch` rulepack text.
- Added a `ForcedRareCatch` test-scenario property and mock patch that makes `FishingUtility.GetCatchesFor(...)` return a deterministic rare catch for recorder tests.
- Added one Odyssey-gated recorder test that forces `silver x50` as the rare catch and asserts the record def, vanilla-shaped description, and concern attachment.

## Design

The Harmony hook lives at `FishingUtility.GetCatchesFor(...)` because that method owns the rare-catch decision and returns the caught things before `JobDriver_Fish` turns them into a letter. The event copies the result list immediately because RimWorld uses a static temporary catch list internally.

The test calls the same utility method instead of publishing on `GameEventBus` directly. A full fishing job would require water-body, fishing-zone, biome, cooldown, and random-roll setup that would not improve coverage of the recorder contract for this slice. The mock keeps the test deterministic while still exercising the production hook and recorder subscription.

## Rules And Exclusions

- Animal fishing is excluded because vanilla rare catches are disabled for `animalFishing`.
- Normal fish catches, negative fishing outcomes, and empty rare-roll results do not record.
- The recorder does not depend on the letter stack; it mirrors vanilla wording rather than intercepting the rare-catch letter.
- The record is written only for pawns accepted by `RecorderManager.ShouldRecord(...)`.

## Verification

- Built with `MSBuild /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`: passed with 0 warnings and 0 errors.
- The in-game `RareCatch` recorder test was added but was not run in this session.
