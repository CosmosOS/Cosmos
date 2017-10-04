; *** Inno Setup version 5.1.0+ Korean messages ***
;
; To download user-contributed translations of this file, go to:
;   http://www.jrsoftware.org/is3rdparty.php
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).
;
; translated : Woong-Jae An (a183393@hanmail.net)
; 
; ������ �� �� �˾ƺ��� ������, �ּ��� "����"�� �ƴ�, "�ǿ�"���� �ϵ��� ����Ͽ����ϴ�.
; ���࿡ ���� ������ ������� ������ ���� �����ϰų� ���� �Ų��� ǥ������ �ٲ����� �ϴ� �κ�, �� ���� ���ǻ����̳� ������ �����ôٸ� �������ð� ���� �ֽñ� �ٶ��ϴ�.
;
; Fixed: Fixed bug in Select Setup Language box that not correctly showed Language Name, Korean. Sorry to find and fix it too late. (5.1.0)
; ����: ��ġ ��� ���� ��ȭ ���ڿ��� ��� �̸��� "�ѱ���"�� ����� ǥ������ ���ϴ� ���׸� �����Ͽ����ϴ�. �ʹ� �ʰ� �߰��Ͽ� ��ģ �� �˼��մϴ�. (5.1.0)
;
; $jrsoftware: issrc/Files/Default.isl,v 1.66 2005/02/25 20:23:48 mlaan Exp $

[LangOptions]
LanguageName=<D55C><AD6D><C5B4>
LanguageID=$0412
LanguageCodePage=949
; If the language you are translating to requires special font faces or
; sizes, uncomment any of the following entries and change them accordingly.
;DialogFontName=
;DialogFontSize=8
;WelcomeFontName=Verdana
;WelcomeFontSize=12
;TitleFontName=Arial
;TitleFontSize=29
;CopyrightFontName=Arial
;CopyrightFontSize=8

[Messages]

; *** Application titles
SetupAppTitle=��ġ
SetupWindowTitle=��ġ - %1
UninstallAppTitle=���α׷� ����
UninstallAppFullTitle=%1 ����

; *** Misc. common
InformationTitle=����
ConfirmTitle=Ȯ��
ErrorTitle=����

; *** SetupLdr messages
SetupLdrStartupMessage=�� ���α׷��� %1��(��) ��ġ�� ���Դϴ�. ��� �Ͻðڽ��ϱ�?
LdrCannotCreateTemp=�ӽ� ������ ������ �� �����ϴ�. ��ġ�� �ߴܵǾ����ϴ�
LdrCannotExecTemp=�ӽ� ���� ���� ������ ������ �� �����ϴ�. ��ġ�� �ߴܵǾ����ϴ�

