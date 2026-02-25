# Research: Simple AdvantechDIO Test GUI

## Decision 1: Host the manual GUI as a dedicated project under `AutoTest/`
- Decision: Place the simple WinForms test harness at `AutoTest/AdvantechDIO.ManualTestGui/`.
- Rationale: Keeps production modules unchanged, minimizes blast radius, and aligns with existing verification-oriented structure (`AutoTest/AdvantechDIO.Tests`).
- Alternatives considered:
  - `AdvantechDIO/` itself: rejected because library and GUI concerns would be mixed.
  - `TDKGUI/`: rejected for weaker semantic separation of test harness scope.
  - `EFEMGUI/`: rejected due to high coupling and unnecessary complexity for a simple test UI.

## Decision 2: UI update strategy for DI change events
- Decision: Treat hardware callbacks as non-UI thread and marshal UI updates via WinForms UI-thread invocation (`BeginInvoke` pattern).
- Rationale: Prevents cross-thread UI access issues while keeping implementation straightforward.
- Alternatives considered:
  - Direct UI updates from event callback: rejected due to cross-thread risk.
  - Complex event queue architecture: rejected as overdesign for manual test tool.

## Decision 3: Operator feedback model
- Decision: Status area is primary output; popup appears only for critical failures; DI monitor popup appears only on first change after monitor start.
- Rationale: Matches clarified requirements and avoids interrupt-heavy UX while preserving visibility.
- Alternatives considered:
  - Popup on every event/error: rejected due to operator disruption.
  - Log-only feedback: rejected because operator needs immediate visual confirmation.

## Decision 4: Settings persistence
- Decision: No runtime setting persistence; each app launch starts from defaults.
- Rationale: Keeps implementation minimal and predictable per session.
- Alternatives considered:
  - Auto-save on exit/start: rejected as non-essential complexity.
  - Manual save/load profile: rejected as out-of-scope for simple test GUI.

## Decision 5: Timeout and recovery behavior
- Decision: Follow module behavior without adding extra GUI-layer timeout orchestration; on critical fault, show popup and reset controls to safe disconnected state.
- Rationale: Reuses existing backend contract and keeps GUI logic minimal.
- Alternatives considered:
  - Add custom per-action timeout framework in GUI: rejected as overdesign.
  - Auto-retry policies: rejected due to ambiguity and increased complexity.
