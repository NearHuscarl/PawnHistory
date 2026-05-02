# Mental Break Recorder Comp Split

`MentalBreakRecorder` had accumulated two different responsibilities: generic mental-break recording and per-break data assembly. That made the recorder harder to extend safely, especially for one-off branches like jailbreak prisoners, drug binge chemicals, and wild decree quests. This change keeps the recorder generic and moves one-off mental-break-specific grammar into focused comps.

## Summary

The recorder now owns the shared mental-break pipeline:

- record eligibility and unsupported-cause filtering
- generic `name`, `Reason`, and `Target` grammar
- generic concerns from `causedByPawn` and `target`
- generic `InGameDesc` fallback for unsupported named templates, including unknown modded mental states when they expose begin-letter text

Break-specific one-off grammar now lives in dedicated comps:

- `RunWild` adds `Faction`
- `Binging_DrugExtreme` and `Binging_DrugMajor` add `Drug`
- `TargetedTantrum` adds `Thing` and the tantrum target concern
- `Jailbreaker` adds `Prisoners` and all prisoner concerns
- `WildDecree` adds `Quest`

## Shipped Scope

- Added `MentalBreakComp` as the shared comp abstraction for mental breaks.
- Refactored `MentalBreakRecorder.CreateRecord(...)` into a generic builder-plus-comp flow.
- Removed the old `HasCustomDescription(...)` branch split.
- Kept simple named templates in the main recorder when they only need generic data.
- Moved focused tests for comp-owned behavior next to the new comp classes.
- Added a fallback-path test using `HumanityBreak` to cover a named-unknown mental break route driven by `InGameDesc`.

## Design

The important rule is that the main recorder owns only generic mental-break data. Any extra grammar or concern that is not part of the shared contract moves into a comp. `Target` remains generic because multiple supported mental breaks share it; one-off additions like `Faction`, `Drug`, `Thing`, `Prisoners`, and `Quest` do not.

This keeps mod support intact. Unknown or newly added mental states are still recordable through the generic fallback path as long as the live mental state exposes begin-letter text.

## Rules

- Existing supported mental-break output is intended to remain unchanged.
- `WildDecree` is the one approved behavior change in this pass: the recorder now supplies `[Quest]`, allowing the existing named rule to resolve as intended.
- `PanicFleeFire`, `SocialFighting`, and `MentalBreakCause.Other` remain outside the supported recording path.

## Verification

- Added comp-local tests for `RunWild`, both drug-binge variants, `TargetedTantrum`, `Jailbreaker`, and `WildDecree`.
- Kept the bulk recorder test focused on the non-comp cases.
- Added a recorder test for generic `InGameDesc` fallback.
- Built the project with `MSBuild` in `Debug` configuration.
