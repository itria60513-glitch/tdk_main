# Contract: Manual Test GUI Actions

## Purpose
Define the functional UI-to-module command contract for the simple AdvantechDIO test screen.

## Command Contract

| UI Action | Backend Method | Inputs | Success Output | Failure Output |
|---|---|---|---|---|
| Connect | `Connect()` | `deviceId` (from config/session) | result code `0`, connected status | non-zero code, status message, optional critical popup |
| Disconnect | `Disconnect()` | none | result code `0`, disconnected status | non-zero code, status message |
| Get DI Port | `GetInput(portIndex, out value)` | `portIndex` | result `0`, DI byte value | guard/sdk code in status area |
| Get DI Bit | `GetInputBit(portIndex, bitIndex, out value)` | `portIndex`, `bitIndex` | result `0`, DI bit value | guard/sdk code in status area |
| Start Monitor | `SnapStart()` | none | result `0`, monitoring on | error code in status area |
| Stop Monitor | `SnapStop()` | none | result `0`, monitoring off | error code in status area |
| Set DO Port | `SetOutput(portIndex, value)` | `portIndex`, `value` | result `0` | guard/sdk code in status area |
| Set DO Bit | `SetOutputBit(portIndex, bitIndex, value)` | `portIndex`, `bitIndex`, `value` | result `0` | guard/sdk code in status area |
| Get DO Port | `GetOutput(portIndex, out value)` | `portIndex` | result `0`, DO byte value | guard/sdk code in status area |
| Get DO Bit | `GetOutputBit(portIndex, bitIndex, out value)` | `portIndex`, `bitIndex` | result `0`, DO bit value | guard/sdk code in status area |

## Validation Rules
- `deviceId` must be integer >= 0.
- `portIndex` must match configured DI/DO port range.
- `bitIndex` must match configured bit range for selected port.
- DO bit write `value` must be `0` or `1`.

## Notification Rules
- DI monitor events: always log/status update.
- Popup notification: first DI event after monitor start, and critical failures only.
- Non-critical method failures: status area only.

## Persistence Rules
- Runtime settings are session-only.
- On app restart, default values are reloaded (no persisted state).
