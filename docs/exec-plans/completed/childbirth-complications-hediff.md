# Childbirth Complications Hediff

Childbirth death is a casualty event, but RimWorld's death log can leave PawnHistory with only the mother's newest health conditions to explain the death. After childbirth, lactation is also new, so the fallback death text could incorrectly read as if lactation caused the death.

## Summary
Added a hidden `ChildbirthComplications` hediff that is attached to the birthing pawn only when childbirth has killed her. The casualty recorder can then resolve the fallback death reason as childbirth complications instead of selecting lactating from the same birth flow.

## Shipped Scope
- Added a Biotech-gated hidden hediff def named `ChildbirthComplications`.
- Added `Extra.HediffDefOf.ChildbirthComplications`.
- Updated the childbirth outcome postfix to add the marker after the carrier is dead.
- Extended the existing natural childbirth death test to assert both the marker and the casualty death text.

## Rules
- Normal healthy, sick, and stillborn births do not add this marker to a living carrier.
- The hediff is hidden and inert; it exists to preserve history semantics, not to add player-facing health content.
- The marker is attached before the current-tick priority record flush resolves casualty fallback text.

## Verification
Ran the Debug MSBuild build successfully:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe' PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false
```
