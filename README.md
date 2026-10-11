# ProcessSH

> A Windows port of RunProcess — a Spotlight-style command launcher with a system-native backdrop and system tray integration.

---

## Overview

ProcessSH is a lightweight Windows application that provides a quick command execution interface inspired by the Windows Run dialog. It combines the power of PowerShell 7 with a polished, modern interface that remains accessible from the system tray at all times.

**Version 2.0.0** — a ground-up rewrite that brings ProcessSH in line with RunProcess 3.4.0, minus session mode and built-in auto-update.

---

## Features

- **Command Execution** – Execute any PowerShell command with real-time output display
- **ANSI Colors** – Full support for 8/16/256 colors and 24-bit truecolor. Output from `git status`, `Get-ChildItem`, `Select-String`, etc. is rendered with proper colors
- **Tab Completion** – Auto-complete aliases, command history, system commands, and file paths
- **Command History** – Navigate previous commands with ↑ / ↓, ranked by frecency (frequency × recency)
- **History Search** – Press `Ctrl+R` to fuzzy-search the entire history
- **Aliases** – Define short names like `gs` → `git status`; they appear first in completion
- **Custom Paths** – Add folders like `D:\Tools` or `%USERPROFILE%\bin`. Executables found there can be run by name, resolved via `PATHEXT`
- **Drag & Drop** – Drag files from Explorer to automatically populate file paths with proper escaping
- **Administrator Execution** – Execute commands with elevated privileges via a UAC prompt
- **Global Hotkey** – Show or hide the window from anywhere with `Ctrl+Alt+R` (customizable in Settings)
- **System Tray Integration** – Access from the system tray; no taskbar icon
- **Window Material** – Mica, Mica Alt, Acrylic, or solid. Applied across all windows
- **Floating Window** – Spotlight-style floating window, positioned at the bottom-left of the screen
- **Single Main Window** – One main window; multiple auxiliary windows (Settings, Help, About, History Search)
- **Custom Working Directory** – Configure the default working directory for new sessions
- **Automatic Hide on Deactivate** – Window hides automatically when the application loses focus
- **Multi-language** – English, Simplified Chinese, and Traditional Chinese

---

## Usage

Open the application, click the icon in the system tray, type a command, and press Enter.

| Input | Result |
|-------|--------|
| `Get-ChildItem` | List files |
| `cd C:\Users` | Change directory (session only; see below) |
| `git status` | Show Git status (with colors) |
| `C:\Windows\notepad.exe` | Launch Notepad |

---

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `Ctrl+Alt+R` | Show or hide the window globally (customizable in Settings) |
| `Ctrl+R` | Search command history |
| `Enter` | Execute |
| `Shift+Enter` | Newline (multi-line input) |
| `Tab` | Completion popup |
| `↑` `↓` | Command history navigation |
| `Esc` | Hide window |

> **Note:** The default hotkey uses `Ctrl+Alt` instead of `Ctrl+Win` because Windows reserves many `Win` key combinations and `RegisterHotKey` cannot reliably capture them.

---

## Appearance

Window material can be selected in **Settings → Appearance**:

| Material | Description | Availability |
|----------|-------------|--------------|
| Mica | Desktop wallpaper sampling, static | Windows 11 |
| Mica Alt | Higher contrast variant of Mica | Windows 11 |
| Acrylic | Real-time blur with noise | Windows 11 (full) |
| None | Solid system window background | All versions |

The chosen style applies to the main window, Settings, Help, About, and History Search.

If a material is unsupported on the current system, the option is hidden from the list and the next best fallback is applied automatically.

---

## Window Behavior

By default, the main window hides automatically when the application loses focus, matching the behavior of Spotlight. This can be disabled in **Settings → General**.

When the Settings, Help, About, or History Search window is open, the main window stays visible until the auxiliary window is closed.

Closing the main window hides it rather than exiting the application. To quit, use **Tray → Quit**.

---

## Working Directory

Configure in **Settings → Paths**. If not set, the user's home directory is used.

This only affects newly opened commands. If the configured path no longer exists, the application falls back to the home directory and displays a warning in Settings.

> **Note:** ProcessSH does not implement session mode. Each command runs in a fresh `pwsh` process. `cd` does not persist across commands. To keep a working directory, use `Set-Location` with an absolute path, or set the default working directory in Settings.

---

## Command History

Every executed command is recorded. The history powers two features:

- **↑ / ↓ navigation** – ranked by frecency: frequently used and recently used commands rise to the top
- **Ctrl+R search** – fuzzy matching across the whole history (`gst` matches `git status`)

History is stored at:

```
%LOCALAPPDATA%\ProcessSH\history.json
```

Maximum 500 entries. The oldest least-recently-used entries are pruned first.

