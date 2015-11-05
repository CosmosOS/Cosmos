;Stefan Bodingh
[LangOptions]

;*** Inno Setup version 5.1.0+ Swedish messages ***
;
; Translated by stefan@bodingh.se
;
;
LanguageName=Svenska
LanguageID=$041D
LanguageCodePage=0
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
SetupAppTitle=Installationsprogram
SetupWindowTitle=Installationsprogram f�r %1
UninstallAppTitle=Avinstallation
UninstallAppFullTitle=%1 Avinstallation

; *** Misc. common
InformationTitle=Information
ConfirmTitle=Bekr�fta
ErrorTitle=Fel

; *** SetupLdr messages
SetupLdrStartupMessage=%1 kommer att installeras. Vill du forts�tta?
LdrCannotCreateTemp=Kan ej skapa en tempor�r fil. Installationen avbryts
LdrCannotExecTemp=Kan inte k�ra fil i tempor�r mapp. Installationen avbryts

; *** Startup error messages
LastErrorMessage=%1.%n%nFel %2: %3
SetupFileMissing=Filen %1 saknas i installationsmappen. R�tta till problemet eller h�mta en ny kopia av programmet.
SetupFileCorrupt=Installationsfilerna �r felaktiga. H�mta en ny kopia av programmet
SetupFileCorruptOrWrongVer=Installationsfilerna �r felaktiga, eller st�mmer ej �verens med denna version av installationsprogrammet. R�tta till felet eller h�mta en ny programkopia.
NotOnThisPlatform=Detta program kan ej k�ras p� %1.
OnlyOnThisPlatform=Detta program m�ste ha %1.
OnlyOnTheseArchitectures=Detta program kan bara installeras p� Windows versioner med f�ljande processor arkitekturer:%n%n%1
MissingWOW64APIs=Den versionen av Windows du k�r har inte den funktionalitet installationsprogrammet beh�ver f�r att genomf�ra en 64-bitars installation. R�tta till problemet genom att installera Service Pack %1.
WinVersionTooLowError=Detta program kr�ver %1, version %2 eller senare.
WinVersionTooHighError=Programmet kan inte installeras p� %1 version %2 eller senare.
AdminPrivilegesRequired=Du m�ste vara inloggad som administrat�r n�r du installerar detta program.
PowerUserPrivilegesRequired=Du m�ste vara inloggad som administrat�r eller medlem av gruppen Privilegierade anv�ndare (Power Users) n�r du installerar  detta program.
SetupAppRunningError=Installationsprogrammet har uppt�ckt att %1 �r ig�ng.%n%nAvsluta det angivna programmet nu. Klicka sedan p� OK f�r att g� vidare, eller p� Avbryt f�r avslut.
UninstallAppRunningError=Avinstalleraren har uppt�ckt att %1 k�rs f�r tillf�llet.%n%nSt�ng all �ppna instanser av det nu, klicka sedan p� OK f�r att g� vidare, eller  Avbryt f�r att avsluta.

; *** Misc. errors
ErrorCreatingDir=Kunde inte skapa mappen "%1"
ErrorTooManyFilesInDir=Kunde inte skapa en fil i mappen "%1" d�rf�r att den inneh�ller f�r m�nga filer

; *** Setup common messages
ExitSetupTitle=Avsluta installationen
ExitSetupMessage=Installationen �r inte f�rdig. Om du avbryter nu, kommer programmet inte att installeras.%n%nDu kan k�ra installationsprogrammet  vid ett senare tillf�lle f�r att avsluta installationen.%n%nVill du avbryta installationen?
AboutSetupMenuItem=&Om installationsprogrammet...
AboutSetupTitle=Om installationsprogrammet
AboutSetupMessage=%1 version %2%n%3%n%n%1 hemsida:%n%4
AboutSetupNote=Svensk �vers�ttning �r gjord av dickg@go.to 1999, 2002%n%nUppdatering till 3.0.2+ av peter@peterandlinda.com, 4.+ av stefan@bodingh.se
TranslatorNote=

