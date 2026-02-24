# LP Method Procedure 流程文件

> **來源：** `lp204.cc` — CLP 類別  
> **用途：** 供後續程式開發參考使用  
> **版本日期：** 2026-02-08

### 文件符號說明

| 符號 | 說明 |
|------|------|
| `→ Host:` | 透過通訊介面發送訊息給 Host（原始碼: `m_host->m_pSerial->SendBlock`） |
| `res` | 硬體操作結果（0 = 成功，非 0 = 失敗） |
| `⚠️ BUG` | 原始碼中發現的問題，新系統開發時應修正 |
| `📌 NOTE` | 與其他方法的差異或需特別注意之處 |

---

## 共用流程模式

以下為多個方法中重複出現的共用模式，各方法以 **[模式X]** 引用。

### [模式A] Busy Guard — PRG 狀態閘門

```
IF m_prgState != prg_READY:
    → Host: io {cmd} 0xc021        ← 指令被拒絕
    RETURN
m_prgState = prg_BUSY
→ Host: io {cmd} 0x2               ← 通知 Host 已開始執行
```

**使用方法：** #1 Brkinit, #2 Brkinitx, #3 Brkload, #4 Brkunload, #14 Brkmap, #16 Brkrdid, #17 Brkwrid, #18 Brkresid, #35 BrkPurge

### [模式B] Status Report — 狀態回報

```
IF m_tas300->m_Status.ecode != 0:
    → Host: io event 0x8000 0x{ecode:02X}       ← TAS300 錯誤碼

IF eqpStatus!='0' OR mode!='0' OR inited!='1' OR opStatus!='0':
    → Host: io event 0x8001 0x{eqpStatus}{mode}{inited}{opStatus}  ← 設備狀態
```

**使用方法：** #1 Brkinit, #2 Brkinitx, #8 Brkstatfxl, #28 Brkene84；以及 #3, #4, #14, #35 的失敗路徑

### [模式C] Extended Error Handle — 擴展錯誤處理

```
[模式B]                                   ← 回報 0x8000 / 0x8001
IF eqpStatus == 'A':                      ← 可恢復錯誤
    m_tas300->rstErr()                    ← 嘗試重置
IF inited != '1':
    → Host: io event {evtCode} 0xc01c    ← 未初始化錯誤
ELSE:
    → Host: io event {evtCode} 0xc017    ← 執行錯誤
```

**使用方法：** #3 Brkload(0x1002), #4 Brkunload(0x1003), #14 Brkmap(0x1323), #35 BrkPurge(0x1130/0x1131)

### [模式D] Busy Guard Finish — PRG 狀態恢復

```
m_prgState = prg_READY
```

### [模式E] E84 Addr Guard — AGV 地址閘門

```
IF addr == 0 (AGV):
    → Host: io {cmd} {errCode}           ← AGV 不支援
    RETURN
```

**使用方法：** #28 Brkene84(0x1), #29 Brkrde84(0xc015), #32 Brkho_avbl(0xc015), #33 Brkes(0xc015), #34 Brkout_e84(0xc015)

---

## 目錄（依功能分類）

