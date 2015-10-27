; *** Inno Setup version 5.1.0+ Spanish messages ***
;
; Para descargar traducciones de usuarios de este archivo, dirigirse a:
;   http://www.jrsoftware.org/is3rdparty.php
;
; Traducción: Ramón Verduzco
; Revisión ortográfica: Talibuea
; E-mail: procesamx@yahoo.com.mx
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).

[LangOptions]
; The following three entries are very important. Be sure to read and 
; understand the '[LangOptions] section' topic in the help file.
LanguageName=Español
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
SetupAppTitle=Instalación
SetupWindowTitle=Instalación - %1
UninstallAppTitle=Desinstalación
UninstallAppFullTitle=Desinstalación de %1

; *** Misc. common
InformationTitle=Información
ConfirmTitle=Confirmación
ErrorTitle=Error

; *** SetupLdr messages
SetupLdrStartupMessage=Se instalará %1. ¿Deseas continuar?
LdrCannotCreateTemp=No se puede crear un archivo temporal. Instalación cancelada
LdrCannotExecTemp=No se puede ejecutar el archivo en el directorio temporal. Instalación cancelada

; *** Startup error messages
LastErrorMessage=%1.%n%nError %2: %3
SetupFileMissing=No se encuentra el archivo %1 en el directorio de instalación. Por favor solucione el problema u obtenga una nueva copia 
SetupFileCorrupt=Los archivos de instalación están dañados. Por favor obtenga una nueva copia del programa
SetupFileCorruptOrWrongVer=Los archivos de instalación están dañados o son incompatibles con esta versión del instalador. Por favor solucione el problema u obtenga una nueva copia del programa
NotOnThisPlatform=Este programa no se ejecutará en %1.
OnlyOnThisPlatform=Este programa debe ser ejecutado en %1.
OnlyOnTheseArchitectures=Este programa solo puede ser instalado en versiones de Windows diseñadas para los siguientes procesadores:%n%n%1
MissingWOW64APIs=La versión de Windows que estás utilizando no cuenta con la funcionalidad requerida por el instalador para realizar una instalación de 64-bits. Para solucionar este problema instale el "Service Pack" %1.
WinVersionTooLowError=Este programa requiere %1 versión %2 o posterior
WinVersionTooHighError=Este programa no puede ser instalado en %1 versión %2 o posterior
AdminPrivilegesRequired=Para instalar este programa debe ingresar al sistema con privilegios de administrador
PowerUserPrivilegesRequired=Para instalar este programa debe ingresar al sistema como administrador o como usuario con dichos privilegios
SetupAppRunningError=El instalador ha detectado que %1 se está ajecutando.%n%nPor favor cierre ese programa ahora, y luego haga clic en Aceptar para continuar, o Cancelar para salir
UninstallAppRunningError=El desinstalador ha detectado que %1 se está ejecutando.%n%nPor favor cierre ese programa ahora, y luego haga clic en Aceptar para continuar, o Cancelar para salir

; *** Misc. errors
ErrorCreatingDir=El instalador no pudo crear el directorio "%1"
ErrorTooManyFilesInDir=No se pudo crear un archivo en el directorio "%1" ya que contiene demasiados archivos

; *** Setup common messages
ExitSetupTitle=Salir de la instalación
ExitSetupMessage=La instalación no se ha completado aún. Si cancela ahora, el programa no será instalado.%n%nPuede ejecutar nuevamente el programa de instalación en otra ocasión para completarla.%n%n¿Salir de la instalación?
AboutSetupMenuItem=&Acerca del instalador...
AboutSetupTitle=Acerca del instalador
AboutSetupMessage=%1 versión %2%n%3%n%n%1 Sitio Web:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< &Atrás
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
SelectLanguageTitle=Selecciona el idioma para la Instalación
SelectLanguageLabel=Selecciona el idioma que se utilizará durante la Instalación:

; *** Common wizard text
ClickNext=Presione "Siguiente" para continuar, o "Cancelar" para salir de la instalación
BeveledLabel=
BrowseDialogTitle=Buscar carpeta
BrowseDialogLabel=Selecciona una carpeta de la lista inferior y presiona "OK"
NewFolderName=Nueva carpeta

; *** "Welcome" wizard page
WelcomeLabel1=Bienvenido al asistente de instalación de [name]
WelcomeLabel2=Está a punto de instalar [name/ver] en su computadora.%n%nEs recomendable que cierre todos los demás programas antes de continuar

; *** "Password" wizard page
WizardPassword=Contraseña
PasswordLabel1=Esta instalación está protegida por contraseña
PasswordLabel3=Por favor ingrese la contraseña y presione "Siguiente" para continuar. Esta es sensible a mayúsculas y minúsculas
PasswordEditLabel=&Contraseña:
IncorrectPassword=La contraseña que escribió no es correcta. Por favor inténtelo de nuevo

