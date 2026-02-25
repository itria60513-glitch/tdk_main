# Feature Specification: AdvantechDIO Solution Integration & Interface Verification

**Feature Branch**: `001-advantechdio-integration`  
**Created**: 2026-02-25  
**Status**: Draft  
**Input**: User description: "在 solution file 新增一個 project reference 我已經做好的 project AdvantechDIO 並且測試所有 interface 的功能皆正常"

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Solution Build Success (Priority: P1)

As a developer, I want the AdvantechDIO project (and its test project) to be properly included in the main solution so that the entire solution builds successfully with no errors and the AdvantechDIO assembly is produced.

**Why this priority**: If the project cannot build within the solution, no other verification is possible. This is the most fundamental prerequisite.

**Independent Test**: Run a full solution build and confirm zero build errors for AdvantechDIO and AdvantechDIO.Tests projects.

**Acceptance Scenarios**:

1. **Given** the TDKServer.sln solution file, **When** a developer builds the solution in Debug configuration, **Then** the AdvantechDIO project compiles without errors and produces `AdvantechDIO.dll`.
2. **Given** the TDKServer.sln solution file, **When** a developer builds the solution in Debug configuration, **Then** the AdvantechDIO.Tests project compiles without errors and produces `AdvantechDIO.Tests.dll`.
3. **Given** the AdvantechDIO project references DIO and TDKLogUtility, **When** the solution is built, **Then** both referenced assemblies are resolved and no missing-reference warnings appear.

---

### User Story 2 — IIOBoardBase Interface Verification (Priority: P1)

As a developer, I want unit tests that confirm all `IIOBoardBase` interface members (Connect, Disconnect, IsConnected, IsVirtual, DeviceID, DeviceName, ExceptionOccurred event) function correctly, so that any DIO board consumer relying on the base contract is assured of correct behavior.

**Why this priority**: `IIOBoardBase` defines the connection lifecycle — all other I/O operations depend on a valid connection being established first.

**Independent Test**: Run the AdvantechDIO.Tests suite targeting constructor, Connect, Disconnect, and Dispose test groups. All tests must pass.

**Acceptance Scenarios**:

1. **Given** valid configuration and logger, **When** the AdvantechDIO object is constructed, **Then** `DeviceID`, `InputPortCount`, `InputBitsPerPort`, `OutputPortCount`, `OutputBitsPerPort` reflect the configuration values, `IsConnected` is false, `IsVirtual` is false, and `DeviceName` is empty.
2. **Given** a null logger or null config, **When** constructing AdvantechDIO, **Then** an `ArgumentNullException` is thrown.
3. **Given** a connected device, **When** `Disconnect()` is called, **Then** `IsConnected` becomes false, `DeviceName` is cleared, and the method returns 0 (success).
4. **Given** a device that is not connected, **When** `Disconnect()` is called, **Then** the method returns 0 (success) without error.
5. **Given** an AdvantechDIO instance, **When** `Dispose()` is called multiple times, **Then** no exception is thrown (idempotent).
6. **Given** a connection attempt to a non-existent device, **When** `Connect()` is called, **Then** the `ExceptionOccurred` event is raised and a non-zero error code is returned.

---

### User Story 3 — IDI Interface Verification (Priority: P1)

As a developer, I want unit tests that confirm all `IDI` interface members (GetInput, GetInputBit, SnapStart, SnapStop, DI_ValueChanged event) enforce correct guard behavior, so that digital input operations reject invalid calls gracefully.

**Why this priority**: Digital input is essential for reading E84 signals and sensor states in the loadport system.

**Independent Test**: Run guard-path and not-connected tests for all DI methods. All return the expected error codes.

**Acceptance Scenarios**:

1. **Given** the device is not connected, **When** `GetInput()` is called, **Then** it returns `NotConnectedError` (-1001) and the output value is 0.
2. **Given** the device is not connected, **When** `GetInputBit()` is called, **Then** it returns `NotConnectedError` (-1001).
3. **Given** the device is connected but DI is not configured (DIPortCount=0), **When** `SnapStart()` is called, **Then** it returns `NotConnectedError`.
4. **Given** the device is connected but DI is not configured, **When** `SnapStop()` is called, **Then** it returns `NotConnectedError`.
5. **Given** the device is connected with DI configured, **When** `GetInput()` is called with a port index exceeding the configured range, **Then** it returns `PortIndexOutOfRangeError` (-1002).
6. **Given** the device is connected with DI configured, **When** `GetInputBit()` is called with a bit index exceeding the configured range, **Then** it returns `BitIndexOutOfRangeError` (-1003).

---

### User Story 4 — IDO Interface Verification (Priority: P1)

As a developer, I want unit tests that confirm all `IDO` interface members (SetOutput, SetOutputBit, GetOutput, GetOutputBit, DO_ValueChanged event) enforce correct guard behavior, so that digital output operations reject invalid calls gracefully.

**Why this priority**: Digital output drives E84 signals (L_REQ, U_REQ, READY, HO_AVBL, ES) and is critical for AMHS handshaking.

**Independent Test**: Run guard-path and not-connected tests for all DO methods. All return the expected error codes.

**Acceptance Scenarios**:

1. **Given** the device is not connected, **When** `SetOutput()` is called, **Then** it returns `NotConnectedError` (-1001).
2. **Given** the device is not connected, **When** `SetOutputBit()` is called, **Then** it returns `NotConnectedError` (-1001).
3. **Given** the device is not connected, **When** `GetOutput()` is called, **Then** it returns `NotConnectedError` (-1001) and the output value is 0.
4. **Given** the device is not connected, **When** `GetOutputBit()` is called, **Then** it returns `NotConnectedError` (-1001) and the output value is 0.
5. **Given** the device is connected with DO configured, **When** `SetOutput()` is called with a port index exceeding the configured range, **Then** it returns `PortIndexOutOfRangeError` (-1002).
6. **Given** the device is connected with DO configured, **When** `SetOutputBit()` is called with a bit index exceeding the configured range, **Then** it returns `BitIndexOutOfRangeError` (-1003).

