# Letter Actions

These helpers resolve a live letter from `Find.LetterStack`, optionally filter it, then choose one supported path.

## Entry Points

- `scenario.Letter<T>()`: create a generic letter action for supported `ChoiceLetter` types.
- `scenario.LetterBabyToChild()`: create a baby-to-child letter action.
- `scenario.LetterGrowthMoment()`: create a growth-moment letter action.

## Shared Methods

- `Filter(Func<TLetter, bool> predicate)`: keep only active letters that match the predicate.
- `Execute()`: resolve the most recent matching letter and return it, applying the configured choice in subclasses.

## `LetterActionSimple<TLetter>`

- `Accept()`: choose the supported accept option.
- `Reject()`: choose the supported reject option.
- Supported `ChoiceLetter` types: `ChoiceLetter_AcceptJoiner`, `ChoiceLetter_RansomDemand`, `ChoiceLetter_AcceptVisitors`.

## `LetterActionBabyToChild`

- `PickColonist()`: choose the colonist-side option for the baby-to-child letter.
- `PickSlave()`: choose the slave-side option for the baby-to-child letter.

## `LetterActionGrowthMoment`

- `PassionIndices(IEnumerable<int> passionIndexes = null)`: choose which generated passion options to take by index.
- `TraitIndex(int? traitIndex = null)`: choose a generated trait by index, or `null` for the no-trait option.
- `Execute()`: populate the growth choices, validate the selected passion count, then submit the letter choices.