; *** Buttons
ButtonBack=< &Tillbaka
ButtonNext=&N�sta >
ButtonInstall=&Installera
ButtonOK=OK
ButtonCancel=Avbryt
ButtonYes=&Ja
ButtonYesToAll=Ja till &Allt
ButtonNo=&Nej
ButtonNoToAll=N&ej till allt
ButtonFinish=&Slutf�r
ButtonBrowse=&Bl�ddra...
ButtonWizardBrowse=&Bl�ddra...
ButtonNewFolder=Skapa ny mapp
SelectLanguageTitle=V�lj spr�k f�r installationen
SelectLanguageLabel=V�lj spr�k som skall anv�ndas under installationen:

; *** Common wizard text
ClickNext=Klicka p� N�sta f�r att forts�tta, eller Avbryt f�r att avsluta installationen.
BeveledLabel=
BrowseDialogTitle=Bl�ddra mapp
BrowseDialogLabel=V�lj en mapp i listan nedan, klicka sedan OK.
NewFolderName=Ny mapp

; *** "Welcome" wizard page
;WizardWelcome=V�lkommen
;WelcomeLabel1=V�lkommen till installationsprogrammet f�r [name].
;WelcomeLabel2=Detta kommer att installera [name/ver] p� din dator.%n%nDet rekommenderas att du avslutar alla andra program innan du forts�tter. Det f�rebygger konflikter under installationens g�ng.
; *** "Password" wizard page
WelcomeLabel1=V�lkommen till installationen av [name]
WelcomeLabel2=Installationsprogrammet kommer att installera [name/ver] p� din dator.
WizardPassword=L�senord
PasswordLabel1=Denna installation �r skyddad med l�senord.
PasswordLabel3=Var god ange l�senordet, klicka sedan p� N�sta f�r att forts�tta. L�senord skiljer p� versaler/gemener.
PasswordEditLabel=&L�senord:
IncorrectPassword=L�senordet du angav �r inkorrekt. F�rs�k igen.

; *** "License Agreement" wizard page
WizardLicense=Licensavtal
LicenseLabel=Var god och l�s f�ljande viktiga information innan du forts�tter.
LicenseLabel3=Var god och l�s f�ljande licensavtal. Du m�ste acceptera villkoren i avtalet innan du kan forts�tta med installationen.
LicenseAccepted=Jag &accepterar avtalet
LicenseNotAccepted=Jag accepterar &inte avtalet

; *** "Information" wizard pages
WizardInfoBefore=Information
InfoBeforeLabel=Var god l�s f�ljande viktiga information innan du forts�tter.
InfoBeforeClickLabel=N�r du �r klar att forts�tta med installationen, klicka p� N�sta.
WizardInfoAfter=Information
InfoAfterLabel=Var god l�s f�ljande viktiga information innan du forts�tter.
InfoAfterClickLabel=N�r du �r klar att forts�tta med installationen, klicka p� N�sta.

; *** "User Information" wizard page
WizardUserInfo=Anv�ndarinformation
UserInfoDesc=Var god och fyll i f�ljande uppgifter.
UserInfoName=&Namn:
UserInfoOrg=&Organisation:
UserInfoSerial=&Serienummer:
UserInfoNameRequired=Du m�ste fylla i ett namn.

