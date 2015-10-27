; *** Inno Setup version 5.1.0+ Spanish messages ***
;
; To download user-contributed translations of this file, go to:
;   http://www.jrsoftware.org/is3rdparty.php
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).
;
; $jrsoftware: issrc/Files/Default.isl,v 1.66 2005/02/25 20:23:48 mlaan Exp $
; Translated by Emiliano Llano D�az
; emiliano_d@hotmail.com

[LangOptions]
LanguageName=Espa<00f1>ol
LanguageID=$0c0a
LanguageCodePage=0
; If the language you are translating to requires special font faces or
; sizes, uncomment any of the following entries and change them accordingly.
;DialogFontName=verdana
;DialogFontSize=8
;WelcomeFontName=Verdana
;WelcomeFontSize=12
;TitleFontName=Arial
;TitleFontSize=29
;CopyrightFontName=Arial
;CopyrightFontSize=8

[Messages]

; *** Application titles
SetupAppTitle=Instalar
SetupWindowTitle=Instalar - %1
UninstallAppTitle=Eliminar
UninstallAppFullTitle=Eliminar - %1

; *** Misc. common
InformationTitle=Informaci�n
ConfirmTitle=Confirmar
ErrorTitle=Error

; *** SetupLdr messages
SetupLdrStartupMessage=Este programa instalar� %1 en su sistema. �Desea continuar?
LdrCannotCreateTemp=Imposible crear un archivo temporal. Instalaci�n cancelada
LdrCannotExecTemp=Imposible ejecutar archivo en el directorio temporal. Instalaci�n interrumpida

; *** Startup error messages
LastErrorMessage=%1.%n%nError %2: %3
SetupFileMissing=El archivo %1 no se encuentra en la carpeta de instalaci�n. Por favor corrija el problema u obtenga una nueva copia del programa.
SetupFileCorrupt=Los archivos de instalaci�n est�n da�ados. Por favor obtenga una copia nueva del programa.
SetupFileCorruptOrWrongVer=Los archivos de instalaci�n est�n da�ados, o son incompatibles con esta versi�n del Instalador. Por favor corrija el problema u obtenga una nueva copia del programa.
NotOnThisPlatform=Este programa no funcionar� en un sistema %1.
OnlyOnThisPlatform=Este programa debe ejecutarse en un sistema %1.
OnlyOnTheseArchitectures=Este programa s�lo puede instalarse en versiones de Windows dise�adas para las siguientes arquitecturas de procesadores:%n%n%1
MissingWOW64APIs=La versi�n de Windows que usa no tiene la funcionalidad requerida para realizar instalaciones de 64 bits. Para corregir este problema, por favor instale el Service Pack %1.
WinVersionTooLowError=Este programa requiere %1 versi�n %2 o posterior.
WinVersionTooHighError=Este programa no puede instalarse en %1 versi�n %2 o posterior.
AdminPrivilegesRequired=Debe iniciar la sesi�n como un administrador cuando instale este programa.
PowerUserPrivilegesRequired=Debe ser el administrador o equivalente para poder instalar este programa.
SetupAppRunningError=El Instalador ha detectado que %1 est� en ejecuci�n.%n%nPor favor cierre todas sus instancias, luego seleccione Aceptar para continuar, o Cancelar para salir.
UninstallAppRunningError=El Instalador ha detectado que %1 est� en ejecuci�n.%n%nPor favor cierre todas sus instancias, luego seleccione Aceptar para continuar, o Cancelar para salir.

; *** Misc. errors
ErrorCreatingDir=Imposible crear la carpeta "%1"
ErrorTooManyFilesInDir=Imposible crear un archivo en la carpeta "%1" porque contiene demasiados archivos

; *** Setup common messages
ExitSetupTitle=Salir de la Instalaci�n
ExitSetupMessage=La instalaci�n no se complet�. Si abandona ahora, el programa no se instalar�.%n%nPodr� ejecutar de nuevo este programa para completar el proceso.%n%n�Salir de la Instalaci�n?
AboutSetupMenuItem=&Acerca del Instalador...
AboutSetupTitle=Acerca del Instalador
AboutSetupMessage=%1 versi�n %2%n%3%n%n%1 p�gina Web:%n%4
AboutSetupNote=
TranslatorNote=


