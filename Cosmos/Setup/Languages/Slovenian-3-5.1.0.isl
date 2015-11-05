; *** Inno Setup version 5.1.0+ Slovenian messages ***
;
; To download user-contributed translations of this file, go to:
;   http://www.jrsoftware.org/is3rdparty.php
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).
;
; Maintained by Miha Remec (innosetup@miharemec.com)
;
; $jrsoftware: issrc/Files/Languages/Slovenian.isl,v 1.13 2005/03/01 08:14:09 mlaan Exp $

[LangOptions]
LanguageName=Slovenski
LanguageID=$0424
LanguageCodePage=1250

[Messages]

; *** Application titles
SetupAppTitle=Namestitev
SetupWindowTitle=Namestitev - %1
UninstallAppTitle=Odstranitev
UninstallAppFullTitle=Odstranitev programa %1

; *** Misc. common
InformationTitle=Informacija
ConfirmTitle=Potrditev
ErrorTitle=Napaka

; *** SetupLdr messages
SetupLdrStartupMessage=V ra�unalnik boste namestili program %1. �elite nadaljevati?
LdrCannotCreateTemp=Ne morem ustvariti za�asne datoteke. Namestitev je prekinjena
LdrCannotExecTemp=Ne morem zagnati datoteke v za�asni mapi. Namestitev je prekinjena

; *** Startup error messages
LastErrorMessage=%1.%n%nNapaka %2: %3
SetupFileMissing=Manjka datoteka %1. Odpravite napako ali si priskrbite drugo kopijo programa.
SetupFileCorrupt=Datoteke namestitvenega programa so okvarjene. Priskrbite si drugo kopijo programa.
SetupFileCorruptOrWrongVer=Datoteke so okvarjene ali nezdru�ljive s to razli�ico namestitvenega programa. Odpravite napako ali si priskrbite drugo kopijo programa.
NotOnThisPlatform=Program ni namenjen za uporabo v %1.
OnlyOnThisPlatform=Program je namenjen za uporabo v %1.
OnlyOnTheseArchitectures=Program lahko namestite le na razli�icah MS Windows sistemov, ki so na�rtovani za naslednje tipe procesorjev:%n%n%1
MissingWOW64APIs=Razli�ica MS Windows, ki jo uporabljate, ne vsebuje okolja, ki ga zahteva namestitveni program za izvedbo 64-bitne namestitve. Problem odpravite z namestitvijo servisnega paketa %1.
WinVersionTooLowError=Ta program zahteva %1 razli�ico %2 ali novej�o.
WinVersionTooHighError=Tega programa ne morete namestiti v %1 razli�ice %2 ali novej�e.
AdminPrivilegesRequired=Namestitev programa lahko opravi le administrator.
PowerUserPrivilegesRequired=Namestitev programa lahko opravi le administrator ali zahtevni uporabnik.
SetupAppRunningError=Program %1 je trenutno odprt.%n%nZaprite program, nato kliknite V redu za nadaljevanje ali Prekini za izhod.
UninstallAppRunningError=Program %1 je trenutno odprt.%n%nZaprite program, nato kliknite V redu za nadaljevanje ali Prekini za izhod.

; *** Misc. errors
ErrorCreatingDir=Namestitveni program ni mogel ustvariti mape "%1"
ErrorTooManyFilesInDir=Namestitveni program ne more ustvariti nove datoteke v mapi "%1", ker vsebuje mapa preve� datotek

; *** Setup common messages
ExitSetupTitle=Prekini namestitev
ExitSetupMessage=Namestitev ni kon�ana. �e jo boste prekinili, program ne bo name��en.%n%nPonovno namestitev lahko izvedete kasneje.%n%n�elite prekiniti namestitev?
AboutSetupMenuItem=&O namestitvenem programu...
AboutSetupTitle=O namestitvenem programu
AboutSetupMessage=%1 razli�ica %2%n%3%n%n%1 doma�a stran:%n%4
AboutSetupNote=
TranslatorNote=Slovenski prevod: Miha Remec%n(innosetup@miharemec.com)