; *** Startup error messages
LastErrorMessage=%1.%n%n���� %2: %3
SetupFileMissing=��ġ ������ %1 ������ �����ϴ�. ������ �ذ��� �ֽðų� ���ο� ��ġ ���α׷��� ���غ��ʽÿ�.
SetupFileCorrupt=��ġ ������ �ջ�Ǿ����ϴ�. ���ο� ��ġ ���α׷��� ���غ��ʽÿ�.
SetupFileCorruptOrWrongVer=��ġ ������ �ջ�Ǿ��ų� �� ������ ��ġ ���α׷��� ȣȯ�� ���� �ʽ��ϴ�. ������ �ذ��� �ֽðų� ���ο� ��ġ ���α׷��� ���غ��ʽÿ�.
NotOnThisPlatform=�� ���α׷��� %1 ���� ��ġ���� �ʽ��ϴ�.
OnlyOnThisPlatform=�� ���α׷��� %1 ������ ��ġ�˴ϴ�.
OnlyOnTheseArchitectures=�� ���α׷��� ������ ���� ���μ��� ��Ű��ó�� �°� �����ε� Windows ������ ��ġ�˴ϴ�:%n%n%1
MissingWOW64APIs=���� ���� Windows �� ��ġ ���α׷��� 64��Ʈ ��ġ�� �����ϱ� ���� ����� �����ϰ� ���� �ʽ��ϴ�. �� ������ �ذ��Ͻ÷���, ���� �� %1 ��(��) ��ġ�Ͻʽÿ�.
WinVersionTooLowError=�� ���α׷��� %1 %2 (��)�� �� �̻��� Windows ������ ��ġ�˴ϴ�.
WinVersionTooHighError=�� ���α׷��� %1 %2 (��)�� �� �̻��� Windows ������ ��ġ���� �ʽ��ϴ�.
AdminPrivilegesRequired=�� ���α׷��� ��ġ�ϱ� ���ؼ��� Administrator ������ �ʿ��մϴ�.
PowerUserPrivilegesRequired=�� ���α׷��� ��ġ�ϱ� ���ؼ��� Administrator �Ǵ� Power Users ������ �ʿ��մϴ�.
SetupAppRunningError=��ġ ���α׷��� %1 ��(��) �������� ���� �����߽��ϴ�.%n%n���� ���� ���α׷��� ��� �����ʽÿ�. ��ġ�� ����Ͻ÷��� "Ȯ��"��, ����Ͻ÷��� "���"�� Ŭ���Ͻʽÿ�.
UninstallAppRunningError=��ġ ���� ���α׷��� %1 ��(��) �������� ���� �����߽��ϴ�.%n%n���� ���� ���α׷��� ��� �����ʽÿ�. ��ġ�� ����Ͻ÷��� "Ȯ��"��, ����Ͻ÷��� "���"�� Ŭ���Ͻʽÿ�.

; *** Misc. errors
ErrorCreatingDir=��ġ ���α׷��� "%1" ������ ������ �� �����ϴ�
ErrorTooManyFilesInDir="%1" ���� �ȿ� ������ �ʹ� ���� ������ ������ �� �����ϴ�

; *** Setup common messages
ExitSetupTitle=��ġ ����
ExitSetupMessage=��ġ�� ���� ������ �ʾҽ��ϴ�. ���� �����Ͻø�, ���α׷��� ��ġ���� ���� ���Դϴ�.%n%n��ġ�� �Ϸ��ϱ� ���ؼ� ��ġ ���α׷��� �ٽ� �����ؾ� �մϴ�.%n%n��ġ�� �����Ͻðڽ��ϱ�?
AboutSetupMenuItem=��ġ�� ���Ͽ�(&A)...
AboutSetupTitle=��ġ�� ���Ͽ�
AboutSetupMessage=%1 %2%n%3%n%n%1 Ȩ������:%n%4
AboutSetupNote=
TranslatorNote=�� �������� �ȿ��翡 ���� �����Ǿ����ϴ�. �� ������ ���� ���� ������ E-Mail: a183393@hanmail.net ���� �� �ֽñ� �ٶ��ϴ�.

; *** Buttons
ButtonBack=< �ڷ�(&B)
ButtonNext=����(&N) >
ButtonInstall=��ġ(&I)
ButtonOK=Ȯ��
ButtonCancel=���
ButtonYes=��(&Y)
ButtonYesToAll=��� ��(&A)
ButtonNo=�ƴϿ�(&N)
ButtonNoToAll=��� �ƴϿ�(&O)
ButtonFinish=�Ϸ�(&F)
ButtonBrowse=ã�ƺ���(&B)...
ButtonWizardBrowse=ã�ƺ���(&R)...
ButtonNewFolder=���� �����(&M)

; *** "Select Language" dialog messages
SelectLanguageTitle=��ġ ��� ����
SelectLanguageLabel=��ġ ���� �߿� ����� �� ������ �ֽʽÿ�:

; *** Common wizard text
ClickNext=��ġ�� ��� �Ͻ÷��� "����"��, �����Ͻ÷��� "���"�� Ŭ���Ͻʽÿ�.
BeveledLabel=
BrowseDialogTitle=���� ã�ƺ���
BrowseDialogLabel=�Ʒ��� ��Ͽ��� ������ �����ϰ� "Ȯ��"�� Ŭ���Ͻʽÿ�.
NewFolderName=�� ����

