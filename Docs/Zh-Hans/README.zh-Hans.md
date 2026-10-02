# ProcessSH

> Windows 版 RunProcess —— 一个仿 Spotlight 的命令启动器，带亚克力材质和系统托盘集成。

---

## 简介

ProcessSH 是一款轻量的 Windows 应用，提供类似 Windows“运行”对话框的快速命令执行界面。它把 PowerShell 7 的能力和现代化的视觉界面结合在一起，并常驻于系统托盘，随时可用。

---

## 功能

- **命令执行**：执行任意 PowerShell 命令，实时显示输出
- **Tab 补全**：自动补全系统命令、历史命令与文件路径
- **命令历史**：使用 ↑ 与 ↓ 方向键浏览先前命令
- **拖放支援**：从资源管理器拖曳文件，自动填入正确转义的路径
- **管理员执行**：透过 UAC 提示以管理员权限执行命令
- **全局快捷键**：随时以 Ctrl+Win+R 显示或隐藏窗口（可在设置中自定义）
- **系统托盘集成**：可从系统托盘存取；不显示任务栏图标
- **Fluent 设计**：亚克力背景 + Segoe Fluent 图标
- **浮动窗口**：Spotlight 风格的浮动窗口，默认位于屏幕左下角
- **多行输入**：支持多行命令输入
- **多重窗口**：每个窗口均为独立的工作阶段
- **自订工作目录**：可设定新窗口的预设工作目录
- **失去焦点时自动隐藏**：应用失去焦点时，窗口会自动隐藏
- **多语言**：支持英文、简体中文、繁体中文

---

## 使用方式

开启应用，点击系统托盘中的 ⚡ 图标，输入命令，然后按 Enter。

| 输入 | 结果 |
|------|------|
| `Get-ChildItem` | 列出文件 |
| `cd C:\Users` | 切换目录 |
| `git status` | 显示 Git 状态 |
| `C:\Windows\notepad.exe` | 启动记事本 |

---

## 快捷键

| 按键 | 动作 |
|------|------|
| `Ctrl+Win+R` | 全局显示或隐藏窗口（可在设置中自定义） |
| `Ctrl+N` | 新增窗口 |
| `Enter` | 执行 |
| `Shift+Enter` | 换行 |
| `Tab` | 补全弹窗 |
| `↑` `↓` | 命令历史浏览 |
| `Esc` | 隐藏窗口 |

---

## 外观

ProcessSH 使用 Windows 原生的亚克力（Acrylic）材质，呈现半透明的现代质感。窗口浮动在屏幕左下角，灵感来自 macOS 的 Spotlight。

---

## 窗口行为

默认情况下，应用失去焦点时所有窗口会自动隐藏，行为与 Spotlight 一致。可于 **设置 → 窗口** 中关闭。

当设置或关于窗口开启时，主窗口会保持可见，直到辅助窗口关闭。

---

## 工作目录

于 **设置 → 工作目录** 中设定。若未设定，将使用使用者的个人专有目录。

此设定仅影响 **新开启的窗口**。若设定的路径已不存在，应用会退回至个人专有目录，并于设置中显示警告。

---

## 管理员执行

在执行命令前勾选「以管理员执行」，即会显示 UAC 提示。命令会在独立的提权程序中执行。

---

## 语言

ProcessSH 支持英文、简体中文与繁体中文。可于 **设置 → 语言** 中即时切换。

---

## 系统需求

- Windows 10 版本 2004（组建 19041）或以上
- x64 或 ARM64
- PowerShell 7 或以上（可从 Microsoft Store 或 GitHub 安装）

---

## 安装

### 下载（建议方式）

从 [Releases](https://github.com/CaoHaoran-Dev/RunProcess/releases) 下载最新的 `ProcessSH-Setup-x.x.x.exe`。

1. 下载并执行安装程序。
2. 安装程序会自动侦测你的系统架构（x64 或 ARM64），并安装对应版本。
3. 若尚未安装 PowerShell 7，安装程序会询问是否开启 Microsoft Store。

### 从原始码编译

```bash
git clone https://github.com/CaoHaoran-Dev/RunProcess.git
cd ProcessSH
dotnet build
dotnet run
```

需要 .NET 10 SDK 与 Windows App SDK 1.6。

### 编译安装程序

```bash
cd ProcessSH
dotnet publish -c Release -r win-x64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\x64
dotnet publish -c Release -r win-arm64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\arm64

# 接着用 Inno Setup 7 编译 installer.iss
& "C:\Program Files\Inno Setup 7\ISCC.exe" ..\installer.iss
```

需要 Inno Setup 7（64 位版）。

---

## 技术架构

C# + WinUI 3 + Windows App SDK

---

## 文件

- [English README](../../README.md)
- [繁体中文 README](../zh-Hant/README.zh-Hant.md)
- [License](../../LICENSE.md)

---

## 授权条款

MIT © 2026 CaoHaoran-Dev