; *** "Select Destination Directory" wizard page
WizardSelectDir=V�lj installationsplats
SelectDirDesc=Var skall [name] installeras?
SelectDirLabel3=Installationsprogrammet kommer att installera [name] i f�ljande mapp
SelectDirBrowseLabel=F�r att forts�tta, klicka p� N�sta. Om du vill v�lja en annan mapp, klicka p� Bl�ddra.
DiskSpaceMBLabel=Programmet kr�ver minst [mb] MB h�rddiskutrymme.
ToUNCPathname=Installationsprogrammet kan inte installera till UNC s�kv�g. Om du f�rs�ker installera via ett n�tverk, ska du ange  n�tverksenheten.
InvalidPath=Du m�ste skriva en fullst�ndig s�kv�g med enhetsbeteckning; till exempel:%C:\APP
InvalidDrive=Enheten du har valt finns inte eller �r inte tillg�nglig. V�lj en annan.
DiskSpaceWarningTitle=Ej tillr�ckligt med diskutrymme
DiskSpaceWarning=Installationsprogrammet beh�ver �tminstone %1 KB ledigt diskutrymme f�r installationen, men den valda enheten har bara %2  KB tillg�ngligt.%n%nVill du forts�tta �nd�?
DirNameTooLong=Mappens namn eller s�kv�g �r f�r l�ng.
InvalidDirName=Mappen du har valt �r inte tillg�nglig.
BadDirName32=Mappens namn f�r ej inneh�lla n�got av f�ljande tecken:%n%n%1
DirExistsTitle=Mappen finns
DirExists=Mappen:%n%n%1%n%nfinns redan. Vill du �nd� forts�tta installationen till den valda mappen?
DirDoesntExistTitle=Mappen finns inte
DirDoesntExist=Mappen:%n%n%1%n%nfinns inte. Vill du skapa den?

; *** "Select Components" wizard page
WizardSelectComponents=V�lj komponenter
SelectComponentsDesc=Vilka komponenter skall installeras?
SelectComponentsLabel2=V�lj de komponenter som du vill ska installeras; avmarkera de komponenter som du inte vill ha. Klicka p� N�sta n�r du �r klar att  forts�tta.
FullInstallation=Fullst�ndig installation

; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Kompakt installation
CustomInstallation=Anpassad installation
NoUninstallWarningTitle=Komponenter finns
NoUninstallWarning=Installationsprogrammet har uppt�ckt att f�ljande komponenter redan finns installerade p� din dator:%n%n%1%n%nAtt  avmarkera dessa komponenter kommer inte att avinstallera dom.%n%nVill du forts�tta i alla fall?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Aktuella val kr�ver minst [mb] MB diskutrymme.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=V�lj extra uppgifter
SelectTasksDesc=Vilka extra uppgifter skall utf�ras?
SelectTasksLabel2=Markera ytterligare uppgifter att utf�ra vid installation av [name], tryck sedan p� N�sta.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=V�lj Startmeny-mapp
SelectStartMenuFolderDesc=Var skall installationsprogrammet placera programmets genv�gar?
SelectStartMenuFolderLabel3=Installationsprogrammet kommer att skapa programmets genv�gar i f�ljande mapp.
SelectStartMenuFolderBrowseLabel=F�r att forts�tta, klicka p� N�sta. Om du vill v�lja en annan mapp, klicka p� Bl�ddra.
MustEnterGroupName=Du m�ste ange en mapp.
GroupNameTooLong=Mappens namn eller s�kv�g �r f�r l�ng.
InvalidGroupName=Mappen du har valt �r inte tillg�nglig.
BadGroupName=Mappnamnet kan inte inneh�lla n�gon av f�ljande tecken:%n%n%1
NoProgramGroupCheck2=&Skapa ingen Startmeny-mapp

; *** "Ready to Install" wizard page
WizardReady=Redo att installera
ReadyLabel1=Installationsprogrammet �r nu redo att installera [name] p� din dator.
ReadyLabel2a=Tryck p� Installera om du vill forts�tta, eller g� Tillbaka om du vill se �ver eller �ndra p� n�got.
ReadyLabel2b=V�lj Installera f�r att p�b�rja installationen.
ReadyMemoUserInfo=Anv�ndarinformation:
ReadyMemoDir=Installationsplats:
ReadyMemoType=Installationstyp:
ReadyMemoComponents=Valda komponenter:
ReadyMemoGroup=Startmeny-mapp:
ReadyMemoTasks=Extra uppgifter:

; *** "Preparing to Install" wizard page
WizardPreparing=F�rbereder installationen
PreparingDesc=Installationsprogrammet f�rbereder installationen av [name] p� din dator.
PreviousInstallNotCompleted=Installationen/avinstallationen av ett tidigare program har inte slutf�rts. Du m�ste starta om datorn f�r att avsluta den  installationen.%n%nEfter att ha startat om datorn k�r du installationsprogrammet igen f�r att slutf�ra installationen av [name].
CannotContinue=Installationsprogrammet kan inte forts�tta. Klicka p� Avbryt f�r att avsluta.

