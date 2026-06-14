# Prisoner Escaped Recorder

Prisoners can escape without starting a prison break when the game decides they have an open path out of captivity. That story is different from a coordinated lock-breaking prison break: it is a failure of containment, and the useful history detail is where the prisoner was standing when they saw the opportunity.

## Summary

Implemented `PrisonerEscapedRecorder` for the `JobGiver_PrisonerEscape` path. The recorder writes a prisoner-only history record when RimWorld gives a prisoner a `Goto` job with `exitMapOnArrival`, and it stores the pawn's map position at the moment the escape job is produced.

## Shipped Scope

- Added `PrisonerEscapedEvent` from a postfix on `JobGiver_PrisonerEscape.TryGiveJob`.
- Added `PrisonerEscapedRecorder` with a local recorder test that lets the pawn AI trigger the real job-giver path.
- Added `PrisonerEscaped` history def and `PH_PrisonerEscaped` rulepack text.

## Rules

- This recorder is separate from `PrisonBreakRecorder`; it does not listen to `PrisonBreakUtility.StartPrisonBreak`.
- The record belongs only to the escaping prisoner and has no concern pawns.
- Location is captured before the pawn reaches the map edge, using the pawn's current spawned map and position when the escape job is returned.

## Verification

- Built with `MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`.
