#ifndef MyAppVersion
  #define MyAppVersion "0.0.0"
#endif

#define MyAppName "Sequence Gap Scanner"
#define MyAppPublisher "Drywater Dev Co"
#define MyAppURL "https://github.com/DrywaterDevCo/SequenceGapScanner"
#define MyAppExeName "SequenceGapScanner.exe"

[Setup]
AppId={{8A4C2E5F-3B7D-4F91-A6E8-2C9D5B1F7A3E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\SequenceGapScanner
DefaultGroupName={#MyAppName}
OutputDir=installer
OutputBaseFilename=SequenceGapScanner-Setup
SetupIconFile=app.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
MinVersion=10.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
