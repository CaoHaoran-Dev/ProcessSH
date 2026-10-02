; ProcessSH 安装脚本
; 需要 Inno Setup 7.x (64 位版)
; 编译前请先执行：
;   cd ProcessSH
;   dotnet publish -c Release -r win-x64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\x64
;   dotnet publish -c Release -r win-arm64 --self-contained true -p:WindowsAppSDKSelfContained=true -o ..\publish\arm64

#define MyAppName "ProcessSH"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "CaoHaoran-Dev"
#define MyAppURL "https://github.com/CaoHaoran-Dev/ProcessSH"
#define MyAppExeName "ProcessSH.exe"

[Setup]
AppId={{C25C7046-251D-4D2A-9B60-7B5C5E183F26}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
OutputDir=.\installer_output
OutputBaseFilename=ProcessSH-Setup-{#MyAppVersion}
ArchitecturesAllowed=x64compatible or arm64
ArchitecturesInstallIn64BitMode=x64compatible or arm64
PrivilegesRequired=admin
WizardStyle=modern

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; x64 版本文件（优先在 x64 兼容系统上安装）
Source: "ProcessSH\publish\x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: PreferX64Files

; ARM64 版本文件（优先在 ARM64 系统上安装）
Source: "ProcessSH\publish\arm64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: PreferArm64Files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
// 判断是否为 ARM64 系统
function PreferArm64Files: Boolean;
begin
  Result := IsArm64;
end;

// 判断是否为 x64 兼容系统（且非 ARM64 优先）
function PreferX64Files: Boolean;
begin
  Result := IsX64Compatible and not IsArm64;
end;

// 安装完成后询问是否跳转 Microsoft Store 安装 PowerShell 7
procedure CurStepChanged(CurStep: TSetupStep);
var
  ErrorCode: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    if MsgBox('ProcessSH requires PowerShell 7 to execute commands.' + #13#10 + #13#10 +
              'Open Microsoft Store to install it now?',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open',
                'ms-windows-store://pdp/?ProductId=9MZ1SNWT0N5D',
                '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
    end;
  end;
end;