; *** "License Agreement" wizard page
WizardLicense=Consentimiento de licencia
LicenseLabel=Por favor lea la siguiente información antes de continuar
LicenseLabel3=Por favor lea los términos de la licencia. Deberá aceptar éstos antes de continuar con la instalación
LicenseAccepted=&Acepto los términos
LicenseNotAccepted=&No acepto los términos

; *** "Information" wizard pages
WizardInfoBefore=Información
InfoBeforeLabel=Por favor lea la siguiente informacion importante antes de continuar 
InfoBeforeClickLabel=Cuando esté listo para continuar con la instalación, presione "Siguiente"
WizardInfoAfter=Información
InfoAfterLabel=Por favor lea la siguiente informacion importante antes de continuar
InfoAfterClickLabel=Cuando esté listo para continuar con la instalación, presione "Siguiente"

; *** "User Information" wizard page
WizardUserInfo=Información del usuario
UserInfoDesc=Por favor introduzca su información
UserInfoName=Nombre de &usuario:
UserInfoOrg=&Organización:
UserInfoSerial=Número de &Serie:
UserInfoNameRequired=Debes escribir un nombre

; *** "Select Destination Location" wizard page
WizardSelectDir=Selección de la ruta destino
SelectDirDesc=¿En dónde deberá instalarse [name]?
SelectDirLabel3=[name] será instalado en la siguiente carpeta
SelectDirBrowseLabel=Para continuar presiona "Siguiente". Si deseas seleccionar una carpeta diferente presiona "Navegar"
DiskSpaceMBLabel=Se requiere de al menos [mb] MB de espacio disponible
ToUNCPathname=No se puede instalar en la ruta de la "UNC". Si está intentando instalar a una red necesitará conectarse y dirigirse hacia dicha dirección
InvalidPath=Debe introducir la ruta completa con la letra de la unidad; por ejemplo:%n%nC:\APP%n%no a una ruta UNC en la forma:%n%n\\servidor\recurso
InvalidDrive=La unidad o recurso compartido UNC que seleccionó no existe o no está disponible. Por favor seleccione otro
DiskSpaceWarningTitle=Espacio insuficiente en disco
DiskSpaceWarning=La instalación requiere de al menos %1 KB de espacio libre y el disco seleccionado solo cuenta con %2 KB disponibles.%n%n¿Deseas continuar de cualquier forma?
DirNameTooLong=El nombre o ruta de la carpeta es demasiado largo
InvalidDirName=El nombre de la carpeta no es válido
BadDirName32=Los nombres de carpetas no deben de tener ninguno de los siguientes caracteres:%n%n%1
DirExistsTitle=La carpeta ya existe
DirExists=La carpeta:%n%n%1%n%nya existe. ¿Aún así deseas instalar en esa misma?
DirDoesntExistTitle=La carpeta no existe
DirDoesntExist=La carpeta:%n%n%1%n%nno existe. ¿Deseas que sea creada esa carpeta?

; *** "Select Components" wizard page
WizardSelectComponents=Selecciona componentes
SelectComponentsDesc=¿Cuales componentes serán instalados?
SelectComponentsLabel2=Selecciona los componentes que deseas instalar; quita la marca de los componentes que no deseas instalar. Presione "Siguiente" para continuar
FullInstallation=Instalación completa
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Instalación Compacta
CustomInstallation=Instalación Personalizada
NoUninstallWarningTitle=Los componentes ya existen
NoUninstallWarning=La Instalación ha detectado que los siguientes componentes ya fueron instalados en su computadora:%n%n%1%n%nSi les quita la marca de selección, éstos no serán desinstalados.%n%n¿Deseas continuar de cualquier forma?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Su selección requiere de al menos [mb] MB de espacio en Disco

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Selecciona las tareas adicionales
SelectTasksDesc=¿Que tareas adicionales deberán ser ejecutadas?
SelectTasksLabel2=Selecciona las tareas adicionales que quieres que se ejecuten mientras [name] se instala y luego presiona "Siguiente"

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Selecciona la carpeta en el Menú "Inicio"
SelectStartMenuFolderDesc=¿En dónde desea que la instalación coloque los accesos directos del programa?
SelectStartMenuFolderLabel3=El programa de instalación creará los accesos directos del programa en la siguiente carpeta del menú "Inicio"
SelectStartMenuFolderBrowseLabel=Para continuar presione "Siguiente" Si desea seleccionar una carpeta distinta presione "Navegar"
MustEnterGroupName=Debe escribir un nombre de carpeta
GroupNameTooLong=El nombre de carpeta o de ruta es demasiado largo
InvalidGroupName=El nombre de la carpeta no es válido
BadGroupName=El nombre de la carpeta no puede tener ninguno de los siguientes caracteres:%n%n%1
NoProgramGroupCheck2=&No crear carpeta en el Menú "Inicio"

