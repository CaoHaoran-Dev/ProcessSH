# ProcessSH

> A Windows port of RunProcess — a Spotlight-style command launcher with an acrylic interface and system tray integration.

---

## Overview

ProcessSH is a lightweight Windows application that provides a quick command execution interface inspired by the Windows Run dialog. It combines the power of PowerShell 7 with a polished, modern interface that remains accessible from the system tray at all times.

---

## Features

- **Command Execution** – Execute any PowerShell command with real-time output display
- **Tab Completion** – Auto-complete system commands, command history, and file paths
- **Command History** – Navigate previous commands using the ↑ and ↓ arrow keys
- **Drag & Drop** – Drag files from Explorer to automatically populate file paths with proper escaping
- **Administrator Execution** – Execute commands with elevated privileges via UAC prompt
- **Global Hotkey** – Show or hide the window from anywhere with Ctrl+Win+R (customizable in Settings)
- **System Tray Integration** – Access from the system tray; no taskbar icon
- **Fluent Design** – Acrylic backdrop with Segoe Fluent Icons
- **Floating Window** – Spotlight-style floating window, positioned at the bottom-left of the screen
- **Multi-line Input** – Support for multi-line commands
- **Multiple Windows** – Each window is an independent session
- **Custom Working Directory** – Configure the default working directory for new windows
- **Automatic Hide on Deactivate** – Windows hide automatically when the application loses focus
- **Multi-language** – English, Simplified Chinese, and Traditional Chinese

---

## Usage

Open the application, click the icon in the system tray, type a command, and press Enter.

| Input | Result |
|-------|--------|
| `Get-ChildItem` | List files |
| `cd C:\Users` | Change directory |
| `git status` | Show Git status |
| `C:\Windows\notepad.exe` | Launch Notepad |

---

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `Ctrl+Win+R` | Show or hide the window globally (customizable in Settings) |
| `Ctrl+N` | New window |
| `Enter` | Execute |
| `Shift+Enter` | Newline |
| `Tab` | Completion popup |
| `↑` `↓` | Command history navigation |
| `Esc` | Hide window |

---

## Appearance

ProcessSH uses the native Windows Acrylic backdrop for a translucent, modern look. The window floats at the bottom-left corner of the screen, inspired by Spotlight on macOS.

---

## Window Behavior

By default, all windows hide automatically when the application loses focus, matching the behavior of Spotlight. This can be disabled in **Settings → Window**.

When the settings or about window is open, the main window stays visible until the auxiliary window is closed.

---

## Working Directory

Configure in **Settings → Working Directory**. If not set, the user's home directory is used.

This only affects newly opened windows. If the configured path no longer exists, the application falls back to the home directory and displays a warning in Settings.

---

## Administrator Execution

Enable "Run as administrator" before executing a command to receive a UAC prompt. The command runs in a separate elevated process.

---

## Language

ProcessSH supports English, Simplified Chinese, and Traditional Chinese. Language can be switched at runtime in **Settings → Language**.

---

## Requirements

- Windows 10 version 2004 (build 19041) or later
- x64 or ARM64
- PowerShell 7 or later (install from Microsoft Store or GitHub)

---

## Installation

### Download (Recommended)

Download the latest `ProcessSH-Setup-x.x.x.exe` from [Releases](https://github.com/CaoHaoran-Dev/RunProcess/releases).

1. Download and run the installer.
2. The installer will detect your architecture (x64 or ARM64) and install the correct version.
3. If PowerShell 7 is not installed, the installer will offer to open Microsoft Store.

### Build from Source

```bash
git clone https://github.com/CaoHaoran-Dev/RunProcess.git
cd ProcessSH
dotnet build
dotnet run
```

Requires .NET 10 SDK and Windows App SDK 1.6.

### Build the Installer

```bash
cd ProcessSH
dotnet publish -c Release -r win-x64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\x64
dotnet publish -c Release -r win-arm64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\arm64

# Then compile installer.iss with Inno Setup 7
& "C:\Program Files\Inno Setup 7\ISCC.exe" ..\installer.iss
```

Requires Inno Setup 7 (64-bit).

---

## Tech Stack

C# + WinUI 3 + Windows App SDK

---

## Documentation

- [简体中文 README](Docs/zh-Hans/README.zh-Hans.md)
- [繁體中文 README](Docs/zh-Hant/README.zh-Hant.md)
- [License](LICENSE.md)

---

## License

MIT © 2026 CaoHaoran-Dev