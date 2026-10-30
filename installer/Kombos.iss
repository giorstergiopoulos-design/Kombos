; Κόμβος (Kombos) - Inno Setup installer script.
; ΣΗΜΕΙΩΣΗ: το #define MyAppVersion παρακάτω πρέπει να ενημερώνεται χειροκίνητα ώστε να ταιριάζει με το
; Kombos.Localization.L.AppVersion σε κάθε νέα έκδοση - δεν διαβάζεται αυτόματα από τον πηγαίο κώδικα.
;
; Πώς χτίζεται:
;   1. dotnet publish ..\Kombos.csproj -c Release -r win-x64 --self-contained false -o ..\publish
;      (framework-dependent - χρειάζεται .NET 10 Desktop Runtime εγκατεστημένο στον υπολογιστή-στόχο)
;   2. Άνοιξε αυτό το αρχείο με τον Inno Setup Compiler (ISCC.exe Kombos.iss) ή πάτησε Compile.
;   3. Το KombosSetup-<version>.exe βγαίνει στο installer\output\.

#define MyAppName "Κόμβος"
#define MyAppNameAscii "Kombos"
#define MyAppVersion "4.9.3"
#define MyAppPublisher "Kombos"
#define MyAppExeName "Kombos.exe"
#define MyPublishDir "..\publish"

[Setup]
AppId={{B3D8F2A1-6C4E-4A9F-9D2B-1E7F5C8A3B60}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppNameAscii}
DefaultGroupName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=output
OutputBaseFilename=KombosSetup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
; Εικονίδιο του ίδιου του installer + εικόνες των σελίδων του οδηγού (100/150/200 % DPI) με το εικονίδιο του Κόμβου.
SetupIconFile=..\kombos.ico
WizardImageFile=wizard-large-100.bmp,wizard-large-150.bmp,wizard-large-200.bmp
WizardSmallImageFile=wizard-small-100.bmp,wizard-small-150.bmp,wizard-small-200.bmp
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64compatible
; Σύντομη περιγραφή εφαρμογής - βλ. README.md για την πλήρη λίστα λειτουργιών.
AppComments=Τοπικός hub 39+ εργαλείων AI με τον Claude ως κεντρικό agent.

; Το Greek.isl ΔΕΝ είναι από τις "επίσημες" γλώσσες που έρχονται προεγκατεστημένες με το Inno Setup -
; είναι το unofficial community translation από το ίδιο το επίσημο repo του Inno Setup
; (jrsoftware/issrc, Files/Languages/Unofficial/Greek.isl). Μπαίνει ΠΡΩΤΟ ώστε να είναι η προεπιλογή -
; η ίδια η εφαρμογή είναι ούτως ή άλλως πλήρως ελληνική στο πρώτο άνοιγμά της, οπότε το βήμα
; εγκατάστασης έπρεπε να ταιριάζει. Το αρχείο είναι αντιγραμμένο εδώ (Languages\Greek.isl) ώστε το
; script να μεταγλωττίζεται ίδιο σε οποιοδήποτε μηχάνημα, χωρίς να χρειάζεται να υπάρχει ήδη
; εγκατεστημένο στο τοπικό Inno Setup.
[Languages]
Name: "greek"; MessagesFile: "Languages\Greek.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[CustomMessages]
greek.StartWithWindowsTask=Εκκίνηση του %1 μαζί με τα Windows
english.StartWithWindowsTask=Start %1 with Windows
french.StartWithWindowsTask=Démarrer %1 avec Windows
german.StartWithWindowsTask=%1 mit Windows starten

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startupicon"; Description: "{cm:StartWithWindowsTask,{#MyAppName}}"; Flags: unchecked

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startupicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
