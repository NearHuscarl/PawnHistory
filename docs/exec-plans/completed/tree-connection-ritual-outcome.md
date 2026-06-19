# Tree Connection Ritual Outcome

Gauranlen tree connection is an identity-shaping ritual for the connector pawn: it creates a persistent relationship between the pawn and a specific living tree, which later controls pruning work, dryad production, connection strength, and connection-loss consequences. The history entry should therefore record the successful connection itself, not the ritual's cosmetic moss growth.

## Summary
Added TreeConnection support to the shared ritual outcome recorder. AnimaTreeLinking and TreeConnection now each have their own ritual outcome comp that attaches the ritual tree as the record concern, and TreeConnection only records when RimWorld's outcome worker actually connects the selected Gauranlen tree to the connector pawn.

## Shipped Scope
- Added the TreeConnection outcome worker to the ritual outcome event hook.
- Resolved TreeConnection's host from the vanilla `connector` ritual role.
- Deduplicated ritual selected and obligation targets before publishing the event.
- Added separate ritual comps for AnimaTreeLinking and TreeConnection.
- Updated AnimaTreeLinking text to use the actual tree grammar rule and attach the anima tree concern.
- Preserved Funeral's obligation target behavior by selecting the deceased pawn from mixed ritual targets.

## Design
Tree-specific behavior lives in ritual-specific comps because both tree-linking rituals need a non-pawn concern and TreeConnection needs post-outcome validation. Each comp uses the exact tree `Thing` as a history concern while using the tree's `ThingDef` for grammar, keeping descriptions stable and UI concerns precise.

TreeConnection records only after `CompTreeConnection.ConnectedPawn` matches the connector. This skips malformed or failed paths where RimWorld ran the outcome worker but did not create the pawn-tree connection.

## Rules
- Do not record Gauranlen moss count; it is ritual-quality scenery rather than the pawn-facing milestone.
- Keep selected targets and obligation targets in the event. Some rituals, especially Funeral, need both the selected ritual focus and the obligation's subject.
- Deduplicate targets after collection so repeated selected/obligation references do not duplicate concerns.

## Verification
- Added comp-local TreeConnection test coverage through the real ritual begin/apply path.
- Moved AnimaTreeLinking coverage into its ritual comp, asserting the tree concern and preserving ordering before `PsylinkLevelGained`.
- Ran the approved Debug MSBuild build successfully with 0 warnings and 0 errors.
