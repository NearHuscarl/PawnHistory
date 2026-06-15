# Slave Rebellion Test Map Size

The slave rebellion recorder tests rely on real RimWorld rebellion participation rules, not a mocked participant list. That means the debug map size is part of the test seam: if the map is too small, a pawn that is meant to be "far away" is still close enough to join a local rebellion, and the recorder will correctly produce grand-rebellion wording instead.

## Summary
Added a recorder-test `DebugMapSize` attribute and used `DebugMapSize(30)` on the slave rebellion local-rebellion tests so they run on a map large enough to keep the "far away" slave outside RimWorld's `35f` local-rebellion radius.

## Shipped Scope
- Added `DebugMapSizeAttribute` in the test infrastructure.
- Taught the test runner to accept an optional per-test map size and instantiate `TestScenario` with that value before `GameUtility.CreateTestGame(...)`.
- Kept the global default `TestScenario.ForcedDebugMapSize` at `25`.
- Applied `DebugMapSize(30)` to the local slave rebellion and jailbreaker-local slave rebellion tests.
- Left the slave rebellion recorder, event payload, and rulepack text unchanged.

## Design
RimWorld's local slave rebellion path includes nearby eligible slaves within `35f` cells of the initiator. The existing test places one slave at `(0, size - 1)` and another at `(size - 1, 0)`, so the corner-to-corner distance is:

`sqrt(2) * (size - 1)`

At `25`, that distance is about `33.94`, so the "far away" slave still joins and the recorder resolves a grand rebellion description.

At `30`, that distance is about `41.01`, which safely exceeds the local-rebellion cutoff while keeping the debug map small.

## Rules
- This change is test-only. Runtime rebellion classification and history text generation were not altered.
- The fix intentionally preserves the existing test setup instead of rewriting the recorder or carrying extra rebellion-type data through the event.
- Tests without `DebugMapSizeAttribute` continue to use the default `25x25` debug map.

## Verification
- Ran the Debug MSBuild build successfully.
- Did not run the in-game recorder test runner from this shell; that remains a manual RimWorld verification step.
