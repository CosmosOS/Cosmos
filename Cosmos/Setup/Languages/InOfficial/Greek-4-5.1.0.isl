; *** Inno Setup version 5.1.8+ Greek messages ***
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).
;
; Translated by Anastasis Chatzioglou
;               http://anasto.go.to
;               baldycom@hotmail.com
;

[LangOptions]
LanguageName=Greek
LanguageID=$408
; If the language you are translating to requires special font faces or
; sizes, uncomment any of the following entries and change them accordingly.
;DialogFontName=MS Shell Dlg
;DialogFontSize=8
;DialogFontStandardHeight=13
;TitleFontName=Arial
;TitleFontSize=29
;WelcomeFontName=Verdana
;WelcomeFontSize=12
;CopyrightFontName=Arial
;CopyrightFontSize=8
DialogFontName=MS Shell Dlg
DialogFontSize=8
;4.1.4+
;DialogFontStandardHeight=13
TitleFontName=Arial
TitleFontSize=29
WelcomeFontName=Arial
WelcomeFontSize=12
CopyrightFontName=Arial
CopyrightFontSize=8

[Messages]
; *** Application titles
SetupAppTitle=�����������
SetupWindowTitle=����������� -%1
UninstallAppTitle=�������������
UninstallAppFullTitle=%1 �������������
; 2.0.x
;DefaultUninstallIconName=������������� %1

; *** Misc. common
InformationTitle=�����������
ConfirmTitle=�����������
ErrorTitle=������

; *** SetupLdr messages
SetupLdrStartupMessage=�� ���������� � ����������� ��� %1. ������ �� ����������;
LdrCannotCreateTemp=������ ��� ���������� ���������� �������. � ����������� �� ����������� ����.
LdrCannotExecTemp=������ ���� �������� ������� ���� ��������� ��������. � ����������� ������������.

; *** Startup error messages
LastErrorMessage=%1.%n%n������ %2: %3
SetupFileMissing=��� ��������� �� ������ %1 ���� �������� ������������. ���� ���������� �� ������������� ��� ��� ��������� ��� ������������.
SetupFileCorrupt=�� ������ ������������ ����� �������������. ���� ���������� �� ������������� ��� ��� ��������� ��� ������������.
SetupFileCorruptOrWrongVer=�� ������ ������������ ����� ������������� � ����� �� ����� ������. ���� ���������� �� ������������� ��� ��� ��������� ��� ������������.
NotOnThisPlatform=���� �� ��������� ��� ������ �� ���������� �� %1.
OnlyOnThisPlatform=���� �� ��������� ���������� ���� �� %1.
; 5.1.0+
OnlyOnTheseArchitectures=���� �� ��������� ������ �� ������������ ���� �� Windows ����������� ��� ������������ �� �������������:%n%n%1
MissingWOW64APIs=� ������ ��� Windows ��� ���������� ��� �������� ��������������� 64-bit. ��� �� ��������� �� �������� ����������� �� Service Pack %1.
WinVersionTooLowError=���� �� ��������� ������� %1 ������ � �������.
WinVersionTooHighError=���� �� ��������� ��� ������ �� ���������� �� %1 ������ � �������.
AdminPrivilegesRequired=������ �� ����� � ������������ ���������� ��� �� ������������� ���� �� ���������.
PowerUserPrivilegesRequired=������ �� ����� � ������������ ����������  � Power User ��� �� ������������� ���� �� ���������.
SetupAppRunningError=� ����������� �������� ��� ���������� � �������� %1. �������� ������� ��� �������� ���� ��� ������� ������� ��� �� ����������.
UninstallAppRunningError=� ������������� �������� ��� ���������� � �������� %1. �������� ������� ��� �������� ���� ��� ������� ������� ��� �� ����������.

; *** Misc. errors
ErrorCreatingDir=� ����������� ��� ������ �� ������������ ��� �������� %1.
ErrorTooManyFilesInDir=��� ������ �� ������������ ��� ������ ���� �������� "%1" ������ ��� �������� ����� ������.

; *** Setup common messages
ExitSetupTitle=����� ������������.
ExitSetupMessage=� ����������� ��� ���� ���������. �� �� ����������� ���� �� ��������� ��� ������������ �� ������������� ��� �� ����������.%n%n�������� �� ���������� ���� ��� ����������� ��������.
AboutSetupMenuItem=&������� �� ��� �����������...
AboutSetupTitle=������� �� ��� �����������.
AboutSetupMessage=%1 ������ %2%n%3%n%n%1 ��������� ������%n%4
AboutSetupNote=Anasto
; 5.1.0+
TranslatorNote=Anastasis Chatzioglou - baldycom@hotmail.com

