; *** Inno Setup version 5.1.0+ Spanish messages ***
;
; Para descargar traducciones de usuarios de este archivo, dirigirse a:
;   http://www.jrsoftware.org/is3rdparty.php
;
; Traducci�n: Ram�n Verduzco
; Revisi�n ortogr�fica: Talibuea
; E-mail: procesamx@yahoo.com.mx
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).

[LangOptions]
; The following three entries are very important. Be sure to read and 
; understand the '[LangOptions] section' topic in the help file.
LanguageName=Espa�ol
LanguageID=$080A
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
SetupAppTitle=Instalaci�n
SetupWindowTitle=Instalaci�n - %1
UninstallAppTitle=Desinstalaci�n
UninstallAppFullTitle=Desinstalaci�n de %1

; *** Misc. common
InformationTitle=Informaci�n
ConfirmTitle=Confirmaci�n
ErrorTitle=Error

; *** SetupLdr messages
SetupLdrStartupMessage=Se instalar� %1. �Deseas continuar?
LdrCannotCreateTemp=No se puede crear un archivo temporal. Instalaci�n cancelada
LdrCannotExecTemp=No se puede ejecutar el archivo en el directorio temporal. Instalaci�n cancelada

; *** Startup error messages
LastErrorMessage=%1.%n%nError %2: %3
SetupFileMissing=No se encuentra el archivo %1 en el directorio de instalaci�n. Por favor solucione el problema u obtenga una nueva copia 
SetupFileCorrupt=Los archivos de instalaci�n est�n da�ados. Por favor obtenga una nueva copia del programa
SetupFileCorruptOrWrongVer=Los archivos de instalaci�n est�n da�ados o son incompatibles con esta versi�n del instalador. Por favor solucione el problema u obtenga una nueva copia del programa
NotOnThisPlatform=Este programa no se ejecutar� en %1.
OnlyOnThisPlatform=Este programa debe ser ejecutado en %1.
OnlyOnTheseArchitectures=Este programa solo puede ser instalado en versiones de Windows dise�adas para los siguientes procesadores:%n%n%1
MissingWOW64APIs=La versi�n de Windows que est�s utilizando no cuenta con la funcionalidad requerida por el instalador para realizar una instalaci�n de 64-bits. Para solucionar este problema instale el "Service Pack" %1.
WinVersionTooLowError=Este programa requiere %1 versi�n %2 o posterior
WinVersionTooHighError=Este programa no puede ser instalado en %1 versi�n %2 o posterior
AdminPrivilegesRequired=Para instalar este programa debe ingresar al sistema con privilegios de administrador
PowerUserPrivilegesRequired=Para instalar este programa debe ingresar al sistema como administrador o como usuario con dichos privilegios
SetupAppRunningError=El instalador ha detectado que %1 se est� ajecutando.%n%nPor favor cierre ese programa ahora, y luego haga clic en Aceptar para continuar, o Cancelar para salir
UninstallAppRunningError=El desinstalador ha detectado que %1 se est� ejecutando.%n%nPor favor cierre ese programa ahora, y luego haga clic en Aceptar para continuar, o Cancelar para salir

; *** Misc. errors
ErrorCreatingDir=El instalador no pudo crear el directorio "%1"
ErrorTooManyFilesInDir=No se pudo crear un archivo en el directorio "%1" ya que contiene demasiados archivos

; *** Setup common messages
ExitSetupTitle=Salir de la instalaci�n
ExitSetupMessage=La instalaci�n no se ha completado a�n. Si cancela ahora, el programa no ser� instalado.%n%nPuede ejecutar nuevamente el programa de instalaci�n en otra ocasi�n para completarla.%n%n�Salir de la instalaci�n?
AboutSetupMenuItem=&Acerca del instalador...
AboutSetupTitle=Acerca del instalador
AboutSetupMessage=%1 versi�n %2%n%3%n%n%1 Sitio Web:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< &Atr�s
ButtonNext=&Siguiente >
ButtonInstall=&Instalar
ButtonOK=OK
ButtonCancel=Cancelar
ButtonYes=&Si
ButtonYesToAll=Si a &Todo
ButtonNo=&No
ButtonNoToAll=N&o a todo
ButtonFinish=&Finalizar
ButtonBrowse=&Navegar...
ButtonWizardBrowse=Na&vegar...
ButtonNewFolder=Nueva &carpeta

