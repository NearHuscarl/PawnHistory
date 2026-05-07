# Ideo Changed Recorder

Ideology changes are story beats when they come from conversion, colony restarts, or rituals. They explain shifts in conviction and identity that players can read as part of a pawn's biography instead of as hidden simulation churn.

## Summary

Shipped a new Ideology-gated `IdeoChanged` recorder backed by a single publication hook in `Pawn_IdeoTracker.SetIdeo`. Real ideology transitions now publish a typed `IdeoChangedEvent` with old/new ideos, reason, and optional converter. The recorder writes player-facing history for the converted pawn, mirrors the event to the converter when appropriate, and blacklists internal-only reasons that already have better dedicated coverage.

## Shipped Scope

- Added `IdeoChangedEvent` and `IdeoChangeReason`.
- Published the event only from `Pawn_IdeoTracker.SetIdeo`.
- Classified upstream call sites into:
  - `ConvertedByAbility`
  - `ConvertedByInteraction`
  - `NewColony`
  - `ConvertedByConversionRitual`
  - `ConvertedBySpeechRitual`
  - `Unknown`
- Added internal blacklist-only reasons:
  - `MentalBreak`
  - `NewColonyPreview`
- Added `IdeoChangedRecorder`, `HistoryRecordDefOf.IdeoChanged`, XML def, and `PH_IdeoChanged` rulepack.
- Added recorder-local Ideology tests and the smallest dialog test helper needed to drive the new-colony `"Next"` commit path.

## Design

### Single publication point

`Pawn_IdeoTracker.SetIdeo` is the only publisher. The patch captures `oldIdeo` in `Prefix` and publishes in `Postfix` only when the call produced a real ideology change. This keeps the recorder attached to the literal state transition instead of duplicating logic across ability, interaction, ritual, and UI code.

### Reason context stack

Upstream callers push a transient reason/converter frame before they enter the real game code path, and pop it in `Finalizer()`. That keeps nested calls deterministic and lets `SetIdeo` stay dumb:

- ability convert -> `ConvertedByAbility`
- social conversion attempt -> `ConvertedByInteraction`
- conversion ritual outcome -> `ConvertedByConversionRitual`
- speech ritual outcome -> `ConvertedBySpeechRitual`
- ideology mental break -> `MentalBreak`
- Archonexus restart configure dialog -> `NewColony`
- direct colonist chooser apply path -> `NewColonyPreview`

Anything else falls back to `Unknown`.

### Recorder rules

- `MentalBreak` and `NewColonyPreview` publish the event but do not create `IdeoChanged` history.
- The converted pawn gets the record when `ShouldRecord` passes.
- The converter gets a mirrored record only for ability and interaction conversions.
- Ritual conversions still attach the converter as a concern on the converted pawn's record, but do not create a converter-side `IdeoChanged` entry. A future ritual-specific event can own that story.
- `oldIdeo` may be null; the rulepack has explicit no-previous-ideology branches instead of inventing fake old beliefs.
- `Unknown` uses `priority=0` generic fallback text.

## New Colony Test Strategy

The new-colony coverage intentionally avoids recreating three Archonexus quest setups. The test opens the real `Dialog_ConfigureIdeo` through `QuestPart_NewColony.TileChosen`, replaces the dialog's `nextAction` with a no-op, seeds one pending pawn conversion, and auto-clicks the real `"Next"` button through a tiny reusable test hook for `Widgets.ButtonText`. That keeps the test on the real commit path without firing the actual colony-move side effects.

## Verification

Built coverage was added for:

- conversion ability
- social conversion attempt
- Archonexus new-colony commit
- conversion ritual
- speech ritual
- unknown fallback
- mental-break blacklist behavior

Shell verification:

- `MSBuild` debug build succeeded for `PawnHistory.csproj`.

Limitations:

- The repository does not expose a shell-runnable in-game recorder test harness, so recorder tests were added but not executed from the shell.
