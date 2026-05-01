# LetterAction<TLetter>

Accepts or rejects a live choice letter after a real game action creates it.

## Constructor

- `LetterAction<TLetter>()`: start a letter action for a `ChoiceLetter` subtype.

## Configuration

- `Filter(Func<TLetter, bool> predicate)`: narrow the letter search.

## Execution

- `Accept().Execute()`: choose the supported accept option.
- `Reject().Execute()`: choose the supported reject option.
- `TraitIndex().PassionIndices([]).Execute()`: complete a `ChoiceLetter_GrowthMoment` by selecting passions and a trait or the no-trait option.

## Supported Choice Letters

- `ChoiceLetter_AcceptJoiner`
- `ChoiceLetter_RansomDemand`
- `ChoiceLetter_AcceptVisitors`
- `ChoiceLetter_GrowthMoment`

## Notes

- Choice resolution is currently keyed by the visible option text for the supported letters above.
- Growth-moment completion primes the private choice lists through reflected access instead of driving the UI dialog.
- If a different letter type needs support, it belongs here rather than in test bodies.
