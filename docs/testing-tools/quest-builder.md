# QuestBuilder

Generates or wraps a quest, accepts it, then runs optional processors.

## Constructors

- `QuestBuilder(QuestScriptDef def = null, float points = 500)`: start a builder for a quest script.

## Setup

- `WithQuest(Quest quest)`: wrap an existing quest.
- `Pawn(Pawn pawn)`: store a pawn context for the test.
- `Do(Action<Quest> processor)`: run a processor after the quest is ready.
- `ChooseReward(Func<QuestPart_Choice.Choice, bool> filter)`: pick a reward choice by filter.

## Execution

- `Execute()`: generate or use the quest, accept it when possible, and return it.

## Notes

- When `questScriptDef` is set, `Execute()` generates the quest through `QuestUtility.GenerateQuestAndMakeAvailable(...)`.
- `Execute()` auto-accepts the quest so recorder tests can reach quest-driven recorders immediately.
