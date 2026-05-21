# TestScenario

`TestScenario` is the recorder-test facade. It creates builders, exposes a few framework knobs, and provides tick/event helpers for delayed flows.

## Builder Entry Points

- `Pawn(int count = 1)`: create a `PawnBuilder` for one or more generated pawns.
- `Pawn(IEnumerable<Pawn>)`: wrap existing pawns in a `PawnBuilder` pipeline.
- `Pawn(Pawn)`: convenience overload for one existing pawn.
- `Thing(ThingDef thingDef, ThingDef stuffDef = null)`: create a `ThingBuilder`.
- `Map(Map map = null)`: create a `MapBuilder` for the given map or `Find.CurrentMap`.
- `Ideo()`: create an `IdeoBuilder`.
- `Incident(GatheringDef def)`: create a `GatheringBuilder`.
- `Incident(IncidentDef def)`: create an `IncidentBuilder` on the current map.
- `Incident(IncidentDef def, IIncidentTarget target)`: create an `IncidentBuilder` for a specific target.
- `RaidFriendly()`: convenience incident builder for a non-hostile raid.
- `Quest(Quest quest)`: wrap an existing quest in a `QuestBuilder`.
- `Quest(QuestScriptDef quest, float points = 500f)`: generate a quest through `QuestBuilder`.
- `Caravan(List<Pawn> pawns)`: create a `CaravanBuilder`.
- `DropPod(List<Thing> things)`: create a `DropPodBuilder` from item payload.
- `DropPod(List<Pawn> pawns)`: create a `DropPodBuilder` from pawn payload.
- `Shuttle(TransportShip transportShip)`: create a `ShuttleBuilder`.
- `Trade(Pawn negotiator)`: create a `TradeDealBuilder`.
- `Ritual(Pawn organizer)`: create a `RitualBuilder`.
- `Letter<T>()`: create a generic `LetterActionSimple<T>` for supported `ChoiceLetter` types.
- `LetterBabyToChild()`: create a `LetterActionBabyToChild`.
- `LetterGrowthMoment()`: create a `LetterActionGrowthMoment`.

## Helper Methods

- `OpenHistoryRecordTab(Pawn pawn)`: jump to the pawn and open the history inspect tab if it exists.
- `OutsideOf(string taggedRoom)`: return a random cell just outside a tagged room.

## Scenario State

- `NeverForceNormalSpeed`: stores and overrides the debug speed setting during accelerated tests.
- `LastRoomRect`: remembers the most recently queued room from `MapBuilder.BuildRoom(...)`.
- `TaggedRooms`: maps room tags to room rects for later spatial lookups.
- `ProcessedPawns`: tracks pawns already touched by the test pipeline.
- `AlwaysHaveCancerOnBirthday`: forces birthday flows to include cancer for birthday-specific tests.
- `ForcedRitualOutcome`: forces ritual outcome selection.
- `ForceRewardPawnInQuest`: forces a specific reward pawn into quest flows.
- `ForceInjuryScar`: forces injury healing to leave a scar.
- `ForcePostHealScar`: forces post-heal scar behavior.
- `AlwaysHaveHelpersInQuest`: forces helper-pawn presence in quest flows that support it.
- `DisableNamePlayerFactionDialog`: disable faction naming popup to avoid interrupting long-running tests.
- `RefugeeAlwaysAssaultOnViolation`: forces the refugee violation path into assault.
- `ForceSlaveRebellionType`: forces a specific slave rebellion type.
- `ForceSlaveRebellionViolent`: forces the violent rebellion branch when relevant.
- `PartyDuration`: overrides simulated party duration.
- `ForcedDebugMapSize`: fixed debug map size used by test plumbing.

Use these only when a builder does not already express the setup cleanly.

## Extension Helpers

- `ForwardDays(float day)`: move game time forward by whole or fractional days.
- `ForwardTicks(int ticks)`: move game time forward by ticks.
- `SpeedUp()`: force ultrafast speed and preserve the prior debug-speed setting.
- `SlowDown()`: restore normal speed and the previous debug-speed setting.
- `RunOnceOn<T>(Action<T> action)`: subscribe once to a `GameEventBus` event type, with timeout cleanup.
- `Loop(Action<ScheduledActionData> action, int interval = 1, int? timeout = null)`: run a repeating scheduled action until cancelled or timed out.
- `WaitUntil(Func<bool> condition, Action thenDo, int interval = 1)`: poll until a condition becomes true, then run one action.
