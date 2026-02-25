# Quickstart: Simple AdvantechDIO Test GUI

## Goal
Run a minimal manual test flow for AdvantechDIO: connect, DI read, DO read/write, monitor start/stop, disconnect.

## Prerequisites
- Windows machine with .NET Framework 4.7.2 runtime.
- Advantech runtime/driver available if testing with physical hardware.
- Build environment for `TDKServer.sln`.

## Build
```powershell
msbuild TDKServer.sln /p:Configuration=Debug
```

## Manual Test Flow
1. Open the simple test GUI.
2. Confirm defaults: DeviceID=0, DI ports=2, DI bits=8, DO ports=1, DO bits=8.
3. Click `Connect`.
4. Execute DI tests:
   - `GetInput(portIndex)`
   - `GetInputBit(portIndex, bitIndex)`
5. Execute DO tests:
   - `SetOutput(portIndex, value)`
   - `SetOutputBit(portIndex, bitIndex, value)`
   - `GetOutput(portIndex)`
   - `GetOutputBit(portIndex, bitIndex)`
6. Click `SnapStart`, trigger DI change, verify:
   - event appears in status/log area
   - first event shows popup
7. Click `SnapStop`.
8. Click `Disconnect`.

## Expected Behavior
- Non-critical failures appear in status area with result code.
- Critical failures show popup and force safe disconnected state.
- Restarting app resets to default values (no persistence).

## Validation Record (2026-02-25)

- Build check: `dotnet msbuild AutoTest\\AdvantechDIO.ManualTestGui\\AdvantechDIO.ManualTestGui.csproj /p:Configuration=Debug` -> PASS.
- Smoke launch check: executable starts successfully and stays alive for 3 seconds before scripted close -> PASS.
- Popup policy verification (code path inspection):
   - First DI popup after `SnapStart` only: `OnDiValueChanged` with `_firstDiPopupShown` gate.
   - Critical popup on FR-015A paths:
      - connect-time failure popup in `BtnConnect_Click`
      - backend exception popup in `OnExceptionOccurred`
   - Non-critical validation and operation failures are written to status area via `WriteStatus` / `WriteResult`.
- Hardware-dependent manual flow (connect to physical device, DI transition trigger, and DO signal verification) remains to be executed on target workstation.
