# Product Sense

`PawnHistory` should record moments that help a player understand who a pawn is, what happened to them, and why it mattered.

## Worth Recording

- identity-shaping moments: joins, bonds, betrayals, deaths, injuries, recoveries, convictions, discoveries
- high-signal outcomes: rare achievements, serious failures, quest-relevant turns, social turning points
- player-readable state changes that tell a story without needing debug knowledge

## Usually Not Worth Recording

- constant background churn with no story value (Hediff_Injury, Milking cow...)
- low-signal internal state that the player cannot interpret from the description
- duplicate records that restate the same event from several weak angles

## Pawn Targeting

- Record on the pawn who meaningfully experienced the event.
- Record on another pawn only when they are genuinely the subject, victim, discoverer, recruit, or otherwise central actor.
- Skip colony-wide spray unless the event is truly colony-level and the design intentionally wants broad recognition.

## Description Quality

- Descriptions should read like game-facing history, not debug output.
- Prefer concrete actors, outcomes, and concerns over technical causes.
- Rulepack text should be specific enough to be memorable and short enough to scan in a history list.
