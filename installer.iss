[Setup]
AppName=POECraftHelper
AppVersion=1.2.0.0
DefaultDirName={autopf}\POECraftHelper
DefaultGroupName=POECraftHelper
OutputDir=.\installer_output
OutputBaseFilename=POECraftHelper_Setup_v1.2.0.0
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\POECraftHelper"; Filename: "{app}\POECraftHelper.exe"
Name: "{commondesktop}\POECraftHelper"; Filename: "{app}\POECraftHelper.exe"

[Run]
Filename: "{app}\POECraftHelper.exe"; Description: "POECraftHelper starten"; Flags: nowait postinstall skipifsilent