# ProcessSH

> Windows 版 RunProcess —— 一款仿 Spotlight 的命令啟動器，具備系統原生背景材質與系統匣整合。

---

## 簡介

ProcessSH 是一款輕量的 Windows 應用程式，提供類似 Windows「執行」對話框的快速命令執行介面。它將 PowerShell 7 的能力與現代化的視覺介面結合，並常駐於系統匣，隨時可用。

**版本 2.0.0** —— 一次從頭重寫，讓 ProcessSH 與 RunProcess 3.4.0 對齊，但不含工作階段模式與內建自動更新。

---

## 功能

- **命令執行** —— 執行任意 PowerShell 命令，即時顯示輸出
- **ANSI 顏色** —— 完整支援 8/16/256 色和 24 位真彩色。`git status`、`Get-ChildItem`、`Select-String` 等輸出會帶顏色顯示
- **Tab 補全** —— 別名、命令歷史、系統命令、檔案路徑
- **命令歷史** —— ↑ / ↓ 依 frecency（頻次 × 最近使用）排序瀏覽
- **歷史搜尋** —— 按 `Ctrl+R` 全量模糊搜尋歷史命令
- **別名** —— 定義 `gs` → `git status` 之類的短名稱，補全裡排最前
- **自訂路徑** —— 加入 `D:\Tools`、`%USERPROFILE%\bin` 等資料夾。其中的可執行檔可直接用名稱執行，依 `PATHEXT` 順序解析
- **拖曳檔案** —— 從檔案總管拖入檔案，自動轉義路徑
- **管理員執行** —— 勾選後透過 UAC 提示以管理員權限執行
- **全域快速鍵** —— `Ctrl+Alt+R` 顯示／隱藏視窗（可於設定中自訂）
- **系統匣常駐** —— 不顯示工作列圖示
- **視窗材質** —— Mica、Mica Alt、壓克力、純色。套用於所有視窗
- **浮動視窗** —— 仿 Spotlight 的浮動視窗，預設位於螢幕左下角
- **單一主視窗** —— 一個主視窗；設定、輔助說明、關於、歷史搜尋為獨立的輔助視窗
- **自訂預設工作目錄**
- **失去焦點時自動隱藏**
- **多語言** —— 英文、簡體中文、繁體中文

---

## 使用方式

開啟應用程式，點選系統匣圖示，輸入命令，按 Enter。

| 輸入 | 結果 |
|------|------|
| `Get-ChildItem` | 列出檔案 |
| `cd C:\Users` | 切換目錄（僅當前命令，見下） |
| `git status` | Git 狀態（帶顏色） |
| `C:\Windows\notepad.exe` | 啟動記事本 |

---

## 快速鍵

| 按鍵 | 動作 |
|------|------|
| `Ctrl+Alt+R` | 全域顯示／隱藏視窗（可於設定中自訂） |
| `Ctrl+R` | 搜尋命令歷史 |
| `Enter` | 執行 |
| `Shift+Enter` | 換行（多行輸入） |
| `Tab` | 補全浮窗 |
| `↑` `↓` | 瀏覽歷史命令 |
| `Esc` | 隱藏視窗 |

> **說明**：預設快速鍵使用 `Ctrl+Alt` 而非 `Ctrl+Win`，因為 Windows 保留了許多 `Win` 組合鍵，`RegisterHotKey` 無法可靠捕捉。

---

## 外觀

在 **設定 → 外觀** 中選擇視窗材質：

| 材質 | 說明 | 支援系統 |
|------|------|---------|
| Mica | 桌面桌布取樣，靜態 | Windows 11 |
| Mica Alt | Mica 的高對比變體 | Windows 11 |
| 壓克力 | 即時模糊，有噪點 | Windows 11（完整） |
| 無 | 系統視窗純色背景 | 所有版本 |

所選樣式會套用到主視窗、設定、輔助說明、關於、歷史搜尋。

若目前系統不支援某個材質，該選項會從清單中隱藏，並自動降級到下一個可用項。

---

## 視窗行為

預設情況下，主視窗在應用程式失去焦點時自動隱藏，行為與 Spotlight 一致。可於 **設定 → 一般** 中關閉。

當設定、輔助說明、關於、歷史搜尋視窗開啟時，主視窗保持可見，直到輔助視窗關閉。

關閉主視窗只會隱藏，不會結束應用程式。要結束，請使用 **系統匣 → 結束**。

---

## 工作目錄

於 **設定 → 路徑** 中設定。未設定時使用使用者主目錄。

此設定僅影響**新開啟的命令**。若設定的路徑不存在，應用程式會回退到主目錄，並於設定中顯示警告。

