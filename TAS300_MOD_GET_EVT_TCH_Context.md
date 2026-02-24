# MOD / GET / EVT / TCH 指令通訊時序（TAS300）

此文件提供 **Commands with No Acknowledgment（MOD, GET, EVT, TCH）** 的通訊上下文，用於描述上位機（Upper Machine）與 TAS300 之間的簡化交握流程。

> 依你提供的圖示，本類指令的特徵是：
> - 上位機送出指令後，TAS300 **僅回覆一次 ACK**（表示已接收）。
> - **沒有**後續的 `INF/ABS`（動作結束/錯誤事件回報）流程。
> - 也**不需要**以 `FIN` 進行結尾確認（相較於 MOV/SET 那種完整序列）。

---

## 內容說明（摘要）

- **適用指令**：`MOD` / `GET` / `EVT` / `TCH`（依章節 3.2）。
- **通訊行為**：
  1. 上位機送出命令（例如：`MOD:ONMGV;`）
  2. TAS300 回覆 `ACK`（例如：`ACK:ONMGV;`）
  3. 通訊即結束（無 `INF/ABS`、無 `FIN`）

---

## Mermaid 時序圖（Sequence Diagram）

```mermaid
sequenceDiagram
    autonumber
    participant U as Upper Machine (上位機)
    participant T as TAS300

    Note over U,T: Commands with No Acknowledgment (MOD, GET, EVT, TCH)

    %% Example: MOD
    U->>T: MOD:ONMGV;  (Command issued)
    T-->>U: ACK:ONMGV;  (Acknowledgement message)

    Note over U,T: End (no INF/ABS, no FIN)
```

---

## 備註 / 使用建議

- 範例以 `MOD:ONMGV;` 示意；`GET/EVT/TCH` 的格式與參數請依你的命令表替換。
- 若你希望我把 `GET`（查詢回應資料）、`EVT`（事件上報）、`TCH`（觸發/狀態）等**實際回傳內容格式**一併補上，請貼該章節的回應格式或範例封包，我可以直接更新此 MD。
