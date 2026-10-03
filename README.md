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
- **Global Hotkey** – Show or hide the window from anywhere with `Ctrl+Alt+R` (customizable in Settings)
- **System Tray Integration** – Access from the system tray; no taskbar icon
- **Fluent Design** – Acrylic backdrop with Segoe Fluent Icons
- **Floating Window** – Spotlight-style floating window, positioned at the bottom-left of the screen
- **Single Main Window** – One main window, multiple independent auxiliary windows (Settings, About)
- **Custom Working Directory** – Configure the default working directory for new sessions
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
| `Ctrl+Alt+R` | Show or hide the window globally (customizable in Settings) |
| `Enter` | Execute |
| `Tab` | Completion popup |
| `↑` `↓` | Command history navigation |
| `Esc` | Hide window |

> **Note:** The default hotkey uses `Ctrl+Alt` instead of `Ctrl+Win` because Windows reserves many `Win` key combinations and `RegisterHotKey` cannot reliably capture them.

---

## Appearance

ProcessSH uses the native Windows Acrylic backdrop for a translucent, modern look. The window floats at the bottom-left corner of the screen, inspired by Spotlight on macOS.

---

## Window Behavior

By default, the main window hides automatically when the application loses focus, matching the behavior of Spotlight. This can be disabled in **Settings → Window**.

When the Settings or About window is open, the main window stays visible until the auxiliary window is closed.

Closing the main window hides it rather than exiting the application. To quit, use **Tray → Quit**.

---

## Working Directory

Configure in **Settings → Working Directory**. If not set, the user's home directory is used.

This only affects newly opened sessions. If the configured path no longer exists, the application falls back to the home directory and displays a warning in Settings.

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

Two installers are provided, choose the one that fits your needs:

| Installer | Size | Requires Windows App SDK Runtime |
|-----------|------|----------------------------------|
| `ProcessSH-Setup-x.x.x-self-contained.exe` | Larger | **No** — includes the runtime |
| `ProcessSH-Setup-x.x.x-framework-dependent.exe` | Smaller | **Yes** — must be installed separately |

**Recommended:** download the `self-contained` installer for a zero-setup experience.

1. Download and run the installer.
2. The installer will detect your architecture (x64 or ARM64) and install the correct version.
3. If PowerShell 7 is not installed, the installer will offer to open Microsoft Store.
4. If you choose the `framework-dependent` installer, make sure to install the [Windows App SDK Runtime](https://aka.ms/windowsappsdk/1.6/latest/windowsappruntimeinstall-x64.exe) first.

### Build from Source

```powershell
git clone https://github.com/CaoHaoran-Dev/ProcessSH.git
cd ProcessSH
dotnet build
dotnet run
```

Requires .NET 10 SDK and Windows App SDK 1.6.

### Build the Installer

A PowerShell script `build.ps1` is provided at the repository root to automate the entire build process:

```powershell
# Build both installers with default version
.\build.ps1

# Specify a version
.\build.ps1 -Version 1.0.2

# Specify Inno Setup path if installed in a non-default location
.\build.ps1 -InnoSetup "D:\Tools\Inno Setup 7\ISCC.exe"

# Only publish, skip installer compilation
.\build.ps1 -SkipInstaller

# Only compile installers (publish already done)
.\build.ps1 -SkipPublish
```

The script performs:

1. `dotnet publish` for `self-contained` and `framework-dependent` configurations, on both `x64` and `ARM64`
2. Validation of the publish output
3. Compilation of `installer.iss` and `installer-framework.iss` with Inno Setup

Output:

- `publish\self-contained\{x64,arm64}\`
- `publish\framework-dependent\{x64,arm64}\`
- `installer_output\ProcessSH-Setup-<version>-self-contained.exe`
- `installer_output\ProcessSH-Setup-<version>-framework-dependent.exe`

Requires [Inno Setup 7](https://jrsoftware.org/isdl.php) (64-bit).

If you prefer to run the commands manually:

```powershell
cd ProcessSH

# Self-contained
dotnet publish -c Release -r win-x64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\self-contained\x64
dotnet publish -c Release -r win-arm64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\self-contained\arm64

# Framework-dependent
dotnet publish -c Release -r win-x64 --self-contained false -p:WindowsAppSDKSelfContained=false -o ..\publish\framework-dependent\x64
dotnet publish -c Release -r win-arm64 --self-contained false -p:WindowsAppSDKSelfContained=false -o ..\publish\framework-dependent\arm64

# Compile installers
& "C:\Program Files\Inno Setup 7\ISCC.exe" ..\installer.iss
& "C:\Program Files\Inno Setup 7\ISCC.exe" ..\installer-framework.iss
```

---

## Tech Stack

- C# + WinUI 3 + Windows App SDK
- [H.NotifyIcon.WinUI](https://github.com/HavenDV/H.NotifyIcon) — system tray integration
- Win32 `RegisterHotKey` — global hotkey

---

## Project Structure

```
ProcessSH/
├── ProcessSH/
│   ├── Assets/              # Icons, images, localization JSON
│   ├── Models/              # AppSettings, CommandHistory, Localization, Suggestion
│   ├── Services/            # CommandExecutor, ElevatedRunner, HotkeyManager
│   ├── ViewModels/          # CommandViewModel
│   ├── Views/               # Settings/About pages and windows
│   ├── App.xaml.cs
│   ├── MainPage.xaml.cs
│   └── MainWindow.xaml.cs
├── installer.iss                    # Self-contained installer
├── installer-framework.iss          # Framework-dependent installer
├── build.ps1                        # One-click build script
└── README.md
```

---

## Documentation

- [简体中文 README](Docs/zh-Hans/README.zh-Hans.md)
- [繁體中文 README](Docs/zh-Hant/README.zh-Hant.md)
- [License](LICENSE.md)

---

## License

MIT © 2026 CaoHaoran-Dev