; *** Buttons
ButtonBack=< Na&zaj
ButtonNext=&Naprej >
ButtonInstall=&Namesti
ButtonOK=V redu
ButtonCancel=Prekini
ButtonYes=&Da
ButtonYesToAll=Da za &vse
ButtonNo=&Ne
ButtonNoToAll=N&e za vse
ButtonFinish=&Kon�aj
ButtonBrowse=&Prebrskaj...
ButtonWizardBrowse=P&rebrskaj...
ButtonNewFolder=&Ustvari novo mapo

; *** "Select Language" dialog messages
SelectLanguageTitle=Izbira jezika namestitve
SelectLanguageLabel=Izberite jezik, ki ga �elite uporabljati med namestitvijo:

; *** Common wizard text
ClickNext=Kliknite Naprej za nadaljevanje namestitve ali Prekini za prekinitev namestitve.
BeveledLabel=
BrowseDialogTitle=Izbira mape
BrowseDialogLabel=Izberite mapo s spiska, nato kliknite V redu.
NewFolderName=Nova mapa

; *** "Welcome" wizard page
WelcomeLabel1=Dobrodo�li v namestitev programa [name].
WelcomeLabel2=V ra�unalnik boste namestili program [name/ver].%n%nPriporo�ljivo je, da pred za�etkom namestitve zaprete vse odprte programe.

; *** "Password" wizard page
WizardPassword=Geslo
PasswordLabel1=Namestitev je za��itena z geslom.
PasswordLabel3=Vpi�ite geslo, nato kliknite Naprej za nadaljevanje. Pri vpisu pazite na male in velike �rke.
PasswordEditLabel=&Geslo:
IncorrectPassword=Geslo, ki ste ga vpisali, ni pravilno. Vpi�ite pravilno geslo.

; *** "License Agreement" wizard page
WizardLicense=Licen�na pogodba za uporabo programa
LicenseLabel=Pred nadaljevanjem preberite licen�no pogodbo za uporabo programa.
LicenseLabel3=Preberite licen�no pogodbo za uporabo programa. Program lahko namestite le, �e se s pogodbo v celoti strinjate.
LicenseAccepted=&Da, sprejemam vse pogoje licen�ne pogodbe
LicenseNotAccepted=N&e, pogojev licen�ne pogodbe ne sprejmem

; *** "Information" wizard pages
WizardInfoBefore=Informacije
InfoBeforeLabel=Pred nadaljevanjem preberite naslednje pomembne informacije.
InfoBeforeClickLabel=Ko boste pripravljeni za nadaljevanje namestitve, kliknite Naprej.
WizardInfoAfter=Informacije
InfoAfterLabel=Pred nadaljevanjem preberite naslednje pomembne informacije.
InfoAfterClickLabel=Ko boste pripravljeni za nadaljevanje namestitve, kliknite Naprej.

; *** "User Information" wizard page
WizardUserInfo=Podatki o uporabniku
UserInfoDesc=Vpi�ite svoje podatke.
UserInfoName=&Ime:
UserInfoOrg=&Podjetje:
UserInfoSerial=&Serijska �tevilka:
UserInfoNameRequired=Vpis imena je obvezen.

; *** "Select Destination Location" wizard page
WizardSelectDir=Izbira ciljnega mesta
SelectDirDesc=Kam �elite namestiti program [name]?
SelectDirLabel3=Program [name] bo name��en v naslednjo mapo.
SelectDirBrowseLabel=Za nadaljevanje kliknite Naprej. �e �elite izbrati drugo mapo, kliknite Prebrskaj.
DiskSpaceMBLabel=Na disku mora biti vsaj [mb] MB prostora.
ToUNCPathname=Programa ne morete namestiti v UNC pot. �e �elite namestiti v omre�je, se morate povezati z omre�nim pogonom.
InvalidPath=Vpisati morate polno pot vklju�no z oznako pogona. Primer:%n%nC:\PROGRAM%n%nali UNC pot v obliki:%n%n\\stre�nik\mapa_skupne_rabe
InvalidDrive=Izbrani pogon ali UNC skupna raba ne obstaja ali ni dostopna. Izberite drugega.
DiskSpaceWarningTitle=Na disku ni dovolj prostora
DiskSpaceWarning=Namestitev potrebuje vsaj %1 KB prostora, toda na izbranem pogonu je na voljo le %2 KB.%n%n�elite kljub temu nadaljevati?
DirNameTooLong=Ime mape ali poti je predolgo.
InvalidDirName=Ime mape ni veljavno.
BadDirName32=Ime mape ne sme vsebovati naslednjih znakov:%n%n%1
DirExistsTitle=Mapa �e obstaja
DirExists=Mapa%n%n%1%n%n�e obstaja. �elite program vseeno namestiti v to mapo?
DirDoesntExistTitle=Mapa ne obstaja
DirDoesntExist=Mapa %n%n%1%n%nne obstaja. �elite ustvariti to mapo?

