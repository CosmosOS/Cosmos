; *** Inno Setup version 5.1.0+ Croatian messages ***
; Translated by: Krunoslav Kanjuh (krunoslav.kanjuh@zg.t-com.hr)


[LangOptions]
LanguageName=Hrvatski
LanguageID=$041a
LanguageCodePage=1250
[Messages]

; If the language you are translating to requires special font faces or
; sizes, uncomment any of the following entries and change them accordingly.
;DialogFontName=MS Shell Dlg
;DialogFontSize=8
;DialogFontStandardHeight=13
;TitleFontName=Arial
;TitleFontSize=29
;WelcomeFontName=Arial
;WelcomeFontSize=12
;CopyrightFontName=Arial
;CopyrightFontSize=8
; *** Application titles
SetupAppTitle=Instalacija
SetupWindowTitle=Instalacija - %1
UninstallAppTitle=Deinstalacija
UninstallAppFullTitle=Deinstalacija %1

; *** Misc. common
InformationTitle=Informacija
ConfirmTitle=Potvrda
ErrorTitle=Gre�ka

; *** SetupLdr messages
SetupLdrStartupMessage=Zapo�eli ste instalaciju programa %1. �elite li nastaviti?
LdrCannotCreateTemp=Ne mogu kreirati privremenu datoteku. Instalacija prekinuta
LdrCannotExecTemp=Ne mogu izvr�iti datoteku u privremenom direktoriju. Instalacija prekinuta

; *** Startup error messages
LastErrorMessage=%1.%n%nGre�ka %2: %3
SetupFileMissing=Datoteka %1 se ne nalazi u instalacijskom direktoriju. Molimo vas rije�ite problem ili nabavite novu kopiju programa.
SetupFileCorrupt=Instalacijske datoteke sadr�e gre�ku. Nabavite novu kopiju programa.
SetupFileCorruptOrWrongVer=Instalacijske datoteke sadr�e gre�ku, ili nisu kompatibilne sa ovom verzijom instalacije. Molimo vas rije�ite problem ili nabavite novu kopiju programa.
NotOnThisPlatform=Ovaj program ne�e raditi na %1.
OnlyOnThisPlatform=Ovaj program se mora pokrenuti na %1.
OnlyOnTheseArchitectures=Ovaj program mo�e biti instaliran na verziji Windowsa dizajniranim za sljede�u procesorsku arhitekturu:%n%n%1
MissingWOW64APIs=Ova verzija Windowsa ne posjeduje funkcije koje zahtjeva instalacija za 64-bitnu instalaciju. Da bi rije�ili problem molim instalirajte Service Pack %1
WinVersionTooLowError=Ovaj program zahtjeva %1 verziju %2 ili noviju.
WinVersionTooHighError=Ovaj program ne mo�e biti instaliran na %1 verziji %2 ili novijoj.
AdminPrivilegesRequired=Morate imati administratorske privilegije prilikom pokretanja ovog programa.
PowerUserPrivilegesRequired=Morate imati administratorske privilegije ili �lan grupe Power Users prilikom instaliranja ovog programa.
SetupAppRunningError=Instalacija je otkrila da je %1 pokrenut.%n%nMolimo zatvorite program i sve njegove kopije i potom kliknite Dalje za nastavak ili Odustani za prekid.
UninstallAppRunningError=Deinstalacija je otkrila da je %1 pokrenut.%n%nMolimo zatvorite program i sve njegove kopije i potom kliknite Dalje za nastavak ili Odustani za prekid.

; *** Misc. errors
ErrorCreatingDir=Instalacija nije mogla kreirati direktorij "%1"
ErrorTooManyFilesInDir=Instalacija nije mogla kreirati datoteku u direktoriju "%1" zato �to on sadr�i previ�e datoteka.

