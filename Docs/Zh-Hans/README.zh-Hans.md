# ProcessSH

> Windows 版 RunProcess —— 一款仿 Spotlight 的命令启动器，使用系统原生背景材质与系统托盘集成。

---

## 简介

ProcessSH 是一款轻量的 Windows 应用，提供类似 Windows「运行」对话框的快速命令执行界面。它将 PowerShell 7 的能力与现代化的视觉界面结合，并常驻系统托盘，随时可用。

**版本 2.0.0** —— 一次从头重写，使 ProcessSH 与 RunProcess 3.4.0 对齐，但不含会话模式与内置自动更新。

---

## 功能

- **命令执行** —— 执行任意 PowerShell 命令，实时显示输出
- **ANSI 颜色** —— 完整支持 8/16/256 色和 24 位真彩色。`git status`、`Get-ChildItem`、`Select-String` 等输出会带颜色显示
- **Tab 补全** —— 别名、命令历史、系统命令、文件路径
- **命令历史** —— ↑ / ↓ 按 frecency（频次 × 最近使用）排序浏览
- **历史搜索** —— 按 `Ctrl+R` 全量模糊搜索历史命令
- **别名** —— 定义 `gs` → `git status` 之类的短名称，补全里排最前
- **自定义路径** —— 添加 `D:\Tools`、`%USERPROFILE%\bin` 等文件夹。其中的可执行文件可直接用名字运行，按 `PATHEXT` 顺序解析
- **拖拽文件** —— 从资源管理器拖入文件，自动转义路径
- **管理员执行** —— 勾选后通过 UAC 提示以管理员权限执行
- **全局热键** —— `Ctrl+Alt+R` 显示/隐藏窗口（可在设置中自定义）
- **系统托盘常驻** —— 不显示任务栏图标
- **窗口材质** —— Mica、Mica Alt、亚克力、纯色。应用于所有窗口
- **浮动窗口** —— 仿 Spotlight 的浮动窗口，默认位于屏幕左下角
- **单一主窗口** —— 一个主窗口；设置、帮助、关于、历史搜索为独立的辅助窗口
- **自定义默认工作目录**
- **失去焦点时自动隐藏**
- **多语言** —— 英文、简体中文、繁体中文

---

## 使用

打开应用，点击系统托盘图标，输入命令，按回车。

| 输入 | 结果 |
|------|------|
| `Get-ChildItem` | 列出文件 |
| `cd C:\Users` | 切换目录（仅当前命令，见下） |
| `git status` | Git 状态（带颜色） |
| `C:\Windows\notepad.exe` | 启动记事本 |

---

## 快捷键

| 按键 | 作用 |
|------|------|
| `Ctrl+Alt+R` | 全局显示/隐藏窗口（可在设置中自定义） |
| `Ctrl+R` | 搜索命令历史 |
| `Enter` | 执行 |
| `Shift+Enter` | 换行（多行输入） |
| `Tab` | 补全浮窗 |
| `↑` `↓` | 浏览历史命令 |
| `Esc` | 隐藏窗口 |

> **说明**：默认热键使用 `Ctrl+Alt` 而非 `Ctrl+Win`，因为 Windows 保留了大量 `Win` 组合键，`RegisterHotKey` 无法可靠捕捉。

---

## 外观

在 **设置 → 外观** 中选择窗口材质：

| 材质 | 说明 | 支持系统 |
|------|------|---------|
| Mica | 桌面壁纸采样，静态 | Windows 11 |
| Mica Alt | Mica 的高对比度变体 | Windows 11 |
| 亚克力 | 实时模糊，有噪点 | Windows 11（完整） |
| 无 | 系统窗口纯色背景 | 所有版本 |

所选样式会应用到主窗口、设置、帮助、关于、历史搜索。

若当前系统不支持某个材质，该选项会从列表里隐藏，并自动降级到下一个可用项。

---

## 窗口行为

默认情况下，主窗口在应用失去焦点时自动隐藏，行为与 Spotlight 一致。可在 **设置 → 通用** 中关闭。

当设置、帮助、关于、历史搜索窗口打开时，主窗口保持可见，直到辅助窗口关闭。

关闭主窗口只隐藏，不退出应用。要退出，请使用 **托盘 → 退出**。

---

## 工作目录

在 **设置 → 路径** 中配置。不设置时使用用户主目录。