---

### User Story 5 — Error Code Contract Compliance (Priority: P2)

As a developer, I want to verify that all public methods returning error codes follow the project's int-based error code convention (0 = success, negative = error), so that consumers can uniformly interpret results.

**Why this priority**: Error code consistency is required by the project constitution and ensures reliable error handling across the module layer.

**Independent Test**: Review all public method return values in tests to confirm they are int-typed and use defined const int error codes.

**Acceptance Scenarios**:

1. **Given** all public methods of AdvantechDIO (Connect, Disconnect, GetInput, GetInputBit, SetOutput, SetOutputBit, GetOutput, GetOutputBit, SnapStart, SnapStop), **When** they succeed, **Then** they return 0.
2. **Given** all guard error codes (`NotConnectedError`, `PortIndexOutOfRangeError`, `BitIndexOutOfRangeError`), **When** referenced, **Then** they are negative `const int` values.
3. **Given** an exception occurs during any public method, **When** the catch block executes, **Then** the method returns a negative error code and logs the error.

---

### Edge Cases

- What happens when Connect is called while already connected? (Should return success immediately without re-initializing)
- What happens when only DI is configured (DOPortCount=0) and a DO method is called? (Should return NotConnectedError for DO-specific guard)
- What happens when only DO is configured (DIPortCount=0) and a DI method is called? (Should return NotConnectedError for DI-specific guard)
- What happens when Dispose is called and Disconnect throws internally? (Should be caught; disposed flag should still be set)
- What happens when port index or bit index is negative? (Should return PortIndexOutOfRangeError or BitIndexOutOfRangeError)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The AdvantechDIO project MUST be included in the TDKServer.sln solution file with correct GUID and build configuration entries for all platforms (Debug/Release × AnyCPU/x86/x64/Mixed Platforms).
- **FR-002**: The AdvantechDIO.Tests test project MUST be included in the solution file under the AutoTest solution folder, with correct project references to AdvantechDIO, DIO, and TDKLogUtility.
- **FR-003**: The solution MUST build successfully (zero errors) for both AdvantechDIO and AdvantechDIO.Tests in Debug configuration.
- **FR-004**: Unit tests MUST cover all `IIOBoardBase` interface members: `Connect()`, `Disconnect()`, `IsConnected`, `IsVirtual`, `DeviceID`, `DeviceName`, and `ExceptionOccurred` event.
- **FR-005**: Unit tests MUST cover all `IDI` interface members: `GetInput()`, `GetInputBit()`, `SnapStart()`, `SnapStop()`, `InputPortCount`, `InputBitsPerPort`, and `DI_ValueChanged` event.
- **FR-006**: Unit tests MUST cover all `IDO` interface members: `SetOutput()`, `SetOutputBit()`, `GetOutput()`, `GetOutputBit()`, `OutputPortCount`, `OutputBitsPerPort`, and `DO_ValueChanged` event.
- **FR-007**: All guard paths (not-connected, port-index-out-of-range, bit-index-out-of-range) MUST be tested and return the correct negative const int error codes.
- **FR-008**: Constructor argument validation MUST be tested — null logger and null config both throw `ArgumentNullException`.
- **FR-009**: Dispose idempotency MUST be verified: calling `Dispose()` multiple times does not throw.
- **FR-010**: All tests MUST pass without requiring physical Advantech hardware (guard-path and error-path testing via reflection or mocking).

### Key Entities

- **AdvantechDIO**: Concrete implementation of `IIOBoard` wrapping the Advantech DAQNavi SDK for digital input/output operations. Depends on `ILogUtility` and `AdvantechDIOConfig`.
- **IIOBoard** (= `IDI` + `IDO`): Aggregate interface defining the full DIO contract. Inherits `IIOBoardBase` for connection lifecycle.
- **IIOBoardBase**: Base interface defining device identity (DeviceID, DeviceName, IsConnected, IsVirtual), connection management (Connect, Disconnect), and exception notification (ExceptionOccurred event).
- **IDI**: Digital input interface providing port/bit read operations, snapshot monitoring, and change notification.
- **IDO**: Digital output interface providing port/bit write/read operations and change notification.
- **AdvantechDIOConfig**: Configuration model holding device index and DI/DO port topology (port count, bits per port).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Full solution build completes with zero errors and zero warnings related to AdvantechDIO or its test project.
- **SC-002**: All unit tests in AdvantechDIO.Tests pass (100% pass rate) without physical hardware.
- **SC-003**: Every public method defined in `IIOBoardBase`, `IDI`, and `IDO` interfaces has at least one corresponding unit test exercising it.
- **SC-004**: Every defined error code constant (`NotConnectedError`, `PortIndexOutOfRangeError`, `BitIndexOutOfRangeError`) is exercised by at least one test verifying the expected return value.
- **SC-005**: Test coverage for guard-path branches (not-connected, out-of-range indices) reaches 100%.

## Assumptions

- The AdvantechDIO project and AdvantechDIO.Tests project already exist and are functional — this feature focuses on ensuring proper solution integration and comprehensive test verification.
- The current solution file (TDKServer.sln) already contains entries for AdvantechDIO and AdvantechDIO.Tests — the task is to verify correctness and completeness of these entries.
- Tests are designed to run on developer machines without Advantech hardware; success-path SDK calls (requiring real hardware) are out of scope for automated unit testing.
- NUnit 3.x and Moq are the test frameworks, consistent with existing test projects.
