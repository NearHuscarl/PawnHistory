# Xenogerm Implanted Recorder

Xenogerm implantation is a lasting identity change. A pawn keeps the xenotype name, icon, and xenogenes from the implanted xenogerm, so the history log should preserve the moment as more than a generic surgery outcome.

## Summary

Added a Biotech-gated `XenogermImplanted` history record emitted from `GeneUtility.ImplantXenogermItem()`. The record is written on the pawn being implanted, names the implanted xenotype, summarizes the most mechanically defining genes, and can mention the pawn's previous built-in xenotype without treating custom-to-custom xenogerm replacement as a race change.

## Shipped Scope

- Added `XenogermImplantedEvent` with the implanted pawn, xenotype name, xenotype icon path, a snapshot of the xenogerm gene defs, and prefix-captured old xenotype state.
- Added `XenogermImplantedRecorder`, which ranks genes by a simple impact score and formats the ranked list in the description.
- Added `XenogermImplanted` to `HistoryRecordDefOf`, `Defs/HistoryRecord.xml`, and the Biotech misc rulepacks.
- Added Biotech test-builder helpers for assigning built-in and custom xenotypes, plus a `ThingBuilder.MakeXenogerm()` helper for reusable xenogerm construction.

## Rules

- The event publishes only after `ImplantXenogermItem()` succeeds with a valid pawn gene tracker and a non-empty xenogerm gene set.
- The record subject is only the implanted pawn.
- `[OldXenotypeName]` is captured before implantation only when the pawn's old gene tracker is not a unique/custom xenotype. Built-in xenotypes such as baseliner or dirtmole are eligible; custom-to-custom replacement uses the generic rule.
- Gene impact currently ranks by `biostatArc * 100 + biostatCpx + Abs(biostatMet)`, descending, with label as a deterministic tie-breaker.

## Verification

- Added recorder-local Biotech tests that call the real `GeneUtility.ImplantXenogermItem()` path, assert old built-in xenotype wording, assert custom old xenotypes are ignored, and check that a lower-impact fourth gene is omitted from the summary.
