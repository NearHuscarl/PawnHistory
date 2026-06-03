# ChildBirth Ritual Doctor Recorder

The childbirth ritual already had family-side narrative through the birth recorder, but it was still missing the ritual-facing story of who actually delivered the child. That gap matters because the ritual outcome is not about the parents repeating their birth narrative. It is about the doctor performing a consequential act inside a formal Ideology ritual.

## Summary
Added `ChildBirth` support to the ritual outcome recorder as a doctor-only history entry. The recorder now publishes childbirth ritual history on the doctor, concerns only the carrier, and uses a separate ritual comp with grammar rules named `doctor` and `carrier`.

The implementation intentionally did not expand or reshape `GaveBirthEvent`, and it does not carry child-specific data through the ritual recorder path.

## Shipped Scope
- enabled `ChildBirth` on the existing ritual outcome event hook
- resolved childbirth ritual host to the `doctor`
- added a separate `RitualOutcomeComp_ChildBirth`
- added doctor-only childbirth ritual text
- added one recorder-local test gated by Biotech and Ideology

## Design
`RitualOutcomeCompletedEvent` stays the same shape. The only childbirth-specific event change is host resolution: `GetOrganizer(...)` returns the ritual doctor for `ChildBirth`, and the existing ritual outcome hook includes the childbirth worker.

This keeps childbirth support inside a narrow path: one host special-case, one childbirth comp, one childbirth test.

## Rules
- only the doctor receives the `RitualOutcome` history record
- the carrier is the only concern
- the recorder uses grammar rule names `doctor` and `carrier`
- `GaveBirthEvent` remains unchanged and continues to own the family-side birth narrative

## Exclusions
- no childbirth ritual record is written to the carrier
- no child-specific ritual grammar or concern data is emitted
- no parent-facing birth narrative was moved into the ritual recorder
- no generalized event-contract change was introduced for childbirth

## Verification
Added one recorder-local test that:
- requires Biotech
- requires Ideology
- runs the real childbirth ritual path
- asserts the doctor receives the childbirth ritual history entry
- asserts the concern is exactly the carrier
- asserts the carrier does not receive a `RitualOutcome` record

Also verified the code by running the approved Debug MSBuild build successfully.