Clear all history from the tray menu: **Clear History**.

---

## Aliases

Aliases let you define short names for frequently used commands. They rank above everything else in the completion popup.

Built-in examples:

| Alias | Expands to |
|-------|-----------|
| `gs` | `git status` |
| `gp` | `git pull --rebase` |
| `ll` | `Get-ChildItem -Force` |
| `serve` | `python3 -m http.server 8000` |
| `which` | `Get-Command` |

Custom aliases live in:

```
%LOCALAPPDATA%\ProcessSH\aliases.json
```

Format:

```json
[
  { "name": "gs", "expansion": "git status" },
  { "name": "serve", "expansion": "python3 -m http.server 8000" }
]
```

Aliases can also be managed in **Settings → Aliases**. Changes take effect immediately.

---

## Custom Paths

**Settings → Paths** has two sections.

### Default Working Directory

Configure the working directory for new commands. If not set, the user's home directory is used. Only affects newly opened commands.

If the configured path no longer exists, the application falls back to the home directory and displays a warning in Settings.

### Custom Paths

Add folders whose executables can be run by name.

| Input | Resolves to |
|-------|-------------|
| `myscript` (in `D:\Tools`, added here) | `D:\Tools\myscript.exe` |
| `fastfetch` (installed via Scoop) | `C:\Users\Me\scoop\shims\fastfetch.exe` |

Folders here are **appended to PATH**, so the system PATH and any PATH set in your PowerShell profile are preserved. Executables are resolved by `pwsh` itself, following `PATHEXT` order.

Custom paths live in:

```
%LOCALAPPDATA%\ProcessSH\paths.json
```

Format:

```json
{
  "Paths": [
    "D:\\Tools",
    "%USERPROFILE%\\bin"
  ]
}
```

Changes take effect immediately.

---

## Administrator Execution

Enable "Run as administrator" before executing a command to receive a UAC prompt. The command runs in a separate elevated `pwsh` process.

> **Note:** In elevated mode, output is not returned to the main window. This is a limitation of the `runas` verb — the elevated process is fully detached.

---

## Language

ProcessSH supports English, Simplified Chinese, and Traditional Chinese. Language can be switched at runtime in **Settings → General → Language**.

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

Requires .NET 10 SDK and Windows App SDK 1.6+.

### Build the Installer

A PowerShell script `build.ps1` is provided at the repository root to automate the entire build process:

```powershell
# Build both installers with default version
.\build.ps1

# Specify a version
.\build.ps1 -Version 2.0.1

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

## Configuration Files

ProcessSH stores user data in:

```
%LOCALAPPDATA%\ProcessSH\
├── aliases.json     # command aliases
├── history.json     # command history (max 500 entries)
├── paths.json       # custom executable search paths
└── settings.json    # application settings
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

Managed automatically. Maximum 500 entries.

### settings.json

Managed by the application. Edited indirectly through the Settings window.

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
│   ├── Models/              # AppSettings, AliasStore, PathStore, CommandHistory, ...
│   ├── Services/            # CommandExecutor, ElevatedRunner, HotkeyManager, LaunchAtLogin
│   ├── Utils/               # ANSIParser, ShellQuoting, CommandPreprocessor, BackdropHelper
│   ├── ViewModels/          # CommandViewModel
│   ├── Views/               # Settings/Help/About pages and windows, HistorySearchPage, OutputTextView
│   ├── App.xaml.cs
│   ├── MainPage.xaml.cs
│   └── MainWindow.xaml.cs
├── installer.iss                    # Self-contained installer
├── installer-framework.iss          # Framework-dependent installer
├── build.ps1                        # One-click build script
└── README.md
```

---

## Differences from RunProcess (macOS)

ProcessSH 2.0.0 aligns with RunProcess 3.4.0 in features and behavior, with the following intentional differences:

| Feature | RunProcess (macOS) | ProcessSH (Windows) |
|---------|-------------------|---------------------|
| Session mode | ✅ Persistent zsh | ❌ Each command is a fresh process |
| Auto-update | ✅ Sparkle | ❌ Manual download from GitHub Releases |
| App bundles | `.app` bundles via `open -b` | Not applicable; uses `.exe` / `.lnk` / `.bat` / `.cmd` |
| Appearance | Liquid Glass / Frosted Glass | Mica / Mica Alt / Acrylic |
| Path separator | `:` (colon) | `;` (semicolon) |
| Home notation | `~` | `%USERPROFILE%` |

---

## Documentation

- [简体中文 README](Docs/zh-Hans/README.zh-Hans.md)
- [繁體中文 README](Docs/zh-Hant/README.zh-Hant.md)
- [License](LICENSE)

---

## License

MIT © 2026 CaoHaoran-Dev