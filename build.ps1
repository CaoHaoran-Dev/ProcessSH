# ProcessSH 一键打包脚本
# 用法：
#   .\build.ps1
#   .\build.ps1 -Version 1.0.2
#   .\build.ps1 -InnoSetup "D:\Tools\Inno Setup 7\ISCC.exe"
#   .\build.ps1 -SkipPublish
#   .\build.ps1 -SkipInstaller

param(
    [string]$Version = "1.0.1",
    [string]$InnoSetup = "",
    [switch]$SkipPublish,
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

# ─────────────────────────────────────────────
# 路径（全部相对脚本所在目录）
# ─────────────────────────────────────────────
$Root             = $PSScriptRoot
$ProjectDir       = Join-Path $Root "ProcessSH"
$PublishDir       = Join-Path $Root "publish"
$InstallerOutput  = Join-Path $Root "installer_output"

# ─────────────────────────────────────────────
# 工具函数：在 STA 线程里弹出文件选择对话框
# ─────────────────────────────────────────────
function Select-ISCCPath {
    param([string]$InitialDir = "D:\")

    $thread = [System.Threading.Thread]::new({
        param($initDir)
        Add-Type -AssemblyName System.Windows.Forms
        $dialog = New-Object System.Windows.Forms.OpenFileDialog
        $dialog.Title = "选择 Inno Setup 的 ISCC.exe"
        $dialog.Filter = "ISCC.exe|ISCC.exe|可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*"
        $dialog.InitialDirectory = $initDir
        $dialog.CheckFileExists = $true
        $dialog.Multiselect = $false

        $result = $dialog.ShowDialog()
        if ($result -eq [System.Windows.Forms.DialogResult]::OK) {
            $script:SelectedISCC = $dialog.FileName
        } else {
            $script:SelectedISCC = $null
        }
    })
    $thread.SetApartmentState([System.Threading.ApartmentState]::STA)
    $thread.Start($InitialDir)
    $thread.Join()

    return $script:SelectedISCC
}

# ─────────────────────────────────────────────
# Inno Setup：留空则自动探测
# ─────────────────────────────────────────────
if ([string]::IsNullOrWhiteSpace($InnoSetup)) {
    $candidates = @(
        "D:\Program Files\Inno Setup 7\ISCC.exe",
        "D:\Program Files (x86)\Inno Setup 7\ISCC.exe",
        "D:\Inno Setup 7\ISCC.exe",
        "D:\Tools\Inno Setup 7\ISCC.exe",
        "C:\Program Files\Inno Setup 7\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 7\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    )
    foreach ($c in $candidates) {
        if (Test-Path $c) {
            $InnoSetup = $c
            break
        }
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " ProcessSH Build Script" -ForegroundColor Cyan
Write-Host " Version:    $Version" -ForegroundColor Cyan
Write-Host " Root:       $Root" -ForegroundColor Cyan
Write-Host " InnoSetup:  $(if ($InnoSetup) { $InnoSetup } else { '(未找到)' })" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ─────────────────────────────────────────────
# 1. Publish
# ─────────────────────────────────────────────
if (-not $SkipPublish) {
    Write-Host "[1/3] Publishing..." -ForegroundColor Yellow

    $configs = @(
        @{ Name = "self-contained";     ExtraArgs = @("--self-contained", "true",  "-p:WindowsAppSDKSelfContained=true") },
        @{ Name = "framework-dependent"; ExtraArgs = @("--self-contained", "false", "-p:WindowsAppSDKSelfContained=false") }
    )
    $archs = @("win-x64", "win-arm64")

    foreach ($cfg in $configs) {
        foreach ($arch in $archs) {
            $outDir = Join-Path $PublishDir "$($cfg.Name)\$arch"
            Write-Host "  -> $($cfg.Name) / $arch" -ForegroundColor Gray

            $args = @(
                "publish",
                "-c", "Release",
                "-r", $arch,
                "-o", $outDir,
                "-p:Version=$Version",
                "-p:FileVersion=$Version.0",
                "-p:AssemblyVersion=$Version.0",
                "-p:InformationalVersion=$Version"
            ) + $cfg.ExtraArgs

            Push-Location $ProjectDir
            try {
                & dotnet @args
                if ($LASTEXITCODE -ne 0) {
                    throw "dotnet publish 失败: $($cfg.Name) / $arch"
                }
            }
            finally {
                Pop-Location
            }
        }
    }

    Write-Host "[1/3] Publish 完成" -ForegroundColor Green
    Write-Host ""
}

# ─────────────────────────────────────────────
# 2. 校验发布目录（与 publish 输出一致：win-x64 / win-arm64）
# ─────────────────────────────────────────────
Write-Host "[2/3] 校验发布目录..." -ForegroundColor Yellow

$requiredDirs = @(
    "$PublishDir\self-contained\win-x64",
    "$PublishDir\self-contained\win-arm64",
    "$PublishDir\framework-dependent\win-x64",
    "$PublishDir\framework-dependent\win-arm64"
)

foreach ($dir in $requiredDirs) {
    if (-not (Test-Path $dir)) {
        Write-Host "  缺少目录: $dir" -ForegroundColor Red
        Write-Host "  请先运行不带 -SkipPublish 的 build.ps1" -ForegroundColor Red
        Read-Host "按 Enter 退出"
        exit 1
    }
    $exe = Join-Path $dir "ProcessSH.exe"
    if (-not (Test-Path $exe)) {
        Write-Host "  缺少文件: $exe" -ForegroundColor Red
        Read-Host "按 Enter 退出"
        exit 1
    }
    Write-Host "  OK: $dir" -ForegroundColor Gray
}

Write-Host "[2/3] 校验完成" -ForegroundColor Green
Write-Host ""

# ─────────────────────────────────────────────
# 3. 编译安装包
# ─────────────────────────────────────────────
if (-not $SkipInstaller) {
    try {
        Write-Host "[3/3] 编译安装包..." -ForegroundColor Yellow

        # ── Inno Setup：自动探测失败则弹出文件选择对话框 ──
        if ([string]::IsNullOrWhiteSpace($InnoSetup) -or -not (Test-Path $InnoSetup)) {
            Write-Host ""
            Write-Host "未找到 Inno Setup 的 ISCC.exe，请在弹出的对话框中选择。" -ForegroundColor Yellow
            Write-Host "常见路径：" -ForegroundColor Gray
            Write-Host "  D:\Program Files\Inno Setup 7\ISCC.exe" -ForegroundColor DarkGray
            Write-Host "  D:\Inno Setup 7\ISCC.exe" -ForegroundColor DarkGray
            Write-Host "  C:\Program Files\Inno Setup 7\ISCC.exe" -ForegroundColor DarkGray
            Write-Host "  C:\Program Files (x86)\Inno Setup 7\ISCC.exe" -ForegroundColor DarkGray
            Write-Host ""

            $selected = Select-ISCCPath -InitialDir "D:\"

            if ([string]::IsNullOrWhiteSpace($selected)) {
                Write-Host "已取消安装包编译。" -ForegroundColor Red
                Read-Host "按 Enter 退出"
                exit 1
            }

            if (-not (Test-Path $selected)) {
                Write-Host "路径不存在: $selected" -ForegroundColor Red
                Read-Host "按 Enter 退出"
                exit 1
            }

            $InnoSetup = $selected
            Write-Host "使用: $InnoSetup" -ForegroundColor Green
            Write-Host ""
        }

        $issFiles = @("installer.iss", "installer-framework.iss")

        foreach ($iss in $issFiles) {
            $issPath = Join-Path $Root $iss
            if (-not (Test-Path $issPath)) {
                Write-Host "  跳过（不存在）: $iss" -ForegroundColor Yellow
                continue
            }

            Write-Host "  -> $iss" -ForegroundColor Gray
            & $InnoSetup $issPath
            if ($LASTEXITCODE -ne 0) {
                throw "Inno Setup 编译失败: $iss"
            }
        }

        Write-Host "[3/3] 安装包编译完成" -ForegroundColor Green
        Write-Host ""
    }
    catch {
        Write-Host ""
        Write-Host "错误: $_" -ForegroundColor Red
        Read-Host "按 Enter 退出"
        exit 1
    }
}

# ─────────────────────────────────────────────
# 完成
# ─────────────────────────────────────────────
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " 构建完成" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "输出目录：" -ForegroundColor White
Write-Host "  发布:   $PublishDir" -ForegroundColor Gray
Write-Host "  安装包: $InstallerOutput" -ForegroundColor Gray
Write-Host ""

if (Test-Path $InstallerOutput) {
    Get-ChildItem $InstallerOutput -Filter "*.exe" | ForEach-Object {
        Write-Host "  $($_.Name)  ($([math]::Round($_.Length / 1MB, 1)) MB)" -ForegroundColor Green
    }
}

Write-Host ""
Read-Host "按 Enter 退出"