此设置只影响**新打开的命令**。如果设置的路径不存在，应用会回退到主目录，并在设置中显示警告。

> **说明**：ProcessSH 不实现会话模式。每条命令都在全新的 `pwsh` 进程中运行，`cd` 不会跨命令保持。要保留工作目录，请使用绝对路径的 `Set-Location`，或在设置里配置默认工作目录。

---

## 命令历史

每条执行过的命令都会记录。历史支撑两个功能：

- **↑ / ↓ 浏览** —— 按 frecency 排序：频次高、最近用过的排前面
- **Ctrl+R 搜索** —— 全量模糊匹配（`gst` 能匹配到 `git status`）

历史存放在：

```
%LOCALAPPDATA%\ProcessSH\history.json
```

最多 500 条，超出时先淘汰最久未用的。

在托盘菜单里点 **清空历史** 可以清空全部。

---

## 别名

别名让你为常用命令定义短名称，在补全浮窗里排最前。

内置示例：

| 别名 | 展开为 |
|------|--------|
| `gs` | `git status` |
| `gp` | `git pull --rebase` |
| `ll` | `Get-ChildItem -Force` |
| `serve` | `python3 -m http.server 8000` |
| `which` | `Get-Command` |

自定义别名存放在：

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

也可以在 **设置 → 别名** 里增删改。修改后立即生效。

---

## 自定义路径

**设置 → 路径** 面板有两个部分。

### 默认工作目录

配置新命令的工作目录。不设置时使用用户主目录。只影响新打开的命令。

如果设置的路径不存在（比如目录被删除），会自动回退到用户主目录，并在设置面板中显示警告。

### 自定义路径

添加文件夹，里面的可执行文件可以直接用名字运行。

| 输入 | 解析为 |
|------|--------|
| `myscript`（在 `D:\Tools`，已添加到这里） | `D:\Tools\myscript.exe` |
| `fastfetch`（Scoop 安装） | `C:\Users\Me\scoop\shims\fastfetch.exe` |

这里的文件夹会被**追加到 PATH**，所以系统 PATH 和你 PowerShell 配置文件里设置的 PATH 都会保留。可执行文件由 `pwsh` 自己按 `PATHEXT` 顺序查找。

自定义路径存放于：

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

修改后立即生效。

---

## 管理员执行

勾选"以管理员执行"后执行命令，会弹出 UAC 提示。命令在独立的提权 `pwsh` 进程中运行。

> **说明**：提权模式下无法把输出回传到主窗口。这是 `runas` 动词的系统限制——提权进程完全独立。

---

## 语言

ProcessSH 支持英文、简体中文、繁体中文。可在 **设置 → 通用 → 语言** 中运行时切换。

---

## 系统要求

- Windows 10 版本 2004（Build 19041）或以上
- x64 或 ARM64
- PowerShell 7 或以上（可从 Microsoft Store 或 GitHub 安装）

---

## 安装

### 下载（推荐）

提供两个安装程序，依需求选择：

| 安装程序 | 体积 | 是否需要 Windows App SDK 运行时 |
|----------|------|-------------------------------|
| `ProcessSH-Setup-x.x.x-self-contained.exe` | 较大 | **不需要** —— 已内建运行时 |
| `ProcessSH-Setup-x.x.x-framework-dependent.exe` | 较小 | **需要** —— 必须另行安装 |

**建议**：下载 `self-contained` 版本，开箱即用。