; *** Buttons
ButtonBack=< &����
ButtonNext=&������� >
ButtonInstall=&�����������
ButtonOK=�&������
ButtonCancel=&�����
ButtonYes=�&��
ButtonYesToAll=��� �� &���
ButtonNo=�&��
ButtonNoToAll=��� &�� ���
ButtonFinish=&�����
ButtonBrowse=&���������...
;4.1.3
ButtonWizardBrowse=&������...
ButtonNewFolder=&���������� ���� �������

; *** "Select Language" dialog messages
; 4.0.x
SelectLanguageTitle=������� ��� ������� ������������
SelectLanguageLabel=������� ��� ������� ��� ����� ���� ��� �������� ��� ������������:


; *** Common wizard text
ClickNext=������� ������� ��� �� ���������� � ����� ��� �� ����������� ��� �����������.
; 2.0.x
;ClickNextModern=������� ������� ��� �� ���������� � ����� ��� �� ����������� ��� �����������.
;;; - anasto -
BeveledLabel=anasto
;4.1.3
BrowseDialogTitle=������ �������
BrowseDialogLabel=������� ������ ���� ����� ��� ���� ���� OK.
NewFolderName=���� �������

; *** "Welcome" wizard page
WelcomeLabel1=������������ ���� ����������� ��� [name].
WelcomeLabel2=�� ����� ����������� ��� [name/ver] ���� ���������� ���.%n%n���� ���������� ��� ���������� �� �������� ���� ���� �������� ��� ������� ���������.

; *** "Password" wizard page
WizardPassword=�������� �������
PasswordLabel1=���� � ����������� ���������� ������ ��� �� ����������.
PasswordLabel3=�������� ����� ��� ������ ��� ��� ������� ������� ��� �� ����������.
PasswordEditLabel=&�������:
IncorrectPassword=� ������� ��� ������ ����� �����. ���������������.

; *** "License Agreement" wizard page
WizardLicense=����� ������
LicenseLabel=�������� �������� ���������� ��� �������� ����������� ���� ����������.
; 2.0.x
;LicenseLabel1=�������� �������� ���������� ��� �������� ����������� ���� ����������. �������������� ��� ����� ������� ��� �� ����� ��� �� �������.
;LicenseLabel2=���������� ���� ����� ��� ������ ������; �� ��������� ��� � ����������� �� �����������. ��� �� ���������� � ����������� ������ �� ���������� ���� ����� ��� ������ ������.
LicenseLabel3=�������� �������� ���������� ��� �������� ����������� ���� ����������. ������ �� ���������� ���� ����� ��� ������ ������ ���� �� ���������� ��� �����������.
LicenseAccepted=&������� ���� ����� ��� ������ ������
LicenseNotAccepted=��� &���������� ���� ����� ��� ������ ������

; *** "Information" wizard pages
WizardInfoBefore=�����������
InfoBeforeLabel=�������� �������� ���������� ��� �������� ����������� ���� ����������.
InfoBeforeClickLabel=�� ����� ������� �� ���������� ������� �������.
WizardInfoAfter=�����������
InfoAfterLabel=�������� �������� ���������� ��� �������� ����������� ���� ����������.
InfoAfterClickLabel=�� ����� ������� �� ���������� ������� �������.

; *** "User Information" wizard page
WizardUserInfo=����������� ��� ��� ������
UserInfoDesc=�������� ����� ��� �����������.
UserInfoName=&����� ������:
UserInfoOrg=&��������:
UserInfoSerial=&�������� ������:
UserInfoNameRequired=������ �� ������ �����.