; *** "Select Language" dialog messages
SelectLanguageTitle=Selecciona el idioma para la Instalaci�n
SelectLanguageLabel=Selecciona el idioma que se utilizar� durante la Instalaci�n:

; *** Common wizard text
ClickNext=Presione "Siguiente" para continuar, o "Cancelar" para salir de la instalaci�n
BeveledLabel=
BrowseDialogTitle=Buscar carpeta
BrowseDialogLabel=Selecciona una carpeta de la lista inferior y presiona "OK"
NewFolderName=Nueva carpeta

; *** "Welcome" wizard page
WelcomeLabel1=Bienvenido al asistente de instalaci�n de [name]
WelcomeLabel2=Est� a punto de instalar [name/ver] en su computadora.%n%nEs recomendable que cierre todos los dem�s programas antes de continuar

; *** "Password" wizard page
WizardPassword=Contrase�a
PasswordLabel1=Esta instalaci�n est� protegida por contrase�a
PasswordLabel3=Por favor ingrese la contrase�a y presione "Siguiente" para continuar. Esta es sensible a may�sculas y min�sculas
PasswordEditLabel=&Contrase�a:
IncorrectPassword=La contrase�a que escribi� no es correcta. Por favor int�ntelo de nuevo

; *** "License Agreement" wizard page
WizardLicense=Consentimiento de licencia
LicenseLabel=Por favor lea la siguiente informaci�n antes de continuar
LicenseLabel3=Por favor lea los t�rminos de la licencia. Deber� aceptar �stos antes de continuar con la instalaci�n
LicenseAccepted=&Acepto los t�rminos
LicenseNotAccepted=&No acepto los t�rminos

; *** "Information" wizard pages
WizardInfoBefore=Informaci�n
InfoBeforeLabel=Por favor lea la siguiente informacion importante antes de continuar 
InfoBeforeClickLabel=Cuando est� listo para continuar con la instalaci�n, presione "Siguiente"
WizardInfoAfter=Informaci�n
InfoAfterLabel=Por favor lea la siguiente informacion importante antes de continuar
InfoAfterClickLabel=Cuando est� listo para continuar con la instalaci�n, presione "Siguiente"

; *** "User Information" wizard page
WizardUserInfo=Informaci�n del usuario
UserInfoDesc=Por favor introduzca su informaci�n
UserInfoName=Nombre de &usuario:
UserInfoOrg=&Organizaci�n:
UserInfoSerial=N�mero de &Serie:
UserInfoNameRequired=Debes escribir un nombre

; *** "Select Destination Location" wizard page
WizardSelectDir=Selecci�n de la ruta destino
SelectDirDesc=�En d�nde deber� instalarse [name]?
SelectDirLabel3=[name] ser� instalado en la siguiente carpeta
SelectDirBrowseLabel=Para continuar presiona "Siguiente". Si deseas seleccionar una carpeta diferente presiona "Navegar"
DiskSpaceMBLabel=Se requiere de al menos [mb] MB de espacio disponible
ToUNCPathname=No se puede instalar en la ruta de la "UNC". Si est� intentando instalar a una red necesitar� conectarse y dirigirse hacia dicha direcci�n
InvalidPath=Debe introducir la ruta completa con la letra de la unidad; por ejemplo:%n%nC:\APP%n%no a una ruta UNC en la forma:%n%n\\servidor\recurso
InvalidDrive=La unidad o recurso compartido UNC que seleccion� no existe o no est� disponible. Por favor seleccione otro
DiskSpaceWarningTitle=Espacio insuficiente en disco
DiskSpaceWarning=La instalaci�n requiere de al menos %1 KB de espacio libre y el disco seleccionado solo cuenta con %2 KB disponibles.%n%n�Deseas continuar de cualquier forma?
DirNameTooLong=El nombre o ruta de la carpeta es demasiado largo
InvalidDirName=El nombre de la carpeta no es v�lido
BadDirName32=Los nombres de carpetas no deben de tener ninguno de los siguientes caracteres:%n%n%1
DirExistsTitle=La carpeta ya existe
DirExists=La carpeta:%n%n%1%n%nya existe. �A�n as� deseas instalar en esa misma?
DirDoesntExistTitle=La carpeta no existe
DirDoesntExist=La carpeta:%n%n%1%n%nno existe. �Deseas que sea creada esa carpeta?

