# PawnLend Quest Reward Test

Implemented the `QuestPawnArrivedComp_PawnLend` recorder test as a Royalty-gated end-to-end quest flow.

## Notes

- Added a narrow `ShuttleBuilder` test helper for existing quest shuttles and transport ships instead of extending `DropPodBuilder`.
- Fixed the `PawnLend` grammar builder to use the shuttle `requiredColonistCount` for `RequiredCount`.
- The test now waits for the pickup shuttle to spawn, loads the required colonists, sends the shuttle away satisfied, asserts the reward pawn history record, and keeps running until the lent colonists are physically back on the colony map after the return shuttle arrives.
- Updated the author-facing testing docs so later quest-recorder tests can discover the new shuttle helper from the normal DSL index.
