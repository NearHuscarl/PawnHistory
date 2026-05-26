# Pregnancy Termination And Contraception History

This work makes fertility-related medical history read the way players actually interpret it in-game. Pregnancy termination is now a first-class history moment instead of disappearing into generic surgery handling, and IUD procedures now have their own recorder and rulepack path instead of being folded under vasectomy-oriented naming.

## Summary

The mod now records `PregnancyTerminated` for the dedicated `TerminatePregnancy` surgery, with both success and botched descriptions driven through the existing surgery recorder pipeline. `Sterilized` and `ReverseVasectomy` keep their original naming and scope, while IUD implant and removal now record through dedicated `IudImplanted` and `IudRemoved` history definitions, comps, and rulepacks.

## Shipped Scope

- Added a new `PregnancyTerminated` history record and rulepack.
- Routed `Recipe_TerminatePregnancy` into the existing surgery event patch so the recorder can use doctor, failure outcome, and injury context without introducing a custom surgery payload type.
- Restored the original `Sterilized` and `ReverseVasectomy` history family names.
- Added dedicated `IudImplanted` and `IudRemoved` history records and rulepacks.
- Split IUD implant and removal into their own surgery comps.
- Gave `IudImplanted` explicit implant wording so the separate IUD rulepack is not just a duplicate of the generic sterilization template.
- Added a dedicated surgery event data source for `Recipe_ImplantIUD` so the custom worker still produces `SurgeryAddHediffData` for the IUD recorder path.
- Patched `Recipe_ImplantIUD.ApplyOnPawn` directly because it overrides `Recipe_AddHediff.ApplyOnPawn`; patching only the base add-hediff worker would miss the implant surgery entirely.
- Kept IUD removal on the existing `Recipe_RemoveHediff` hook because `RemoveIUD` uses `Recipe_RemoveHediff` as its worker class rather than a custom `Recipe_RemoveIUD`.
- Added recorder-local tests for IUD success paths, IUD removal failure text, and pregnancy termination success and failure.

## Design

`PregnancyTerminated` intentionally uses the surgery recorder path instead of a separate pregnancy event hook. The dedicated recipe is the narrowest truthful entrypoint for the player-facing event, and the surgery pipeline already carries the botched-surgery context needed for severity and wound text.

The new pregnancy record does not carry recipe-specific payload data. Matching is done by recipe identity, and description text is fully rulepack-driven.

Separate IUD history defs are required by the current description system. `descriptionMaker` is attached to `HistoryRecordDef`, so a truly separate IUD rulepack cannot share the same record definition as the vasectomy and sterilization histories.

## Rules

- `PregnancyTerminated` only covers the explicit `TerminatePregnancy` surgery.
- `ForceEndPregnancy(...)` is intentionally ignored.
- `Sterilized` still covers tubal ligation and vasectomy.
- `ReverseVasectomy` still covers vasectomy reversal.
- `IudImplanted` and `IudRemoved` are separate history records with separate rulepacks.
- Fertility surgery records continue to attach only the doctor as a concern.

## Verification

- Built `PawnHistory.csproj` in `Debug` with MSBuild.
- Added recorder-local coverage for:
  - tubal ligation and vasectomy success
  - vasectomy reversal success
  - IUD implant success and sterilized-failure text
  - IUD removal success and sterilized-failure text
  - pregnancy termination success
  - pregnancy termination botched surgery text
