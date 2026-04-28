# Ancient Uplink Quest Discovery

Implemented the `QuestDiscovered` uplink path for Odyssey by patching `CompAncientUplink.Notify_Hacked(...)` and publishing a `QuestDiscoveredEvent` only when a hacker pawn exists and the call created a new quest.

## Notes

- Extended `QuestDiscoveredSource` with `Uplink` and reused the existing `QuestDiscoveredRecorder` and `QuestDiscovered` history record def.
- Added an uplink-specific `PH_QuestDiscovered` rulepack entry so the record reads as a pawn discovering a quest while hacking an uplink.
- Added `Extra.ThingDefOf.AncientUplink` to keep Odyssey thing lookup on the repo's `DefOf` path.
- Added one Odyssey-gated recorder-local test that calls `CompAncientUplink.Notify_Hacked(hacker)` and asserts the hacker receives the expected history record with the uplink concern and generated quest attached.
