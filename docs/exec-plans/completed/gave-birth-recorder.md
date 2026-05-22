# Gave Birth Recorder

Birth is a high-signal family event in Biotech: it creates a new pawn, can involve a genetic parent set that differs from the birthing pawn, and can also produce stillbirth, infant illness, inbreeding, or childbirth death. The recorder captures those outcomes at the literal birth outcome method so history records are tied to the real baby creation path rather than to broader ritual completion events.

## Summary

`GaveBirthRecorder` records births emitted from `PregnancyUtility.ApplyBirthOutcome()` for the baby and the mother. For pawn births, the mother is the birthing pawn. For growth vat births, where no birthing pawn exists, the mother record is written to the genetic mother when available.

The baby and mother records use mirrored descriptions. Natural birth outcome descriptions are reused from `RitualOutcomePossibility.description`; vat births, surrogacy, inbreeding, and temporary-name clauses are expressed as history-facing rulepack text rather than UI-letter boilerplate.

## Shipped Scope

- Added a Harmony patch for `PregnancyUtility.ApplyBirthOutcome()`.
- Added `GaveBirthEvent` with baby, mother, genetic mother, father, outcome kind, vat/surrogacy/inbred flags, and reused natural outcome description.
- Added `GaveBirthRecorder` with records for the baby and mother.
- Added `GaveBirth` history def and `PH_GaveBirth` rulepack.
- Added recorder-local tests for natural outcomes, surrogacy, inbreeding, childbirth death, and all growth-vat parent/fallback outcome variants.

## Rules

- Do not use `RitualOutcomeCompletedEvent` for birth recording, because ritual completion can fire outside this birth-specific code path.
- Father is attached as a concern only when the generated description names him.
- Mother death is not part of the `GaveBirth` description; the existing death and relative-death recorders cover it.
- Growth-vat fallback descriptions intentionally omit parent names when either genetic parent is missing, matching vanilla letter behavior.

## Exclusions

The vanilla naming-deadline letter text is not recorded. It is an immediate UI instruction rather than a durable pawn-history event, while the temporary name itself is kept.

## Verification

- Built with `MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`.
- Parsed `Defs/HistoryRecord.xml` and `Defs/RulePackDefs/RulePacks_Relationship.xml` as XML.
- Added recorder-local tests for in-game execution; they were not run inside RimWorld during this implementation pass.
