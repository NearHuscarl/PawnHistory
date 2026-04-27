# Quality Score

Use this as the bar for recorder work.

## Patch Quality

- Patch the narrowest real game entrypoint that naturally represents the event.
- Avoid broad or ambiguous hooks when a more literal source exists.
- Keep Harmony state deterministic and easy to reset.

## Event Quality

- Publish a typed, domain-named event.
- Normalize upstream noise before `CreateRecord(...)`.
- Keep event payloads focused on what the recorder actually needs.

## Recorder Quality

- Call `ShouldRecord(...)` inside `CreateRecord(...)`.
- Record on the correct pawn and attach the right concerns, location, and quest when relevant.
- Prefer rulepacks and `Resolve()` over string format.

## DLC and Def Quality

- Gate DLC-dependent behavior explicitly.
- Add or update `HistoryRecordDefOf` and XML defs together.
- Keep naming literal and predictable.

## Test Quality

- Use the real game path that reaches the Harmony patch.
- Keep tests local to the recorder.
- Cover the actor, the record def, and any important attachments such as quest or concern references.