; *** "Welcome" wizard page
WelcomeLabel1=[name] �� ��ġ�� ���� ���� ȯ���մϴ�
WelcomeLabel2=�� ���α׷��� [name/ver] ��(��) ��ġ�� ���Դϴ�.%n%n��ġ�� ����Ͻñ� ���� ���� ���� ��� ���α׷��� ���Ḧ ���մϴ�.

; *** "Password" wizard page
WizardPassword=��ȣ
PasswordLabel1=�� ���α׷��� ��ġ�� ��ȣ�� ��ȣ�ǰ� �ֽ��ϴ�.
PasswordLabel3=��ȣ�� �Է��Ͻ� �� "����"�� Ŭ���� �ֽʽÿ�. ��ȣ�� ��ҹ��ڸ� �����մϴ�.
PasswordEditLabel=��ȣ(&P):
IncorrectPassword=�Է��Ͻ� ��ȣ�� �ùٸ��� �ʽ��ϴ�. �ٽ� �Է��� �ֽʽÿ�.

; *** "License Agreement" wizard page
WizardLicense=����� ���
LicenseLabel=��ġ�� ����Ͻñ� ���� �Ʒ��� �߿��� ������ �� �о�ʽÿ�.
LicenseLabel3=���� ����� ����� �ڼ��� �о��ֽʽÿ�. ��ġ�� ����Ͻ÷��� �� ��࿡ �����ؾ� �մϴ�.
LicenseAccepted=����� ��࿡ �����մϴ�(&A)
LicenseNotAccepted=����� ��࿡ �������� �ʽ��ϴ�(&D)

; *** "Information" wizard pages
WizardInfoBefore=����
InfoBeforeLabel=��ġ�� ����Ͻñ� ���� �Ʒ��� �߿��� ������ �� �о�ʽÿ�.
InfoBeforeClickLabel=��ġ�� ����� �غ� �Ǽ�����, "����"�� Ŭ���� �ֽʽÿ�.
WizardInfoAfter=����
InfoAfterLabel=��ġ�� ����ġ�� ���� �Ʒ��� �߿��� ������ �� �о�ʽÿ�.
InfoAfterClickLabel=�� �����̴ٸ�, "����"�� Ŭ���� �ֽʽÿ�.

; *** "User Information" wizard page
WizardUserInfo=����� ����
UserInfoDesc=����� ������ �Է��� �ֽʽÿ�.
UserInfoName=����� �̸�(&U):
UserInfoOrg=��ü��(&O):
UserInfoSerial=�ø��� �ѹ�(&S):
UserInfoNameRequired=�̸��� �Է��ؾ� �մϴ�.

; *** "Select Destination Location" wizard page
WizardSelectDir=��ġ�� ��ġ ����
SelectDirDesc=��� [name] ��(��) ��ġ�Ͻðڽ��ϱ�?
SelectDirLabel3=��ġ ���α׷��� [name] ��(��) ���� ������ ��ġ�� ���Դϴ�.
SelectDirBrowseLabel=����Ͻ÷��� "����"�� Ŭ���Ͻʽÿ�. �ٸ� ������ �����Ͻ÷���, "ã�ƺ���"�� Ŭ���Ͻʽÿ�.
DiskSpaceMBLabel=�ּ� [mb] MB �� ��ũ ���� ������ ��ġ�� �ʿ��մϴ�.
ToUNCPathname=�����Ͻ� ��Ʈ��ũ ��η� ��ġ�� �� �����ϴ�. ��Ʈ��ũ ����̺꿡 ��ġ�Ͻ÷���, ��Ʈ��ũ ����̺꿡 ������ �� �־�� �մϴ�.
InvalidPath=����̺� ���ڸ� ������ ��ü ��θ� �Է��ϼž� �մϴ�. ��:%n%nC:\APP%n%n ��Ʈ��ũ ����̺��� ��:%n%n\\server\share
InvalidDrive=��ġ�� ����̺곪 ��Ʈ��ũ ��ΰ� �������� �ʰų� ������ �� �����ϴ�. �ٸ� ��θ� �����Ͻʽÿ�.
DiskSpaceWarningTitle=��ũ ���� ����
DiskSpaceWarning=�� ���α׷��� ��ġ�ϴ� ���� �ּ� %1 KB �� ���������� �ʿ��ϳ�, �����Ͻ� ����̺�� %2 KB �� ��� �����մϴ�.%n%n�׷��� ����Ͻðڽ��ϱ�?
DirNameTooLong=���� �̸��̳� ��ΰ� �ʹ� ��ϴ�.
InvalidDirName=���� �̸��� ��Ȯ���� �ʽ��ϴ�.
BadDirName32=���� �̸����� ���� ���ڵ��� ������ �� �����ϴ�.%n%n%1
DirExistsTitle=�����ϴ� ����
DirExists=����:%n%n%1%n%n��(��) �̹� �����մϴ�. �׷��� �� ������ ��ġ�Ͻðڽ��ϱ�?
DirDoesntExistTitle=�������� �ʴ� ����
DirDoesntExist=����:%n%n%1%n%n��(��) �������� �ʽ��ϴ�. ������ ���� ����ðڽ��ϱ�?

