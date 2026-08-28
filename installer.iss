; Script generated for Inno Setup
; AutoCommit - Polyglot Knowledge Engine

#define MyAppName "AutoCommit"
#define MyAppVersion "3.4.0"
#define MyAppPublisher "hctrung2k4"
#define MyAppURL "https://github.com/nothingistoo-late/AutoCommit"
#define MyAppExeName "AutoCommit.exe"

[Setup]
; Basic Application Info
AppId={{D9A3B576-C182-442C-8219-4F57128C1032}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases

; Installation Folders & Output
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=dist
OutputBaseFilename=AutoCommit_Setup
Compression=lzma2/max
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequiredOverridesAllowed=dialog

; Visual Styling & Icons
SetupIconFile=app.ico
WizardImageFile=AutoCommit.png
WizardSmallImageFile=AutoCommit2.png
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; Main Executable & Binaries
Source: "publish\AutoCommit-win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Assets & Documentation
Source: "app.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "config.example.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "GUIDE.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "notes\*"; DestDir: "{app}\notes"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Start Menu Shortcuts
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"
Name: "{group}\1. Chạy AutoCommit Tự Động"; Filename: "{app}\notes\1_Chay_Tu_Dong.bat"
Name: "{group}\2. Đăng ký Windows Task Scheduler"; Filename: "{app}\notes\2_Dang_Ky_Task_Scheduler.bat"
Name: "{group}\3. Gỡ bỏ Windows Task Scheduler"; Filename: "{app}\notes\3_Go_Bo_Task_Scheduler.bat"
Name: "{group}\Hướng dẫn sử dụng (GUIDE)"; Filename: "{app}\GUIDE.md"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

; Desktop Shortcut
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\app.ico"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
