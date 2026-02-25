# Tasks: Simple AdvantechDIO Test GUI

**Input**: Design documents from `specs/001-simple-test-gui/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/manual-test-gui-contract.md`, `quickstart.md`

**Tests**: No new automated test tasks are included. This feature specifies manual validation flow.

**Organization**: Tasks are grouped by user story for independent implementation and validation.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the minimal manual GUI project scaffold and solution wiring.

- [X] T001 Create WinForms project file in `AutoTest/AdvantechDIO.ManualTestGui/AdvantechDIO.ManualTestGui.csproj`
- [X] T002 Add manual GUI project to solution in `TDKServer.sln`
- [X] T003 [P] Add startup entry point in `AutoTest/AdvantechDIO.ManualTestGui/Program.cs`
- [X] T004 [P] Add basic app config (non-user-persistent) in `AutoTest/AdvantechDIO.ManualTestGui/App.config`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build shared form skeleton and common operation plumbing before user-story features.

**⚠️ CRITICAL**: No user story work should start before this phase is complete.

- [X] T005 Create base form layout (single-screen groups + status area) in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.Designer.cs`
- [X] T006 Create form code-behind with session state fields in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T007 Implement AdvantechDIO instance lifecycle wiring in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T008 Implement shared input parsing/range validation helpers in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T009 Implement shared status-area logging helper in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T010 Implement UI thread-safe invoke helper for event callbacks in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`

**Checkpoint**: Foundation ready. User stories can proceed.

---

## Phase 3: User Story 1 - Quick Connection Test (Priority: P1) 🎯 MVP

**Goal**: Operator can enter DeviceID and execute connect/disconnect with clear state feedback.

**Independent Test**: Launch app, set DeviceID, connect, verify connected state/device name, disconnect, verify reset state.

- [X] T011 [US1] Add DeviceID input and connection status controls in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.Designer.cs`
- [X] T012 [US1] Implement Connect button handler using `Connect()` in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T013 [US1] Implement Disconnect button handler using `Disconnect()` in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T014 [US1] Implement connected/disconnected UI state toggling in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`

**Checkpoint**: US1 independently functional (MVP).

---

## Phase 4: User Story 2 - Basic DI Read Test (Priority: P2)

**Goal**: Operator can read DI by port and by bit with guard-aware validation feedback.

**Independent Test**: While connected, run DI port read and DI bit read for valid and invalid indexes.

- [X] T015 [P] [US2] Add DI read controls (port, bit, result fields/buttons) in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.Designer.cs`
- [X] T016 [US2] Implement `GetInput(portIndex, out value)` action handler in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T017 [US2] Implement `GetInputBit(portIndex, bitIndex, out value)` action handler in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T018 [US2] Add DI input-validation error messages to status area in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`

**Checkpoint**: US2 independently functional.

---

## Phase 5: User Story 3 - Basic DO and Monitor Test (Priority: P3)

**Goal**: Operator can execute DO set/get and DI monitor start/stop with simple notification rules.

**Independent Test**: While connected, perform each DO action and monitor start/stop; verify first-change popup and status updates.

- [X] T019 [P] [US3] Add DO and monitor controls in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.Designer.cs`
- [X] T020 [US3] Implement `SetOutput` and `SetOutputBit` handlers in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T021 [US3] Implement `GetOutput` and `GetOutputBit` handlers in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T022 [US3] Add explicit DO input validation for `portIndex`/`bitIndex`/`value` ranges in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T023 [US3] Implement `SnapStart` and `SnapStop` handlers with monitor state flag in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T024 [US3] Handle `DI_ValueChanged` event with first-change popup rule in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T025 [US3] Handle `ExceptionOccurred` and critical-failure popup policy (FR-015A: connection loss after connect, backend `ExceptionOccurred`, connect-time controller init failure) with safe-disconnect reset in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`

**Checkpoint**: US3 independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final consistency pass and manual validation alignment.

- [X] T026 Align default startup values with spec in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T027 Verify command/notification behavior against contract in `specs/001-simple-test-gui/contracts/manual-test-gui-contract.md`
- [ ] T028 Run manual quickstart validation and record observed results in `specs/001-simple-test-gui/quickstart.md`
- [X] T029 Final cleanup of status/error texts for concise operator readability in `AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs`
- [X] T030 Add final scope guard review to ensure no non-essential features were introduced in `specs/001-simple-test-gui/spec.md`
- [X] T031 Verify popup is triggered only for FR-015A critical failures and non-critical errors remain in status area in `specs/001-simple-test-gui/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1): starts immediately.
- Foundational (Phase 2): depends on Setup completion.
- User Story phases (Phase 3-5): depend on Foundational completion.
- Polish (Phase 6): depends on target user stories completion.

### User Story Dependencies

- US1 (P1): starts after Phase 2; no dependency on other stories.
- US2 (P2): starts after Phase 2; independent of US3.
- US3 (P3): starts after Phase 2; independent of US2.

### Within Each User Story

- UI controls before handlers.
- Validation and status feedback integrated before story checkpoint.
- Each story validated independently using its own independent test criteria.

---

## Parallel Opportunities

- Phase 1: `T003` and `T004` can run in parallel after `T001`.
- Phase 4: `T015` can be developed in parallel with finalization of US1 state texts.
- Phase 5: `T019` can run in parallel with implementation planning of DO handlers.
- Cross-story: US2 and US3 can be developed in parallel after Phase 2 completion.

---

## Parallel Example: User Story 2

```text
Run in parallel:
- T015 Add DI read controls in AutoTest/AdvantechDIO.ManualTestGui/MainForm.Designer.cs
- UI wording polish from US1 in AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs
```

## Parallel Example: User Story 3

```text
Run in parallel:
- T019 Add DO and monitor controls in AutoTest/AdvantechDIO.ManualTestGui/MainForm.Designer.cs
- Draft event/error handling branches in AutoTest/AdvantechDIO.ManualTestGui/MainForm.cs
```

---

## Implementation Strategy

### MVP First (US1)

1. Complete Phase 1 and Phase 2.
2. Complete US1 (Phase 3).
3. Validate connect/disconnect flow independently.

### Incremental Delivery

1. Deliver US1 (connection flow).
2. Add US2 (DI reads).
3. Add US3 (DO + monitor).
4. Finish with Phase 6 polish and quickstart validation.

### Notes

- `[P]` marks tasks that can execute in parallel without file/dependency conflicts.
- No automation test creation is planned; this feature is explicitly manual-test oriented.
- Keep implementation minimal and avoid introducing non-essential abstractions.