> **說明**：ProcessSH 不實作工作階段模式。每條命令都在全新的 `pwsh` 行程中執行，`cd` 不會跨命令保持。要保留工作目錄，請使用絕對路徑的 `Set-Location`，或在設定中設定預設工作目錄。

---

## 命令歷史

每條執行過的命令都會記錄。歷史支撐兩個功能：

- **↑ / ↓ 瀏覽** —— 依 frecency 排序：頻次高、最近用過的排前面
- **Ctrl+R 搜尋** —— 全量模糊比對（`gst` 能匹配到 `git status`）

歷史存放於：

```
%LOCALAPPDATA%\ProcessSH\history.json
```

最多 500 條，超出時先淘汰最久未用的。

在系統匣選單裡點 **清空歷史** 可以清空全部。

---

## 別名

別名讓你為常用命令定義短名稱，在補全浮窗裡排最前。

內建範例：

| 別名 | 展開為 |
|------|--------|
| `gs` | `git status` |
| `gp` | `git pull --rebase` |
| `ll` | `Get-ChildItem -Force` |
| `serve` | `python3 -m http.server 8000` |
| `which` | `Get-Command` |

自訂別名存放於：

```
%LOCALAPPDATA%\ProcessSH\aliases.json
```

格式：

```json
[
  { "name": "gs", "expansion": "git status" },
  { "name": "serve", "expansion": "python3 -m http.server 8000" }
]
```

也可以在 **設定 → 別名** 裡增刪改。修改後立即生效。

---

## 自訂路徑

**設定 → 路徑** 面板有兩個部分。

### 預設工作目錄

設定新命令的工作目錄。未設定時使用使用者主目錄。僅影響新開啟的命令。

若設定的路徑不存在（例如目錄被刪除），會自動回退到使用者主目錄，並於設定面板中顯示警告。

### 自訂路徑

加入資料夾，其中的可執行檔可以直接用名稱執行。

| 輸入 | 解析為 |
|------|--------|
| `myscript`（在 `D:\Tools`，已加入這裡） | `D:\Tools\myscript.exe` |
| `fastfetch`（Scoop 安裝） | `C:\Users\Me\scoop\shims\fastfetch.exe` |

這裡的資料夾會被**追加到 PATH**，所以系統 PATH 和你 PowerShell 設定檔裡設定的 PATH 都會保留。可執行檔由 `pwsh` 自己依 `PATHEXT` 順序查找。

自訂路徑存放於：

```
%LOCALAPPDATA%\ProcessSH\paths.json
```

格式：

```json
{
  "Paths": [
    "D:\\Tools",
    "%USERPROFILE%\\bin"
  ]
}
```

修改後立即生效。

---

## 管理員執行

勾選「以管理員執行」後執行命令，會彈出 UAC 提示。命令在獨立的提權 `pwsh` 行程中執行。

> **說明**：提權模式下無法將輸出回傳到主視窗。這是 `runas` 動詞的系統限制——提權行程完全獨立。

---

## 語言

ProcessSH 支援英文、簡體中文、繁體中文。可於 **設定 → 一般 → 語言** 中即時切換。

---

## 系統需求

- Windows 10 版本 2004（Build 19041）或以上
- x64 或 ARM64
- PowerShell 7 或以上（可從 Microsoft Store 或 GitHub 安裝）

---

## 安裝

### 下載（建議方式）

提供兩個安裝程式，依需求選擇：

| 安裝程式 | 體積 | 是否需要 Windows App SDK 執行階段 |
|----------|------|-----------------------------------|
| `ProcessSH-Setup-x.x.x-self-contained.exe` | 較大 | **不需要** —— 已內建執行階段 |
| `ProcessSH-Setup-x.x.x-framework-dependent.exe` | 較小 | **需要** —— 必須另行安裝 |

**建議**：下載 `self-contained` 版本，開箱即用。

