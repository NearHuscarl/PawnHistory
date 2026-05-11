# UI Implementation Rules

Use this document before changing RimWorld UI code in this repository.

## Rules
- Prefer composition in `Build` methods. Use `Column`, `Row`, `Expanded`, `Spacer`, `SizedBox`, `Padding`, `ScrollView`, `Stack`, `Align`, `Center`, `Positioned`, `ColoredBox`, `DecoratedBox`, `CustomPaint`, and `ConstrainedBox` to express layout instead of manual rectangle math.
- Use Flutter naming semantics for generic widgets. `SizedBox` fixes size, `SizedBox.Shrink()` is the zero-size factory, `ColoredBox` paints a flat background color, `DecoratedBox` paints color/border decoration, `CustomPaint` wraps a raw draw callback, `ConstrainedBox` imposes min/max constraints, `Positioned` places non-measuring stack children, and `TextField` covers single-line and multiline text input.
- `Spacer` is flex-only, equivalent to `Expanded(flex: flex, child: SizedBox.Shrink())`. Use `SizedBox(width: ...)` or `SizedBox(height: ...)` for fixed gaps.
- `Center` is the default for centering a child. Use `Align` only when non-center positioning is needed, and keep its API on Flutter-style `Alignment`, not Unity `TextAnchor`.
- Do not add ambiguous generic widgets. Names like `Box`, `Overlay`, or `MouseArea` are not acceptable when a Flutter-style name already communicates the role.
- Keep state objects mostly data-only. Interaction methods belong on the page/window coordinator or a narrow action interface, matching the style of `AddRecordDialog`.
- Do not reintroduce broad view/controller/command splits for simple UI state. Prefer page-local methods and narrow callback/action interfaces.
- Do not pass a whole page object into generic widgets. Pass only the state slice and callbacks a composed widget needs.
- Avoid per-frame refresh work. Synchronize pagination or scroll state on known events; prefer fixing the producer or fixture that owns a state change over adding UI-side stale-state detectors.
- Avoid unnecessary per-draw lambdas in hot UI. Use method groups or narrow action interfaces for stable actions; only allocate closures where a value must be bound for a specific composed element.
- Leaf widgets may call Verse IMGUI directly. Composite/domain widgets should build other widgets rather than drawing full rows, tables, or overlays manually.
- Stack adornments must not affect measurement. Use `Positioned` for borders and overlays that should fill the final draw rect but must not change row height.
- Tooltip must not occupy extra layout. Its measure result must be exactly the child measure; only its draw phase should register the tip region.
- Generic spacing belongs in `Theme`. Page-specific constants belong in a local layout class only when they are intrinsic to that UI.