; *** "Select Components" wizard page
WizardSelectComponents=���� ��� ��ġ
SelectComponentsDesc=� ���� ��Ҹ� ��ġ�Ͻðڽ��ϱ�?
SelectComponentsLabel2=��ġ�ϰ� ���� ���� ��Ҵ� �����Ͻð�, ��ġ�ϰ� ���� ���� ���� ��Ҵ� ������ �����Ͻʽÿ�. ��ġ�� ����� �غ� �Ǽ����� "����"�� Ŭ���Ͻʽÿ�.
FullInstallation=��ü ��ġ
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=�ּ� ��ġ
CustomInstallation=����� ��ġ
NoUninstallWarningTitle=���� ���� ��� ����
NoUninstallWarning=��ġ ���α׷��� ���� ���� ��Ұ� �̹� ��ġ�Ǿ� ������ �߰��߽��ϴ�.%n%n%1%n%n���α׷� ���� �� �� ���� ��� ���� ���ŵ��� ���� ���Դϴ�.%n%n�׷��� ��� �Ͻðڽ��ϱ�?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=������ ���� ��� ��ġ�� �ʿ��� �ּ� �뷮: [mb] MB

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=�߰� ���� ����
SelectTasksDesc=� ������ �߰��� �����Ͻðڽ��ϱ�?
SelectTasksLabel2=[name] �� ��ġ �������� �߰��� �����ϰ��� �ϴ� ������ �����Ͻð�, "����"�� Ŭ���Ͻʽÿ�.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=���� �޴� ���� ����
SelectStartMenuFolderDesc=��� ���� ���α׷��� �ٷ� ���⸦ ����ðڽ��ϱ�?
SelectStartMenuFolderLabel3=��ġ ���α׷��� ���α׷��� �ٷ� ���⸦ ���� ���� �޴� ������ ���� ���Դϴ�.
SelectStartMenuFolderBrowseLabel=����Ͻ÷��� "����"�� Ŭ���Ͻʽÿ�. �ٸ� ������ �����Ͻ÷��� "ã�ƺ���"�� Ŭ���Ͻʽÿ�.
MustEnterGroupName=���� �̸��� �Է��ؾ� �մϴ�.
GroupNameTooLong=���� �̸� �Ǵ� ��ΰ� �ʹ� ��ϴ�.
InvalidGroupName=���� �̸��� ��Ȯ���� �ʽ��ϴ�.
BadGroupName=���� �̸����� ������ ���ڵ��� ������ �� �����ϴ�.%n%n%1
NoProgramGroupCheck2=���� �޴� ������ ������ ����(&D)

