# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TDKServiceMiniPC is a C# (.NET Framework 4.7.2) control system for TAS300 (E4) Loadport equipment. It provides hardware control, serial communication (RS-232C), and logging infrastructure for automated material handling systems (AMHS). The project communicates with TAS300 hardware using the TDK A protocol format.

## Build Commands

```bash
# Build entire solution
msbuild TDKServiceMiniPC.sln /p:Configuration=Debug

# Build specific project
msbuild TDKLogUtility/TDKLogUtility.csproj /p:Configuration=Debug

# Build release
msbuild TDKServiceMiniPC.sln /p:Configuration=Release
```

## Testing

Testing framework: NUnit 3.x + Moq (no test projects exist yet).

```bash
# When test projects are added, run with:
nunit3-console.exe AutoTest/[ProjectName].Tests/bin/Debug/[ProjectName].Tests.dll
# Or build test solution:
msbuild AutoTest/AutoTest.sln /p:Configuration=Debug
```

Test naming convention: `MethodName_Scenario_ExpectedResult`

## Governing Document

**`constitution_CH.md` (v3.0.1)** is the authoritative project charter. All development decisions must comply. When in doubt, defer to the constitution.

## Architecture

Defined by `constitution_CH.md`. Four-layer structure with strict dependency direction:

```
Service Layer     (TDKService) — Host communication, command parsing, response formatting
    ↓
Controller Layer  (TDKController/Controller) — Facade exposing ILoadportController
    ↓
Module Layer      (Device modules: LoadportActor, N2Purge, CarrierIDReader, LightCurtain, E84)
    ↓
Infrastructure    (TDKLogUtility [READ-ONLY], TDKCommunication, Config)
```

- Upper layers may inject and use lower layers. Lower layers must NOT reference upper layers.
- Same-layer modules interact through interfaces only.

### Projects

| Project | Type | Status | Notes |
|---------|------|--------|-------|
| TDKService | EXE/DLL | Planned | Host communication, command parsing, response formatting |
| TDKLogUtility | DLL | Implemented | **Read-only** — do not modify |
| TDKController | DLL | Planned | Main controller + LoadportActor module |
| TDKCommunication | DLL | Planned | Provides `IConnector` interface |

### Key Interfaces

- **`ILogUtility`** (`TDKLogUtility.Module`) — Logging facade, defined in `TDKLogUtility/Interface/AbstractLogUtility.cs:22`. Read-only, do not modify.
- **`IConnector`** (`CommunicationChannel`) — Communication channel for RS232/TCP, defined in `CommChannel.cs:27`. Read-only.
- **`HRESULT`** (`ExceptionManagement.HRESULT`) — System exception type. Do not modify or mock.

### Standard Project Layout

```
[ProjectName]/
├── GUI/
├── Config/
├── Module/
└── Interface/

AutoTest/
├── AutoTest.sln
├── [ProjectName].Tests/
│   ├── Unit/
│   ├── Integration/
│   └── Helpers/
```

## Code Standards

From `constitution_CH.md` — these are mandatory:

- All code comments and XML docs must be in **English**; specs are in Traditional Chinese (zh-TW).
- Constructor injection required: null-check with `ArgumentNullException`, store as `private readonly`.
- Event-exposing dependencies use property with subscribe/unsubscribe pattern in setter (constitution Section IV.2).
- Single `.cs` file per module. No new classes or files without explicit user approval.
- Methods: prefer ≤50 lines, ≤3 nesting levels, ≤4 parameters.
- Public/internal methods must be wrapped in try-catch with logging before rethrow.
- Constants prefer `enum`, **except** error codes which must use `int` with `const int` fields.

### Error Code Policy

All public methods returning error codes must use `int` (not enum), with `const int` fields per module:

| Range | Meaning |
|-------|---------|
| 0 | Success |
| 1–99 | Info (rarely used) |
| 100–199 | Warning |
| < 0 | Error (module-specific) |

Module error ranges (negative): E84 (-1 to -99), LoadportActor (-100 to -199), N2Purge (-200 to -299), CarrierIDReader (-300 to -399), LightCurtain (-400 to -499).

## Reference Implementation

`lp204.cc` is the existing C++ loadport controller. It is **reference only** — used to understand what functionality exists and what is returned to the Host. It is NOT an architecture blueprint for the C# implementation. For architecture, always follow `constitution_CH.md`.

### TDK A Protocol (TAS300 Communication)

Frame format: `SOH | LEN | ADR | CMD | CSh | CSl | DEL`
- RS-232C: 9600 bps default, 8N1, no flow control
- Checksum: sum bytes from LEN to end of CMD, take low 8 bits, encode as 2-char ASCII hex
- Command handshaking: Send command → receive ACK → wait for INF/ABS completion
- Quick commands (GET/EVT/MOD): wait for ACK only
- Operation commands (MOV/SET): wait for ACK then INF/ABS
- Omission mode enabled: no FIN command required
- Two-stage timeout: ACK (5s), then INF/ABS (10s, E191E372 §3.4 default)

Protocol details: `E191E372 Interface Specifications.md`
Method procedures (44+ operations): `LP_Method_Procedure.md`
Spec for command/response handling: `spec_CH.md` Section 3

### Host Commands and Responses

The system accepts commands from a host controller in the format `io <command> [params]\r\n` and returns responses as `io <command> <status> [data]\r\n`. Status codes: `0x1` = success, `0x2` = accepted/executing, `0xc017` = execution failed, `0xc021` = busy/not ready.