; *** "Select Destination Location" wizard page
; 4.0.x
WizardSelectDir=�������� ��� �������� ��� �� ������������ �� ���������.
SelectDirDesc=��� �� ������������ �� [name];
;SelectDirLabel=�������� ��� �������� ��� �� ������������ �� ���������. ������� ������� ��� �� ����������.
DiskSpaceMBLabel=���� �� ��������� ���������� [mb] MB ���� ���� �����.
ToUNCPathname=� ����������� ��� ������ �� ����� �� ����� �������. �� ������ �� ����� � ����������� �� ����� ������� ������ �� ������� ����� �� �����.
InvalidPath=����� ��� ����� ��������.%n����������:%n%nC:\APP
InvalidDrive=� ������� ������ � � ������ ������� ��� ��������� ��� ������� � ��� ����� �����������. �������� �����.
DiskSpaceWarningTitle=��� ������� ������� ����� ��� �����.
DiskSpaceWarning=� ����������� ���������� ����������� %1 KB �������� ���� ��� ����� ���� � ����������� ������ �������� ����� %2 KB.%n%n������ �� ���������� ����������;
BadDirName32=������� ��������� ��� ������� �� ��������� ������� ��� ���� �������� ����������:%n%n%1
DirExistsTitle=� ��������� �������.
DirExists=� ���������:%n%n%1%n%n������� ���. ������ �� ����� � ����������� �� ����� ��� ��������;
DirDoesntExistTitle=� ��������� ��� �������.
DirDoesntExist=� ���������:%n%n%1%n%n��� �������. ������ �� ������������;
;4.1.3
InvalidDirName=����� ����� �������.
;4.1.5
DirNameTooLong=�� ����� ��� ������� ����� ���� ������.
;4.1.8
;SelectDirLabel2=�� [name] �� ����������� ���� �������� ������.%n%n��� �������� ���� �������. �� ������ ���� ������, ���� ������.
SelectDirLabel3=�� [name] �� ����������� ���� �������� ������.
SelectDirBrowseLabel=��� �������� ���� �������. �� ������ ���� ������, ���� ������.

; *** "Select Components" wizard page
WizardSelectComponents=������� ����������
SelectComponentsDesc=���� ��������� ������ �� �������������;
SelectComponentsLabel2=�������� �� ��������� ��� ������ �� ������������� ��� ������� ������� ��� �������� ��� ������������.
FullInstallation=������ �����������.
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=������������ �����������.
CustomInstallation=���������� �����������.
NoUninstallWarningTitle=�� ��������� ��������.
NoUninstallWarning=� ����������� ���������� ��� �� �������� ��������� ����� ��� ������������� ���� ���������� ���:%n%n%1
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=� ������������ ������� ������� ����������� [mb] MB �������� ���� ���� �����.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=������� ��������� ���������
SelectTasksDesc=����� �������� ��������� ������ �� ������;
SelectTasksLabel2=�������� ��� �������� ��������� ��� ������ �� ������ ���� ��� ����������� ��� [name] ��� ������� �������.

; *** "Select Start Menu Folder" wizard page
; 2.0.x
;ReadyMemoTasks=�������� ���������:
WizardSelectProgramGroup=������� ��������� ��� ����� ��������.
SelectStartMenuFolderDesc=��� �� ������������ �� ������������ ��� ������������;
; 4.0.x
;SelectStartMenuFolderLabel=�������� ��� �������� ��� ����� �������� ���� ����� ������ ������������� �� ������������ ��� ������������ ��� ������� �������.
; 5.1.0+
;NoIconsCheck=&����� ���������� ����������
MustEnterGroupName=������ �� ������ �� ����� ���� ���������.
BadGroupName=������� ��������� ��� ������� �� ��������� ������� ��� ���� �������� ����������:%n%n%1
NoProgramGroupCheck2=&����� ���������� ��������� ��� ����� ��������.
;4.1.3
InvalidGroupName=�� ����� ��� group ��� ����� �����.
;4.1.4+
GroupNameTooLong=�� ����� ��� group ����� ���� ������.
;4.1.8
;SelectStartMenuFolderLabel2=To setuo �� ������������ ��� ������������ ��� ������������ ��� �������� group.%n%n��� ��������, ���� �������. �� ������ ���� group, ���� ������.
SelectStartMenuFolderLabel3=o setuo �� ������������ ��� ������������ ��� ������������ ��� �������� group.
SelectStartMenuFolderBrowseLabel=��� ��������, ���� �������. �� ������ ���� group, ���� ������.


; *** "Ready to Install" wizard page
WizardReady=������� ��� �����������
ReadyLabel1=� ����������� ��� [name] ����� ������ �� ���������� ���� ���������� ���.
ReadyLabel2a=������� ����������� ��� �� ���������� � ���� �� ������ �� �������� ������� ���������.
ReadyLabel2b=������� ����������� ��� �� ����������.
ReadyMemoUserInfo=����������� ������:
ReadyMemoDir=��������� ����������:
ReadyMemoType=����� ������������:
ReadyMemoComponents=���������� ���������:
ReadyMemoGroup=��������� ��� ����� ��������:
ReadyMemoTasks=�������� ���������:

; *** "Preparing to Install" wizard page
WizardPreparing=������������ ������������
PreparingDesc=� ����������� ������������ �� ��������� [name] �� ����������� ���� ����������.
PreviousInstallNotCompleted=The installation/removal of a previous program was not completed. You will need to restart your computer to complete that installation.%n%nAfter restarting your computer, run Setup again to complete the installation of [name].
CannotContinue=Setup cannot continue. Please click Cancel to exit.

