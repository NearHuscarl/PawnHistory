# Human Pregnancy Recorder

Implemented a Biotech-gated human pregnancy recorder that writes history at the same discovery moment vanilla uses for the pregnancy notification, instead of trying to infer conception timing.

## Summary

Human pregnancies now create history when `HediffComp_MessageAfterTicks` reaches the notification tick for `PregnantHuman`. The recorder writes the event from the pregnant carrier's point of view, adds a father point of view when the father is known, and adds a distinct genetic-mother point of view for surrogacy or IVF-style pregnancies where `Mother != carrier`.

## Shipped Scope

- Added `PregnancyStartedEvent` and a Harmony patch on `HediffComp_MessageAfterTicks.CompPostTick`.
- Narrowed publication to `Hediff_Pregnant` with `HediffDefOf.PregnantHuman`.
- Added `PregnancyStartedRecorder`.
- Added Biotech-gated history def and rulepack text.
- Added recorder-local Biotech tests for natural pregnancy, father fallback, surrogacy, unknown father, and duplicate prevention.

## Design

- The patch reads the private `ticksUntilMessage` field through `Accessor` and publishes only when the value is exactly `0`, before vanilla sends the letter and decrements to `-1`.
- The recorder intentionally stays discovery-based. It does not attempt to backfill pregnancy to pawn generation or conception time.
- Father records use relation-first wording when the father currently has a meaningful relation label for the pregnant carrier, and fall back to generic child-focused wording otherwise.
- Distinct genetic-mother records treat the father as the concern, while father records treat the pregnant carrier as the concern.

## Rules

- Carrier records are always eligible when `ShouldRecord(carrier)` passes.
- Distinct genetic-mother records only emit when `mother != null`, `mother != carrier`, and `ShouldRecord(mother)` passes.
- Father records only emit when `father != null` and `ShouldRecord(father)` passes.
- Surrogacy father POV is centered on the pregnant carrier, not the genetic mother.
- No history-backfill registration was added for this def.

## Verification

- Added recorder-local tests covering:
  - natural pregnancy boundary behavior at tick 600 vs tick 601
  - father generic fallback when no relation exists
  - surrogacy with distinct carrier and genetic mother
  - unknown-father carrier-only recording
  - duplicate prevention after the notification tick
- Built the mod successfully with the approved Debug MSBuild command.