; *** "Select Components" wizard page
WizardSelectComponents=Selecciona componentes
SelectComponentsDesc=�Cuales componentes ser�n instalados?
SelectComponentsLabel2=Selecciona los componentes que deseas instalar; quita la marca de los componentes que no deseas instalar. Presione "Siguiente" para continuar
FullInstallation=Instalaci�n completa
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Instalaci�n Compacta
CustomInstallation=Instalaci�n Personalizada
NoUninstallWarningTitle=Los componentes ya existen
NoUninstallWarning=La Instalaci�n ha detectado que los siguientes componentes ya fueron instalados en su computadora:%n%n%1%n%nSi les quita la marca de selecci�n, �stos no ser�n desinstalados.%n%n�Deseas continuar de cualquier forma?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Su selecci�n requiere de al menos [mb] MB de espacio en Disco

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Selecciona las tareas adicionales
SelectTasksDesc=�Que tareas adicionales deber�n ser ejecutadas?
SelectTasksLabel2=Selecciona las tareas adicionales que quieres que se ejecuten mientras [name] se instala y luego presiona "Siguiente"

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Selecciona la carpeta en el Men� "Inicio"
SelectStartMenuFolderDesc=�En d�nde desea que la instalaci�n coloque los accesos directos del programa?
SelectStartMenuFolderLabel3=El programa de instalaci�n crear� los accesos directos del programa en la siguiente carpeta del men� "Inicio"
SelectStartMenuFolderBrowseLabel=Para continuar presione "Siguiente" Si desea seleccionar una carpeta distinta presione "Navegar"
MustEnterGroupName=Debe escribir un nombre de carpeta
GroupNameTooLong=El nombre de carpeta o de ruta es demasiado largo
InvalidGroupName=El nombre de la carpeta no es v�lido
BadGroupName=El nombre de la carpeta no puede tener ninguno de los siguientes caracteres:%n%n%1
NoProgramGroupCheck2=&No crear carpeta en el Men� "Inicio"

; *** "Ready to Install" wizard page
WizardReady=Listo para instalar
ReadyLabel1=El programa est� listo para iniciar la instalaci�n de [name] en su computadora
ReadyLabel2a=Presiona "Instalar" para continuar o "Atr�s" para revisar o cambiar cualquier par�metro
ReadyLabel2b=Presiona "Instalar" para continuar con la instalaci�n
ReadyMemoUserInfo=Informaci�n del usuario:
ReadyMemoDir=Ubicaci�n del destino:
ReadyMemoType=Tipo de instalaci�n:
ReadyMemoComponents=Componentes seleccionados:
ReadyMemoGroup=Carpeta en el men� "Inicio":
ReadyMemoTasks=Tareas adicionales:

; *** "Preparing to Install" wizard page
WizardPreparing=Preparandose para instalar
PreparingDesc=El programa se est� preparando para instalar [name] en su computadora
PreviousInstallNotCompleted=La instalaci�n/remoci�n de un programa previo no fu� completada. Deber�s reiniciar tu computadora para completar esa instalaci�n.%n%nDespu�s de reiniciar, ejecuta nuevamente el programa de instalaci�n de [name] para completar el proceso
CannotContinue=La instalaci�n no puede continuar. Presiona "Cancelar" para salir