; *** Buttons
ButtonBack=< &Atr�s
ButtonNext=&Siguiente >
ButtonInstall=&Instalar
ButtonOK=Aceptar
ButtonCancel=Cancelar
ButtonYes=&S�
ButtonYesToAll=S� a &Todo
ButtonNo=&No
ButtonNoToAll=N&o a Todo
ButtonFinish=&Terminar
ButtonBrowse=&Examinar...
ButtonWizardBrowse=E&xaminar...
ButtonNewFolder=&Crear nueva carpeta


; *** "Select Language" dialog messages
SelectLanguageTitle=Seleccione el idioma de la instalaci�n
SelectLanguageLabel=Seleccione el idioma que desea usar durante la instalaci�n:

; *** Common wizard text
ClickNext=Haga clic sobre Siguiente para continuar o sobre Cancelar para salir.
BeveledLabel=
BrowseDialogTitle=Buscar carpeta
BrowseDialogLabel=Seleccione una carpeta de la lista y luego haga clic en Aceptar.
NewFolderName=Nueva carpeta

; *** "Welcome" wizard page
WelcomeLabel1=Bienvenido a la instalaci�n de [name].
WelcomeLabel2=Este programa instalar� [name/ver] en su computadora.%n%nSe recomienda cerrar todos los programas en ejecuci�n antes de continuar.  Esto ayudar� a prevenir conflictos durante el proceso.

; *** "Password" wizard page
WizardPassword=Contrase�a
PasswordLabel1=Esta instalaci�n est� protegida por contrase�a.
PasswordLabel3=Por favor suministre su contrase�a.%n%nUse may�sculas y min�sculas.
PasswordEditLabel=&Contrase�a:
IncorrectPassword=La contrase�a suministrada no es correcta. Por favor intente de nuevo.

; *** "License Agreement" wizard page
WizardLicense=Acuerdo de Licencia
LicenseLabel=Por favor lea la siguiente informaci�n importante antes de continuar.
LicenseLabel3=Por favor lea el siguiente Acuerdo de Licencia.  Use la barra de desplazamiento o presione la tecla Av P�g para ver el resto de la licencia.
LicenseAccepted=&Acepto los t�rminos del Acuerdo
LicenseNotAccepted=&No acepto los t�rminos del Acuerdo

; *** "Information" wizard pages
WizardInfoBefore=Informaci�n
InfoBeforeLabel=Por favor lea la siguiente informaci�n importante antes de continuar.
InfoBeforeClickLabel=Cuando est� listo para continuar con la instalaci�n, haga clic sobre el bot�n Siguiente.
WizardInfoAfter=Informaci�n
InfoAfterLabel=Por favor lea la siguiente informaci�n importante antes de continuar.
InfoAfterClickLabel=Cuando est� listo para continuar, haga clic sobre el bot�n Siguiente.

; *** "User Information" wizard page
WizardUserInfo=Informaci�n de usuario
UserInfoDesc=Por favor proporcione su informaci�n.
UserInfoName=Nombre de &usuario:
UserInfoOrg=&Organizaci�n:
UserInfoSerial=N�mero de &Serie:
UserInfoNameRequired=Debe proporcionar su nombre.

