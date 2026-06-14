 Role Change Ritual Outcome

## Summary

Role change rituals use a different vanilla flow from the shared `IdeoRoleChangedEvent` path. They decide success through `RitualOutcomeEffectWorker_RoleChange.GetForcedOutcome(...)` and then mutate the pawn's ideology role inside `Apply`.

The recorder is therefore separate from generic ritual outcomes. It observes the RoleChange worker directly, diffs the pawn's role before and after vanilla `Apply`, and records a dedicated `IdeoRoleChanged` history entry.

## Behavior

- No role to first role records that the pawn became the new role.
- Existing role to another role records the old and new roles.
- Existing role to no role records that the pawn gave up the old role.
- No role change after the ritual records the failed outcome text.

Vanilla allows no-role to first-role assignment. When a pawn with no current role is assigned as the role changer, `RitualRoleAssignments.UpdateRoleChangeTargetRole` selects the first valid active role.

## Design

- `IdeoRoleChangedEvent` stays unchanged.
- `IdeoRoleChangedEvent` is published by a prefix/postfix patch on `RitualOutcomeEffectWorker_RoleChange.Apply`.
- The prefix captures the role changer and old role.
- The postfix reads the pawn's current role after vanilla logic and publishes whether the role actually changed.
- `IdeoRoleChangedRecorder` writes `IdeoRoleChanged` using a dedicated `PH_IdeoRoleChanged` rulepack.

## Testing

- `ForcedRitualOutcome` now patches `RitualOutcomeEffectWorker_RoleChange.GetForcedOutcome`, so tests can force successful or failed RoleChange outcomes.
- `RitualBuilder.RoleChange(...)` only configures and starts the real ritual path. It does not duplicate outcome logic or seed attendance data.
- Recorder tests cover first role, role replacement, role removal, and forced failure.

## Verification

Build verification was run after implementation with:

`MSBuild PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`