; *** "Installing" wizard page
WizardInstalling=������� ������������
InstallingLabel=�������� ���������� �� ����������� � ����������� ��� [name] ���� ���������� ���.

; *** "Setup Completed" wizard page
; 2.0.x
;WizardFinished=� ����������� ������������
FinishedHeadingLabel=Completing the [name] Setup Wizard
FinishedLabelNoIcons=� ����������� ��� [name] ���� ���������� ��� �������� �� ��������.
FinishedLabel=� ����������� ��� [name] ���� ���������� ��� �������� �� ��������. �������� �� ���������� �� ��������� ����������� �� ��������� ��� ������������� ��� ����� ��������.
ClickFinish=������� ����� ��� �� ����������� �� ��������� ������������.
FinishedRestartLabel=��� �� ����������� � ����������� ���  [name] ������ �� ����� ������������ ��� ���������� ���. ������ �� ����� ����;
FinishedRestartMessage=��� �� ����������� � ����������� ���  [name] ������ �� ����� ������������ ��� ���������� ���.%n%n������ �� ����� ����;
ShowReadmeCheck=��� ���� �� ������� ��� ����������� ��� ������������
YesRadio=&��� �� ����� ������������ ����.
NoRadio=&��� �� ���� ������������ ��������.
; used for example as 'Run MyProg.exe'
RunEntryExec=�� ���������� �� ��������� %1
; used for example as 'View Readme.txt'
RunEntryShellExec=�� ���������� �� %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=����������� ��� ������� ��������
; 4.0.x
;SelectDirectory=�������� ��������
SelectDiskLabel2=����������� ��� �������� %1 ��� ������� �������.
PathLabel=&��������
FileNotInDir2=�� ������ "%1" ��� ��������� ��� "%2". ����������� �� ����� ��������.
SelectDirectoryLabel=����� ��� ��������� ��� �������� ���������.

; *** Installation phase messages
SetupAborted=� ����������� ��� ������������.%n%n��������� �� �������� ��� ��������� ���� ��� �����������.
EntryAbortRetryIgnore=������� Retry ��� �� ����������������,  Ignore ��� �� ���������� � Abort ��� �� ����������� ��� �����������.

; *** Installation status messages
StatusCreateDirs=���������� ���������...
StatusExtractFiles=����������� �������...
StatusCreateIcons=���������� ����������...
StatusCreateIniEntries=���������� ��� ��� ������ ����������...
StatusCreateRegistryEntries=���������� ��� ������ ����������...
StatusRegisterFiles=���������� �������
StatusSavingUninstall=����������� ��������������...
StatusRunProgram=������������ ��� �����������...
StatusRollback=Rolling back changes...

; *** Misc. errors
; 2.0.x
;ErrorInternal=������ %1
ErrorInternal2=������ %1
ErrorFunctionFailedNoCode=%1 ������
ErrorFunctionFailed=%1 ������, ���. %2
ErrorFunctionFailedWithMessage=%1 ������, ���. %2%n%3
ErrorExecutingProgram=��� ������ �� ���������� �� ������:%n%1

;2.0.x
;ErrorDDEExecute=DDE: ������ ���� ��� �������� ��� ��������� (code: %1)
;ErrorDDECommandFailed=DDE: � ������ �������.
;ErrorDDERequest=DDE: ������ ���� ��� �������� ��� ��������� (code: %1)

; *** Registry errors
ErrorRegOpenKey=��� ������ �� ��������� �� ������ ������� ����������:%n%1\%2
ErrorRegCreateKey=��� ������ �� ������������ �� ������ ������� ����������:%n%1\%2
ErrorRegWriteKey=��� ������ �� ����� ���������� ��� ������ ������� ����������:%n%1\%2

; *** INI errors
ErrorIniEntry=��� ������ �� ����� ���������� ��� ��� ������ ���������� "%1".