; *** "Select Destination Directory" wizard page
WizardSelectDir=Seleccione la Carpeta Destino
SelectDirDesc=�En d�nde se instalar� [name]?
SelectDirLabel3=Se instalar� [name] en la siguiente carpeta.
SelectDirBrowseLabel=Para continuar haga clic en Siguiente. Si quiere seleccionar otra carpeta, haga clic en Examinar.
DiskSpaceMBLabel=Se requiere un m�nimo de [mb] MB de espacio en el disco.
ToUNCPathname=No se puede instalar en un directorio UNC. Si est� tratando de instalar en una red, necesitar� asignarlo a una unidad de red.
InvalidPath=Debe proporcionar una ruta completa con la letra de la unidad; por ejemplo:%nC:\APP
InvalidDrive=La unidad que seleccion� no existe. Por favor seleccione otra.
DiskSpaceWarningTitle=No hay espacio suficiente en el disco
DiskSpaceWarning=Se requiere al menos %1 KB de espacio libre para la instalaci�n, pero la unidad seleccionada solamente tiene %2 KB disponibles.%n%n�Desea continuar de todas formas?
DirNameTooLong=El nombre de la carpeta o ruta es demasiado largo.
InvalidDirName=El nombre de la carpeta no es v�lido.
BadDirName32=El nombre de una carpeta no puede incluir los siguientes caracteres:%n%n%1
DirExistsTitle=La carpeta ya existe
DirExists=La carpeta:%n%n%1%n%nya existe. �Desea instalar en esta carpeta de todas formas?
DirDoesntExistTitle=La carpeta no existe
DirDoesntExist=La carpeta:%n%n%1%n%n no existe. �Desea crear la carpeta?

; *** "Select Components" wizard page
WizardSelectComponents=Selecci�n de Componentes
SelectComponentsDesc=�Qu� componentes desea instalar?
SelectComponentsLabel2=Seleccione los componentes que desea instalar; desactive aquellos que no desea.  Haga clic sobre Siguiente cuando est� listo para continuar.
FullInstallation=Instalaci�n Completa
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Instalaci�n Compacta
CustomInstallation=Instalaci�n Personalizada
NoUninstallWarningTitle=Componentes Existentes
NoUninstallWarning=La Instalaci�n ha detectado que los siguientes componentes ya est�n instalados en su computadora:%n%n%1%n%nAl desactivarlos, no se instalar�n.%n%n�Desea continuar de todos modos?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=La selecci�n actual requiere al menos [mb] MB de espacio en disco.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Seleccione las Tareas Adicionales
SelectTasksDesc=�Qu� tareas adicionales deber�n ejecutarse?
SelectTasksLabel2=Seleccione las tareas adicionales que desea ejecutar mientras se instala [name], luego haga clic sobre el bot�n Siguiente.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Seleccione la carpeta del Men� de Inicio
SelectStartMenuFolderDesc=�En d�nde deber�n colocarse los iconos de acceso directo al programa?
SelectStartMenuFolderLabel3=La instalaci�n crear� los atajos del programa en el siguiente men� de Inicio.
SelectStartMenuFolderBrowseLabel=Para continuar haga clic en Siguiente. Si quiere cambiar el men� haga clic en Examinar.
MustEnterGroupName=Debe proporcionar un nombre de carpeta.
GroupNameTooLong=El nombre de la carpeta o ruta es demasiado largo.
InvalidGroupName=Nombre de carpeta inv�lido.
BadGroupName=El nombre de la carpeta no puede incluir ninguno de los siguientes caracteres:%n%n%1
NoProgramGroupCheck2=&No crear carpeta en el men� Inicio

; *** "Ready to Install" wizard page
WizardReady=Listo para Instalar
ReadyLabel1=El programa est� listo para iniciar la instalaci�n de [name] en su computadora.
ReadyLabel2a=Haga clic sobre Instalar para continuar con el proceso o sobre Atr�s si desea revisar o cambiar la configuraci�n.
ReadyLabel2b=Haga clic sobre Instalar para continuar con el proceso.
ReadyMemoUserInfo=Informaci�n del usuario:
ReadyMemoDir=Carpeta Destino:
ReadyMemoType=Tipo de Instalaci�n:
ReadyMemoComponents=Componentes Seleccionados:
ReadyMemoGroup=Carpeta del Men� de Inicio:
ReadyMemoTasks=Tareas adicionales:

; *** "Preparing to Install" wizard page
WizardPreparing=Prepar�ndose a Instalar 
PreparingDesc=El programa de instalaci�n est� prepar�ndose para copiar [name] en su computadora.
PreviousInstallNotCompleted=La instalaci�n/ eliminaci�n previa del programa no se complet�. Deber� reiniciar la computadora para completar el proceso.%n%nUna vez reiniciada la computadora ejecute de nuevo este programa para completar la instalaci�n de [name].
CannotContinue=No se pudo continuar con la instalaci�n. Haga clic sobre el bot�n Cancelar para salir.

