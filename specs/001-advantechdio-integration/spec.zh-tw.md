# 功能規格：AdvantechDIO 專案整合與介面驗證

**功能分支**：`001-advantechdio-integration`
**建立日期**：2026-02-25
**狀態**：草稿
**輸入說明**：「在 solution file 新增一個 project reference 我已經做好的 project AdvantechDIO 並且測試所有 interface 的功能皆正常」

## 使用者情境與測試

### 使用者故事 1 — 解決方案可成功建置（P1）

作為開發者，我希望 AdvantechDIO 專案（及其測試專案）能正確納入主解決方案，確保整個 solution 能順利建置且產生 AdvantechDIO 組件。

**優先原因**：若專案無法在 solution 內建置，後續驗證皆無法進行，這是最基本的前提。

**獨立測試**：執行完整 solution build，確認 AdvantechDIO 與 AdvantechDIO.Tests 專案皆無建置錯誤。

**驗收情境**：
1. 已有 TDKServer.sln，當開發者以 Debug 組態建置 solution，則 AdvantechDIO 專案可無錯誤產生 AdvantechDIO.dll。
2. 同上，AdvantechDIO.Tests 專案可無錯誤產生 AdvantechDIO.Tests.dll。
3. AdvantechDIO 專案參考 DIO 與 TDKLogUtility，建置時皆能正確解析且無遺失參考警告。

---

### 使用者故事 2 — IIOBoardBase 介面驗證（P1）

作為開發者，我希望有單元測試能驗證所有 IIOBoardBase 介面成員（Connect、Disconnect、IsConnected、IsVirtual、DeviceID、DeviceName、ExceptionOccurred 事件）皆能正確運作，確保依賴此基礎合約的消費端都能正常運作。

**優先原因**：IIOBoardBase 定義連線生命週期，所有 I/O 操作都需先建立有效連線。

**獨立測試**：執行 AdvantechDIO.Tests 針對建構子、Connect、Disconnect、Dispose 測試群組，所有測試皆須通過。

**驗收情境**：
1. 傳入合法設定與 logger，建構 AdvantechDIO 物件後，DeviceID、InputPortCount、InputBitsPerPort、OutputPortCount、OutputBitsPerPort 皆反映設定值，IsConnected 與 IsVirtual 為 false，DeviceName 為空字串。
2. logger 或 config 為 null 時建構 AdvantechDIO，會丟出 ArgumentNullException。
3. 已連線裝置呼叫 Disconnect()，IsConnected 變為 false，DeviceName 清空，回傳 0。
4. 未連線裝置呼叫 Disconnect()，回傳 0 且無錯誤。
5. AdvantechDIO 實例多次呼叫 Dispose() 不會丟例外（具冪等性）。
6. 嘗試連線不存在裝置時呼叫 Connect()，會觸發 ExceptionOccurred 事件且回傳非 0 錯誤碼。

---

### 使用者故事 3 — IDI 介面驗證（P1）

作為開發者，我希望單元測試能驗證所有 IDI 介面成員（GetInput、GetInputBit、SnapStart、SnapStop、DI_ValueChanged 事件）皆有正確防呆，確保數位輸入操作遇到無效呼叫時能妥善處理。

**優先原因**：數位輸入為讀取 E84 訊號與感測器狀態之基礎。

**獨立測試**：針對所有 DI 方法執行 not-connected 與防呆路徑測試，皆回傳預期錯誤碼。

**驗收情境**：
1. 未連線時呼叫 GetInput()，回傳 NotConnectedError（-1001），輸出值為 0。
2. 未連線時呼叫 GetInputBit()，回傳 NotConnectedError（-1001）。
3. 已連線但未設定 DI（DIPortCount=0）時呼叫 SnapStart()，回傳 NotConnectedError。
4. 已連線但未設定 DI 時呼叫 SnapStop()，回傳 NotConnectedError。
5. 已連線且有 DI 設定時，GetInput() 傳入超出範圍 port index，回傳 PortIndexOutOfRangeError（-1002）。
6. 已連線且有 DI 設定時，GetInputBit() 傳入超出範圍 bit index，回傳 BitIndexOutOfRangeError（-1003）。

---

### 使用者故事 4 — IDO 介面驗證（P1）

作為開發者，我希望單元測試能驗證所有 IDO 介面成員（SetOutput、SetOutputBit、GetOutput、GetOutputBit、DO_ValueChanged 事件）皆有正確防呆，確保數位輸出操作遇到無效呼叫時能妥善處理。

**優先原因**：數位輸出為 E84 訊號（L_REQ、U_REQ、READY、HO_AVBL、ES）控制之關鍵。

**獨立測試**：針對所有 DO 方法執行 not-connected 與防呆路徑測試，皆回傳預期錯誤碼。

**驗收情境**：
1. 未連線時呼叫 SetOutput()，回傳 NotConnectedError（-1001）。
2. 未連線時呼叫 SetOutputBit()，回傳 NotConnectedError（-1001）。
3. 未連線時呼叫 GetOutput()，回傳 NotConnectedError（-1001），輸出值為 0。
4. 未連線時呼叫 GetOutputBit()，回傳 NotConnectedError（-1001），輸出值為 0。
5. 已連線且有 DO 設定時，SetOutput() 傳入超出範圍 port index，回傳 PortIndexOutOfRangeError（-1002）。
6. 已連線且有 DO 設定時，SetOutputBit() 傳入超出範圍 bit index，回傳 BitIndexOutOfRangeError（-1003）。