; *** "Ready to Install" wizard page
WizardReady=��ġ �غ� �Ϸ�
ReadyLabel1=[name] ��(��) ��ġ�� �غ� �Ǿ����ϴ�.
ReadyLabel2a="��ġ"�� Ŭ���Ͽ� ��ġ�� �����Ͻðų�, "�ڷ�"�� Ŭ���Ͽ� ��ġ ������ �����ϰų� �ٲٽ� �� �ֽ��ϴ�.
ReadyLabel2b="��ġ"�� Ŭ���Ͽ� ��ġ�� �����Ͻʽÿ�.
ReadyMemoUserInfo=����� ����:
ReadyMemoDir=��ġ ���:
ReadyMemoType=��ġ ����:
ReadyMemoComponents=��ġ�� �������:
ReadyMemoGroup=���� �޴� ����:
ReadyMemoTasks=�߰��� ����Ǵ� �ɼ�:

; *** "Preparing to Install" wizard page
WizardPreparing=��ġ �غ� ��...
PreparingDesc=��ġ ���α׷��� [name] ��(��) ��ġ�� �غ� �ϰ� �ֽ��ϴ�.
PreviousInstallNotCompleted=������ ��ġ�� ���α׷� ���� �۾��� �Ϸ���� �ʾҽ��ϴ�. ������ ��ġ�� �Ϸ��ϱ� ���Ͽ� ��ǻ�͸� ����� �� �ʿ䰡 �ֽ��ϴ�.%n%n��ǻ�͸� ����� �� ��, ��ġ ���α׷��� ������Ͽ� [name] �� ��ġ�� �Ϸ��Ͻʽÿ�.
CannotContinue=��ġ�� ����� �� �����ϴ�. "���"�� Ŭ���Ͽ� ��ġ�� �����Ͻʽÿ�.

; *** "Installing" wizard page
WizardInstalling=��ġ ��...
InstallingLabel=��ġ ���α׷��� [name] ��(��) ��ġ�ϴ� ���� ��ٷ� �ֽʽÿ�.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=[name] ��ġ �Ϸ�
FinishedLabelNoIcons=��ġ ���α׷��� [name] �� ��ġ�� �Ϸ��߽��ϴ�.
FinishedLabel=��ġ ���α׷��� [name] �� ��ġ�� �Ϸ��߽��ϴ�. ��ġ�� �ٷ� ���⸦ �����Ͻø� ���α׷��� ����˴ϴ�.
ClickFinish="�Ϸ�"�� Ŭ���Ͽ� ��ġ�� �Ϸ��Ͻʽÿ�.
FinishedRestartLabel=[name] �� ��ġ�� �Ϸ��Ϸ��� �ý����� �ٽ� ���۵Ǿ�߸� �մϴ�. ���� �ý����� �ٽ� �����Ͻðڽ��ϱ�?
FinishedRestartMessage=[name] �� ��ġ�� �Ϸ��Ϸ��� �ý����� �ٽ� ���۵Ǿ�߸� �մϴ�.%n%n���� �ý����� �ٽ� �����Ͻðڽ��ϱ�?
ShowReadmeCheck=README ������ �о�ϴ�
YesRadio=��, ���� �ý����� �ٽ� �����ϰڽ��ϴ�(&Y)
NoRadio=�ƴϿ�, ���߿� �ý����� �ٽ� �����ϰڽ��ϴ�(&N)
; used for example as 'Run MyProg.exe'
RunEntryExec=%1 �����ϱ�
; used for example as 'View Readme.txt'
RunEntryShellExec=%1 �о��

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=���� ��ũ�� �ʿ�
SelectDiskLabel2=��ũ %1 ��(��) �ְ� "Ȯ��" ���߸� Ŭ���Ͻʽÿ�.%n%n���� ���� ��ũ�� �Ʒ��� ǥ�õ� ��ο� �ٸ� ������ �ִٸ� �ùٸ� ��θ� �Է��Ͻðų� "ã�ƺ���" ���߸� Ŭ���� �ùٸ� ��θ� ������ �ֽʽÿ�.
PathLabel=���(&P):
FileNotInDir2="%1" ������ "%2" �� �������� �ʽ��ϴ�. �ùٸ� ��ũ�� �ְų� �ٸ� ������ �����Ͻʽÿ�.
SelectDirectoryLabel=���� ��ũ�� �ִ� ��θ� �����Ͻʽÿ�.