; *** "Select Components" wizard page
WizardSelectComponents=Izbira komponent
SelectComponentsDesc=Katere komponente �elite namestiti?
SelectComponentsLabel2=Ozna�ite komponente, ki jih �elite namestiti; odzna�ite komponente, ki jih ne �elite namestiti. Kliknite Naprej, ko boste pripravljeni za nadaljevanje.
FullInstallation=Polna namestitev
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Osnovna namestitev
CustomInstallation=Nastavljiva namestitev
NoUninstallWarningTitle=Komponente �e obstajajo
NoUninstallWarning=Namestitveni program je ugotovil, da so naslednje komponente �e name��ene v ra�unalniku:%n%n%1%n%nOdzna�itev teh komponent �e ne pomeni tudi njihove odstranitve.%n%n�elite vseeno nadaljevati?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Za izbrano namestitev potrebujete vsaj [mb] MB prostora na disku.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Izbira dodatnih opravil
SelectTasksDesc=Katera dodatna opravila �elite izvesti?
SelectTasksLabel2=Izberite dodatna opravila, ki jih bo namestitveni program opravil med namestitvijo programa [name], nato kliknite Naprej.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Izbira mape v meniju �Start�
SelectStartMenuFolderDesc=Kje naj namestitveni program ustvari programske ikone?
SelectStartMenuFolderLabel3=Namestitveni program bo ustvaril programske ikone v naslednji mapi v meniju �Start�.
SelectStartMenuFolderBrowseLabel=Za nadaljevanje kliknite Naprej. �e �elite izbrati drugo mapo, kliknite Prebrskaj.
MustEnterGroupName=Ime skupine mora biti vpisano.
GroupNameTooLong=Ime mape ali poti je predolgo.
InvalidGroupName=Ime mape ni veljavno.
BadGroupName=Ime skupine ne sme vsebovati naslednjih znakov:%n%n%1
NoProgramGroupCheck2=&Ne ustvari mape v meniju �Start�

; *** "Ready to Install" wizard page
WizardReady=Pripravljen za namestitev
ReadyLabel1=Namestitveni program je pripravljen za namestitev programa [name] v va� ra�unalnik.
ReadyLabel2a=Kliknite Namesti za za�etek name��anja. Kliknite Nazaj, �e �elite pregledati ali spremeniti katerokoli nastavitev.
ReadyLabel2b=Kliknite Namesti za za�etek name��anja.
ReadyMemoUserInfo=Podatki o uporabniku:
ReadyMemoDir=Ciljno mesto:
ReadyMemoType=Tip namestitve:
ReadyMemoComponents=Izbrane komponente:
ReadyMemoGroup=Mapa v meniju �Start�:
ReadyMemoTasks=Dodatna opravila:

; *** "Preparing to Install" wizard page
WizardPreparing=Pripravljam za namestitev
PreparingDesc=Namestitveni program je pripravljen za namestitev programa [name] v va� ra�unalnik.
PreviousInstallNotCompleted=Namestitev/odstranitev prej�njega programa ni bila kon�ana. Da bi jo dokon�ali, morate ra�unalnik ponovno zagnati.%n%nPo ponovnem zagonu ra�unalnika ponovno odprite namestitveni program, da boste kon�ali namestitev programa [name].
CannotContinue=Namestitveni program ne more nadaljevati. Pritisnite Prekini za izhod.