1. 下載並執行安裝程式。
2. 安裝程式會自動偵測你的系統架構（x64 或 ARM64），並安裝對應版本。
3. 若尚未安裝 PowerShell 7，安裝程式會詢問是否開啟 Microsoft Store。
4. 若選擇 `framework-dependent` 版本，請先安裝 [Windows App SDK 執行階段](https://aka.ms/windowsappsdk/1.6/latest/windowsappruntimeinstall-x64.exe)。

### 從原始碼編譯

```powershell
git clone https://github.com/CaoHaoran-Dev/ProcessSH.git
cd ProcessSH
dotnet build
dotnet run
```

需要 .NET 10 SDK 與 Windows App SDK 1.6+。

### 編譯安裝程式

倉庫根目錄提供 `build.ps1` 指令碼，自動完成整個建置流程：

```powershell
# 使用預設版本建置兩個安裝包
.\build.ps1

# 指定版本
.\build.ps1 -Version 2.0.1

# 若 Inno Setup 不在預設路徑，指定 ISCC.exe
.\build.ps1 -InnoSetup "D:\Tools\Inno Setup 7\ISCC.exe"

# 只 publish，不編譯安裝包
.\build.ps1 -SkipInstaller

# 只編譯安裝包（publish 已完成）
.\build.ps1 -SkipPublish
```

指令碼會執行：

1. 對 `self-contained` 和 `framework-dependent` 兩種設定，分別在 `x64` 和 `ARM64` 上執行 `dotnet publish`
2. 驗證發佈輸出
3. 用 Inno Setup 編譯 `installer.iss` 和 `installer-framework.iss`

輸出：

- `publish\self-contained\{x64,arm64}\`
- `publish\framework-dependent\{x64,arm64}\`
- `installer_output\ProcessSH-Setup-<version>-self-contained.exe`
- `installer_output\ProcessSH-Setup-<version>-framework-dependent.exe`

需要 [Inno Setup 7](https://jrsoftware.org/isdl.php)（64 位版）。

若想手動執行命令：

```powershell
cd ProcessSH

# 自包含
dotnet publish -c Release -r win-x64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\self-contained\x64
dotnet publish -c Release -r win-arm64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\self-contained\arm64

# 框架依賴
dotnet publish -c Release -r win-x64 --self-contained false -p:WindowsAppSDKSelfContained=false -o ..\publish\framework-dependent\x64
dotnet publish -c Release -r win-arm64 --self-contained false -p:WindowsAppSDKSelfContained=false -o ..\publish\framework-dependent\arm64

# 編譯安裝包
& "C:\Program Files\Inno Setup 7\ISCC.exe" ..\installer.iss
& "C:\Program Files\Inno Setup 7\ISCC.exe" ..\installer-framework.iss
```

---

## 設定檔

ProcessSH 把使用者資料存在：

```
%LOCALAPPDATA%\ProcessSH\
├── aliases.json     # 命令別名
├── history.json     # 命令歷史（最多 500 條）
├── paths.json       # 自訂可執行檔搜尋路徑
└── settings.json    # 應用程式設定
```

### aliases.json

```json
[
  { "name": "gs", "expansion": "git status" },
  { "name": "serve", "expansion": "python3 -m http.server 8000" }
]
```

### paths.json

```json
{
  "Paths": [
    "D:\\Tools",
    "%USERPROFILE%\\bin"
  ]
}
```

### history.json

自動維護，最多 500 條。

### settings.json

由應用程式管理，透過設定視窗間接修改。

---

## 技術架構

- C# + WinUI 3 + Windows App SDK
- [H.NotifyIcon.WinUI](https://github.com/HavenDV/H.NotifyIcon) —— 系統匣整合
- Win32 `RegisterHotKey` —— 全域快速鍵

---

## 專案結構

```
ProcessSH/
├── ProcessSH/
│   ├── Assets/              # 圖示、圖片、在地化 JSON
│   ├── Models/              # AppSettings、AliasStore、PathStore、CommandHistory 等
│   ├── Services/            # CommandExecutor、ElevatedRunner、HotkeyManager、LaunchAtLogin
│   ├── Utils/               # ANSIParser、ShellQuoting、CommandPreprocessor、BackdropHelper
│   ├── ViewModels/          # CommandViewModel
│   ├── Views/               # 設定／輔助說明／關於頁面與視窗、HistorySearchPage、OutputTextView
│   ├── App.xaml.cs
│   ├── MainPage.xaml.cs
│   └── MainWindow.xaml.cs
├── installer.iss                    # 自包含安裝指令碼
├── installer-framework.iss          # 框架依賴安裝指令碼
├── build.ps1                        # 一鍵建置指令碼
└── README.md
```

---

## 與 RunProcess（macOS）的差異

ProcessSH 2.0.0 在功能與行為上與 RunProcess 3.4.0 對齊，以下是**有意保留的差異**：

| 功能 | RunProcess (macOS) | ProcessSH (Windows) |
|------|-------------------|---------------------|
| 工作階段模式 | ✅ 長駐 zsh | ❌ 每條命令獨立行程 |
| 自動更新 | ✅ Sparkle | ❌ 手動從 GitHub Releases 下載 |
| 應用程式套件 | `.app` bundle，`open -b` 啟動 | 不適用，改用 `.exe` / `.lnk` / `.bat` / `.cmd` |
| 外觀 | Liquid Glass / 毛玻璃 | Mica / Mica Alt / 壓克力 |
| 路徑分隔符 | `:`（冒號） | `;`（分號） |
| 家目錄表示 | `~` | `%USERPROFILE%` |

---

## 文件

- [English README](../../README.md)
- [简体中文 README](../zh-Hans/README.zh-Hans.md)
- [License](../../LICENSE)

---

## License

MIT © 2026 CaoHaoran-Dev