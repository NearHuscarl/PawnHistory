# Quest Refugee Betrayal Offer Recorder

When the hospitality refugee quest reaches the betrayal-offer branch, the mod now records that moment on the opposing faction leader. The point of view is the political actor making the offer, while the visible quest context remains the original refugee hospitality quest that the player is actually following.

## Summary

Added a Royalty-gated `QuestRefugeeBetrayalOffer` history record driven by `QuestPart_AddQuest_RefugeeBetrayal.Notify_QuestSignalReceived`. The implementation reuses the same "a subquest was just created" detection pattern used by other quest-generation hooks, but it does not model this as quest discovery and it does not attach the hidden betrayal subquest to the record.

## Shipped Behavior

- The recorder fires only when the refugee betrayal offer path actually creates its hidden subquest.
- The record is written only on `factionOpponent`.
- Vanilla `FactionOpponentPawnParams` requires that pawn to be a non-hostile world pawn faction leader, so the record lands on the political actor making the offer.
- All refugee lodgers are attached as concerns because they are the concrete betrayal targets.
- Lodgers do not receive reciprocal records from this event.
- The record attaches the visible parent `Hospitality_Refugee` quest, not the hidden `RefugeeBetrayal` subquest.

## Design

- Added `QuestRefugeeBetrayalOfferEvent` as a dedicated typed event instead of extending `QuestDiscoveredEvent`, because the semantics are different.
- Patched `QuestPart_AddQuest_RefugeeBetrayal.Notify_QuestSignalReceived` with a small quest-count state object.
- The postfix only uses the newly generated hidden subquest as proof that the offer was created; it normalizes the published payload down to:
  - `FactionOpponent`
  - `Lodgers`
  - `RefugeeFaction`
  - `ParentQuest`
- `QuestRefugeeBetrayalOfferRecorder` resolves description text through a dedicated rulepack and writes a single `HistoryRecord`.

## Important Semantics

- This is not a "learned about a quest" event. It is a betrayal offer made by one faction leader against the refugee group.
- The player-facing quest reference must stay on the parent refugee hospitality quest. Using the hidden generated subquest here would expose the wrong quest context in history UI.
- `asker` from `QuestPart_AddQuest_RefugeeBetrayal` is not used as the record subject. In vanilla hospitality refugee generation that pawn is the refugee leader, not the opponent leader.

## Verification

- Added one deterministic Royalty recorder-local test that constructs `QuestPart_AddQuest_RefugeeBetrayal` directly on a real `Hospitality_Refugee` quest.
- The test asserts:
  - the opponent faction leader receives `QuestRefugeeBetrayalOffer`
  - the description matches the dedicated template
  - concerns contain all lodgers
  - the record points at the parent refugee quest
  - lodgers do not receive the record
- Ran the Debug `MSBuild` build successfully after the implementation.