; *** "Installing" wizard page
WizardInstalling=Instalando
InstallingLabel=Por favor espere mientras se instala [name] en su computadora.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Completando el asistente de instalaci�n de [name]
FinishedLabelNoIcons=El programa ha terminado de instalar [name] en su computadora.
FinishedLabel=Ha conclu�do la instalaci�n de [name] en su computadora. El programa puede ser ejecutado seleccionando los �conos instalados.
ClickFinish=Presione "Finalizar" para salir de la instalaci�n.
FinishedRestartLabel=Para completar la instalaci�n de [name], el instalador deber� reiniciar su computadora. �Quiere reiniciar ahora?
FinishedRestartMessage=Para completar la instalaci�n de [name], el instalador deber� reiniciar su computadora.%n%n�Quiere reiniciar ahora?
ShowReadmeCheck=Si, quiero leer el archivo "LEEME" ahora
YesRadio=&Si, reiniciar la computadora en este momento
NoRadio=&No, reiniciar� la computadora mas adelante
; used for example as 'Run MyProg.exe'
RunEntryExec=Ejecutar %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Ver %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=La instalaci�n necesita el siguiente disco
SelectDiskLabel2=Por favor inserte el disco %1 y presione "OK".%n%nSi los archivos del disco pueden ser encontrados en una carpeta diferente a la mostrada abajo, escribe la direccion correcta o presiona el boton "Navegar"
PathLabel=&Ruta:
FileNotInDir2=El archivo "%1" no pudo ser localizado en "%2". Por favor inserte el disco correcto o seleccione otra carpeta.
SelectDirectoryLabel=Por favor especifique la ruta del siguiente disco.

; *** Installation phase messages
SetupAborted=La instalaci�n no se complet�.%n%nPor favor solucione el problema y ejecute nuevamente el programa de instalaci�n.
EntryAbortRetryIgnore=Presione "Reintentar" para intentar nuevamente, "Ignorar" para proseguir o "Abortar" para cancelar la instalaci�n.
; *** Installation status messages
StatusCreateDirs=Creando directorios...
StatusExtractFiles=Extrayendo archivos...
StatusCreateIcons=Creando accesos directos...
StatusCreateIniEntries=Creando entradas INI...
StatusCreateRegistryEntries=Creando entradas de registro...
StatusRegisterFiles=Registrando archivos...
StatusSavingUninstall=Guardando informaci�n para desinstalaci�n...
StatusRunProgram=Terminando la intalacion...
StatusRollback=Deshaciendo cambios...

; *** Misc. errors
ErrorInternal2=Error interno: %1
ErrorFunctionFailedNoCode=%1 fall�
ErrorFunctionFailed=%1 fall�; c�digo %2
ErrorFunctionFailedWithMessage=%1 fall�; c�digo %2.%n%3
ErrorExecutingProgram=No pudo ejecutarse el archivo:%n%1

; *** Registry errors
ErrorRegOpenKey=Error al abrir la clave del registro:%n%1\%2
ErrorRegCreateKey=Error al crear la clave del registro:%n%1\%2
ErrorRegWriteKey=Error al escribir la clave en el registro:%n%1\%2

; *** INI errors
ErrorIniEntry=Error al crear las entradas INI en el archivo "%1".