; *** "Installing" wizard page
WizardInstalling=Instalando
InstallingLabel=Por favor espere mientras se instala [name] en su computadora.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Finalizando el asistente de instalaci�n de [name]
FinishedLabelNoIcons=El programa termin� la instalaci�n de [name] en su computadora.
FinishedLabel=El programa termin� la instalaci�n de [name] en su computadora.  El programa puede ejecutarse seleccionando el icono creado.
ClickFinish=Haga clic sobre Terminar para concluir la Instalaci�n.
FinishedRestartLabel=Para completar la instalaci�n de [name], debe reiniciar su computadora.  �Desea reiniciar ahora?
FinishedRestartMessage=Para completar la instalaci�n de [name], debe reiniciar su computadora.%n%n�Desea reiniciar ahora?
ShowReadmeCheck=S�, deseo ver el archivo LEAME
YesRadio=&S�, deseo reiniciar la computadora ahora
NoRadio=&No, reiniciar� la computadora m�s tarde
; used for example as 'Run MyProg.exe'
RunEntryExec=Ejecutar %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Ver %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=La Instalaci�n necesita el siguiente disco
SelectDiskLabel2=Por favor inserte el Disco %1 y haga clic sobre Aceptar.%n%nSi los archivos se localizan en una carpeta diferente a la mostrada abajo, proporcione la ruta correcta o haga clic sobre Examinar.
PathLabel=&Ruta:
FileNotInDir2=El archivo "%1" no se encuentra en "%2".  Por favor inserte el disco correcto o seleccione otra carpeta.
SelectDirectoryLabel=Por favor especifique la ubicaci�n del siguiente disco.

; *** Installation phase messages
SetupAborted=La instalaci�n no fue terminada.%n%nPor favor corrija el problema y ejecute Instalar de nuevo.
EntryAbortRetryIgnore=Haga clic sobre Reintentar para intentar de nuevo, Ignorar para proceder de cualquier forma o sobre Cancelar para interrumpir la instalaci�n.

; *** Installation status messages
StatusCreateDirs=Creando carpetas...
StatusExtractFiles=Copiando archivos...
StatusCreateIcons=Creando iconos del programa...
StatusCreateIniEntries=Creando entradas INI...
StatusCreateRegistryEntries=Creando entradas en el registro...
StatusRegisterFiles=Registrando archivos...
StatusSavingUninstall=Guardando informaci�n para eliminar el programa...
StatusRunProgram=Terminando la instalaci�n...
StatusRollback=Deshaciendo los cambios...

; *** Misc. errors
ErrorInternal2=Error Interno %1
ErrorFunctionFailedNoCode=%1 fall�
ErrorFunctionFailed=%1 fall�; c�digo %2
ErrorFunctionFailedWithMessage=%1 fall�; c�digo %2.%n%3
ErrorExecutingProgram=Imposible ejecutar el archivo:%n%1

; *** Registry errors
ErrorRegOpenKey=Error abriendo la clave de registro:%n%1\%2
ErrorRegCreateKey=Error creando la clave de registro:%n%1\%2
ErrorRegWriteKey=Error escribiendo en la clave de registro:%n%1\%2

; *** INI errors
ErrorIniEntry=Error creando entrada en archivo INI "%1".