; *** File copying errors
FileAbortRetryIgnore=������� Retry ��� �� ����������������,  Ignore ��� �� ���������� � Abort ��� �� ����������� ��� �����������.
FileAbortRetryIgnore2=������� Retry ��� �� ����������������,  Ignore ��� �� ���������� � Abort ��� �� ����������� ��� �����������.
SourceIsCorrupted=�� ������ ���������� ����� ������������.
SourceDoesntExist=�� ������ ���������� "%1" ��� �������.
ExistingFileReadOnly=�� ������ ����� ������������ ���� ��� ��������.%n%n������� Retry ��� �� �� ����������� ��� �� ������������ ����, Ignore ��� �� �� ������������ � Abort ��� �� ����������� ��� �����������.
ErrorReadingExistingDest=������������� ������ ���� ��� �������� ��� �������:
FileExists=�� ������ �������.%n%n������ �� �����������;
ExistingFileNewer=��� ������ ��� ������� ���� ���������� ��� ����� �������� ������� ��� ������ ��� ������������. ����������� �� ��������� �� ������� ������.%n%n������ �� ��������� �� ������� ������;
ErrorChangingAttr=�������� ������ ���� ���������� �� ��������� �� �������������� ��� �������:
ErrorCreatingTemp=�������� ������ ���� ���������� �� ������������ ��� ������ ���� �������� ����������:
ErrorReadingSource=�������� ������ ���� ���������� ��������� ��� ������� ����������:
ErrorCopying=�������� ������ ���� ���������� �� ���������� �� ������:
ErrorReplacingExistingFile=�������� ������ ���� ���������� �� �������������� �� ������� ������:
ErrorRestartReplace=�������� ������ ���� ���������� �� ����� ������������ ��� ������������� �������:
ErrorRenamingTemp=�������� ������ ���� ���������� ������������ ���� ������� ���� �������� ����������:
ErrorRegisterServer=�������� ������ ���� ���������� ����������� DLL/OCX: %1
ErrorRegisterServerMissingExport=DllRegisterServer export not found
ErrorRegisterTypeLib=Unable to register the type library: %1

; *** Post-installation errors
ErrorOpeningReadme=�������� ������ ���� ���������� �� �������� �� ������ �����������.
ErrorRestartingComputer=�������� ������ ���� ���������� ������������� ��� ����������.%n�������� ������������� ��� ���������� ���.

; *** Uninstaller messages
UninstallNotFound=�� ������ "%1" ��� �������. � ������������� ��� ������ �� �����
; 4.0.x
UninstallOpenError=�� ������ "%1" ��� ������� �� ��������. � ������������� ��� ������ �� �����
UninstallUnsupportedVer=�� ������ "%1" ��� ������������� ��� ���� ��� ������ ��� ������������, � ������������� ��� ������ �� ����������
UninstallUnknownEntry=�� ������ "%1" ��� ������������� ��� ���� ��� ������ ��� ������������, � ������������� ��� ������ �� ����������
ConfirmUninstall=����� �������� ��� ������ �� ���������� �� %1 ��� ��� �� ��������� ���;
; 5.1.0+
UninstallOnlyOnWin64=���� � �������� ������ �� �������������� ���� �� 64-bit Windows.
OnlyAdminCanUninstall=� ������������� ������ �� ���������� ���� ��� ��� ����������� ����������
UninstallStatusLabel=�������� ���������� ��� �� %1 ���������� ��� ��� ���������� ���
UninstalledAll=� ������������� ��� %1 ����� �� ��������.
UninstalledMost=� ������������� ��� %1 ����� �� ��������.%n%n������ ��������� ��� ���������� ���� ���������� ��� �� ������ �� �� ���������� �����.
UninstalledAndNeedsRestart=To complete the uninstallation of %1, your computer must be restarted.%n%nWould you like to restart now?
UninstallDataCorrupted="%1" ���� �� ������ ����� ������������. ��� ������ �� ����� �������������.

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=������ �� ���������� �� ����� ������;
ConfirmDeleteSharedFile2=�� ����� ������ ��� ���������������� ��� ������ ���������. ������ �� ����������;%n%n�� ������ ��������� �� ������������ ���� ��� ���������� ����� �� �� ����������. �� ��� ����� �������� ������ �� ��� ������� ��� ��� ��������� ������ ��������.
SharedFileNameLabel=����� �������:
SharedFileLocationLabel=���������:
WizardUninstalling=������� ��������������:
StatusUninstalling=������������� ��� %1...

[CustomMessages]
NameAndVersion=%1 ������ %2
AdditionalIcons=�������� ���������:
CreateDesktopIcon=���������� ���� &���������� ���� ��������� ��������
CreateQuickLaunchIcon=���������� ���� ���������� ��� &������� ��������
ProgramOnTheWeb=�� %1 ��� Internet
UninstallProgram=������������� ��� %1
LaunchProgram=�������� ��� %1
AssocFileExtension=%������������ ��� %1 �� ��� %2 �������� �������
AssocingFileExtension=������� ������������ ��� %1 �� ��� %2 �������� �������...