; *** "Installing" wizard page
WizardInstalling=Name��anje
InstallingLabel=Po�akajte, da bo program [name] name��en v va� ra�unalnik.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Zaklju�ek namestitve programa [name]
FinishedLabelNoIcons=Program [name] je name��en v va� ra�unalnik.
FinishedLabel=Program [name] je name��en v va� ra�unalnik. Program za�enete tako, da odprete pravkar ustvarjene programske ikone.
ClickFinish=Kliknite tipko Kon�aj za zaklju�ek namestitve.
FinishedRestartLabel=Za dokon�anje namestitve programa [name] morate ra�unalnik znova zagnati. Ali ga �elite znova zagnati zdaj?
FinishedRestartMessage=Za dokon�anje namestitve programa [name] morate ra�unalnik znova zagnati. %n%nAli ga �elite znova zagnati zdaj?
ShowReadmeCheck=�elim prebrati datoteko z navodili
YesRadio=&Da, ra�unalnik znova za�eni zdaj
NoRadio=&Ne, ra�unalnik bom znova zagnal pozneje

; used for example as 'Run MyProg.exe'
RunEntryExec=Odpri %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Preberi %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Namestitveni program potrebuje naslednjo disketo
SelectDiskLabel2=Vstavite disketo %1 and kliknite V redu.%n%n�e se datoteke s te diskete nahajajo v drugi mapi, kot je navedena spodaj, vpi�ite pravilno pot ali kliknite Prebrskaj.
PathLabel=&Pot:
FileNotInDir2=Datoteke "%1" ni v mapi "%2". Vstavite pravilno disketo ali izberite drugo mapo.
SelectDirectoryLabel=Vpi�ite lokacijo naslednje diskete.

; *** Installation phase messages
SetupAborted=Namestitev ni bila kon�ana.%n%nOdpravite te�avo in znova odprite namestitveni program.
EntryAbortRetryIgnore=Kliknite Ponovi za ponovitev, Prezri za nadaljevanje kljub problemu, ali Prekini za prekinitev namestitve.

; *** Installation status messages
StatusCreateDirs=Ustvarjam mape...
StatusExtractFiles=Raz�irjam datoteke...
StatusCreateIcons=Ustvarjam bli�njice...
StatusCreateIniEntries=Vpisujem v INI datoteke...
StatusCreateRegistryEntries=Vpisujem v register...
StatusRegisterFiles=Registriram datoteke...
StatusSavingUninstall=Zapisujem podatke za odstranitev programa...
StatusRunProgram=Zaklju�ujem namestitev...
StatusRollback=Obnavljam prvotno stanje...

; *** Misc. errors
ErrorInternal2=Interna napaka: %1
ErrorFunctionFailedNoCode=%1 ni uspel(a)
ErrorFunctionFailed=%1 ni uspel(a); koda %2
ErrorFunctionFailedWithMessage=%1 ni uspela; koda %2.%n%3
ErrorExecutingProgram=Ne morem odpreti programa:%n%1

; *** Registry errors
ErrorRegOpenKey=Napaka pri odpiranju klju�a v registru:%n%1\%2
ErrorRegCreateKey=Napaka pri ustvarjanju klju�a v registru:%n%1\%2
ErrorRegWriteKey=Napaka pri pisanju klju�a v registru:%n%1\%2

; *** INI errors
ErrorIniEntry=Napaka pri vpisu v INI datoteko "%1".

