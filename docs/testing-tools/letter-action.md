# LetterAction<TLetter>

Accepts or rejects a live choice letter after a real game action creates it.

## Constructor

- `LetterAction<TLetter>()`: start a letter action for a `ChoiceLetter` subtype.

## Configuration

- `Filter(Func<TLetter, bool> predicate)`: narrow the letter search.

## Execution

- `Accept()`: choose the supported accept option.
- `Reject()`: choose the supported reject option.

## Supported Choice Letters

- `ChoiceLetter_AcceptJoiner`
- `ChoiceLetter_RansomDemand`
- `ChoiceLetter_AcceptVisitors`

## Notes

- Choice resolution is currently keyed by the visible option text for the supported letters above.
- If a different letter type needs support, it belongs here rather than in test bodies.