; *** "Installing" wizard page
WizardInstalling=Installerar
InstallingLabel=V�nta medan [name] installeras p� din dator.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Avslutar installationen av [name]
FinishedLabelNoIcons=[name] har nu installerats p� din dator.
FinishedLabel=[name] har nu installerats p� din dator. Programmet kan startas genom att v�lja n�gon av ikonerna.
ClickFinish=V�lj Slutf�r f�r att avsluta installationen.
FinishedRestartLabel=F�r att slutf�ra installationen av [name], m�ste datorn startas om. Vill du starta om nu?
FinishedRestartMessage=F�r att slutf�ra installationen av [name], m�ste datorn startas om.%n%nVill du starta om datorn nu?
ShowReadmeCheck=Ja, jag vill se filen L�S MIG
YesRadio=&Ja, jag vill starta om datorn nu
NoRadio=&Nej, jag startar sj�lv om datorn senare

; used for example as 'Run MyProg.exe'
RunEntryExec=K�r %1

; used for example as 'View Readme.txt'
RunEntryShellExec=L�s %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Installationsprogrammet beh�ver n�sta diskett
SelectDiskLabel2=Var god s�tt i diskett %1 och tryck OK.%n%nOm filerna kan hittas i en annan mapp �n den som visas nedan, skriv in r�tt s�kv�g eller v�lj Bl�ddra.
PathLabel=&S�kv�g:
FileNotInDir2=Kunde inte hitta filen "%1" i "%2". Var god s�tt i korrekt diskett eller v�lj en annan mapp.
SelectDirectoryLabel=Var god ange s�kv�gen f�r n�sta diskett.

; *** Installation phase messages
SetupAborted=Installationen slutf�rdes inte.%n%nVar god r�tta till felet och k�r installationen igen.
EntryAbortRetryIgnore=V�lj F�rs�k igen eller Ignorera f�r att forts�tta i alla fall, Avbryt f�r att avbryta installationen.

; *** Installation status messages
StatusCreateDirs=Skapar mappar...
StatusExtractFiles=Packar upp filer...
StatusCreateIcons=Skapar programikoner...
StatusCreateIniEntries=Skriver INI-v�rden...
StatusCreateRegistryEntries=Skriver register-v�rden...
StatusRegisterFiles=Registrerar filer...
StatusSavingUninstall=Sparar information f�r avinstallation...
StatusRunProgram=Slutf�r installationen...
StatusRollback=�terst�ller �ndringar...

; *** Misc. errors
ErrorInternal2=Internt fel: %1
ErrorFunctionFailedNoCode=%1 misslyckades
ErrorFunctionFailed=%1 misslyckades; kod %2
ErrorFunctionFailedWithMessage=%1 misslyckades; kod %2.%n%3
ErrorExecutingProgram=Kan inte k�ra filen:%n%1

; *** Registry errors
ErrorRegOpenKey=Fel vid �ppning av registernyckel:%n%1\%2
ErrorRegCreateKey=Kan ej skapa registernyckel:%n%1\%2
ErrorRegWriteKey=Kan ej skriva till registernyckel:%n%1\%2

; *** INI errors
ErrorIniEntry=Kan inte skriva nytt INI-v�rde i filen "%1".