1. 下载并运行安装程序。
2. 安装程序会自动检测你的系统架构（x64 或 ARM64），并安装对应版本。
3. 若尚未安装 PowerShell 7，安装程序会询问是否开启 Microsoft Store。
4. 若选择 `framework-dependent` 版本，请先安装 [Windows App SDK 运行时](https://aka.ms/windowsappsdk/1.6/latest/windowsappruntimeinstall-x64.exe)。

### 从源代码编译

```powershell
git clone https://github.com/CaoHaoran-Dev/ProcessSH.git
cd ProcessSH
dotnet build
dotnet run
```

需要 .NET 10 SDK 与 Windows App SDK 1.6+。

### 编译安装程序

仓库根目录提供 `build.ps1` 脚本，自动完成整个构建流程：

```powershell
# 使用默认版本构建两个安装包
.\build.ps1

# 指定版本
.\build.ps1 -Version 2.0.1

# 若 Inno Setup 不在默认路径，指定 ISCC.exe
.\build.ps1 -InnoSetup "D:\Tools\Inno Setup 7\ISCC.exe"

# 只 publish，不编译安装包
.\build.ps1 -SkipInstaller

# 只编译安装包（publish 已完成）
.\build.ps1 -SkipPublish
```

脚本会执行：

1. 对 `self-contained` 和 `framework-dependent` 两种配置，分别在 `x64` 和 `ARM64` 上执行 `dotnet publish`
2. 验证发布输出
3. 用 Inno Setup 编译 `installer.iss` 和 `installer-framework.iss`

输出：

- `publish\self-contained\{x64,arm64}\`
- `publish\framework-dependent\{x64,arm64}\`
- `installer_output\ProcessSH-Setup-<version>-self-contained.exe`
- `installer_output\ProcessSH-Setup-<version>-framework-dependent.exe`

需要 [Inno Setup 7](https://jrsoftware.org/isdl.php)（64 位版）。

若想手动执行命令：

```powershell
cd ProcessSH

# 自包含
dotnet publish -c Release -r win-x64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\self-contained\x64
dotnet publish -c Release -r win-arm64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\self-contained\arm64

# 框架依赖
dotnet publish -c Release -r win-x64 --self-contained false -p:WindowsAppSDKSelfContained=false -o ..\publish\framework-dependent\x64
dotnet publish -c Release -r win-arm64 --self-contained false -p:WindowsAppSDKSelfContained=false -o ..\publish\framework-dependent\arm64

# 编译安装包
& "C:\Program Files\Inno Setup 7\ISCC.exe" ..\installer.iss
& "C:\Program Files\Inno Setup 7\ISCC.exe" ..\installer-framework.iss
```

---

## 配置文件

ProcessSH 把用户数据存在：

```
%LOCALAPPDATA%\ProcessSH\
├── aliases.json     # 命令别名
├── history.json     # 命令历史（最多 500 条）
├── paths.json       # 自定义可执行文件搜索路径
└── settings.json    # 应用设置
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

自动维护，最多 500 条。

### settings.json

由应用管理，通过设置窗口间接修改。

---

## 技术栈

- C# + WinUI 3 + Windows App SDK
- [H.NotifyIcon.WinUI](https://github.com/HavenDV/H.NotifyIcon) —— 系统托盘集成
- Win32 `RegisterHotKey` —— 全局热键

---

## 项目结构

```
ProcessSH/
├── ProcessSH/
│   ├── Assets/              # 图标、图片、本地化 JSON
│   ├── Models/              # AppSettings、AliasStore、PathStore、CommandHistory 等
│   ├── Services/            # CommandExecutor、ElevatedRunner、HotkeyManager、LaunchAtLogin
│   ├── Utils/               # ANSIParser、ShellQuoting、CommandPreprocessor、BackdropHelper
│   ├── ViewModels/          # CommandViewModel
│   ├── Views/               # 设置/帮助/关于页面与窗口、HistorySearchPage、OutputTextView
│   ├── App.xaml.cs
│   ├── MainPage.xaml.cs
│   └── MainWindow.xaml.cs
├── installer.iss                    # 自包含安装脚本
├── installer-framework.iss          # 框架依赖安装脚本
├── build.ps1                        # 一键构建脚本
└── README.md
```

---

## 与 RunProcess（macOS）的差异

ProcessSH 2.0.0 在功能与行为上与 RunProcess 3.4.0 对齐，以下是**有意保留的差异**：

| 功能 | RunProcess (macOS) | ProcessSH (Windows) |
|------|-------------------|---------------------|
| 会话模式 | ✅ 持久 zsh | ❌ 每条命令独立进程 |
| 自动更新 | ✅ Sparkle | ❌ 手动从 GitHub Releases 下载 |
| 应用包 | `.app` bundle，`open -b` 启动 | 不适用，改用 `.exe` / `.lnk` / `.bat` / `.cmd` |
| 外观 | Liquid Glass / 毛玻璃 | Mica / Mica Alt / 亚克力 |
| 路径分隔符 | `:`（冒号） | `;`（分号） |
| 家目录表示 | `~` | `%USERPROFILE%` |

---

## 文档

- [English README](../../README.md)
- [繁體中文 README](../zh-Hant/README.zh-Hant.md)
- [License](../../LICENSE)

---

## License

MIT © 2026 CaoHaoran-Dev