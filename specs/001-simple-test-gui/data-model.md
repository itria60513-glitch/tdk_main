# Data Model: Simple AdvantechDIO Test GUI

## Entity: TestSession
- Description: Current runtime session state for manual test execution.
- Fields:
  - `deviceId` (int, default 0)
  - `isConnected` (bool)
  - `isMonitoring` (bool)
  - `connectedDeviceName` (string)
  - `lastCriticalError` (string, optional)
- Validation:
  - `deviceId >= 0`
- State transitions:
  - `Disconnected -> Connected` on successful connect
  - `Connected -> Monitoring` on successful monitor start
  - `Monitoring -> Connected` on monitor stop
  - `Connected|Monitoring -> Disconnected` on disconnect or critical connection failure

## Entity: IoCommandInput
- Description: Operator-entered values for one command execution.
- Fields:
  - `commandType` (enum-like string: Connect, Disconnect, GetInput, GetInputBit, SnapStart, SnapStop, SetOutput, SetOutputBit, GetOutput, GetOutputBit)
  - `portIndex` (int, optional by command)
  - `bitIndex` (int, optional by command)
  - `value` (byte, optional by command)
  - `timestamp` (DateTime)
- Validation:
  - `portIndex` required for port-based commands and within configured range
  - `bitIndex` required for bit-based commands and within configured range
  - `value` for bit write must be `0` or `1`

## Entity: IoCommandResult
- Description: Standardized result shown after each action.
- Fields:
  - `commandType` (string)
  - `resultCode` (int)
  - `resultText` (string)
  - `returnedValue` (byte?, optional)
  - `timestamp` (DateTime)
- Validation:
  - `resultCode == 0` means success
  - `resultCode < 0` means error

## Entity: DiChangeEvent
- Description: Captured DI monitor event emitted while monitoring is active.
- Fields:
  - `timestamp` (DateTime)
  - `message` (string)
  - `isFirstPopupShown` (bool)
- Validation:
  - First event after monitor start may trigger popup; later events are status/log only.