**Supported host commands** (extracted from `lp204.cc`):

| Command | Parameters | Function |
|---------|-----------|----------|
| `init` | — | Origin search (ORGSH) |
| `initx` | `2` | Abort origin (ABORG) |
| `load` | `[1\|2]` | Load FOUP: no param=full, 1=to dock, 2=clamp only |
| `unload` | `[1\|2]` | Unload FOUP: no param=full, 1=to dock, 2=to clamp |
| `evon` | `<eventID>` | Enable event reporting (0x2001, 0x2002, 0x200d, 0x200e, 0x2022, 0x2027, 0x2029, 0x1092) |
| `evoff` | `<eventID>` | Disable event reporting |
| `id` | — | Read loadport identification |
| `statfxl` | — | Query fixload status |
| `statnzl` | — | Query nozzle (N2 purge) status |
| `stat_m` | — | Query machine status |
| `stat_pdo` | — | Query FOUP placement status |
| `stat_lp` | — | Query loadport status |
| `lamp` | `<id> <act>` | Control lamp (id=1-10, act=0:off/1:on/2:blink) |
| `map` | — | Execute wafer mapping |
| `rmap` | — | Return mapping result |
| `rdid` | `<page>` | Read carrier ID (page 1-17, 98=tag, 99=all) |
| `wrid` | `<page> <lotID>` | Write carrier ID |
| `resid` | — | Reset carrier ID reader |
| `e84t` | `<tp1-tp6> <td1>` | Set E84 timing parameters |
| `smcr` | — | Save E84 settings to flash |
| `ene84` | `<onoff> <addr>` | Enable/disable E84 |
| `ene84nz` | `<onoff>` | Enable/disable E84 with N2 purge |
| `enltc` | `<onoff>` | Enable/disable light curtain |
| `rde84` | `<addr>` | Read E84 I/O status |
| `ho_avbl` | `<val> <addr>` | Set HO_AVBL signal |
| `es` | `<val> <addr>` | Set ES signal |
| `out_e84` | `<hex4> <addr>` | Direct E84 output write |
| `act_purge` | — | Activate N2 purge |
| `deact_purge` | — | Deactivate N2 purge |
| `getconf` | — | Read configuration |
| `setconf` | `<p1-p4>` | Write configuration |
| `esmode` | `<mode>` | Set ES mode (0=normal, 1=always ON) |
| `mch` | `<hex>` | Set machine status bits |
| `shutdown` | `[0\|1]` | 0=reboot, 1=shutdown |
| `date` | `[YYYY MM DD hh mm]` | Query or set system date |
| `ver_sbc` | — | Check SBC hardware version |
| `mrt` | `<val>` | Set RS232 max retry count |
| `update` | `<len>` | Firmware update (followed by binary transfer) |

### State Machines

- **Program state** (TDKService): `NOTINIT(0)` → `READY(1)` ↔ `BUSY(2)`. Managed by TDKService (Busy Guard). Commands rejected when busy.
- **Fixload/AMHS state** (LoadportActor): `NOTINIT(0)`, `READY(1)`, `BUSY(2)`, `AMHS(3)`.
- **FOUP status** (LoadportActor): `UNKNOWN(-1)`, `NOFOUP(0)`, `PLACED(1)`, `CLAMPED(2)`, `DOCKED(3)`, `OPENED(4)`.

### Special Event Codes

Reported to host via event system:
- `0x8000`: TAS300 error code / startup event
- `0x8001`: TAS300 status code
- `0x8002`: Operation return code
- `0x8003`/`0x8004`: N2 purge status / PGWFL event
- `0x8021`: Barcode reader error (0x01=motor, 0x02=timeout, 0x03=NG, 0x04=ERROR)
- `0x8024`: Hermos RFID error (0x10=timeout)
- `0x8027`: Omron RFID error (0x1*=comm, 0x7*=hardware, 0xF0=timeout)
- `0x8030`: E84 I/O and FOUP status
- `0x8031`: E84 loop error code

### Hardware Devices

The system communicates with these devices (all via RS-232C through `IConnector`):

| Device | Baud | Config | Purpose |
|--------|------|--------|---------|
| TAS300 Loadport | 9600 | 8N1 | Main loadport control (TDK A protocol) |
| Host Controller | 9600 | 8N1 | Text command protocol (`\r\n` terminated) |
| Keyence BL-600 | 9600 | 7E1 | Barcode reader |
| Hermos RFID | 19200 | 8E1 | RFID carrier ID reader (dual checksum: ADD + XOR) |
| Omron V700 | 9600 | 8E1 | RFID carrier ID reader (ASCII or HEX mode) |

Carrier ID reader is runtime-configurable: `1`=barcode, `2`=Hermos, `3`=Omron ASCII, `4`=Omron HEX.

### E84 AMHS Cycle

11-state automated load/unload cycle: `ENABLED` → `CS_ON` → `VALID_ON` → `LUREQ_ON` → `TRREQ_ON` → `READY_ON` → `BUSY_ON` → `LUREQ_OF` → `COMPT_ON` → `READY_OF` → `VALID_OF` → back to `ENABLED`.

Timing constants (configurable via `e84t`): TP1=2s, TP2=2s, TP3=60s, TP4=60s, TP5=2s, TP6=2s, TD1=1s.

### Dual-Port Architecture

The system supports two loadport instances (LP1, LP2) that are cross-linked as siblings. Light curtain control requires cross-port coordination. E84 uses separate digital I/O ports per loadport.