; *** "Ready to Install" wizard page
WizardReady=Listo para instalar
ReadyLabel1=El programa está listo para iniciar la instalación de [name] en su computadora
ReadyLabel2a=Presiona "Instalar" para continuar o "Atrás" para revisar o cambiar cualquier parámetro
ReadyLabel2b=Presiona "Instalar" para continuar con la instalación
ReadyMemoUserInfo=Información del usuario:
ReadyMemoDir=Ubicación del destino:
ReadyMemoType=Tipo de instalación:
ReadyMemoComponents=Componentes seleccionados:
ReadyMemoGroup=Carpeta en el menú "Inicio":
ReadyMemoTasks=Tareas adicionales:

; *** "Preparing to Install" wizard page
WizardPreparing=Preparandose para instalar
PreparingDesc=El programa se está preparando para instalar [name] en su computadora
PreviousInstallNotCompleted=La instalación/remoción de un programa previo no fué completada. Deberás reiniciar tu computadora para completar esa instalación.%n%nDespués de reiniciar, ejecuta nuevamente el programa de instalación de [name] para completar el proceso
CannotContinue=La instalación no puede continuar. Presiona "Cancelar" para salir

; *** "Installing" wizard page
WizardInstalling=Instalando
InstallingLabel=Por favor espere mientras se instala [name] en su computadora.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Completando el asistente de instalación de [name]
FinishedLabelNoIcons=El programa ha terminado de instalar [name] en su computadora.
FinishedLabel=Ha concluído la instalación de [name] en su computadora. El programa puede ser ejecutado seleccionando los íconos instalados.
ClickFinish=Presione "Finalizar" para salir de la instalación.
FinishedRestartLabel=Para completar la instalación de [name], el instalador deberá reiniciar su computadora. ¿Quiere reiniciar ahora?
FinishedRestartMessage=Para completar la instalación de [name], el instalador deberá reiniciar su computadora.%n%n¿Quiere reiniciar ahora?
ShowReadmeCheck=Si, quiero leer el archivo "LEEME" ahora
YesRadio=&Si, reiniciar la computadora en este momento
NoRadio=&No, reiniciaré la computadora mas adelante
; used for example as 'Run MyProg.exe'
RunEntryExec=Ejecutar %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Ver %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=La instalación necesita el siguiente disco
SelectDiskLabel2=Por favor inserte el disco %1 y presione "OK".%n%nSi los archivos del disco pueden ser encontrados en una carpeta diferente a la mostrada abajo, escribe la direccion correcta o presiona el boton "Navegar"
PathLabel=&Ruta:
FileNotInDir2=El archivo "%1" no pudo ser localizado en "%2". Por favor inserte el disco correcto o seleccione otra carpeta.
SelectDirectoryLabel=Por favor especifique la ruta del siguiente disco.

; *** Installation phase messages
SetupAborted=La instalación no se completó.%n%nPor favor solucione el problema y ejecute nuevamente el programa de instalación.
EntryAbortRetryIgnore=Presione "Reintentar" para intentar nuevamente, "Ignorar" para proseguir o "Abortar" para cancelar la instalación.
; *** Installation status messages
StatusCreateDirs=Creando directorios...
StatusExtractFiles=Extrayendo archivos...
StatusCreateIcons=Creando accesos directos...
StatusCreateIniEntries=Creando entradas INI...
StatusCreateRegistryEntries=Creando entradas de registro...
StatusRegisterFiles=Registrando archivos...
StatusSavingUninstall=Guardando información para desinstalación...
StatusRunProgram=Terminando la intalacion...
StatusRollback=Deshaciendo cambios...

; *** Misc. errors
ErrorInternal2=Error interno: %1
ErrorFunctionFailedNoCode=%1 falló
ErrorFunctionFailed=%1 falló; código %2
ErrorFunctionFailedWithMessage=%1 falló; código %2.%n%3
ErrorExecutingProgram=No pudo ejecutarse el archivo:%n%1

; *** Registry errors
ErrorRegOpenKey=Error al abrir la clave del registro:%n%1\%2
ErrorRegCreateKey=Error al crear la clave del registro:%n%1\%2
ErrorRegWriteKey=Error al escribir la clave en el registro:%n%1\%2

; *** INI errors
ErrorIniEntry=Error al crear las entradas INI en el archivo "%1".

