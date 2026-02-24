# MOV / SET 指令通訊時序圖（TAS300）

此文件提供 **Operation & Initialization Commands（MOV, SET）** 的通訊上下文，用於描述上位機（Upper Machine）與 TAS300 之間的標準交握流程與例外行為。

---

## 內容說明（摘要）

- **標準流程**：上位機送出 `MOV`/`SET` → TAS300 回 `ACK`（開始執行）→ 動作結束回 `INF`（正常）或 `ABS`（錯誤）→（視模式）上位機送 `FIN`。
- **No FIN（Omission / Don't Confirm）模式**：收到 `INF/ABS` 後即視為結束，無需送 `FIN`。
- **忙碌/等待 FIN 狀態**：若 TAS300 正在等待 `FIN`，此時再送 `MOV/SET` 會回 `NAK`（可能伴隨 `/INTER/CBUSY`）。
- **例外：MOV:STOP**：可被接受並中斷當前動作，且會丟棄此前序通訊序列。

---

## Mermaid 時序圖（Sequence Diagram）

> 將以下區塊貼到支援 Mermaid 的 Markdown 環境（如 Azure DevOps Wiki、部分 Markdown Preview、Obsidian 等）即可渲染。

```mermaid
sequenceDiagram
    autonumber
    participant U as Upper Machine (上位機)
    participant T as TAS300

    Note over U,T: Operation & Initialization Commands (MOV, SET)

    %% --- Normal command flow ---
    rect rgb(235, 245, 255)
    Note over U,T: 【一般流程】MOV/SET → ACK → (Operating) → INF/ABS → (可選) FIN
    U->>T: MOV:ORGSH;  (或 SET:xxxx;)
    T-->>U: ACK:ORGSH;  (開始執行 / Operation start)
    Note over T: Operating... (設備動作中)
    alt Operation OK
        T-->>U: INF:ORGSH;  (Operation end)
    else Operation Error
        T-->>U: ABS:ORGSH/PROTS;  (Error event)
    end

    alt FIN required (需要 FIN)
        U->>T: FIN:ORGSH;  (確認收到結果 / 結束此通訊序列)
    else No FIN mode (Omission / Don't Confirm)
        Note over U: 不需要送 FIN，收到 INF/ABS 即視為結束
    end
    end

    %% --- Busy / waiting FIN behavior ---
    rect rgb(255, 243, 230)
    Note over U,T: 【例外】若 TAS300 正在等待 FIN，再送 MOV/SET → NAK
    U->>T: MOV:XXXX; / SET:XXXX; (在等待 FIN 期間送出)
    T-->>U: NAK  (或 /INTER/CBUSY)
    end

    %% --- STOP exception ---
    rect rgb(240, 255, 240)
    Note over U,T: 【例外】MOV:STOP 可被接受，並丟棄前序通訊序列
    U->>T: MOV:STOP;
    T-->>U: ACK:STOP; (接受 STOP)
    Note over T: 丟棄此前序通訊序列 / 中斷當前動作
    end
```

---

## 備註

- `ORGSH` / `PROTS` 僅為示例代號，請依你的設備命令表替換。
- 若你需要我把 `SET` 的參數格式（例如 `SET:AAA,BBB;`）或特定錯誤碼對應補進來，貼一下你的指令格式/列表，我可以直接更新此 MD。