; *** File copying errors
FileAbortRetryIgnore=Haga clic sobre Reintentar para probar de nuevo, Ignorar para omitir este archivo (no recomendado) o Cancelar para interrumpir la instalaci�n.
FileAbortRetryIgnore2=Haga clic sobre Reintentar para probar de nuevo, Ignorar para proceder de cualquier forma (no se recomienda) o Cancelar para interrumpir la instalaci�n.
SourceIsCorrupted=El archivo de origen est� da�ado
SourceDoesntExist=El archivo de origen "%1" no existe
ExistingFileReadOnly=El archivo existente est� marcado como de s�lo lectura.%n%nHaga clic sobre Reintentar para quitar el atributo de s�lo lectura y probar de nuevo, Ignorar para saltar este archivo o Cancelar para interrumpir la instalaci�n.
ErrorReadingExistingDest=Ocurri� un error al tratar de leer el archivo:
FileExists=El archivo ya existe.%n%n�Desea sobrescribirlo?
ExistingFileNewer=El archivo existente es m�s reciente que el que est� tratando de instalar.  Se recomienda conservarlo.%n%n�Desea mantener el archivo existente?
ErrorChangingAttr=Ocurri� un error al tratar de cambiar los atributos del archivo:
ErrorCreatingTemp=Ocurri� un error al tratar de crear un archivo en la carpeta destino:
ErrorReadingSource=Ocurri� un error al tratar de leer el archivo origen:
ErrorCopying=Ocurri� un error al tratar de copiar el archivo:
ErrorReplacingExistingFile=Ocurri� un error al tratar de reemplazar el archivo existente:
ErrorRestartReplace=Fall� el reintento de reemplazar:
ErrorRenamingTemp=Ocurri� un error al tratar de renombrar un archivo en la carpeta destino:
ErrorRegisterServer=Imposible registrar la librer�a DLL/OCX: %1
ErrorRegisterServerMissingExport=No se encontr� DllRegisterServer export
ErrorRegisterTypeLib=Imposible registrar la biblioteca de tipo: %1

; *** Post-installation errors
ErrorOpeningReadme=Ocurri� un error al tratar de abrir el archivo LEAME.
ErrorRestartingComputer=El programa de Instalaci�n no puede reiniciar la computadora. Por favor h�galo manualmente.

; *** Uninstaller messages
UninstallNotFound=El archivo "%1" no existe. No se puede eliminar.
UninstallOpenError=No se pudo abrir el archivo "%1". No es posible eliminar el programa
UninstallUnsupportedVer=El archivo de bit�cora para eliminar "%1" est� en un formato no reconocido por esta versi�n del programa. No se puede continuar
UninstallUnknownEntry=Se encontr� un registro desconocido (%1 en la bit�cora de eliminaci�n
ConfirmUninstall=�Est� seguro que desea eliminar completamente %1 y todos sus componentes?
UninstallOnlyOnWin64=Esta instalaci�n s�lo puede eliminarse en un sistema Windows de 64 bits.
OnlyAdminCanUninstall=Este programa s�lo puede eliminarlo un usuario con privilegios administrativos.
UninstallStatusLabel=Por favor espere mientras se elimina %1 de su computadora.
UninstalledAll=%1 se elimin� con �xito de su computadora.
UninstalledMost=Finaliz� la eliminaci�n de %1.%n%nAlgunos elementos no pudieron quitarse. Estos pueden borrarse manualmente.
UninstalledAndNeedsRestart=Para completar la eliminaci�n de %1 se requiere reiniciar la computadora.%n%n�Desea reiniciarla en este momento?
UninstallDataCorrupted=El archivo "%1" est� da�ado. No se puede eliminar

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=�Eliminar Archivos Compartidos?
ConfirmDeleteSharedFile2=El sistema informa que el siguiente archivo compartido no es utilizado por otros programas.  �Desea eliminar este archivo?%n%nSi otros programas est�n us�ndolo y es eliminado, �stos podr�an no funcionar correctamente. Si no est� seguro, elija No.  Dejar el archivo en su sistema no causa ning�n da�o.
SharedFileNameLabel=Nombre del archivo:
SharedFileLocationLabel=Ubicaci�n:
WizardUninstalling=Estado de la Eliminaci�n
StatusUninstalling=Eliminando %1...

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 versi�n %2
AdditionalIcons=Iconos adicionales:
CreateDesktopIcon=Crear un atajo en el &Escritorio
CreateQuickLaunchIcon=Crear un icono de acceso &r�pido
ProgramOnTheWeb=%1 en la Web
UninstallProgram=Eliminar %1
LaunchProgram=Ejecutar %1
AssocFileExtension=&Asociar %1 con la extensi�n de archivo %2
AssocingFileExtension=Asociar %1 con la extensi�n de archivo %2...
