# QuestBuilder

Generates or wraps a quest, auto-accepts it when possible, then runs queued processors.

## Setup

- `scenario.Quest(Quest quest)`: start from an existing quest.
- `scenario.Quest(QuestScriptDef quest, float points = 500f)`: generate a quest from a script def and point budget.
- `WithQuest(Quest quest)`: replace the current quest source with an existing quest.
- `Pawn(Pawn pawn)`: store a pawn reference on the builder for quest-specific setup.
- `Do(Action<Quest> processor)`: queue a processor to run after the quest is accepted.
- `ChooseReward(Func<QuestPart_Choice.Choice, bool> filter)`: choose the first reward-pawn option inside the first matching choice part.

## Execution

- `Execute()`: generate the quest when needed, auto-accept it, run queued processors, and return it.
