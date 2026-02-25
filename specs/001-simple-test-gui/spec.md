# Feature Specification: Simple AdvantechDIO Test GUI

**Feature Branch**: `001-simple-test-gui`  
**Created**: 2026-02-25  
**Status**: Draft  
**Input**: User description: "Need only a simple test GUI, no overdesign. Focus on quick verification of connect/disconnect, basic DI/DO read/write, and DI change detection."

## Clarifications

### Session 2026-02-25

- Q: How should DI changes be presented during monitoring? -> A: Log all changes; show in status area; popup notification only on first change after monitor start.
- Q: How should operation failures be reported? -> A: Critical errors use popup; normal errors use status area only.
- Q: Should settings persist between app restarts? -> A: No persistence; always start from default values.

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Quick Connection Test (Priority: P1)

As a test operator, I want a simple screen to enter DeviceID and perform connect/disconnect so I can confirm hardware communication quickly.

**Why this priority**: All other tests depend on successful connection.

**Independent Test**: Launch app, enter DeviceID, click connect, verify connection status, click disconnect, verify status reset.

**Acceptance Scenarios**:

1. **Given** app is open and disconnected, **When** user enters DeviceID and clicks connect, **Then** app shows connected state or failure message.
2. **Given** app is connected, **When** user clicks disconnect, **Then** app returns to disconnected state.

---

### User Story 2 - Basic DI Read Test (Priority: P2)

As a test operator, I want to read DI by port and by port+bit so I can validate sensor/input state quickly.

**Why this priority**: DI verification is part of minimum hardware acceptance.

**Independent Test**: While connected, run DI read by port and DI read by bit for valid indexes and invalid indexes.

**Acceptance Scenarios**:

1. **Given** connected state and valid DI port, **When** user reads input port, **Then** app displays the returned byte value.
2. **Given** connected state and valid DI port+bit, **When** user reads input bit, **Then** app displays 0 or 1.
3. **Given** invalid DI port or bit index, **When** user executes read, **Then** app shows validation error and remains responsive.

---

### User Story 3 - Basic DO and Monitor Test (Priority: P3)

As a test operator, I want buttons for DO set/get and DI monitor start/stop so I can finish core I/O checks in one simple UI.

**Why this priority**: This completes minimal test coverage after connection and DI read are available.

**Independent Test**: While connected, execute each DO action once and run monitor start/stop with at least one DI change event.

**Acceptance Scenarios**:

1. **Given** connected state and valid DO inputs, **When** user executes set/get by port or bit, **Then** app displays operation result and values.
2. **Given** connected state, **When** user starts DI monitor and DI changes occur, **Then** app records event log and shows change notification.
3. **Given** monitor is running, **When** user clicks stop monitor, **Then** new DI changes no longer trigger monitor notifications.

---

### Edge Cases

- User attempts any DI/DO action before connecting.
- User enters out-of-range port or bit index.
- User clicks monitor start multiple times.
- User clicks monitor stop before monitor is active.
- Device disconnects unexpectedly during test.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a simple single-screen GUI for manual I/O testing.
- **FR-002**: System MUST allow user to enter DeviceID and execute connect/disconnect.
- **FR-003**: System MUST initialize default values as DeviceID `0`, InputPortCount `2`, InputBitsPerPort `8`, OutputPortCount `1`, OutputBitsPerPort `8`.
- **FR-004**: System MUST provide DI read by port index.
- **FR-005**: System MUST provide DI read by port and bit index.
- **FR-006**: System MUST provide DI monitor start and stop actions.
- **FR-007**: When DI monitor is active and DI changes, system MUST write a log record and display the change in a status area; a popup notification appears only on the first change after monitor start.
- **FR-008**: System MUST provide DO set by port value.
- **FR-009**: System MUST provide DO set by port and bit value.
- **FR-010**: System MUST provide DO get by port index.
- **FR-011**: System MUST provide DO get by port and bit index.
- **FR-012**: System MUST show operation result code and brief status for every action in the status area.
- **FR-013**: System MUST validate user input for numeric format and range before execution.
- **FR-014**: Feature scope MUST stay minimal and MUST NOT include advanced dashboards, scheduling, user roles, or automated test workflow orchestration.
- **FR-015**: System MUST show popup alerts only for critical failures (for example, connection loss); non-critical failures MUST remain in the status area.
- **FR-015A**: Critical failures are limited to: (1) connection loss after a successful connect, (2) `ExceptionOccurred` event raised by `AdvantechDIO`, (3) controller initialization failure during `Connect()`.
- **FR-016**: System MUST NOT persist runtime settings; every application launch MUST use default values.

### Key Entities *(include if feature involves data)*

- **Test Session**: Current GUI state, including DeviceID and connection status.
- **I/O Command Input**: User-entered parameters for port, bit, and value.
- **I/O Command Result**: Returned code, value, status message, and timestamp.
- **DI Change Event**: Detected DI transition with event time and notification outcome.

## Assumptions

- The operator runs tests manually and does not require automation.
- Existing AdvantechDIO methods are used as-is for connection and I/O actions.
- A basic logging target is available.
- Test settings are entered per session and do not require saving/loading.

## Dependencies

- Access to target Advantech DIO hardware.
- Availability of underlying communication and logging modules.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Operator can complete connect -> DI read -> DO read/write -> monitor start/stop -> disconnect flow within 5 minutes.
- **SC-002**: 100% of invalid input attempts are rejected with clear messages and no app crash.
- **SC-003**: During DI monitor test, intentionally triggered DI changes are logged and visible to operator.
- **SC-004**: GUI includes only required controls for listed test methods and excludes non-essential advanced features.
