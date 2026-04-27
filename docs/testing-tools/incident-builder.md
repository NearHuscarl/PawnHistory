# IncidentBuilder

Executes a storyteller incident and returns the pawns it spawned.

## Constructors

- `IncidentBuilder(IncidentDef def)`: build an incident on the current map.
- `IncidentBuilder(IncidentDef def, IIncidentTarget target)`: build an incident for a specific target.

## Configuration

- `Point(int point)`: set incident points.
- `TraderKind(TraderKindDef traderKindDef)`: set trader kind.
- `RaidStrategy(RaidStrategyDef raidStrategy)`: set raid strategy.
- `RaidArrivalMode(PawnsArrivalModeDef pawnsArrivalModeDef)`: set arrival mode.
- `Faction(Faction faction)`: set the incident faction.
- `NonHostileFaction()`: pick a neutral non-hidden faction.

## Execution

- `Execute()`: run the incident and return spawned pawns.
