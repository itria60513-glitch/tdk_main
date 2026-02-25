# Feature Specification: Advantech DIO Test Console

**Feature Branch**: `001-advantech-dio-winform-test`  
**Created**: 2026-02-25  
**Status**: Draft  
**Input**: User description: "Create a desktop test project for AdvantechDIO with configurable DeviceID, connect/disconnect, DI read options (port, port+bit), DI change monitoring start/stop, and DO test actions for set/get by port and bit."
 
## Clarifications
 
### Session 2026-02-25

- Q: During `SnapStart`, how should UI alerts behave under rapid consecutive DI changes? -> A: Log every DI change, but throttle popup alerts.
- Q: What throttle interval should be applied to popup alerts during DI monitoring? -> A: 1 second (max 1 popup per second).
- Q: Should DI/DO operations have a timeout? If a device is unresponsive, how long should the application wait? -> A: No timeout; operations may block indefinitely until device responds or user closes app.
- Q: How should operation failures be reported to the user? -> A: Show result in status bar; only critical failures (e.g., lost connection) trigger popup alerts.
- Q: Should user configuration (device ID, test selections) persist across application restarts? -> A: Persist to XML file; load automatically on startup.
### User Story 1 - Connect and Validate Device Session (Priority: P1)

As a test engineer, I want to set a target device ID and control connect/disconnect from a single screen so that I can quickly validate whether the board is reachable before running I/O tests.

**Why this priority**: No I/O verification is possible until connection setup works reliably.

**Independent Test**: Launch the tool, set a device ID, connect, verify connected status and device information display, then disconnect and verify status reset.

**Acceptance Scenarios**:

1. **Given** the test screen is open and no active connection, **When** the user enters a valid device ID and selects connect, **Then** the system establishes a session and displays connected state.
2. **Given** an active connection, **When** the user selects disconnect, **Then** the system releases the session and displays disconnected state.
3. **Given** an invalid or unavailable device ID, **When** the user selects connect, **Then** the system shows a clear failure message and remains disconnected.

---

### User Story 2 - Read Digital Input States (Priority: P2)

As a test engineer, I want to choose a DI port or a specific DI bit to read so that I can confirm real hardware input states during diagnostics.

**Why this priority**: Input verification is a core acceptance activity for hardware integration and field diagnostics.

**Independent Test**: While connected, read DI by port and by bit with valid and invalid indexes, then confirm displayed values and error handling.

**Acceptance Scenarios**:

1. **Given** an active connection and a valid DI port index, **When** the user requests DI port read, **Then** the system returns and displays the current byte value.
2. **Given** an active connection and valid DI port/bit indexes, **When** the user requests DI bit read, **Then** the system returns and displays `0` or `1`.
3. **Given** an active connection and an out-of-range DI port or bit index, **When** the user requests read, **Then** the system does not crash and shows a clear validation error.

---

### User Story 3 - Monitor DI Changes with Start/Stop Controls (Priority: P2)

As a test engineer, I want to start and stop DI change monitoring and receive immediate notification when DI changes so that I can verify sensor transitions in real time.

**Why this priority**: Real-time change observation is essential for validating interrupts and timing-sensitive input behavior.

**Independent Test**: Start monitoring, trigger hardware DI transitions, verify log entries and pop-up notification, then stop monitoring and confirm no further notifications.

**Acceptance Scenarios**:

1. **Given** an active connection, **When** the user starts DI monitoring and DI changes occur, **Then** the system records each change and displays a user-visible alert.
2. **Given** monitoring is active, **When** the user stops monitoring, **Then** further DI changes no longer trigger alerts.
3. **Given** no active connection, **When** the user tries to start or stop monitoring, **Then** the system shows a clear warning and keeps current state unchanged.

---

### User Story 4 - Execute DO Write and Readback Tests (Priority: P3)

As a test engineer, I want dedicated controls for DO set/get by port and by bit so that I can execute complete output validation in one place.

**Why this priority**: Output testing depends on prior connection and is typically performed after input and monitoring checks.

**Independent Test**: Use each DO action independently with valid indexes and values, verify result code and displayed output values.

**Acceptance Scenarios**:

