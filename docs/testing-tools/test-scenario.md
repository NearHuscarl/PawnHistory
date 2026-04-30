# TestScenario

`TestScenario` is the facade that recorder tests receive. It creates the builder objects and exposes a small amount of scenario state used by the test framework.

## Common Entry Points

- `Pawn(int count = 1)`: create or reuse pawns through `PawnBuilder`.
- `Pawn(IEnumerable<Pawn>)` and `Pawn(Pawn)`: wrap existing pawns into a builder pipeline.
- `Thing(ThingDef, ThingDef stuffDef = null)`: create `ThingBuilder`.
- `Map(IntVec3? pos = null)`: create `MapBuilder`.
- `Incident(GatheringDef)`: create `GatheringBuilder`.
- `Incident(IncidentDef)` and `Incident(IncidentDef, IIncidentTarget)`: create `IncidentBuilder`.
- `Quest(Quest)` and `Quest(QuestScriptDef, float points = 500f)`: create `QuestBuilder`.
- `Caravan(List<Pawn>)`: create `CaravanBuilder`.
- `DropPod(List<Thing>)` and `DropPod(List<Pawn>)`: create `DropPodBuilder`.
- `Trade(Pawn negotiator)`: create `TradeDealBuilder`.
- `Ritual(Pawn organizer)`: create `RitualBuilder`.
- `Letter<T>() where T : ChoiceLetter`: create `LetterAction<T>`.
- `RaidFriendly()`: convenience for a friendly raid incident.

## Helper Methods

- `OpenHistoryRecordTab(Pawn pawn)`: opens the history UI for manual/debug-oriented checks.
- `OutsideOf(string taggedRoom)`: finds a position outside a tagged room created by `MapBuilder`.

## Scenario State

These members are framework knobs. Use them only when a builder does not already express the setup cleanly.

- `NeverForceNormalSpeed`
- `LastRoomRect`
- `TaggedRooms`
- `ProcessedPawns`
- `AlwaysHaveCancerOnBirthday`
- `ForcedRitualOutcome`
- `ForceRewardPawnInQuest`
- `ForceInjuryScar`
- `ForcePostHealScar`
- `ForcedDebugMapSize`

`DeathOnNextHitPawns` exists for builder plumbing and should normally be driven through `PawnBuilder.DiesOnNextHit()`.

## Time Helpers

`TestScenarioExtensions` provides:

- `ForwardTime(float day)`
- `ForwardTicks(int ticks)`
- `SpeedUp()`
- `SlowDown()`
- `RunUntil(Func<bool>, Action, Action onFinish = null, int interval = 1)`
- `WaitUntil(Func<bool>, Action, int interval = 1)`

Use these when the real code path is asynchronous, delayed by ticks, or needs the game to advance to finish.

## Practical Guidance

- Start from `TestScenario` instead of creating new builders directly.
- Prefer builder chains over touching scenario state manually.
- Reuse existing pawns only when the test benefits from world continuity. Otherwise let the builder generate controlled setup.
