# Mechlink Installed Recorder

Implemented a Biotech-gated `MechlinkInstalledRecorder` that records when a pawn successfully installs a mechlink and becomes a mechanitor.

## Notes

- The Harmony hook is `Hediff_Mechlink.PostAdd(...)`, which is the narrowest real game moment for successful mechlink installation.
- The event publishes only when Biotech is active, the pawn exists, and the `MechlinkImplant` hediff remained installed after `PostAdd`.
- The history record is recorded on the installed pawn and uses a dedicated Biotech-gated rulepack and history record def.
- Added one recorder-local Biotech test that drives the real `PostAdd` path through `AddHediff(HediffDefOf.MechlinkImplant)`.