1. **Given** an active connection and valid output port index/value, **When** the user performs output-by-port write, **Then** the operation completes and returns success.
2. **Given** an active connection and valid output port/bit/value, **When** the user performs output-by-bit write, **Then** the selected bit state is updated as requested.
3. **Given** an active connection and valid output port or port/bit indexes, **When** the user performs output read operations, **Then** the returned value is displayed.
4. **Given** an out-of-range output port or bit index, **When** the user performs output operations, **Then** the system reports validation errors without crashing.

### Edge Cases

- User attempts any DI or DO operation while disconnected.
- User starts DI monitoring repeatedly without stopping first.
- User stops DI monitoring when monitoring is not active.
- DI changes occur in rapid bursts while monitoring is active; system logs all changes and throttles popups to 1 per second.
- User enters non-numeric, empty, or negative values in device/port/bit/value inputs.
- Hardware disconnects unexpectedly during active monitoring or DO operation.
- DI and DO actions are triggered rapidly in succession.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide an editable device identifier input and use it as the target for connection attempts; configuration values MUST be persisted to XML and auto-loaded on startup.
- **FR-002**: The system MUST provide explicit `Connect` and `Disconnect` actions and display current connection state.
- **FR-003**: The system MUST initialize default test configuration values at startup as follows: device ID `0`, input port count `2`, input bits per port `8`, output port count `1`, output bits per port `8`.
- **FR-004**: The system MUST provide a DI port read action where users can select an input port index and view returned byte value.
- **FR-005**: The system MUST provide a DI bit read action where users can select input port and bit indexes and view returned bit value.
- **FR-006**: The system MUST provide a `Start DI Monitor` action that begins DI change monitoring.
- **FR-007**: The system MUST provide a `Stop DI Monitor` action that stops DI change monitoring.
- **FR-008**: While DI monitoring is active and a DI change is detected, the system MUST record a log entry for every change and show on-screen alerts using a throttled strategy (max 1 popup per second) to prevent excessive popup interruption.
- **FR-009**: The system MUST provide a DO port write action where users set output by port index and byte value.
- **FR-010**: The system MUST provide a DO bit write action where users set output by port index, bit index, and bit value.
- **FR-011**: The system MUST provide a DO port read action where users select output port index and view returned byte value.
- **FR-012**: The system MUST provide a DO bit read action where users select output port and bit indexes and view returned bit value.
- **FR-013**: The system MUST validate device, port, bit, and value inputs before execution and block invalid requests with clear messages.
- **FR-014**: For each DI/DO action, the system MUST display operation result code and a brief human-readable status in a persistent status bar.
- **FR-015**: For critical operation failures (e.g., connection loss during DI/DO execution), the system MUST trigger a popup alert; non-critical errors remain in status bar only.
- **FR-016**: On operation failure, the system MUST keep the application responsive and allow subsequent test actions without restart.
- **FR-017**: The system MUST serialize and deserialize user configuration (device ID, port/bit selections) to/from an XML file for automatic persistence and recovery.

### Key Entities *(include if feature involves data)*

- **Connection Session**: Represents current hardware session state, including target device identifier, connected/disconnected status, and last connection result.
- **Port Topology**: Represents input/output port counts and bits-per-port constraints used for validating user selections.
- **I/O Test Command**: Represents a user-triggered action (read/write/monitor start/stop) with parameters and execution result.
- **I/O Test Result**: Represents returned value, result code, status text, and timestamp for display and traceability.
- **DI Change Event Record**: Represents each detected DI transition with timestamp, source port/bit context, and notification status.

## Assumptions

- The target hardware and required runtime dependencies are available on the test workstation.
- Test operators have permission to access the configured hardware device.
- Logging destination is available and writable during test execution.

## Dependencies

- Availability of a compatible Advantech DIO device for end-to-end validation.
- Availability of an operational logging mechanism for event recording.
- Existing DIO interface methods remain behaviorally stable for connect, read, write, and monitoring operations.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of planned connect/disconnect test runs complete without application crash.
- **SC-002**: In at least 95% of DI/DO command executions with valid inputs, users receive a result code and displayed value/status within 2 seconds.
- **SC-003**: During DI monitoring tests, 100% of intentionally triggered DI transitions produce both a recorded log entry and a visible user alert.
- **SC-004**: 100% of invalid input attempts (out-of-range or malformed) are rejected with clear feedback and without application termination.
- **SC-005**: At least 90% of test operators can complete the full validation flow (connect, DI read, monitor start/stop, DO read/write, disconnect) on first attempt without external assistance.
