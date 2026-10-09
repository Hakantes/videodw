#define MyAppName "YouTube ve Video İndirici"
#define MyAppVersion "2.0.1"
#define MyAppPublisher "Hakantes"
#define MyAppURL "https://github.com/Hakantes/videodw"
#define MyAppExeName "youtube dowload.exe"

[Setup]
AppId={{E684D355-63F6-4A59-86BC-B368A91522F5}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=.
OutputBaseFilename=VideoDownloader-Setup
SetupIconFile=Publish\app.ico
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\app.ico
UninstallDisplayName={#MyAppName} {#MyAppVersion}

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "Publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function IsDotNet481Installed(): Boolean;
var
  Release: Cardinal;
begin
  Result := False;
  if RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) then
  begin
    // 533320 is .NET 4.8.1 on Windows 11, 533325 on Windows 10
    if Release >= 533320 then
      Result := True;
  end;
end;

function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  Result := True;
  if not IsDotNet481Installed() then
  begin
    if MsgBox('Bu uygulamanın çalışması için Microsoft .NET Framework 4.8.1 veya daha yeni bir sürüm gereklidir.' + #13#10 + #13#10 +
              'Bilgisayarınızda .NET Framework 4.8.1 tespit edilemedi. Resmi Microsoft indirme sayfasına gitmek ister misiniz?' + #13#10 + #13#10 +
              '("Evet": İndirme sayfasını açar, "Hayır": Kuruluma yine de devam eder)', mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://go.microsoft.com/fwlink/?linkid=2203304', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
    end;
  end;
end;
