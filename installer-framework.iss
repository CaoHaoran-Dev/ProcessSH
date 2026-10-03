; ProcessSH 安装脚本（框架依赖版）
; 需要 Inno Setup 7.x (64 位版)
; 编译前请先执行 build.ps1 或手动 publish

#define MyAppName "ProcessSH"
#define MyAppVersion "1.0.1"
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
OutputBaseFilename=ProcessSH-Setup-{#MyAppVersion}-framework-dependent
ArchitecturesAllowed=x64compatible or arm64
ArchitecturesInstallIn64BitMode=x64compatible or arm64
PrivilegesRequired=admin
WizardStyle=modern

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\framework-dependent\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: PreferX64Files
Source: "publish\framework-dependent\win-arm64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: PreferArm64Files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
function PreferArm64Files: Boolean;
begin
  Result := IsArm64;
end;

function PreferX64Files: Boolean;
begin
  Result := IsX64Compatible and not IsArm64;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ErrorCode: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    if MsgBox('This version requires Windows App SDK Runtime.' + #13#10 +
              'If ProcessSH fails to start, install it from:' + #13#10 +
              'https://aka.ms/windowsappsdk/1.6/latest/windowsappruntimeinstall-x64.exe' + #13#10 + #13#10 +
              'Open the download page now?',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open',
                'https://aka.ms/windowsappsdk/1.6/latest/windowsappruntimeinstall-x64.exe',
                '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
    end;

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