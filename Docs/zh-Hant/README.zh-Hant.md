# ProcessSH

> Windows 版 RunProcess —— 一款仿 Spotlight 的命令啟動器，具備壓克力材質與系統匣整合。

---

## 簡介

ProcessSH 是一款輕量的 Windows 應用程式，提供類似 Windows「執行」對話框的快速命令執行介面。它將 PowerShell 7 的能力與現代化的視覺介面結合，並常駐於系統匣，隨時可用。

---

## 功能

- **命令執行**：執行任意 PowerShell 命令，即時顯示輸出
- **Tab 補全**：自動補全系統命令、歷史命令與檔案路徑
- **命令歷史**：使用 ↑ 與 ↓ 方向鍵瀏覽先前命令
- **拖放支援**：從檔案總管拖曳檔案，自動填入正確轉義的檔案路徑
- **管理員執行**：透過 UAC 提示以管理員權限執行命令
- **全域快速鍵**：隨時以 Ctrl+Win+R 顯示或隱藏視窗（可於設定中自訂）
- **系統匣整合**：可從系統匣存取；不顯示工作列圖示
- **Fluent 設計**：壓克力背景 + Segoe Fluent 圖示
- **浮動視窗**：Spotlight 風格的浮動視窗，預設位於螢幕左下角
- **多行輸入**：支援多行命令輸入
- **多重視窗**：每個視窗皆為獨立的工作階段
- **自訂工作目錄**：可設定新視窗的預設工作目錄
- **失去焦點時自動隱藏**：應用程式失去焦點時，視窗會自動隱藏
- **多語言**：支援英文、簡體中文、繁體中文

---

## 使用方式

開啟應用程式，點選系統匣中的圖示，輸入命令，然後按下 Enter。

| 輸入 | 結果 |
|------|------|
| `Get-ChildItem` | 列出檔案 |
| `cd C:\Users` | 切換目錄 |
| `git status` | 顯示 Git 狀態 |
| `C:\Windows\notepad.exe` | 啟動記事本 |

---

## 快速鍵

| 按鍵 | 動作 |
|------|------|
| `Ctrl+Win+R` | 全域顯示或隱藏視窗（可於設定中自訂） |
| `Ctrl+N` | 新增視窗 |
| `Enter` | 執行 |
| `Shift+Enter` | 換行 |
| `Tab` | 補全彈出視窗 |
| `↑` `↓` | 命令歷史瀏覽 |
| `Esc` | 隱藏視窗 |

---

## 外觀

ProcessSH 使用 Windows 原生的壓克力（Acrylic）材質，呈現半透明的現代質感。視窗浮動在螢幕左下角，靈感來自 macOS 的 Spotlight。

---

## 視窗行為

預設情況下，應用程式失去焦點時所有視窗會自動隱藏，行為與 Spotlight 一致。可於 **設定 → 視窗** 中關閉。

當設定或關於視窗開啟時，主視窗會保持可見，直到輔助視窗關閉。

---

## 工作目錄

於 **設定 → 工作目錄** 中設定。若未設定，將使用使用者的個人專屬目錄。

此設定僅影響 **新開啟的視窗**。若設定的路徑已不存在，應用程式會退回至個人專屬目錄，並於設定中顯示警告。

---

## 管理員執行

在執行命令前勾選「以管理員執行」，即會顯示 UAC 提示。命令會在獨立的提權程序中執行。

---

## 語言

ProcessSH 支援英文、簡體中文與繁體中文。可於 **設定 → 語言** 中即時切換。

---

## 系統需求

- Windows 10 版本 2004（組建 19041）或以上
- x64 或 ARM64
- PowerShell 7 或以上（可從 Microsoft Store 或 GitHub 安裝）

---

## 安裝

### 下載（建議方式）

從 [Releases](https://github.com/CaoHaoran-Dev/RunProcess/releases) 下載最新的 `ProcessSH-Setup-x.x.x.exe`。

1. 下載並執行安裝程式。
2. 安裝程式會自動偵測你的系統架構（x64 或 ARM64），並安裝對應版本。
3. 若尚未安裝 PowerShell 7，安裝程式會詢問是否開啟 Microsoft Store。

### 從原始碼編譯

```bash
git clone https://github.com/CaoHaoran-Dev/RunProcess.git
cd ProcessSH
dotnet build
dotnet run
```

需要 .NET 10 SDK 與 Windows App SDK 1.6。

### 編譯安裝程式

```bash
cd ProcessSH
dotnet publish -c Release -r win-x64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\x64
dotnet publish -c Release -r win-arm64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\arm64

# 接著用 Inno Setup 7 編譯 installer.iss
& "C:\Program Files\Inno Setup 7\ISCC.exe" ..\installer.iss
```

需要 Inno Setup 7（64 位版）。

---

## 技術架構

C# + WinUI 3 + Windows App SDK

---

## 文件

- [English README](../../README.md)
- [简体中文 README](../zh-Hans/README.zh-Hans.md)
- [License](../../LICENSE.md)

---

## 授權條款

MIT © 2026 CaoHaoran-Dev