---

### 使用者故事 5 — 錯誤碼合約一致性驗證（P2）

作為開發者，我希望所有公開方法回傳錯誤碼皆遵循專案 int 型態規範（0=成功，負數=錯誤），以利消費端統一判斷結果。

**優先原因**：錯誤碼一致性為專案憲章要求，確保模組層錯誤處理可靠。

**獨立測試**：檢查所有公開方法回傳值皆為 int 並使用定義的 const int 錯誤碼。

**驗收情境**：
1. AdvantechDIO 所有公開方法（Connect、Disconnect、GetInput、GetInputBit、SetOutput、SetOutputBit、GetOutput、GetOutputBit、SnapStart、SnapStop）成功時皆回傳 0。
2. 所有防呆錯誤碼（NotConnectedError、PortIndexOutOfRangeError、BitIndexOutOfRangeError）皆為負數 const int。
3. 任一公開方法發生例外時，catch 區塊會回傳負數錯誤碼並寫入 log。

---

### 邊界情境

- 已連線時重複呼叫 Connect？（應立即回傳成功，不重複初始化）
- 僅設定 DI（DOPortCount=0）時呼叫 DO 方法？（DO 防呆應回傳 NotConnectedError）
- 僅設定 DO（DIPortCount=0）時呼叫 DI 方法？（DI 防呆應回傳 NotConnectedError）
- Dispose 時 Disconnect 內部丟例外？（應被捕捉，disposed flag 仍設為 true）
- port index 或 bit index 為負數？（應回傳 PortIndexOutOfRangeError 或 BitIndexOutOfRangeError）

## 功能需求

- **FR-001**：AdvantechDIO 專案必須被納入 TDKServer.sln，且所有平台組態（Debug/Release × AnyCPU/x86/x64/Mixed Platforms）皆有正確 GUID 與建置設定。
- **FR-002**：AdvantechDIO.Tests 測試專案必須納入 solution 的 AutoTest 資料夾，並正確參考 AdvantechDIO、DIO、TDKLogUtility。
- **FR-003**：solution 以 Debug 組態建置時，AdvantechDIO 與 AdvantechDIO.Tests 皆須無錯誤。
- **FR-004**：單元測試必須涵蓋所有 IIOBoardBase 介面成員：Connect()、Disconnect()、IsConnected、IsVirtual、DeviceID、DeviceName、ExceptionOccurred 事件。
- **FR-005**：單元測試必須涵蓋所有 IDI 介面成員：GetInput()、GetInputBit()、SnapStart()、SnapStop()、InputPortCount、InputBitsPerPort、DI_ValueChanged 事件。
- **FR-006**：單元測試必須涵蓋所有 IDO 介面成員：SetOutput()、SetOutputBit()、GetOutput()、GetOutputBit()、OutputPortCount、OutputBitsPerPort、DO_ValueChanged 事件。
- **FR-007**：所有防呆路徑（未連線、port/bit index 超範圍）皆須測試並回傳正確負數 const int 錯誤碼。
- **FR-008**：建構子參數驗證必須測試，logger 或 config 為 null 時會丟 ArgumentNullException。
- **FR-009**：Dispose 冪等性必須驗證，多次呼叫不會丟例外。
- **FR-010**：所有測試皆須可在無 Advantech 實體硬體下通過（利用 reflection 或 mock 測試防呆/錯誤路徑）。

### 關鍵實體

- **AdvantechDIO**：IIOBoard 具體實作，包裝 Advantech DAQNavi SDK，負責數位輸入/輸出。依賴 ILogUtility 與 AdvantechDIOConfig。
- **IIOBoard**（= IDI + IDO）：完整 DIO 合約聚合介面，繼承 IIOBoardBase。
- **IIOBoardBase**：定義裝置識別（DeviceID、DeviceName、IsConnected、IsVirtual）、連線管理（Connect、Disconnect）、例外通知（ExceptionOccurred 事件）。
- **IDI**：數位輸入介面，提供 port/bit 讀取、snapshot 監控、變化通知。
- **IDO**：數位輸出介面，提供 port/bit 寫入/讀取、變化通知。
- **AdvantechDIOConfig**：設定模型，包含裝置 index 與 DI/DO port 拓撲（port 數、每 port bit 數）。

## 成功標準

- **SC-001**：完整 solution build 無 AdvantechDIO 相關錯誤與警告。
- **SC-002**：AdvantechDIO.Tests 所有單元測試皆通過（100% 通過率），且不需實體硬體。
- **SC-003**：IIOBoardBase、IDI、IDO 介面所有公開方法皆有對應單元測試。
- **SC-004**：所有定義錯誤碼常數（NotConnectedError、PortIndexOutOfRangeError、BitIndexOutOfRangeError）皆有測試驗證其回傳值。
- **SC-005**：防呆路徑（未連線、index 超範圍）測試覆蓋率達 100%。

## 假設

- AdvantechDIO 與 AdvantechDIO.Tests 專案已存在且可運作，本功能僅聚焦於 solution 整合與測試驗證完整性。
- 目前 solution file（TDKServer.sln）已含 AdvantechDIO 與 AdvantechDIO.Tests 專案，僅需檢查其正確性與完整性。
- 測試設計可於無 Advantech 硬體下執行，成功路徑（需實體硬體）不屬自動化測試範圍。
- 單元測試框架為 NUnit 3.x 與 Moq，與現有專案一致。
