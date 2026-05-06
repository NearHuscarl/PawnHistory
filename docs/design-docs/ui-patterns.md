# UI Patterns

`PawnHistory` UI code should stay idiomatic to RimWorld/Verse IMGUI while keeping responsibilities explicit.

## Immediate-Mode Presenter Split

Use this split for UI that has input, validation, or stateful interactions:

- Host/page/window owns long-lived UI state and the presenter/controller.
- View code is immediate-mode draw code only.
- Presenter/controller handles meaningful commands only.
- Pure validators stay reusable and side-effect free.

## Responsibilities

- Host/page/window:
  - owns UI state
  - sync external state from the outside
  - calls view draw helpers
  - wires presenter callbacks
- View:
  - draws widgets
  - may mutate UI-only state such as raw text buffers or scroll position
  - may detect raw IMGUI mechanics such as text changes, focused Enter presses, or button clicks
  - emits meaningful commands
  - does not validate submitted values
  - does not mutate game/domain state directly beyond existing UI interactions the screen already owns
- Presenter/controller:
  - handles meaningful commands
  - validates submitted values
  - updates committed UI state
  - calls submit actions or other side effects only when valid
- Validator/helper:
  - pure function only
  - reusable outside a specific screen

## Avoid

- fake persistent widget/control classes
- routing every keystroke through the presenter
- mixing draw code, validation, and page/domain mutations in one method

## Preferred Style

- Prefer small static view/helper methods over layered abstractions.
- Let the view mutate raw UI state when that change is purely presentational.
- Keep command types local and concrete.
- Keep presenter APIs command-based rather than lifecycle-based.
