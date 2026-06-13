# Trial Ritual Outcome Comp

Trial rituals are accusation stories with two central pawns: the judge who prosecutes the case and the convict who is accused. The history record should preserve that relationship for both pawns without recording every spectator as a trial subject.

## Summary
Added Trial support to the shared ritual outcome recorder. Base `Trial`, `TrialPrisoner`, and `TrialMentalState` now record the same ritual outcome description for the judge and convict only, with the other trial pawn retained as the stored concern after `HistoryRecord` filters out the owner pawn.

## Shipped Scope
- Added Trial outcome capture for `RitualOutcomeEffectWorker_Trial.Apply` and its overridden `GetOutcome`.
- Added a dedicated `RitualOutcomeComp_Trial` that matches all three Trial precepts.
- Added Trial-specific `PH_RitualOutcome` rules: `[Convict] was [Outcome] in [Judge]'s [Ritual][InFrontOfOthers].`
- Added DefOf entries and a `convict` ritual role id needed by the recorder and tests.
- Added `RitualBuilder.Trial(...)` for recorder tests using the real leader trial ability path.

## Design
The comp owns the Trial-specific actor selection. It records exactly the `leader` role pawn and the `convict` role pawn, while returning both pawns as concerns for both records. `HistoryRecord` removes the record owner from the concern list, so the judge record stores the convict and the convict record stores the judge without conditional concern logic in the comp.

Base `Trial` is included because RimWorld uses it for normal targets. `CompAbilityEffect_StartTrial` switches to `TrialMentalState` for pawns in mental states, to `TrialPrisoner` for colony prisoners, and otherwise falls back to base `Trial`.

## Rules
- Do not record spectators.
- Use one shared description for judge and convict, not mirrored POV text.
- Keep variant selection in test setup aligned with RimWorld's target priority: mental state, prisoner, base Trial.
- Keep Trial under `RitualOutcome`; no separate history record def is introduced.

## Verification
- Added recorder-local tests for base Trial, prisoner Trial, and mental-state Trial.
- Ran Debug MSBuild successfully:
  `MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`
- Checked the touched files with `git diff --check`; only CRLF normalization warnings were reported. A pre-existing trailing-whitespace warning remains in `Source/PawnTracker/Events/TaleEventAdapter.cs`.
- The in-game recorder tests were added but not run from this shell session.