; *** Setup common messages
ExitSetupTitle=Prekid instalacije
ExitSetupMessage=Instalacija nije izvr�ena. Ako sad iza�ete, program ne�e biti instaliran.%n%nInstalaciju mo�ete pokrenuti kasnije ukoliko �elite zavr�iti instalaciju.%n%nPrekid instalacije?
AboutSetupMenuItem=&O instalaciji...
AboutSetupTitle=O instalaciji
AboutSetupMessage=%1 verzija %2%n%3%n%n%1 home page:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< Na&zad
ButtonNext=&Nastavak >
ButtonInstall=&Instaliraj
ButtonOK=U redu
ButtonCancel=Otka�i
ButtonYes=&Da
ButtonYesToAll=Da za &sve
ButtonNo=&Ne
ButtonNoToAll=N&e za sve
ButtonFinish=&Zavr�i
ButtonBrowse=&Odaberi...
ButtonWizardBrowse=&Odaberi...
ButtonNewFolder=&Kreiraj novi direktorij

; *** "Select Language" dialog messages
SelectLanguageTitle=Izaberite jezik instalacije
SelectLanguageLabel=Izberite jezik koji �elite koristiti pri instalaciji:

; *** Common wizard text
ClickNext=Kliknite na Nastavak za nastavak ili Otka�i za prekid instalacije.
BeveledLabel=
BrowseDialogTitle=Odabir direktorija
BrowseDialogLabel=Odaberi direktorij iz liste koja slijedi te klikni na OK.
NewFolderName=Novi direktorij

; *** "Welcome" wizard page
WelcomeLabel1=Dobro do�li u instalaciju programa [name].
WelcomeLabel2=Ovaj program �e instalirati [name/ver] na va�e ra�unalo.%n%nPreporu�amo da zatvorite sve programe prije nego nastavite dalje.

; *** "Password" wizard page
WizardPassword=Lozinka
PasswordLabel1=Instalacija je za�ti�ena lozinkom.
PasswordLabel3=Upi�ite lozinku. Lozinke su osjetljive na mala i velika slova.
PasswordEditLabel=&Lozinka:
IncorrectPassword=Upisana je pogre�na lozinka. Poku�ajte ponovo.

; *** "License Agreement" wizard page
WizardLicense=Ugovor o kori�tenju
LicenseLabel=Molimo vas, prije nastavka, pa�ljivo pro�itajte slijede�e va�ne informacije.
LicenseLabel3=Molimo vas, pa�ljivo pro�itajte Ugovor o kori�tenju. Morate prihvatiti uvjete ugovora kako bi mogli nastaviti s instalacijom.
LicenseAccepted=&Prihva�am ugovor
LicenseNotAccepted=&Ne prihva�am ugovor

; *** "Information" wizard pages
WizardInfoBefore=Informacija
InfoBeforeLabel=Molimo vas, pro�itajte sljede�e va�ne informacije prije nastavka.
InfoBeforeClickLabel=Kada budete spremni nastaviti instalaciju odaberite Nastavak.
WizardInfoAfter=Informacija
InfoAfterLabel=Molimo vas, pro�itajte sljede�e va�ne informacije prije nastavka.
InfoAfterClickLabel=Kada budete spremni nastaviti instalaciju odaberite Nastavak.

; *** "User Information" wizard page
WizardUserInfo=Informacije o korisniku
UserInfoDesc=Upi�ite informacije o vama.
UserInfoName=&Ime korisnika:
UserInfoOrg=&Organizacija:
UserInfoSerial=&Serijski broj:
UserInfoNameRequired=Morate upisati ime.

; *** "Select Destination Directory" wizard page
WizardSelectDir=Odaberite odredi�ni direktorij
SelectDirDesc=Direktorij gdje �e biti instaliran program.
SelectDirLabel3=Instalacija �e instalirati [name] u sljede�i direktorij
SelectDirBrowseLabel=Da bi nastavio klikni na Nastavak. Ako �eli� odabrati drugi direktorij klikni na odabir.
DiskSpaceMBLabel=Ovaj program zahtjeva bar [mb] MB slobodnog prostora na disku.
ToUNCPathname=Instalacija ne mo�e instalirati na UNC datoteku. Ako poku�avate instalirati na mre�u, morate mapirati mre�ni disk.
InvalidPath=Morate unijeti punu stazu zajedno sa slovom diska; npr:%nC:\APP
InvalidDrive=Disk koji ste odabrali ne postoji. Odaberite neki drugi.
DiskSpaceWarningTitle=Nedovoljno prostora na disku
DiskSpaceWarning=Instalacija zahtjeva bar %1 KB slobodnog prostora, a odabrani disk ima samo %2 KB na raspolaganju.%n%nDa li �elite nastaviti?
DirNameTooLong=Predugi naziv direktorija ili staze.
InvalidDirName=Naziv direktorija je pogre�an.
BadDirName32=Naziv direktorija ne smije sadr�avati niti jedan od sljede�ih znakova nakon to�ke:%n%n%1
DirExistsTitle=Direktorij ve� postoji
DirExists=Direktorij:%n%n%1%n%nve� postoji. �elite li svejedno instalirati u njega?
DirDoesntExistTitle=Direktorij ne postoji
DirDoesntExist=Direktorij:%n%n%1%n%nne postoji. �elite li ga napraviti?