; *** File copying errors
FileAbortRetryIgnore=Presiona "Reintentar" para intentarlo de nuevo, "Ignorar" para omitir este archivo (no recomendable) o "Abortar" para cancelar la instalación.
FileAbortRetryIgnore2=Presiona "Reintentar" para intentarlo de nuevo, "Ignorar" para continuar de cualquier forma (not recomendable), o "Abortar" para cancelar la instalación.
SourceIsCorrupted=El archivo de origen está dañado
SourceDoesntExist=El archivo de origen "%1" no existe
ExistingFileReadOnly=El archivo está marcado como "solo lectura".%n%nPresione "Reintentar" para quitar el atributo de "solo lectura" e intentar de nuevo, "Ignorar" para omitir este archivo, o "Abortar" para cancelar la instalación.
ErrorReadingExistingDest=Se ha presentado un error mientras se intentaba leer el archivo:
FileExists=El archivo ya existe.%n%n¿Deseas que el instalador lo sobreescriba?
ExistingFileNewer=El archivo existente es mas nuevo que el que se está intentando instalar. Se recomienda conservar el archivo existente.%n%n¿Desea conservar el archivo existente?
ErrorChangingAttr=Se ha presentado un error al intentar cambiar los atributos del archivo:
ErrorCreatingTemp=Se ha presentado un error al intentar crear un archivo en el directorio de destino:
ErrorReadingSource=Se ha presentado un error al intentar leer el archivo de origen:
ErrorCopying=Se ha presentado un error al intentar copiar el archivo:
ErrorReplacingExistingFile=Se ha presentado un error al intentar remplazar el archivo existente:
ErrorRestartReplace=Error al reintentar reemplazo:
ErrorRenamingTemp=Se ha presentado un error al intentar renombrar un archivo en el directorio de destino:
ErrorRegisterServer=Imposible registrar DLL/OCX: %1
ErrorRegisterServerMissingExport=DllRegisterServer export no fue encontrado
ErrorRegisterTypeLib=Imposible registrar la librería: %1

; *** Post-installation errors
ErrorOpeningReadme=Ocurrió un error al intentar abrir el archivo "LEEME".
ErrorRestartingComputer=El programa de instalación no pudo reiniciar el equipo. Por favor, hágalo manualmente.

; *** Uninstaller messages
UninstallNotFound=El arechivo "%1" no existe. No se puede desinstalar.
UninstallOpenError=El archivo "%1" no se pudo abrir. No se puede desinstalar
UninstallUnsupportedVer=La bitácora de desinstalación "%1" está en un formato que no reconoce esta versión del desinstalador. No se puede desinstalar
UninstallUnknownEntry=Un registro desconocido (%1) fue encontrado en la bitácora de desinstalación
ConfirmUninstall=¿Está seguro que quiere desintalar completamente %1 y todos sus componentes?
UninstallOnlyOnWin64=Este programa solo puede ser desinstalado en una versión de Windows de 64-bits.
OnlyAdminCanUninstall=Este programa solo puede ser desinstalado por un usuario con privilegios de administrador.
UninstallStatusLabel=Por favor espere mientras %1 es desinstalado de su computadora.
UninstalledAll=%1 fué removido exitosamente de su computadora.
UninstalledMost=Desinstalación de %1 completada.%n%nAlgunos elementos no se pudieron remover. Estos pueden ser eliminados manualmente.
UninstalledAndNeedsRestart=Para completar la desinstalación de %1, su computadora deberá ser reiniciada.%n%n¿Deseas reiniciar ahora?
UninstallDataCorrupted=El archivo "%1" está dañado. No se puede desinstalar

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=¿Borrar archivos compartidos?
ConfirmDeleteSharedFile2=El sistema indica que el siguiente archivo compartido no está siendo usado por ningun programa. ¿Desea que la desinstalación remueva estos archivos?%n%nDesea que la desinstalación remueva estos archivos?%n%nSi algún programa está usando este archivo y éste es removido, el programa no funcionará apropiadamente. Si no está seguro elija "No". Si deja el archivo en su sistema no causará ningun problema.
SharedFileNameLabel=Nombre del archivo:
SharedFileLocationLabel=Ruta:
WizardUninstalling=Estado de la desintalación
StatusUninstalling=Desinstalando %1...

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 versión %2
AdditionalIcons=Iconos adicionales:
CreateDesktopIcon=Crear un icono en el &escritorio
CreateQuickLaunchIcon=Crear un icono de acceso &rápido
ProgramOnTheWeb=%1 en la Red
UninstallProgram=Desinstalar %1
LaunchProgram=Ejecutar %1
AssocFileExtension=&Asociar %1 con la extensión de archivo %2
AssocingFileExtension=Asociando %1 con la extensión de archivo %2...
