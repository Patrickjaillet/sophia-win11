#define MyAppName "Sophia Script for Win11"
#define MyAppVersion "1.2.0.0"
#define MyAppPublisher "Patrick JAILLET"
#define MyAppURL "https://patrickjaillet.github.io/sophia-win11"
#define MyAppExeName "SophiaWin11.exe"

[Setup]
AppId={{9C6C6C6E-3E1C-4C6E-8B0F-SOPHIAWIN11}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={autopf}\SophiaWin11
DefaultGroupName={#MyAppName}
OutputDir=Output
OutputBaseFilename=SophiaWin11-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Files]
Source: "..\src\SophiaWin11.App\bin\Release\net9.0-windows10.0.22621.0\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Sophia Script for Win11"; Flags: nowait postinstall skipifsilent
