# Sky Lantern Ritual Outcome

Sky Lantern festivals are an Ideology ritual pattern, not a standalone ritual precept. The game creates a normal `Festival` ritual precept and fills it with the `CelebrationSkyLanterns` pattern, which changes the ritual behavior, target, outcome effect, and generated label while keeping the precept def as `Festival`.

## Summary

PawnHistory now has deterministic test coverage for Sky Lantern ritual outcomes through the existing festival participant recorder path. Participants receive the same generic festival-style history entry as other social festival variants, matching the chosen product behavior.

## Shipped Scope

- Added DefOf access for `RitualPatternDef` `CelebrationSkyLanterns` and `RitualOutcomeEffectDef` `CelebrationSkyLanterns`.
- Extended the ideology test builder so a generic precept can be filled with a specific ritual pattern.
- Added a Sky Lantern ritual test helper that selects the ritual by `Precept_Ritual.sourcePattern`.
- Added recorder-local Sky Lantern coverage in `RitualOutcomeComp_Festival`.

## Design

No new recorder comp was added. The existing festival comp remains the correct owner because Sky Lanterns record on every participant with the same generic wording as social festivals, drum parties, and dance parties.

The key distinction is generation: there is no `SkyLantern` `PreceptDef` to match. Matching or testing it as a precept would encode the wrong game model. Tests create a `Festival` precept filled with `CelebrationSkyLanterns` and start that filled ritual from a ritual spot.

## Rules

- Match Sky Lantern behavior by ritual pattern or outcome effect, not by a fake precept def.
- Keep the runtime recorder path under `RitualOutcomeComp_Festival` while the player-facing wording remains generic.
- Force a negative Sky Lantern outcome in tests to avoid the random friendly-visitor extra outcome attached to positive Sky Lantern results.

## Verification

- Built with `MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`; build succeeded with 0 warnings and 0 errors.
- Added the in-game recorder test, but did not run the RimWorld test harness from this shell.