; *** Installation phase messages
SetupAborted=��ġ�� �Ϸ���� �ʾҽ��ϴ�.%n%n������ �ذ��ϰ� ��ġ ���α׷��� �ٽ� �����Ͻʽÿ�.
EntryAbortRetryIgnore=�ٽ� �õ��Ͻ÷��� "�ٽ� �õ�"��, �����ϰ� ��ġ�� ����Ͻ÷��� "����"��, ��ġ�� �����Ͻ÷��� "���"�� Ŭ���Ͻʽÿ�.

; *** Installation status messages
StatusCreateDirs=���� ���� ��...
StatusExtractFiles=������ ������ Ǫ�� ��...
StatusCreateIcons=�ٷ� ���� ���� ��...
StatusCreateIniEntries=INI ��Ʈ�� ���� ��...
StatusCreateRegistryEntries=������Ʈ�� Ű ���� ��...
StatusRegisterFiles=���� ��� ��...
StatusSavingUninstall=���α׷� ���� ���� ���� ��...
StatusRunProgram=��ġ ������ ��...
StatusRollback=��ġ ���� ���·� �ǵ����� ��...

; *** Misc. errors
ErrorInternal2=���� ����: %1
ErrorFunctionFailedNoCode=%1 ����
ErrorFunctionFailed=%1 ����; �ڵ� %2
ErrorFunctionFailedWithMessage=%1 ����; �ڵ� %2.%n%3
ErrorExecutingProgram=���� ���� �Ұ���:%n%1

; *** Registry errors
ErrorRegOpenKey=������Ʈ�� Ű ���� �� ���� �߻�:%n%1\%2
ErrorRegCreateKey=������Ʈ�� Ű ���� �� ���� �߻�:%n%1\%2
ErrorRegWriteKey=������Ʈ�� Ű ���� �� ���� �߻�:%n%1\%2

; *** INI errors
ErrorIniEntry="%1" ���Ͽ� INI ��Ʈ�� ���� �� ���� �߻�.

; *** File copying errors
FileAbortRetryIgnore=�ٽ� �õ��Ͻ÷��� "�ٽ� �õ�"��, �����Ͻ÷��� "����"�� (�������� ����), ��ġ�� �����Ͻ÷��� "���"�� Ŭ���Ͻʽÿ�.
FileAbortRetryIgnore2=�ٽ� �õ��Ͻ÷��� "�ٽ� �õ�"��, �����Ͻ÷��� "����"�� (�������� ����), ��ġ�� �����Ͻ÷��� "���"�� Ŭ���Ͻʽÿ�.
SourceIsCorrupted=���� ������ �ջ�Ǿ����ϴ�
SourceDoesntExist=���� ���� "%1" ��(��) �������� �ʽ��ϴ�
ExistingFileReadOnly=���� ������ �б� �����Դϴ�.%n%n�б� ���� �Ӽ��� �����ϰ� �ٽ� �õ��Ͻ÷��� "�ٽ� �õ�"��, �� ������ �����Ͻ÷��� "����"��, ��ġ�� �����Ͻ÷��� "���"�� Ŭ���Ͻʽÿ�.
ErrorReadingExistingDest=���� �д� ���� ���� �߻�:
FileExists=�̹� �����ϴ� �����Դϴ�.%n%n������ ����ðڽ��ϱ�?
ExistingFileNewer=�̹� �����ϴ� ������ ��ġ�Ϸ��� ���Ϻ��� �Ź����Դϴ�. ���� ������ ������ ���� �����մϴ�.%n%n���� ������ �����Ͻðڽ��ϱ�?
ErrorChangingAttr=���� �Ӽ� ���� ���� ���� �߻�:
ErrorCreatingTemp=��� ������ ���� ���� �� ���� �߻�:
ErrorReadingSource=���� ������ �д� �� ���� �߻�:
ErrorCopying=���� ���� �� ���� �߻�:
ErrorReplacingExistingFile=���� ����� �� ���� �߻�:
ErrorRestartReplace=RestartReplace ����:
ErrorRenamingTemp=��� ���� ���� ���� �̸� ���� ���� ���� �߻�:
ErrorRegisterServer=���� DLL/OCX ��(��) ����� �� ����: %1
ErrorRegisterServerMissingExport=DllRegisterServer export �߰ߵ��� ����
ErrorRegisterTypeLib=���� ���� ������ ����� �� ����: %1