; *** File copying errors
FileAbortRetryIgnore=V�lj F�rs�k igen eller Ignorera f�r hoppa �ver denna fil (ej rekommenderat), eller Avbryt installationen.
FileAbortRetryIgnore2=V�lj F�rs�k igen eller Ignorera och forts�tt i alla fall (ej rekommenderat), eller Avbryt installationen.
SourceIsCorrupted=K�llfilen �r felaktig
SourceDoesntExist=K�llfilen "%1" finns inte
ExistingFileReadOnly=Den nuvarande filen �r skrivskyddad.%n%nV�lj F�rs�k igen f�r att ta bort skrivskyddet, Ignorera f�r att hoppa �ver denna fil, eller Avbryt installationen.
ErrorReadingExistingDest=Ett fel uppstod vid f�rs�k att l�sa den befintliga filen:
FileExists=Filen finns redan.%n%nVill du skriva �ver den?
ExistingFileNewer=Den befintliga filen �r nyare �n den som ska installeras. Du rekommenderas att beh�lla den befintliga filen. %n%nVill Du beh�lla den befintliga filen?
ErrorChangingAttr=Ett fel uppstod vid f�rs�k att �ndra attribut p� den befintliga filen:
ErrorCreatingTemp=Ett fel uppstod vid ett f�rs�k att skapa installationsmappen:
ErrorReadingSource=Ett fel uppstod vid ett f�rs�k att l�sa k�llfilen:
ErrorCopying=Ett fel uppstod vid kopiering av filen:
ErrorReplacingExistingFile=Ett fel uppstod vid ett f�rs�k att ers�tta den befintliga filen:
ErrorRestartReplace=RestartReplace misslyckades:
ErrorRenamingTemp=Ett fel uppstod vid ett f�rs�k att byta namn p� en fil i installationsmappen:
ErrorRegisterServer=Kunde inte registrera DLL/OCX: %1
ErrorRegisterServerMissingExport=DllRegisterServer export kunde inte hittas
ErrorRegisterTypeLib=Kunde inte registrera typbibliotek: %1

; *** Post-installation errors
ErrorOpeningReadme=Ett fel uppstod vid �ppnandet av L�S MIG-filen.
ErrorRestartingComputer=Installationsprogrammet kunde inte starta om datorn. Var god g�r det manuellt.

; *** Uninstaller messages
UninstallNotFound=Filen "%1" finns inte. Kan inte avinstallera.
UninstallOpenError=Filen "%1" kan inte �ppnas. Kan inte avinstallera.
UninstallUnsupportedVer=Avinstallationsloggen "%1" �r i ett format som denna version inte k�nner igen. Kan ej avinstallera
UninstallUnknownEntry=En ok�nd rad (%1) hittades i avinstallationsloggen
ConfirmUninstall=�r du s�ker p� att du vill ta bort %1 och alla tillh�rande komponenter?
UninstallOnlyOnWin64=Denna installation kan endast avinstalleras p� en 64-bitarsversion av Windows.
OnlyAdminCanUninstall=Denna installation kan endast avinstalleras av en anv�ndare med administrativa r�ttigheter.
UninstallStatusLabel=Var god och v�nta medan %1 tas bort fr�n din dator.
UninstalledAll=%1 �r nu borttaget fr�n din dator.
UninstalledMost=Avinstallationen av %1 �r nu klar.%n%nEn del filer/mappar gick ej att ta bort. Dessa kan tas bort manuellt.
UninstalledAndNeedsRestart=F�r att slutf�ra avinstallationen av %1 m�ste datorn startas om.%n%nVill du starta om nu?
UninstallDataCorrupted=Filen "%1" �r felaktig. Kan inte avinstallara

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Ta bort delad fil?
ConfirmDeleteSharedFile2=Systemet indikerar att f�ljande delade fil inte l�ngre anv�nds av n�gra program. Vill du ta bort den delade filen?%n%n%1%n%nOm n�got program fortfarande anv�nder denna fil och den raderas, kommer programmet kanske att sluta fungera. Om du �r os�ker, v�lj Nej. Att l�ta filen ligga kvar i systemet kommer inte att orsaka n�gon skada.
SharedFileNameLabel=Filnamn:
SharedFileLocationLabel=Plats:
WizardUninstalling=Avinstallationsstatus
StatusUninstalling=Avinstallerar %1...
[CustomMessages]
NameAndVersion=%1 version %2
AdditionalIcons=�terst�ende ikoner:
CreateDesktopIcon=Skapa en ikon p� skrivbordet
CreateQuickLaunchIcon=Skapa en ikon i Snabbstartf�ltet
ProgramOnTheWeb=%1 p� Webben
UninstallProgram=Avinstallera %1
LaunchProgram=Starta %1
AssocFileExtension=&Associera %1 med %2 filnamnstill�gg
AssocingFileExtension=Associerar %1 med %2 filnamnstill�gg...
