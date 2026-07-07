# Ideo Role Change Common Path

Ideology role changes have one notification point for the pawn-facing role mutation:
`Precept_Role.Notify_PawnAssigned` and `Precept_Role.Notify_PawnUnassigned`.
Role change rituals, role replacement, and direct unassignment eventually notify through those hooks.
The surrounding cause is supplied by `RitualOutcomeEffectWorker_RoleChange.Apply`, `Precept_RoleSingle.Assign`, or
`Precept_RoleSingle.RecacheActivity`.

This implementation moves history recording to the shared role notification hooks and keeps surrounding patches only as short-lived reason context.

## Summary

`IdeoRoleChangedEvent` now carries typed role data and a reason instead of pre-resolved role labels.
The ritual outcome worker no longer publishes the event directly. It sets a context frame with the role changer, old role,
target role, and spectators; the role notification patches publish when vanilla reports the actual role assignment or unassignment.

The `Assign` patch only supplies replacement context when an occupied single role is assigned to another pawn.
When this happens inside a ritual, it augments the active ritual frame instead of replacing it.
Low-believer role loss is handled by a focused `RecacheActivity` context patch for the deactivation branch.

## Shipped Scope

- Ritual role added, changed, and removed records keep their previous wording.
- A displaced role holder records that another pawn took the role.
- A low-believer role holder records that the ideology no longer had enough believers to keep the role active.
- Conversion-related role loss remains covered by ideology change or mental-break records and does not emit a separate role-loss record.
- Direct unoccupied role assignment remains out of scope because no pawn loses a role there.

## Design

- `Precept_Role.Notify_PawnAssigned` and `Precept_Role.Notify_PawnUnassigned` are the shared publishers.
- `RitualOutcomeEffectWorker_RoleChange.Apply` only supplies ritual reason context while vanilla applies the outcome.
- `Precept_RoleSingle.Assign` only supplies replacement context for occupied-role assignment by adding the replacement pawn to the active frame.
- `Precept_RoleSingle.RecacheActivity` only supplies low-believer context for the deactivation branch.
- Context patches save the previous frame, replace it only when they own a cause, and restore the previous frame in finalizers.
- The real nested case is role replacement during a role-change ritual: ritual `Apply` sets a ritual frame, then vanilla enters
  `Precept_RoleSingle.Assign` for the occupied target role. `Assign` preserves the ritual frame and adds the replacement pawn.
- Raw access to the protected role `active` flag lives in `Accessor`, keeping reflection out of the event file.

## Verification

- Added tests for role loss by replacement and role loss by low believer count.
- Build verification passed with 0 warnings and 0 errors:
  `MSBuild PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`
