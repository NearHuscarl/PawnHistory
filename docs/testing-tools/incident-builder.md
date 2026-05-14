# IncidentBuilder

Runs a real storyteller incident and returns the pawns it introduced.

## Setup

- `scenario.Incident(IncidentDef def)`: build an incident on the current map.
- `scenario.Incident(IncidentDef def, IIncidentTarget target)`: build an incident for a specific target.
- `Point(int point)`: override incident points.
- `TraderKind(TraderKindDef traderKindDef)`: force a trader kind for trader incidents.
- `RaidNeverFlee()`: force raid pawns to never flee individually.
- `RaidStrategy(RaidStrategyDef raidStrategy)`: force a raid strategy.
- `RaidArrivalMode(PawnsArrivalModeDef pawnsArrivalModeDef)`: force a raid arrival mode.
- `Faction(Faction faction)`: force the incident faction.

## Execution

- `Execute()`: run the incident and return the spawned pawns, preferring the new lord's pawns when a lord is created.

Use `scenario.RaidFriendly()` when the intended setup is simply a non-hostile raid.