### 初始化與運動控制
| # | 方法 | 事件碼 | 說明 |
|---|------|--------|------|
| 1 | [Brkinit()](#1-brkinit) | `0x1000` | Origin Search 初始化 |
| 2 | [Brkinitx()](#2-brkinitx) | `0x1001` | Abort Origin 初始化 |
| 3 | [Brkload(ldtype)](#3-brkloadint-ldtype) | `0x1002` | Load FOUP |
| 4 | [Brkunload(uldtype)](#4-brkunloadint-uldtype) | `0x1003` | Unload FOUP |
| 14 | [Brkmap()](#14-brkmap) | `0x1323` | Wafer Mapping |
| 35 | [BrkPurge(purgeType)](#35-brkpurgeint-purgetype) | `0x1130/1131` | N2 Purge |

### 狀態查詢
| # | 方法 | 說明 |
|---|------|------|
| 7 | [Brkid()](#7-brkid) | 回報 LP 識別資訊 |
| 8 | [Brkstatfxl()](#8-brkstatfxl) | 查詢 FXL 狀態 |
| 9 | [Brkstatnzl()](#9-brkstatnzl) | 查詢 N2 Nozzle 狀態 |
| 10 | [Brkstat_m()](#10-brkstat_m) | 查詢 Module 狀態 |
| 11 | [Brkstat_pdo()](#11-brkstat_pdo) | 查詢 PDO 狀態 |
| 12 | [Brkstat_lp()](#12-brkstat_lp) | 查詢 LP 狀態 |
| 15 | [Brkrmap()](#15-brkrmap) | 讀取已存 Map 結果 |

### RFID / Barcode
| # | 方法 | 事件碼 | 說明 |
|---|------|--------|------|
| 16 | [Brkrdid(page)](#16-brkrdidint-page) | `0x1092` | 讀取 ID |
| 17 | [Brkwrid(page,lotID,len)](#17-brkwridint-page-char-lotid-int-lotidlen) | `0x1093` | 寫入 ID |
| 18 | [Brkresid()](#18-brkresid) | `0x10BC` | 重置 ID |

### 事件控制
| # | 方法 | 說明 |
|---|------|------|
| 5 | [Brkevon(evtID)](#5-brkevonchar-evtid) | 啟用事件通知 |
| 6 | [Brkevoff(evtID)](#6-brkevoffchar-evtid) | 關閉事件通知 |

### 燈號與機台
| # | 方法 | 說明 |
|---|------|------|
| 13 | [Brklamp(lampID,lampACT)](#13-brklampint-lampid-int-lampact) | 控制燈號 |
| 31 | [Brkmchstatus(status)](#31-brkmchstatusint-status) | 設定 Machine Status |

### E84 AMHS 控制
| # | 方法 | 說明 |
|---|------|------|
| 19 | [Brke84t(tp1..td1)](#19-brke84tint-tp1-tp2-tp3-tp4-tp5-tp6-td1) | 設定 E84 時序參數 |
| 28 | [Brkene84(onoff,addr)](#28-brkene84int-onoff-int-addr) | 啟用/停用 AMHS |
| 29 | [Brkrde84(addr)](#29-brkrde84int-addr) | 讀取 E84 I/O |
| 30 | [Brkesmode(mode)](#30-brkesmodeint-mode) | 設定 ES 模式 |
| 32 | [Brkho_avbl(ho_avbl,addr)](#32-brkho_avblint-ho_avbl-int-addr) | 設定 HO_AVBL |
| 33 | [Brkes(es,addr)](#33-brkesint-es-int-addr) | 設定 ES 信號 |
| 34 | [Brkout_e84(out_e84,addr)](#34-brkout_e84char-out_e84-int-addr) | 直接設定 E84 Output |

### 配置管理
| # | 方法 | 說明 |
|---|------|------|
| 20 | [Brksmcr()](#20-brksmcr) | Save to Flash |
| 21 | [Brkenltc(onoff)](#21-brkenltcint-onoff) | LTC 啟用/停用 |
| 22 | [Brkene84nz(onoff)](#22-brkene84nzint-onoff) | E84 N2 Nozzle 控制 |
| 23 | [Brkgetconf()](#23-brkgetconf) | 讀取設定 |
| 24 | [Brksetconf(p1..p4)](#24-brksetconfint-p1-p2-p3-p4) | 寫入設定 |

### 系統維護
| # | 方法 | 說明 |
|---|------|------|
| 25 | [Brkshutdown(cmd)](#25-brkshutdownint-cmd) | 關機/重啟 |
| 26 | [Brkupdate(lengthStr,len)](#26-brkupdatechar-lengthstr-int-len) | 啟動韌體更新 |
| 27 | [Brkassemblefile(received,len)](#27-brkassemblefilechar-received-int-len) | 組裝更新檔案 |
| 36 | [Brkdate()](#36-brkdate) | 回報日期時間 |
| 43 | [CheckHWstatus()](#43-checkhwstatus) | 檢查 SBC 版本 |
| 44 | [SendToHostMaxReceiveTimes()](#44-sendtohostmaxreceivetimes) | 回報重試次數 |

### 事件回呼（由硬體/E84 觸發）
| # | 方法 | 事件碼 | 說明 |
|---|------|--------|------|
| 37 | [E84_8031_Event(errorcode)](#37-e84_8031_eventint-errorcode) | `0x8031` | E84 錯誤事件 |
| 38 | [E84_8030_Event(foup_errcond)](#38-e84_8030_eventbool-foup_errcond) | `0x8030` | E84 I/O 狀態事件 |
| 39 | [E84_st_chg / E84_st_chg2](#39-e84_st_chg--e84_st_chg2) | `0x2022` | E84 狀態變更 |
| 40 | [TasPodEvt(off_2_on)](#40-taspodevtint-off_2_on) | `0x2001/2/d/e` | FOUP 事件 |
| 41 | [TasManSwEvt()](#41-tasmanswevt) | `0x2027` | 手動開關事件 |
| 42 | [TasPGEvent(evtparm)](#42-taspgeventchar-evtparm) | `0x8004` | N2 壓力事件 |

---

## 1. Brkinit()

**用途：** Load Port 初始化（Origin Search）

**流程：**

1. **[模式A]** Busy Guard：`cmd = "init"`
2. 執行 `res = m_tas300->movOP("ORGSH")`
3. 呼叫 `GetFxlAmhsStatus()`
4. **[模式B]** Status Report — 回報 `0x8000` / `0x8001`（📌 **不論 res 成功或失敗都會執行**）
5. 結果判斷：
   - ✅ `res == 0` → Host: `io event 0x1000 0x1`
   - ❌ `res != 0` → Host: `io event 0x1000 0xc01c`
6. **[模式D]** 恢復 `prg_READY`

---

## 2. Brkinitx()

**用途：** Load Port 初始化（Abort Origin）

**流程：**

1. **[模式A]** Busy Guard：`cmd = "initx"`
2. 執行 `res = m_tas300->movOP("ABORG")`
3. 呼叫 `GetFxlAmhsStatus()`
4. **[模式B]** Status Report（📌 **不論 res 成功或失敗都會執行**）
5. 結果判斷：
   - ✅ `res == 0` → Host: `io event 0x1001 0x1`
   - ❌ `res != 0` → Host: `io event 0x1001 0xc01c`
6. **[模式D]** 恢復 `prg_READY`

---

## 3. Brkload(int ldtype)

**用途：** Load FOUP

**參數對應 movOP 命令：**

| ldtype | movOP 命令 | 說明 |
|--------|-----------|------|
| `0` | `"CLOAD"` | Load |
| `1` | `"CLDYD"` | Clamp |
| 其他 | `"PODCL"` | dock |

**流程：**

1. **[模式A]** Busy Guard：`cmd = "load"`
2. 依 `ldtype` 執行對應 `res = m_tas300->movOP(...)`
3. 呼叫 `GetFxlAmhsStatus()`
4. 結果判斷：
   - ✅ `res == 0` → Host: `io event 0x1002 0x1`
   - ❌ `res != 0`：
     1. → Host: `io event 0x8002 {res}` — TAS300 操作返回碼
     2. **[模式C]** Extended Error Handle：`evtCode = 0x1002`
5. **[模式D]** 恢復 `prg_READY`

> 📌 **與 Brkinit/Brkinitx 的差異：** [模式B] Status Report 僅在**失敗路徑**內執行，成功時不回報 `0x8000`/`0x8001`。並且失敗時額外發送 `0x8002` 診斷事件。

---

## 4. Brkunload(int uldtype)

**用途：** Unload FOUP

**參數對應 movOP 命令：**

| uldtype | 條件 | movOP 命令 | 說明 |
|---------|------|-----------|------|
| `0` | `m_fpStatus == FPS_CLAMPED` | `"PODOP"` | Pod Open（加速） |
| `0` | 其他 | `"ABORG"` | Abort Origin（避免連續 CULOD 問題） |
| `1` | — | `"CULYD"` | Undock |
| 其他 | — | `"CULFC"` |  CloseDoor |

**流程：**

1. **[模式A]** Busy Guard：`cmd = "unload"`
2. 依 `uldtype` 及 `m_fpStatus` 執行對應 `res = m_tas300->movOP(...)`
3. 呼叫 `GetFxlAmhsStatus()`
4. 結果判斷：
   - ✅ `res == 0` → Host: `io event 0x1003 0x1`
   - ❌ `res != 0`：
     1. → Host: `io event 0x8002 {res}` — TAS300 操作返回碼
     2. **[模式C]** Extended Error Handle：`evtCode = 0x1003`
5. **[模式D]** 恢復 `prg_READY`

---

## 5. Brkevon(char *evtID)

**用途：** 啟用事件通知

**流程：**

1. 檢查 `evtID` 是否為 `"0x2001"`
   - ❌ 否 → Host: `io evon 0x1`，RETURN
2. 執行 `m_tas300->evtON()` — 啟用 TAS300 事件通知
3. 執行 `m_tas300->fpeON()` — 啟用 TAS300 Foup 事件通知
4. → Host: `io evon 0x1`

---

## 6. Brkevoff(char *evtID)

**用途：** 關閉事件通知

**流程：**

1. → Host: `io evoff 0x1`

---

## 7. Brkid()

**用途：** 回報 Load Port 識別資訊（含版本號）

**流程：**

1. → Host: `io id 0x1 2023 10 20 {_SBCVersion} 204`

**欄位說明：**

| 位置 | 值 | 說明 |
|------|-----|------|
| id0 | `2023` | Code Number |
| id1 | `10` | Reserved |
| id2 | `20` | Reserved |
| id3 | `{_SBCVersion}` | SBC 版本 (1=舊版, 2=新版) |
| id4 | `204` | Firmware Revision |

---

## 8. Brkstatfxl()

**用途：** 查詢 Load Port FXL 狀態 (foup status + 設備狀態)

> 📌 **注意：** 此方法直接呼叫 `m_tas300->statfxl()`，而非 `GetFxlAmhsStatus()`，不會更新 `m_fxlamhsState` 狀態機。

**流程：**

1. 執行 `res = m_tas300->statfxl()`
2. 結果判斷：
   - ❌ `res != 0` → Host: `io statfxl 0xc017 {res}`，RETURN
3. **[模式B]** Status Report — 回報 `0x8000` / `0x8001`
4. → Host: `io statfxl 0x1 {m_tas300->m_statfxl}`

---

## 9. Brkstatnzl()

**用途：** 查詢 N2 Purge Nozzle 狀態

**流程：**

1. 執行 `res = m_tas300->statn2purge()`
2. 結果判斷：
   - ✅ `res == 0` → Host: `io statnzl 0x1 0x{gasPressure}{nozzlePos}`
   - ❌ `res != 0` → Host: `io statnzl 0xc017 {res}`

---

## 10. Brkstat_m()

**用途：** 查詢 Module 狀態（固定值）

**流程：** → Host: `io stat_m 0x1 0x03`

---

## 11. Brkstat_pdo()

**用途：** 查詢 PDO 狀態（固定值）

**流程：** → Host: `io stat_pdo 0x1 0x00`

---

## 12. Brkstat_lp()

**用途：** 查詢 LP 狀態（固定值）

**流程：** → Host: `io stat_lp 0x1 0x00`

---

## 13. Brklamp(int lampID, int lampACT)

**用途：** 控制燈號操作

**參數：**
- `lampID` — 燈號 ID
- `lampACT` — 0:OFF, 1:ON, 2:Blink

**流程：**

1. 執行 `res = m_tas300->lampOP(lampID, lampACT)`
2. 結果判斷：
   - ✅ `res == 0` → Host: `io lamp 0x1`
   - ❌ `res != 0` — ⚠️ **不回傳任何訊息** (僅寫 log)

---

## 14. Brkmap()

**用途：** 執行 Wafer Mapping

> 📌 **注意：** 此方法**不呼叫** `GetFxlAmhsStatus()`（與 Brkinit/Brkload 等不同）。

**流程：**

1. **[模式A]** Busy Guard：`cmd = "map"`
2. 執行 `res = m_tas300->movOP("MAPDO")`
3. 結果判斷：
   - ❌ `res != 0`（movOP 失敗）：
     1. **[模式C]** Extended Error Handle：`evtCode = 0x1323`
     2. GOTO 結束
   - ✅ `res == 0`（movOP 成功，讀取 map 資料）：
     1. 執行 `res = m_tas300->mapResult()`
     2. 若 `res != 0` → Host: `io event 0x1323 0xc017`，GOTO 結束
     3. 若 `res == 0`：
        - 複製結果到 `m_mapRes`，轉換為 SEMI 格式
        - → Host: `io event 0x1323 0x1 '{m_mapRes}'`
4. **[模式D]** 恢復 `prg_READY`

**SEMI 格式轉換表：**

| TAS300 原始值 | SEMI 值 | 說明 |
|--------------|---------|------|
| `'0'` | `'1'` | No wafer |
| `'1'` | `'3'` | Wafer normal |
| `'2'` | `'5'` | Crossed |
| `'W'` | `'4'` | Double slotted |
| 其他 | `'0'` | Undefined |

---

## 15. Brkrmap()

**用途：** 讀取已儲存的 Map 結果（不重新 mapping）

**流程：** → Host: `io rmap 0x1 '{m_mapRes}'`

---

## 16. Brkrdid(int page)

**用途：** 讀取 RFID / Barcode 資料

**參數：** `page` — 有效範圍: 1~17, 98, 99

**流程：**

1. 檢查 `page` 範圍 → 無效則 RETURN
2. **[模式A]** Busy Guard：`cmd = "rdid"`
3. 依據 ID Reader 類型分支處理：

### 3a. BL600 (Barcode Reader)

```
MotorON()
  ├─ 失敗 → Host: io event 0x1092 0xc017
  │         → Host: io event 0x8021 0x01
  │         GOTO MotorOFF
  └─ 成功 →
      迴圈讀取 (最多 8 次, 間隔 2 秒):
        ReadBarCode(bcode, 50, &len)
        直到 m_readBcodeFinish == true 或 8 次用盡
      ├─ 逾時/失敗:
      │   → Host: io event 0x1092 0xc017
      │   ├─ m_readBcodeFinish==false → Host: io event 0x8021 0x05
      │   └─ 其他               → Host: io event 0x8021 0x02
      ├─ 讀到 "NG":
      │   → Host: io event 0x1092 0xc017 + io event 0x8021 0x03
      ├─ 讀到 "ERROR":
      │   → Host: io event 0x1092 0xc017 + io event 0x8021 0x04
      └─ 成功:
          ESC (0x1b) 填充至 16 字元
          → Host: io event 0x1092 0x1 {page} '{bcode}{padding}'
MotorOFF()
```

### 3b. Hermos RFID

```
page != 98,99 → ReadRFID(page, content, 300, &len)      ← 單頁
page == 98,99 → ReadMULTIPAGE(page, content, 500, &len) ← 多頁
  ├─ 失敗 (res != 0):
  │   → Host: io event 0x1092 0xc017
  │   ├─ res == 6 → Host: io event 0x8024 0x0{m_hmos->m_failCode}
  │   └─ 其他     → Host: io event 0x8024 0x10
  └─ 成功 (res == 0):
      → Host: io event 0x1092 0x1 {content}    ← 📌 不含 page，不含引號
```

### 3c. Omron RFID

```
ReadRFID(page, content, 300, &len)
  ├─ 失敗 (res != 0):
  │   → Host: io event 0x1092 0xc017
  │   ├─ res == 6 → Host: io event 0x8027 0x{m_omron->m_CompleteCode}
  │   └─ 其他     → Host: io event 0x8027 0xF0
  └─ 成功 (res == 0):
      → Host: io event 0x1092 0x1 {page} '{content}'   ← 📌 含 page，含引號
```

4. **[模式D]** 恢復 `prg_READY`

> 📌 **Hermos vs Omron 回傳格式不同：** Hermos 成功回傳不含 page 與引號；Omron 成功回傳含 page 與引號。錯誤診斷碼也不同 (Hermos: `0x8024`, Omron: `0x8027`)。

---

## 17. Brkwrid(int page, char* lotID, int lotIDlen)

**用途：** 寫入 RFID 資料

**參數：** `page` (有效: 1~17)、`lotID` (字串)、`lotIDlen` (必須 >= 16)

**流程：**

1. 檢查 `page` 範圍 (1~17) → 無效則 RETURN
2. 檢查 `lotIDlen >= 16` → 不足則 RETURN
3. **[模式A]** Busy Guard：`cmd = "wrid"`
4. 依據 ID Reader 類型分支處理：

| ID Reader | 流程 |
|-----------|------|
| **BL600** | 直接 → Host: `io event 0x1093 0x1`（Barcode 不支援寫入） |
| **Hermos** | 取 lotID 前 16 字元†，`m_hmos->WriteRFID(page, s, 16)` |
| **Omron** | 取 lotID 前 16 字元†，`m_omron->WriteRFID(page, s, 16)` |

> † lotIDlen == 16 → 從 index 0 開始；否則從 index 1 開始

**Hermos 結果判斷：**
- ✅ `res == 0` → Host: `io event 0x1093 0x1`
- ❌ `res != 0` → Host: `io event 0x1093 0xc017`
  - `res == 7` → Host: `io event 0x8024 0x0{m_hmos->m_failCode}`
  - 其他 → Host: `io event 0x8024 0x10`

**Omron 結果判斷：**
- ✅ `res == 0` → Host: `io event 0x1093 0x1`
- ❌ `res != 0` → Host: `io event 0x1093 0xc017`
  - `res == 7` → Host: `io event 0x8027 0x{m_omron->m_CompleteCode}`
  - 其他 → Host: `io event 0x8027 0xF0`

5. **[模式D]** 恢復 `prg_READY`

---

## 18. Brkresid()

**用途：** 重置 ID (RFID Reset)

**流程：**

1. **[模式A]** Busy Guard：`cmd = "resid"`
2. 等待 2 秒 (`sleep(2)`)
3. → Host: `io event 0x10BC 0x1`
4. **[模式D]** 恢復 `prg_READY`

---

## 19. Brke84t(int tp1, tp2, tp3, tp4, tp5, tp6, td1)

**用途：** 設定 E84 時序參數

**流程：**

1. 寫入 `m_e84->m_tp1` ~ `m_e84->m_tp6` 及 `m_e84->m_td1`
2. 設定 `m_e84->m_constChanged = true`
3. → Host: `io e84t 0x1`

---

## 20. Brksmcr()

**用途：** Save to Flash (儲存設定)

**流程：** → Host: `io smcr 0x1`

---

## 21. Brkenltc(int onoff)

**用途：** 啟用/停用 Light Curtain (LTC)

**參數：** 0:停用, 1:啟用(傳輸中), 2:啟用(永遠)

**流程：**

1. 若 `_LTCEnDis == 2` (Always) → 拒絕覆寫（📌 防止 Host GUI 初始化時覆蓋 Always 設定）
2. 否則：
   1. `_LTCEnDis = onoff`
   2. 更新 `m_pCfg->cfgfileStr[7]`
   3. 比對 LP ID Reader 是否變更 (`cfgfileStr[1]`, `cfgfileStr[3]` vs `orgcfgfStr`)
   4. `m_pCfg->wrCfgnSet(...)` 寫入配置檔
3. → Host: `io enltc 0x1`

---

## 22. Brkene84nz(int onoff)

**用途：** 啟用/停用 E84 中 N2 Purge Nozzle Up/Down

**參數：** 0:停用, 1:啟用

**流程：**

1. 若 `_N2PurgeNozzleDown_InE84 != onoff`：
   1. `_N2PurgeNozzleDown_InE84 = onoff`
   2. `ret = m_pCfg->wrSingleCfgFile(1, onoff)`
   3. 若 `ret != 0` → Host: `io ene84nz 0xc017`，RETURN
2. → Host: `io ene84nz 0x1`

---

## 23. Brkgetconf()

**用途：** 讀取 Load Port 設定

**流程：**

1. 轉換 ID Reader 編碼：`1→'b', 2→'h', 3→'o', 4→'m'`
2. → Host: `io getconf 0x1 '1{rd1}2{rd2} lten={_LTCEnDis} onlv={_LTCOnLevel}'`

---

## 24. Brksetconf(int p1, p2, p3, p4)

**用途：** 寫入 Load Port 設定

**參數：**

| 參數 | 說明 | 值域 |
|------|------|------|
| `p1` | LP1 ID Reader | 1:Barcode, 2:Hermos, 3:Omron, 4:m |
| `p2` | LP2 ID Reader | 同上 |
| `p3` | LTC Enable | 0:Disable, 1:InTransfer, 2:Always |
| `p4` | LTC ON Level | 0:0V, 1:24V |

**流程：**

1. 轉換 `p1`, `p2` 為字元碼 → 寫入 `cfgfileStr[1]`, `cfgfileStr[3]`
2. `p3` → `cfgfileStr[7]`，`p4` → `cfgfileStr[10]`
3. 比對 ID Reader 是否變更
4. `m_pCfg->wrCfgnSet(...)` 寫入配置檔
5. → Host: `io setconf 0x1`

---

## 25. Brkshutdown(int cmd)

**用途：** 系統關機/重啟

**流程：**

1. → Host: `io shutdown {cmd} 0x1`
2. `cmd == 1` → `system("shutdown -h now")` (關機)；其他 → `system("shutdown -r now")` (重啟)

---

## 26. Brkupdate(char *lengthStr, int len)

**用途：** 啟動韌體更新流程（準備接收檔案）

**流程：**

1. 若 `len > 20` → RETURN
2. 解析 `lengthStr` → `m_updatefilelen`
3. `m_receivedlen = 0`
4. `m_bacceptingfile = true`
5. `update_timeout = 600` (600 秒)
6. → Host: `io update 0x2`

---

## 27. Brkassemblefile(char *received, int len)

**用途：** 接收並組裝更新檔案

**流程：**

1. `m_receivedlen += len`，`called_count++`
2. 首次呼叫時 (called_count == 1)：
   - 備份 `/usr/tdk/lp/lp.cc` → `/usr/tdk/lp/lp-{MM_DD_YYYY-HH_MM_SS}-bk.cc`
   - 建立新 `/usr/tdk/lp/lp.cc`
3. 寫入 `received` 至檔案
4. 當 `m_receivedlen >= m_updatefilelen` 或 `update_timeout == 0`：
   1. 重置所有狀態
   2. 關閉檔案
   3. 編譯：`system("make -C /usr/tdk/lp 2>/usr/tdk/lp/cmpile_err.txt")`
   4. 等待 8 秒
   5. 讀取編譯結果：
      - 無錯誤 → Host: `io event 0x8010 0x1 'compile succeed'`
      - 有錯誤 → Host: `io event 0x8010 0xff '{error_content}'`
      - 檔案無法開啟 → Host: `io event 0x8010 0xff 'compile_err file not created'`

> ⚠️ **BUG（原始碼 lp204.cc）：** 在步驟 4.1 中，`m_receivedlen` 和 `m_updatefilelen` 都被重置為 0，之後步驟 4.3 的條件判斷 `m_receivedlen == m_updatefilelen` 永遠為 `true`（0 == 0）。因此「長度不符」的 else 分支 (`io event 0x8010 0xff 'wrong length'`) 永遠不會被執行。新系統實作時應在重置前保存原始值進行比對。

---

## 28. Brkene84(int onoff, int addr)

**用途：** 啟用/停用 E84 AMHS

**參數：** `onoff` (1:啟用, 0:停用)、`addr` (1:OHT, 0:AGV)

**流程：**

1. **[模式E]** Addr Guard — `addr == 0` → Host: `io ene84 0x1`（📌 回覆成功而非 0xc015），RETURN

### 啟用 (onoff == 1)

```
IF m_fxlamhsState == fxl_AMHS:
    已啟用 → Host: io ene84 0x1, RETURN

Lget:
    res = GetFxlAmhsStatus()
    ├─ res != 0 → Host: io ene84 0xc017, RETURN
    └─ res == 0:
        [模式B] Status Report
        IF m_fxlamhsState != fxl_READY:
            ├─ m_fpStatus > FPS_CLAMPED → Host: io ene84 0xc021, RETURN
            └─ m_fpStatus <= FPS_CLAMPED (且 _UnclampInE84 未定義):
                res = m_tas300->movOP("CULOD")
                ├─ 失敗 → Host: io ene84 0xc017, RETURN
                └─ 成功 → GOTO Lget
        res = m_e84->EnableAMHS()
        ├─ 失敗 → Host: io ene84 0xc017, RETURN
        └─ 成功:
            m_fxlamhsState = fxl_AMHS
            → Host: io ene84 0x1
```

### 停用 (onoff == 0)

1. `sleep(1)`
2. `m_e84->DisableAMHS()`
3. `m_fxlamhsState = fxl_READY`
4. `GetFxlAmhsStatus()` — 不檢查回傳值
5. **[模式B]** Status Report
6. → Host: `io ene84 0x1`

---

## 29. Brkrde84(int addr)

**用途：** 讀取 E84 I/O 狀態

**流程：**

1. **[模式E]** Addr Guard — `addr == 0` → Host: `io rde84 0xc015`，RETURN
2. `res = m_e84->R_INPUT(&VALID, &CS_0, &CS_1, &LTCIN, &TR_REQ, &BUSY, &COMPT, &CONT)`
   - ❌ `res != 0` → Host: `io rde84 0xc017`，RETURN
3. 取得 `inport = m_e84->m_Input`，`outport = m_e84->m_Output`
4. 轉為 2 位 hex 字串 (`prm1`, `prm2`)
5. → Host: `io rde84 0x1 {prm1} {prm2}`

---

## 30. Brkesmode(int mode)

**用途：** 設定 E84 Emergency Stop 模式

**參數：** 0:Normal Mode, 1:Always ON Mode

**流程：**

1. 若 `mode` 在 0~1 範圍內：
   1. `_ESMode = mode`
   2. 若 `_ESMode == 1`：
      - `res = m_e84->W_ES(1)`
      - ❌ `res != 0` → ⚠️ **BUG：原始碼執行 `return` 但未發送 Host 訊息**，Host 不會收到任何回應
   3. → Host: `io esmode 0x1`
2. 若 `mode` 不在範圍內 → Host: `io esmode 0xc013`

> ⚠️ **BUG（原始碼 lp204.cc）：** 當 `mode == 1` 且 `W_ES(1)` 失敗時，程式碼將 `s` 設為 `"io esmode 0xc017"` 後直接 `return`，跳過了後面的 `SendBlock` 呼叫。Host 端將不會收到任何回應。新系統應在 return 前發送錯誤訊息。

---

## 31. Brkmchstatus(int status)

**用途：** 設定 Machine Status

**參數：** 0~255

**流程：**

1. 若 `status` 在 0~255 → `m_e84->m_mchStatus = status`，→ Host: `io mch 0x1`
2. 否則 → Host: `io mch 0xc013`

---

## 32. Brkho_avbl(int ho_avbl, int addr)

**用途：** 設定 E84 Handoff Available 信號

**流程：**

1. **[模式E]** Addr Guard — `addr == 0` → Host: `io ho_avbl 0xc015`，RETURN
2. `res = m_e84->W_HO_AVBL(ho_avbl)`
   - ❌ `res != 0` → Host: `io ho_avbl 0xc017`，RETURN
3. → Host: `io ho_avbl 0x1`

---

## 33. Brkes(int es, int addr)

**用途：** 設定 E84 Emergency Stop 信號

**參數：** `es` (1:no emergency stop, 0:emergency stop)

**流程：**

1. **[模式E]** Addr Guard — `addr == 0` → Host: `io es 0xc015`，RETURN
2. `res = m_e84->W_ES(es)`
   - ❌ `res != 0` → Host: `io es 0xc017`，RETURN
3. → Host: `io es 0x1`

---

## 34. Brkout_e84(char *out_e84, int addr)

**用途：** 直接設定 E84 Output Port 值

**參數：** `out_e84` — 4 字元 hex 字串 (如 `"0x3F"`)

**流程：**

1. **[模式E]** Addr Guard — `addr == 0` → Host: `io out_e84 0xc015`，RETURN
2. 若 `m_fxlamhsState == fxl_AMHS` → Host: `io out_e84 0xc017`，RETURN
3. 解析 `out_e84[2..3]` 為 hex → `oe84`
   - 解析失敗 → Host: `io out_e84 0xc013`，RETURN
4. `res = m_e84->W_E84OUTPUT(SRC_HOST_out_e84_Brkout_e84, oe84)`
   - ❌ `res != 0` → Host: `io out_e84 0xc017`，RETURN
5. → Host: `io out_e84 0x1`

---

## 35. BrkPurge(int purgeType)

**用途：** 執行 N2 Purge Nozzle 動作

**參數對應：**

| purgeType | movOP | Host cmd | 成功碼 | 失敗碼 |
|-----------|-------|----------|--------|--------|
| `1` (Activate) | `"BPNUP"` | `act_purge` | `0x1130 0x1` | `0x1130 0xc901` |
| 其他 (Deactivate) | `"BPNDW"` | `deact_purge` | `0x1131 0x1` | `0x1131 0xc900` |

**流程：**

1. **[模式A]** Busy Guard：`cmd = "act_purge"` 或 `"deact_purge"`
2. 依 `purgeType` 執行對應 `res = m_tas300->movOP(...)`
3. 呼叫 `GetFxlAmhsStatus()`
4. 結果判斷：
   - ✅ `res == 0`：
     - → Host: `io event {成功碼}`
   - ❌ `res != 0`：
     1. → Host: `io event 0x8002 {res}` — TAS300 操作返回碼
     2. **[模式B]** Status Report — 回報 `0x8000` / `0x8001`
        - `ecode` 說明：`0x28` = nozzle up error; `0x68` = nozzle down error
     3. 若 `eqpStatus == 'A'` → `m_tas300->rstErr()`
     4. 查詢 `res = m_tas300->statn2purge()`
        - ✅ `res == 0` → Host: `io event 0x8003 0x{gasPressure}{nozzlePos}`
        - ❌ `res != 0` → Host: `io event 0x8003 {res}`
     5. → Host: `io event {失敗碼}`
5. **[模式D]** 恢復 `prg_READY`

> 📌 **與 Brkload/Brkunload 差異：** 失敗路徑不判斷 `inited`，而是額外查詢 N2 Purge 狀態 (`statn2purge`)，並使用專用失敗碼 (`0xc901`/`0xc900`)。

---

## 36. Brkdate()

**用途：** 回報系統日期時間

**流程：** → Host: `io date {YYYY MM DD HH mm}`

---

## 37. E84_8031_Event(int errorcode)

**用途：** 發送 E84 錯誤事件

**流程：** → Host: `io event 0x8031 {errorcode}`

---

## 38. E84_8030_Event(bool foup_errcond)

**用途：** 發送 E84 I/O 及 FOUP 事件狀態

**流程：**

1. 計算 `fpEvent`：
   - `foup_errcond == true` → `fpEvent = 0x10 | m_tas300->m_fpEvent`
   - `foup_errcond == false` → `fpEvent = m_tas300->m_fpEvent`
2. → Host: `io event 0x8030 0x{m_e84->m_Input:02x} 0x{m_e84->m_Output:02x} 0x{fpEvent:02x}`

---

## 39. E84_st_chg / E84_st_chg2

**用途：** E84 狀態變更事件

| 方法 | foup_errcond | 說明 |
|------|-------------|------|
| `E84_st_chg(evtcode, status)` | `false` | 非 foup 錯誤 |
| `E84_st_chg2(evtcode, status)` | `true` | foup 錯誤 |

**共同流程：**

1. 呼叫 `E84_8030_Event(foup_errcond)`
2. → Host: `io event 0x2022 {evtcode} {status}`

---

## 40. TasPodEvt(int off_2_on)

**用途：** TAS300 Pod（FOUP）事件處理

**參數：** 0=PODOF, 1=SMTON, 2=ABNST, 3=PODON

**狀態轉換與事件發送矩陣：**

| off_2_on | fpStatus 設定 | fpEvent 設定 | 前次狀態 (offonStatus) | 發送事件 |
|----------|--------------|-------------|----------------------|---------|
| **0** (PODOF) | `FPS_NOFOUP` | `FPEVT_PODOF` | -1 或 3 | `0x2002`(remove) + `0x200e`(not present) |
| | | | >0 (非-1,非3) | `0x200e`(not present) |
| **1** (SMTON) | *(不變)* | `FPEVT_SMTON` | -1 且 fpStatus==NOFOUP | `0x200d`(present) |
| | | | 0 | `0x200d`(present) |
| | | | 3 | `0x2002`(remove) |
| **2** (ABNST) | *(不變)* | `FPEVT_ABNST` | -1 且 fpStatus==PLACED | `0x2002`(remove) |
| | | | 3 | `0x2002`(remove) |
| **3** (PODON) | `FPS_PLACED` | `FPEVT_PODON` | -1 或 0 | `0x200d`(present) + `0x2001`(placed) |
| | | | 其他 | `0x2001`(placed) |

> 📌 所有事件的參數都是 `0x0`。事件發送後更新 `offonStatus` 為對應值 (0/1/2/3)。

---

## 41. TasManSwEvt()

**用途：** Manual Switch 按鈕觸發事件

**流程：** → Host: `io event 0x2027 0x0`

---

## 42. TasPGEvent(char *evtparm)

**用途：** N2 Pressure Gauge 事件

**流程：**

1. 若 `strlen(evtparm) < 3` → RETURN
2. 事件判斷：

| evtparm 前3字元 | Host 訊息 | 說明 |
|---------|-----------|------|
| `"1ON"` | `io event 0x8004 0x01` | N2 sensor ON |
| `"1OF"` | `io event 0x8004 0x02` | N2 sensor OFF |
| 其他 | `io event 0x8004 0x03` | 未知 |

---

## 43. CheckHWstatus()

**用途：** 檢查硬體狀態並判斷 SBC 版本

**流程：**

1. 取得 Linux kernel 主版本號
2. 判斷：

| 主版本 | _SBCVersion | Host 訊息 |
|--------|------------|-----------|
| >= 4 | 2 (新版) | `io sbc_new` |
| < 4 | 1 (舊版) | `io sbc_original` |
| 其他 | 1 | `io cannot recognize sbc version` |

3. → Host: `max retry times:{msgMaxRetries}`

---

## 44. SendToHostMaxReceiveTimes()

**用途：** 回報最大訊息解析重試次數

**流程：** → Host: `Parsing Msg Max Retry Times:{msgMaxRetries}`

---

---

## 附錄 A：需要實作的方法

### A.1 GetFxlAmhsStatus()

**用途：** 收集 Load Port 狀態，更新 FXL AMHS 狀態機

**被呼叫於：** #1, #2, #3, #4, #8(間接), #28, #35

**原始實作流程：**

```
res = m_tas300->statfxl()
IF res == 0:
    記錄 m_fpStatus (NOFOUP/PLACED/CLAMPED/DOCKED/OPENED/UNKNOWN)
    IF m_fxlamhsState != fxl_AMHS:    ← 若非 AMHS 模式才更新狀態
        IF inited == '0':
            m_fxlamhsState = fxl_NOTINIT
        ELSE:
            IF m_fpStatus <= FPS_PLACED:
                m_fxlamhsState = fxl_READY
            ELSE:
                m_fxlamhsState = fxl_BUSY
RETURN res
```

> ⚠️ **提醒：此方法需要實作。** 確保能正確收集 Load Port 狀態 (foup 狀態、設備狀態、錯誤碼) 並更新 FXL AMHS 狀態機。

### A.2 TasErrorHandle() — 建議封裝為共用方法

**說明：** 原始碼中此方法呼叫已被全部註解掉，改為各 method 內直接內嵌處理。建議新系統封裝為共用方法：

```
TasErrorHandle(evtCode):
    [模式B] Status Report (0x8000 / 0x8001)
    IF eqpStatus == 'A': rstErr()
    IF inited != '1':
        → Host: io event {evtCode} 0xc01c
    ELSE:
        → Host: io event {evtCode} 0xc017
```

> ⚠️ **提醒：建議實作。** 可統一 #3 Brkload, #4 Brkunload, #14 Brkmap, #35 BrkPurge 中重複的錯誤處理邏輯。

---

## 附錄 B：Host 回應碼對照表

| 回應碼 | 說明 | 使用場景 |
|--------|------|---------|
| `0x1` | 成功/完成 | 所有指令成功回應 |
| `0x2` | 執行中 (Accepted) | Busy Guard 進入後通知 |
| `0xc013` | 參數錯誤 | #30 Brkesmode, #31 Brkmchstatus, #34 Brkout_e84 |
| `0xc015` | 不支援 (AGV) | #29, #32, #33, #34 |
| `0xc017` | 執行失敗 | 通用執行錯誤 |
| `0xc01c` | 未初始化失敗 | #1, #2 失敗; #3, #4, #14 且 inited!='1' |
| `0xc021` | 指令被拒絕 | Busy Guard: 非 prg_READY 狀態 |

---

## 附錄 C：事件碼對照表

### 完成事件 (0x1xxx)

| 事件碼 | 方法 | 說明 |
|--------|------|------|
| `0x1000` | #1 Brkinit | Init 完成 |
| `0x1001` | #2 Brkinitx | Initx 完成 |
| `0x1002` | #3 Brkload | Load 完成 |
| `0x1003` | #4 Brkunload | Unload 完成 |
| `0x1092` | #16 Brkrdid | RFID/Barcode 讀取完成 |
| `0x1093` | #17 Brkwrid | RFID 寫入完成 |
| `0x10BC` | #18 Brkresid | Reset ID 完成 |
| `0x1130` | #35 BrkPurge | Purge Activate 完成 |
| `0x1131` | #35 BrkPurge | Purge Deactivate 完成 |
| `0x1323` | #14 Brkmap | Map 完成 |

### 狀態變更事件 (0x2xxx)

| 事件碼 | 方法 | 說明 |
|--------|------|------|
| `0x2001` | #40 TasPodEvt | FOUP Placed |
| `0x2002` | #40 TasPodEvt | FOUP Remove Action |
| `0x2022` | #39 E84_st_chg | E84 狀態變更 |
| `0x2027` | #41 TasManSwEvt | Manual Switch |
| `0x200d` | #40 TasPodEvt | FOUP Present |
| `0x200e` | #40 TasPodEvt | FOUP Not Present |

### 診斷/錯誤事件 (0x8xxx)

| 事件碼 | 說明 | 來源 |
|--------|------|------|
| `0x8000` | TAS300 Error Code | [模式B] |
| `0x8001` | Equipment Status | [模式B] |
| `0x8002` | TAS300 Operation Return Code | #3, #4, #35 失敗路徑 |
| `0x8003` | N2 Purge Status | #35 失敗路徑 |
| `0x8004` | N2 Pressure Gauge | #42 TasPGEvent |
| `0x8010` | 更新/編譯結果 | #27 Brkassemblefile |
| `0x8021` | Barcode Reader 錯誤 | #16 Brkrdid (BL600) |
| `0x8024` | Hermos RFID 錯誤 | #16, #17 (Hermos) |
| `0x8027` | Omron RFID 錯誤 | #16, #17 (Omron) |
| `0x8030` | E84 I/O + FOUP 狀態 | #38 E84_8030_Event |
| `0x8031` | E84 Error Code | #37 E84_8031_Event |

---

## 附錄 D：FOUP Status 定義

| 常數 | 值 | 說明 |
|------|-----|------|
| `FPS_UNKNOWN` | -1 | 未知 |
| `FPS_NOFOUP` | 0 | 無 FOUP |
| `FPS_PLACED` | 1 | 已放置 |
| `FPS_CLAMPED` | 2 | 已夾持 |
| `FPS_DOCKED` | 3 | 已對接 |
| `FPS_OPENED` | 4 | 已開啟 |

## 附錄 E：FOUP Event 定義

| 常數 | 值 | 說明 |
|------|-----|------|
| `FPEVT_NONE` | 0xFF | 無事件 |
| `FPEVT_PODOF` | 0 | Pod Off |
| `FPEVT_SMTON` | 1 | Sensor On |
| `FPEVT_ABNST` | 2 | Abnormal State |
| `FPEVT_PODON` | 3 | Pod On |

## 附錄 F：Program State 定義

| 常數 | 說明 |
|------|------|
| `prg_NOTINIT` | 未初始化 |
| `prg_READY` | 就緒 |
| `prg_BUSY` | 忙碌中 |

## 附錄 G：FXL AMHS State 定義

| 常數 | 說明 |
|------|------|
| `fxl_NOTINIT` | 未初始化 |
| `fxl_READY` | 就緒 |
| `fxl_BUSY` | 忙碌中 |
| `fxl_AMHS` | AMHS 已啟用 |

## 附錄 H：Equipment Status 結構

```
struct STATUS {
  char eqpStatus;  // '0' = normal, 'A' = recoverable error, 'E' = fatal error
  char mode;       // '0' = online, '1' = maintain
  char inited;     // '0' = not inited, '1' = inited
  char ecode;      // binary code, 0 = no error
  char opStatus;   // operation status
};
```

正常狀態: `eqpStatus='0', mode='0', inited='1', opStatus='0'`

---

## 附錄 I：原始碼已發現之 Bug 彙總

| # | 方法 | 問題描述 | 建議修正 |
|---|------|---------|---------|
| 1 | #27 Brkassemblefile | `m_receivedlen` 與 `m_updatefilelen` 在比對前已被重置為 0，導致 `"wrong length"` 分支為 dead code | 在重置前先保存原始值進行比對 |
| 2 | #30 Brkesmode | 當 `W_ES(1)` 失敗時，`return` 前未呼叫 `SendBlock`，Host 不會收到任何回應 | 在 `return` 前發送 `io esmode 0xc017` 給 Host |
| 3 | #13 Brklamp | 執行失敗時不回傳任何訊息給 Host | 建議失敗時也回傳錯誤碼 |