; *** "Select Components" wizard page
WizardSelectComponents=Odaberite komponente
SelectComponentsDesc=Koje komponente �elite instalirati?
SelectComponentsLabel2=Odaberite komponente koje �elite instalirati ili uklonite kva�icu uz komponente koje ne �elite:
FullInstallation=Puna instalacija

; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Kompakt (minimalna) instalacija
CustomInstallation=Instalacija prema �elji
NoUninstallWarningTitle=Postoje�e komponente
NoUninstallWarning=Instalacija je utvrdila da na va�em ra�unalu ve� postoje slijede�e komponente:%n%n%1%n%nNeodabir tih komponenata ne dovodi do njihove deinstalacije.%n%n�elite li ipak nastaviti?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Va� izbor zahtijeva najmanje [mb] MB prostora na disku.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Odaberite radnje
SelectTasksDesc=Koje radnje �elite da se izvr�e?
SelectTasksLabel2=Odaberite radnje koje �e se izvr�iti tokom instalacije programa [name]:

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Odaberite programsku grupu
SelectStartMenuFolderDesc=Lokacija pre�ice programa
SelectStartMenuFolderLabel3=Instalacija �e kreirati pre�ice za programe u sljede�em Start Menu direktoriju
SelectStartMenuFolderBrowseLabel=Da bi nastavio klikni na Nastavak. Ako �eli� odabrati drugi direktorij klikni na odabir.
MustEnterGroupName=Morate unijeti ime programske grupe.
GroupNameTooLong=Predugi naziv direktorija ili staze.
InvalidGroupName=Naziv direktorija je pogre�an.
BadGroupName=Ime programske grupe ne smije sadr�avati sljede�e znakove:%n%n%1
NoProgramGroupCheck2=&Ne kreiraj %1 programsku grupu

; *** "Ready to Install" wizard page
WizardReady=Spreman za instalaciju
ReadyLabel1=Instalacija je spremna instalirati [name] na va�e ra�unalo.
ReadyLabel2a=Kliknite na Instaliraj ako �elite instalirati program ili na Nazad ako �elite pregledati ili promjeniti postavke.
ReadyLabel2b=Kliknite na Instaliraj ako �elite instalirati program.
ReadyMemoUserInfo=Informacije o korisniku:
ReadyMemoDir=Odredi�ni direktorij:
ReadyMemoType=Tip instalacije:
ReadyMemoComponents=Odabrane komponente:
ReadyMemoGroup=Programska grupa:
ReadyMemoTasks=Dodatni zadaci:

; *** "Preparing to Install" wizard page
WizardPreparing=Pripremam instalaciju
PreparingDesc=Instalacija se priprema da instalira [name] na va�e ra�unalo.
PreviousInstallNotCompleted=Instalacija/deinstalacija prethodnog programa nije zavr�ena. Morate restartati ra�unalo kako bi zavr�ili tu instalaciju.%n%nNakon restartanja ra�unala, ponovno pokrenite Setup kako bi dovr�ili instalaciju [name].
CannotContinue=Instalacija ne mo�e nastaviti. Molimo kliknite na Odustani za izlaz.