; *** Post-installation errors
ErrorOpeningReadme=README ������ ���� ���� ������ �߻��߽��ϴ�.
ErrorRestartingComputer=��ġ ���α׷��� �ý����� ����� �� �� �����ϴ�. �������� ����� �� �ֽʽÿ�.

; *** Uninstaller messages
UninstallNotFound="%1" ������ �������� �ʽ��ϴ�. ���α׷��� ������ �� �����ϴ�.
UninstallOpenError="%1" ������ �� �� �����ϴ�. ���α׷��� ������ �� �����ϴ�
UninstallUnsupportedVer=���α׷� ���� ���� ������ "%1" ��(��) �� ������ ���� ���α׷��� �ν��� �� ���� �������� �Ǿ� �ֽ��ϴ�. ���α׷��� ������ �� �����ϴ�
UninstallUnknownEntry=�� �� ���� ��Ʈ�� (%1) �� ���α׷� ���� ���� ���Ͽ� ��ϵǾ� �ֽ��ϴ�
ConfirmUninstall=������ %1 ��(��) �� ���� ��ҵ��� ������ �����Ͻðڽ��ϱ�?
UninstallOnlyOnWin64=�� ���α׷��� 64��Ʈ Windows ������ ���ŵ˴ϴ�.
OnlyAdminCanUninstall=�� ���α׷��� Administrator ������ �ִ� ����ڸ� �����Ͻ� �� �ֽ��ϴ�.
UninstallStatusLabel=%1 ��(��) ���ŵǴ� ���� ��ٷ� �ֽʽÿ�.
UninstalledAll=%1 ��(��) ������ ���ŵǾ����ϴ�.
UninstalledMost=%1 �� ���Ű� �������ϴ�.%n%n�Ϻ� �׸��� ������ �� �������ϴ�. ���� �׸���� ���� �����Ͻñ� �ٶ��ϴ�.
UninstalledAndNeedsRestart=%1 �� ���Ÿ� �Ϸ��Ϸ���, ��ǻ�Ͱ� �ٽ� ���۵Ǿ�� �մϴ�.%n%n���� ��ǻ�͸� �ٽ� �����Ͻðڽ��ϱ�?
UninstallDataCorrupted="%1" ������ �ջ�Ǿ����ϴ�. ���α׷��� ������ �� �����ϴ�

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=���� ������ �����Ͻðڽ��ϱ�?
ConfirmDeleteSharedFile2=�ý����� ���� ���� ������ � ���α׷������� ������ ������ �߰��߽��ϴ�. ���� ���� ������ �����Ͻðڽ��ϱ�?%n%n���� �� ���� ���ϵ��� �ٸ� ���α׷��鿡 ���ؼ� ���ȴٸ�, �� ���� ���� ���� �� �ٸ� ���α׷����� ����� �۵����� ���� �� �ֽ��ϴ�. Ȯ������ �ʴٸ� "�ƴϿ�" �� Ŭ���Ͻʽÿ�. ������ ���ܵξ �ý��ۿ� ������ ��ġ�� �ʽ��ϴ�.
SharedFileNameLabel=���� �̸�:
SharedFileLocationLabel=���:
WizardUninstalling=��ġ ���� ����
StatusUninstalling=%1 ���� ��...

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 ���� %2
AdditionalIcons=������ ����:
CreateDesktopIcon=���� ȭ�鿡 ������ ����(&D)
CreateQuickLaunchIcon=���� ���࿡ ������ ����(&Q)
ProgramOnTheWeb=�� ���� %1
UninstallProgram=%1 ����
LaunchProgram=%1 ����
AssocFileExtension=%2 Ȯ���ڸ� %1 �� ����(&A)
AssocingFileExtension=%2 Ȯ���ڸ� %1 �� ���� ��...
