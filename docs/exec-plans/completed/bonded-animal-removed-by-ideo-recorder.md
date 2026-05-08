# Bonded Animal Removed By Ideology Recorder

When a pawn changes ideoligion into one that forbids animal bonding, RimWorld silently strips the bond relation and only surfaces it in a letter. That is a meaningful identity and relationship change for both sides of the bond, and it can otherwise disappear from history entirely for the animal because the animal stops qualifying as a bonded pawn immediately after the relation is removed.

## Summary
Added a dedicated ideology bond-removal event and recorder that mirrors the `DivorceByIdeo` pattern: one `HistoryRecordDef`, two point-of-view descriptions, one record on the human pawn, and one final record on each removed bonded animal.

## Shipped Scope
- Added a `Pawn_IdeoTracker.SetIdeo` patch that snapshots bonded animals before ideology mutation and publishes only the removed animals afterward.
- Added `BondRemovedByIdeoRecorder` with a human POV branch and a bonded-animal POV branch.
- Added the ideology-gated `HistoryRecordDef`, `HistoryRecordDefOf` entry, and relationship rulepack text.
- Added a recorder-local ideology test that exercises the real `SetIdeo` path.

## Design
The event payload follows the same shape as `DivorceByIdeoEvent`: the acting human pawn plus the list of affected relationship counterparts. It intentionally does not carry ideology data because the recorder text does not need it and the divorce implementation does not either.

The recorder follows the same write pattern as `DivorceByIdeoRecorder`:
- human record gets the shared def, human POV text, and all removed animals as concerns
- each removed animal gets the shared def, bonded-animal POV text, and the human as the single concern

## Rules
- Human-side writes still respect `ShouldRecord(...)`.
- Animal-side writes intentionally do not check `ShouldRecord(...)`, because the ideology change removes the bond before the recorder runs and this is meant to be the animal's last bond-related history entry.
- The trigger is limited to the actual `SetIdeo` bond-removal path rather than inferring from broader ideology-change events.

## Verification
- Added a recorder-local test that bonds a colonist to a husky, switches the colonist to an ideology with `Bonding_Disapproved`, and verifies both the human and animal records.
- Build verification was run after the implementation.