; *** "Installing" wizard page
WizardInstalling=Instaliram
InstallingLabel=Pri�ekajte dok ne zavr�i instalacija programa [name] na va�e ra�unalo.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Zavr�avam instalaciju [name]
FinishedLabelNoIcons=Instalacija programa [name] je zavr�ena.
FinishedLabel=Instalacija programa [name] je zavr�ena. Program mo�ete pokrenuti preko instaliranih ikona.
ClickFinish=Kliknite na Zavr�i da biste iza�li iz instalacije.
FinishedRestartLabel=Da biste instalacija programa [name] zavr�ili, potrebno je ponovno pokrenuti ra�unalo. Da li �elite to sada u�initi?
FinishedRestartMessage=Zavr�itak instalacija programa [name], zahtijeva ponovno pokretanje ra�unala.%n%nDa li �elite to u�initi sada?
ShowReadmeCheck=Da, �elim pro�itati README datoteku.
YesRadio=&Da, �elim sada ponovno pokrenuti ra�unalo
NoRadio=&Ne, kasnije �u ga ponovno pokrenuti

; used for example as 'Run MyProg.exe'
RunEntryExec=Pokreni %1

; used for example as 'View Readme.txt'
RunEntryShellExec=Pogledati %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Instalacija treba sljede�i disk
SelectDiskLabel2=Umetnite disketu %1 i kliknite na OK.%n%nAko se datoteke s ove diskete nalaze na nekom drugom mediju %2 , upi�ite ispravnu stazu do njega ili kliknite na Odaberi.
PathLabel=&Staza:
FileNotInDir2=Datoteka "%1" ne postoji u "%2". Molimo vas ubacite odgovoraju�i disk ili odaberete drugi %3.
SelectDirectoryLabel=Molimo vas odaberite lokaciju sljede�eg diska.

; *** Installation phase messages
SetupAborted=Instalacija nije zavr�ena.%n%nMolimo vas, rije�ite problem i opet pokrenite instalaciju.
EntryAbortRetryIgnore=Kliknite na Ponovi da poku�ate opet, Ignoriraj da nastavite, ili Prekid za prekid instalacije.

; *** Installation status messages
StatusCreateDirs=Kreiram direktorije...
StatusExtractFiles=Izdvajam datoteke...
StatusCreateIcons=Kreiram ikone...
StatusCreateIniEntries=Kreiram INI datoteke...
StatusCreateRegistryEntries=Kreiram podatke za registry...
StatusRegisterFiles=Registriram datoteke...
StatusSavingUninstall=Snimam deinstalacijske informacije...
StatusRunProgram=Zavr�avam instalaciju...
StatusRollback=Poni�tavam promjene...

; *** Misc. errors
ErrorInternal2=Interna gre�ka: %1
ErrorFunctionFailedNoCode=%1 nije uspjelo
ErrorFunctionFailed=%1 nije uspjelo; code %2
ErrorFunctionFailedWithMessage=%1 nije uspjelo; code %2.%n%3
ErrorExecutingProgram=Ne mogu izvr�iti datoteku:%n%1

; *** Registry errors
ErrorRegOpenKey=Gre�ka pri otvaranju registry klju�a:%n%1\%2
ErrorRegCreateKey=Gre�ka pri kreiranju registry klju�a:%n%1\%2
ErrorRegWriteKey=Gre�ke pri pisanju u registry klju�:%n%1\%2

; *** INI errors
ErrorIniEntry=Greska pri kreiranju INI podataka u datoteci "%1".

; *** File copying errors
FileAbortRetryIgnore=Kliknite Ponovi da poku�ate ponovo, Ignoriraj da presko�ite ovu datoteku (ne preporu�a se), ili Prekid za prekid instalacije.
FileAbortRetryIgnore2=Kliknite Ponovi da poku�ate ponovno, Ignoriraj da nastavite u svakom slu�aju (ne preporu�a se), ili Prekid za prekid instalacije
SourceIsCorrupted=Izvori�na datoteka je o�te�ena
SourceDoesntExist=Izvori�na datoteka "%1" ne postoji
ExistingFileReadOnly=Postoje�a datoteka je ozna�ena "samo-za-�itanje".%n%nKliknite Ponovi da uklonite oznaku "samo-za-�itanje" i poku�ajte ponovno, ignoriraj da presko�ite ovu datoteku, ili Prekid da prekinete instalaciju.
ErrorReadingExistingDest=Pojavila se gre�ka prilikom poku�aja �itanja postoje�e datoteke:
FileExists=Datoteka ve� postoji.%n%n�elite li pisati preko nje?
ExistingFileNewer=Postoje�a datoteka je novija od one koju poku�avate instalirati. Preporu�a se zadr�ati postoje�u datoteku.%n%n�elite li zadr�ati postoje�u datoteku?
ErrorChangingAttr=Pojavila se gre�ka prilikom poku�aja promjene atributa postoje�e datoteke:
ErrorCreatingTemp=Pojavila se gre�ka prilikom poku�aja kreiranja datoteke u odredi�nom direktoriju:
ErrorReadingSource=Pojavila se gre�ka prilikom poku�aja �itanja izvori�ne datoteke:
ErrorCopying=Pojavila se gre�ka prilikom poku�aja kopiranja datoteke:
ErrorReplacingExistingFile=Pojavila se gre�ka prilikom poku�aja zamjene datoteke:
ErrorRestartReplace=Zamjena nakon ponovnog pokretanja nije uspjela:
ErrorRenamingTemp=Pojavila se gre�ka prilikom poku�aja preimenovanja datoteke u odredisnom direktoriju:
ErrorRegisterServer=Ne mogu registrirati DLL/OCX: %1
ErrorRegisterServerMissingExport=DllRegisterServer export nije prona�en
ErrorRegisterTypeLib=Ne mogu registrirati type library: %1