; *** File copying errors
FileAbortRetryIgnore=Kliknite Ponovi za ponovitev, Prezri za preskok datoteke (ni priporo�ljivo) ali Prekini za prekinitev namestitve.
FileAbortRetryIgnore2=Kliknite Ponovi za ponovitev, Prezri za nadaljevanje (ni priporo�ljivo) ali Prekini za prekinitev namestitve.
SourceIsCorrupted=Izvorna datoteka je okvarjena
SourceDoesntExist=Izvorna datoteka "%1" ne obstaja
ExistingFileReadOnly=Obstoje�a datoteka je ozna�ena samo za branje.%n%nPritisnite Ponovi za odstranitev te lastnosti in ponovni poskus, Prezri za preskok te datoteke, ali Prekini za prekinitev namestitve.
ErrorReadingExistingDest=Pri branju obstoje�e datoteke je pri�lo do napake:
FileExists=Datoteka �e obstaja.%n%n�elite, da jo namestitveni program prepi�e?
ExistingFileNewer=V ra�unalniku �e imate name��eno novej�o datoteko. Priporo�ljivo je, da obstoje�o (novej�o) datoteko obdr�ite.%n%n�elite obdr�ati obstoje�o (novej�o) datoteko?
ErrorChangingAttr=Pri poskusu spremembe lastnosti datoteke je pri�lo do napake:
ErrorCreatingTemp=Pri ustvarjanju datoteke v ciljni mapi je pri�lo do napake:
ErrorReadingSource=Pri branju izvorne datoteke je pri�lo do napake:
ErrorCopying=Pri kopiranju datoteke je pri�lo do napake:
ErrorReplacingExistingFile=Pri poskusu zamenjave obstoje�e datoteke je pri�lo do napake:
ErrorRestartReplace=RestartReplace failed:
ErrorRenamingTemp=Pri poskusu preimenovanja datoteke v ciljni mapi je pri�lo do napake:
ErrorRegisterServer=Registracija DLL/OCX ni mo�na: %1
ErrorRegisterServerMissingExport=DllRegisterServer funkcija ne obstaja
ErrorRegisterTypeLib=Prijava vrste knji�nice ni mogo�a: %1

; *** Post-installation errors
ErrorOpeningReadme=Pri odpiranju datoteke README je pri�lo do napake.
ErrorRestartingComputer=Namestitveni program ni uspel znova zagnati ra�unalnika. Ponovni zagon opravite ro�no.

; *** Uninstaller messages
UninstallNotFound=Datoteka "%1" ne obstaja. Odstranitev ni mogo�a.
UninstallOpenError=Datoteke "%1" ne morem odpreti. Ne morem odstraniti
UninstallUnsupportedVer=Dnevni�ka datoteka "%1" je v obliki, ki je ta razli�ica odstranitvenega programa ne razume. Programa ni mogo�e odstraniti
UninstallUnknownEntry=V dnevni�ki datoteki je bil najden neznani vpis (%1)
ConfirmUninstall=Ste prepri�ani, da �elite v celoti odstraniti program %1 in pripadajo�e komponente?
UninstallOnlyOnWin64=To namestitev je mogo�e odstraniti le v 64-bitni razli�ici MS Windows.
OnlyAdminCanUninstall=Ta program lahko odstrani le administrator.
UninstallStatusLabel=Po�akajte, da odstranim program %1 iz va�ega ra�unalnika.
UninstalledAll=Program %1 je bil uspe�no odstranjen iz va�ega ra�unalnika.
UninstalledMost=Odstranjevanje programa %1 je kon�ano.%n%nNekateri deli niso bili odstranjeni in jih lahko odstranite ro�no.
UninstalledAndNeedsRestart=Za dokon�anje odstranitve programa %1 morate ra�unalnik znova zagnati.%n%nAli ga �elite znova zagnati zdaj?
UninstallDataCorrupted=Datoteka "%1" je okvarjena. Odstranitev ni mo�na

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=�elite odstraniti datoteko v skupni rabi?
ConfirmDeleteSharedFile2=Spodaj izpisane datoteke v skupni rabi ne uporablja ve� noben program. �elite odstraniti to datoteko?%n%n�e jo uporablja katerikoli program in jo boste odstranili, tak program verjetno ne bo ve� deloval pravilno. �e niste prepri�ani, kliknite Ne. �e boste datoteko ohranili v ra�unalniku, ne bo ni� narobe.
SharedFileNameLabel=Ime datoteke:
SharedFileLocationLabel=Lokacija:
WizardUninstalling=Odstranjevanje programa
StatusUninstalling=Odstranjujem %1...

[CustomMessages]

NameAndVersion=%1 razli�ica %2
AdditionalIcons=Dodatne ikone:
CreateDesktopIcon=Ustvari ikono na &namizju
CreateQuickLaunchIcon=Ustvari ikono za &hitri zagon
ProgramOnTheWeb=%1 na spletu
UninstallProgram=Odstrani %1
LaunchProgram=Odpri %1
AssocFileExtension=&Pove�i %1 s pripono %2
AssocingFileExtension=Povezujem %1 s pripono %2...
