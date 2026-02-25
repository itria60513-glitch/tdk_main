# Implementation Plan: Simple AdvantechDIO Test GUI

**Branch**: `001-simple-test-gui` | **Date**: 2026-02-25 | **Spec**: `specs/001-simple-test-gui/spec.md`
**Input**: Feature specification from `specs/001-simple-test-gui/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Deliver a minimal WinForms manual test GUI for AdvantechDIO that validates connect/disconnect, DI read (port/bit), DI monitor start/stop, and DO read/write (port/bit). Keep scope intentionally narrow: single-screen workflow, no runtime settings persistence, status-area-first feedback, and popup alerts only for critical failures.

## Technical Context

**Language/Version**: C# 7.3, .NET Framework 4.7.2  
**Primary Dependencies**: WinForms, `AdvantechDIO`, `DIO`, `TDKLogUtility`  
**Storage**: N/A (explicitly no runtime setting persistence)  
**Testing**: Manual test flow validation + existing NUnit project remains unchanged  
**Target Platform**: Windows desktop with Advantech runtime/driver support  
**Project Type**: Desktop test utility (single-screen WinForms)  
**Performance Goals**: Complete full test flow within 5 minutes; action feedback shown within operator-perceivable immediate response window  
**Constraints**: Keep UI simple, no overdesign, no dashboards/scheduling/roles; status-area-first messaging; critical-only popup policy  
**Scale/Scope**: Single operator, single device target per session, one manual test screen

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Gate 1 - Tech stack compliance: PASS. Uses .NET Framework 4.7.2 and existing project dependencies.
- Gate 2 - Simplicity/YAGNI: PASS. Scope restricted to manual verification flows from spec only.
- Gate 3 - Architecture boundaries: PASS. AdvantechDIO library remains separate; GUI acts as consumer.
- Gate 4 - Error handling and reporting: PASS. Plan preserves int error-code contract and clear operator feedback.
- Gate 5 - File/class policy: PASS with user intent. Feature explicitly requests a test GUI; any new GUI files stay minimal and feature-scoped.

Post-Design Re-check (after Phase 1 artifacts): PASS
- Research, data model, contract, and quickstart stay within simple manual test scope.
- No additional architecture layers, persistence, or automation orchestration introduced.
- Planned UI interaction contract remains aligned with existing AdvantechDIO public methods and error-code behavior.

## Project Structure

### Documentation (this feature)

```text
specs/001-simple-test-gui/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
└── tasks.md
```

### Source Code (repository root)

```text
AdvantechDIO/
├── Module/
├── Config/
└── AdvantechDIO.csproj

AutoTest/
├── AdvantechDIO.Tests/
└── AdvantechDIO.ManualTestGui/       # Planned simple WinForms test harness
```

**Structure Decision**: Implement as a minimal dedicated manual GUI project under `AutoTest/` to reduce production blast radius while reusing existing `AdvantechDIO` library and interfaces.

## Complexity Tracking

No constitution violations requiring justification.