; *** File copying errors
FileAbortRetryIgnore=Presiona "Reintentar" para intentarlo de nuevo, "Ignorar" para omitir este archivo (no recomendable) o "Abortar" para cancelar la instalaci�n.
FileAbortRetryIgnore2=Presiona "Reintentar" para intentarlo de nuevo, "Ignorar" para continuar de cualquier forma (not recomendable), o "Abortar" para cancelar la instalaci�n.
SourceIsCorrupted=El archivo de origen est� da�ado
SourceDoesntExist=El archivo de origen "%1" no existe
ExistingFileReadOnly=El archivo est� marcado como "solo lectura".%n%nPresione "Reintentar" para quitar el atributo de "solo lectura" e intentar de nuevo, "Ignorar" para omitir este archivo, o "Abortar" para cancelar la instalaci�n.
ErrorReadingExistingDest=Se ha presentado un error mientras se intentaba leer el archivo:
FileExists=El archivo ya existe.%n%n�Deseas que el instalador lo sobreescriba?
ExistingFileNewer=El archivo existente es mas nuevo que el que se est� intentando instalar. Se recomienda conservar el archivo existente.%n%n�Desea conservar el archivo existente?
ErrorChangingAttr=Se ha presentado un error al intentar cambiar los atributos del archivo:
ErrorCreatingTemp=Se ha presentado un error al intentar crear un archivo en el directorio de destino:
ErrorReadingSource=Se ha presentado un error al intentar leer el archivo de origen:
ErrorCopying=Se ha presentado un error al intentar copiar el archivo:
ErrorReplacingExistingFile=Se ha presentado un error al intentar remplazar el archivo existente:
ErrorRestartReplace=Error al reintentar reemplazo:
ErrorRenamingTemp=Se ha presentado un error al intentar renombrar un archivo en el directorio de destino:
ErrorRegisterServer=Imposible registrar DLL/OCX: %1
ErrorRegisterServerMissingExport=DllRegisterServer export no fue encontrado
ErrorRegisterTypeLib=Imposible registrar la librer�a: %1

; *** Post-installation errors
ErrorOpeningReadme=Ocurri� un error al intentar abrir el archivo "LEEME".
ErrorRestartingComputer=El programa de instalaci�n no pudo reiniciar el equipo. Por favor, h�galo manualmente.

; *** Uninstaller messages
UninstallNotFound=El arechivo "%1" no existe. No se puede desinstalar.
UninstallOpenError=El archivo "%1" no se pudo abrir. No se puede desinstalar
UninstallUnsupportedVer=La bit�cora de desinstalaci�n "%1" est� en un formato que no reconoce esta versi�n del desinstalador. No se puede desinstalar
UninstallUnknownEntry=Un registro desconocido (%1) fue encontrado en la bit�cora de desinstalaci�n
ConfirmUninstall=�Est� seguro que quiere desintalar completamente %1 y todos sus componentes?
UninstallOnlyOnWin64=Este programa solo puede ser desinstalado en una versi�n de Windows de 64-bits.
OnlyAdminCanUninstall=Este programa solo puede ser desinstalado por un usuario con privilegios de administrador.
UninstallStatusLabel=Por favor espere mientras %1 es desinstalado de su computadora.
UninstalledAll=%1 fu� removido exitosamente de su computadora.
UninstalledMost=Desinstalaci�n de %1 completada.%n%nAlgunos elementos no se pudieron remover. Estos pueden ser eliminados manualmente.
UninstalledAndNeedsRestart=Para completar la desinstalaci�n de %1, su computadora deber� ser reiniciada.%n%n�Deseas reiniciar ahora?
UninstallDataCorrupted=El archivo "%1" est� da�ado. No se puede desinstalar

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=�Borrar archivos compartidos?
ConfirmDeleteSharedFile2=El sistema indica que el siguiente archivo compartido no est� siendo usado por ningun programa. �Desea que la desinstalaci�n remueva estos archivos?%n%nDesea que la desinstalaci�n remueva estos archivos?%n%nSi alg�n programa est� usando este archivo y �ste es removido, el programa no funcionar� apropiadamente. Si no est� seguro elija "No". Si deja el archivo en su sistema no causar� ningun problema.
SharedFileNameLabel=Nombre del archivo:
SharedFileLocationLabel=Ruta:
WizardUninstalling=Estado de la desintalaci�n
StatusUninstalling=Desinstalando %1...

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 versi�n %2
AdditionalIcons=Iconos adicionales:
CreateDesktopIcon=Crear un icono en el &escritorio
CreateQuickLaunchIcon=Crear un icono de acceso &r�pido
ProgramOnTheWeb=%1 en la Red
UninstallProgram=Desinstalar %1
LaunchProgram=Ejecutar %1
AssocFileExtension=&Asociar %1 con la extensi�n de archivo %2
AssocingFileExtension=Asociando %1 con la extensi�n de archivo %2...