; *** Post-installation errors
ErrorOpeningReadme=Pojavila se gre�ka prilikom poku�aja otvaranja README datoteke.
ErrorRestartingComputer=Instalacija ne mo�e ponovno pokrenuti ra�unalo. Molimo vas, u�inite to ru�no.

; *** Uninstaller messages
UninstallNotFound=Datoteka "%1" ne postoji. Deinstalacija prekinuta.
UninstallOpenError=Datoteku "%1" ne mogu otvoriti. Deinstalacija nije mogu�a
UninstallUnsupportedVer=Deinstalacijska datoteka "%1" je u formatu koji nije propoznat od ove verzije deinstalera. Nije mogu�a deinstalacija
UninstallUnknownEntry=Nepoznat zapis (%1) je prona�en u deinstalacijskoj datoteci
ConfirmUninstall=Da li ste sigurni da �elite ukloniti %1 i sve njegove komponente?
UninstallOnlyOnWin64=Ova instalacija mo�e biti uklonjena samo na 64-bitnim Windows-ima
OnlyAdminCanUninstall=Ova instalacija mo�e biti uklonjena samo od korisnika sa administratorskim privilegijama.
UninstallStatusLabel=Pri�ekajte dok %1 ne bude uklonjen s va�eg ra�unala.
UninstalledAll=Program %1 je uspje�no uklonjen sa va�eg ra�unala.
UninstalledMost=Deinstalacija programa %1 je zavr�ena.%n%nNeke elemente nije bilo mogu�e ukloniti. Molimo vas da to u�inite ru�no.
UninstalledAndNeedsRestart=Da bi zavr�ili deinstalaciju %1, Va�e ra�unalo morate ponovno pokrenuti%n%n�elite li to u�initi sada? 
UninstallDataCorrupted="%1" datoteka je o�te�ena. Deinstalacija nije mogu�a.

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Brisanje zajedni�ke datoteke?
ConfirmDeleteSharedFile2=Sistem ukazuje da sljede�e zajedni�ke datoteke ne koristi niti jedan program. �elite li da Deintalacija ukloni te zajedni�ke datoteke?%n%nAko neki programi i nadalje koriste te datoteke, a one se obri�u, ti programi ne�e ipravno raditi. Ako niste sigurni, odaberite Ne. Ostavljanje datoteka ne�e uzrokovati �tetu va�em sistemu.
SharedFileNameLabel=Datoteka:
SharedFileLocationLabel=Staza:
WizardUninstalling=Deinstalacija
StatusUninstalling=Deinstaliram %1...
[CustomMessages]
NameAndVersion=%1 verzija %2
AdditionalIcons=Dodatne ikone:
CreateDesktopIcon=Kreiraj ikonu na &desktopu
CreateQuickLaunchIcon=Kreiraj ikonu u Quick Launch izborniku
ProgramOnTheWeb=%1 je na Web-u
UninstallProgram=Deinstaliraj %1
LaunchProgram=Pokreni %1
AssocFileExtension=Pridru&�i %1 sa %2 ekstenzijom datoteke
AssocingFileExtension=Pridru�ujem %1 sa %2 ekstenzijom datoteke
