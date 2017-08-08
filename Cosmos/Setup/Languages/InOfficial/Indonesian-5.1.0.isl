; *** Inno Setup version 5.1.0+ Indonesian messages ***
;
; To download user-contributed translations of this file, go to:
;   http://www.jrsoftware.org/is3rdparty.php
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).
;
; Alih bahasa oleh:
;   Jaimy Azle (jazle@sat.net.id) - http://delphi.log.web.id
; Inno Setup Indonesian Translation home page:
;   http://delphi.log.web.id/comptools/index.html#tools
;

[LangOptions]
; The following three entries are very important. Be sure to read and
; understand the '[LangOptions] section' topic in the help file.
LanguageName=Indonesia
LanguageID=$0421
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
SetupAppTitle=Instalasi
SetupWindowTitle=Instalasi - %1
UninstallAppTitle=Deinstalasi
UninstallAppFullTitle=Deinstalasi %1

; *** Misc. common
InformationTitle=Informasi
ConfirmTitle=Konfirmasi
ErrorTitle=Error

; *** SetupLdr messages
SetupLdrStartupMessage=Program ini akan menginstal aplikasi %1. Anda akan melanjutkannya?
LdrCannotCreateTemp=Gagal membuat file temporer. Instalasi dibatalkan
LdrCannotExecTemp=Gagal mengeksekusi file dalam direktori temporer. Instalasi dibatalkan

; *** Startup error messages
LastErrorMessage=%1.%n%nKesalahan %2: %3
SetupFileMissing=File %1 tidak ditemukan dalam direktori instalasi. Silahkan periksa atau ganti dengan copy program instalasi yang lebih baru.
SetupFileCorrupt=File instalasi rusak. Silahkan ganti dengan copy program instalasi yang lebih baru.
SetupFileCorruptOrWrongVer=File instalasi yangrusak, atau tidak kompatibel dengan versi program instalasi ini. Silahkan periksa atau ganti dengan copy program instalasi yang lebih baru.
NotOnThisPlatform=Program ini tidak bisa berjalan di %1.
OnlyOnThisPlatform=Program ini harus dijalankan di %1.
OnlyOnTheseArchitectures=Aplikasi ini hanya bisa diinstal pada versi Windows yang didisain untuk arsitektur prosesor berikut:%n%n%1
MissingWOW64APIs=Versi Windows yang anda jalankan tidak memiliki fungsionalitas yang dibutuhkan oleh program instalasi untuk melakukan instalasi 64-bit. Untuk memperbaikinya, silahkan instal Service Pack %1.
WinVersionTooLowError=Program ini membutuhkan %1 versi %2 atau yang lebih baru.
WinVersionTooHighError=Program ini tidak dapat diinstal pada %1 versi %2 atau yang lebih baru.
AdminPrivilegesRequired=Anda harus login sebagai administrator untuk menginstal aplikasi ini.
PowerUserPrivilegesRequired=Anda harus login sebagai administrator atau dalam grup Power Users untuk menginstal aplikasi ini.
SetupAppRunningError=Program instalasi mendeteksi aplikasi %1 masih dijalankan.%n%nSilahkan tutup semua aplikasi tersebut, kemudian klik OK untuk melanjutkan, atau Batal untuk keluar.
UninstallAppRunningError=Program deinstalasi mendeteksi aplikasi %1 masih dijalankan.%n%nSilahkan tutup semua aplikasi tersebut, kemudian klik OK untuk melanjutkan, atau Batal untuk keluar.

; *** Misc. errors
ErrorCreatingDir=Gagal membuat direktori "%1"
ErrorTooManyFilesInDir=Gagal membuat file di direktori "%1" karena sudah terlalu banyak file dalam direktori tersebut

; *** Setup common messages
ExitSetupTitle=Keluar dari program instalasi
ExitSetupMessage=Instalasi belum selesai. Jika anda keluar sekarang, Aplikasi tidak akan diinstal.%n%nAnda bisa menjalankan program ini di lain waktu.%n%nKeluar dari program instalasi?
AboutSetupMenuItem=&Tentang Instalasi...
AboutSetupTitle=Tentang Instalasi
AboutSetupMessage=%1 versi %2%n%3%n%n%1 home page:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< &Kembali
ButtonNext=&Lanjut >
ButtonInstall=&Instal
ButtonOK=OK
ButtonCancel=Batal
ButtonYes=&Ya
ButtonYesToAll=Ya &semua
ButtonNo=&Tidak
ButtonNoToAll=T&idak semua
ButtonFinish=&Selesai
ButtonBrowse=&Browse...
ButtonWizardBrowse=B&rowse...
ButtonNewFolder=Buat &Folder Baru

; *** "Select Language" dialog messages
SelectLanguageTitle=Pilihan Bahasa
SelectLanguageLabel=Pilih bahasa yang hendak digunakan selama proses instalasi:

; *** Common wizard text
ClickNext=Klik Lanjut untuk meneruskan, atau Batal untuk keluar dari Instalasi.
BeveledLabel=PT. Ungaran Sari Garments
BrowseDialogTitle=Browse untuk Folder
BrowseDialogLabel=Pilih folder pada daftar berikut, kemudian klik OK.
NewFolderName=Folder Baru

; *** "Welcome" wizard page
WelcomeLabel1=Selamat Datang di Program Instalasi [name]
WelcomeLabel2=Program ini akan menginstal [name/ver] pada komputer anda.%n%nSangat disarankan untuk menutup terlebih dulu semua aplikasi sebelum anda melanjutkan.%n%nPERHATIAN: Aplikasi ini dilindungi oleh undang-undang hak cipta (HAKI) dan kesepakatan internasional.%n%nSegala bentuk reproduksi, atau distribusi dari aplikasi ini, atau bagian apapun dari aplikasi ini tanpa ijin tertulis adalah melanggar hukum dan akan dituntut ke meja hijau.

; *** "Password" wizard page
WizardPassword=Password
PasswordLabel1=Instalasi ini diproteksi dengan password.
PasswordLabel3=Silahkan masukkan password, kemudian klik Lanjut untuk melanjutkan. Password adalah case-sensitive.
PasswordEditLabel=&Password:
IncorrectPassword=Password yang anda masukkan tidak valid. Silahkan coba kembali.

; *** "License Agreement" wizard page
WizardLicense=Kesepakatan atas Lisensi
LicenseLabel=Silahkan baca terlebih dulu informasi penting berikut sebelum melanjutkan.
LicenseLabel3=Silahkan baca terlebih dulu kesepakatan lisensi berikut. Anda harus setuju dan mematuhi kesepakatan tersebut terlebih dulu sebelum melanjutkan.
LicenseAccepted=Saya &setuju dengan kesepakatan tersebut
LicenseNotAccepted=Saya &tidak setuju dengan kesepakatan tersebut

; *** "Information" wizard pages
WizardInfoBefore=Informasi
InfoBeforeLabel=Silahkan baca terlebih dulu informasi penting berikut sebelum melanjutkan.
InfoBeforeClickLabel=Jika anda sudah siap untuk melanjutkan, klik Lanjut.
WizardInfoAfter=Informasi
InfoAfterLabel=Silahkan baca terlebih dulu informasi penting berikut sebelum melanjutkan.
InfoAfterClickLabel=Jika anda sudah siap untuk melanjutkan, klik Lanjut.

; *** "User Information" wizard page
WizardUserInfo=Informasi Pengguna
UserInfoDesc=Silahkan masukkan informasi diri anda.
UserInfoName=&Nama User:
UserInfoOrg=&Organisasi:
UserInfoSerial=Nomor &Seri:
UserInfoNameRequired=Anda harus memasukkan nama.

; *** "Select Destination Directory" wizard page
WizardSelectDir=Pilih Direktori Tujuan
SelectDirDesc=Dimana aplikasi [name] akan anda install?
SelectDirLabel3=Aplikasi [name] akan diinstall pada folder berikut.
SelectDirBrowseLabel=Untuk melanjutkan, klik Next. Jika hendak memilih folder lainnya, klik Browse.
DiskSpaceMBLabel=Program membutuhkan minimal [mb] MB ruang pada disk.
ToUNCPathname=Instalasi tidak dapat dilakukan pada pathname UNC. Untuk menginstal melalui network, silahkan map network drive-nya terlebih dulu.
InvalidPath=Anda harus memasukkan path secara lengkap dengan drivenya; contoh:%n%nC:\APP%n%natau UNC path dalam format:%n%n\\server\share
InvalidDrive=Drive atau path UNC yang anda pilih tidak ada atau tidak dapat diakses. Silahkan pilih yang lainnya.
DiskSpaceWarningTitle=Ruang disk tidak mencukupi
DiskSpaceWarning=Instalasi membutuhkan paling tidak %1 KB ruang disk untuk melakukan proses, drive yang terpilih hanya memliki %2 KB.%n%nAkan dilanjutkan saja?
DirNameTooLong=Nama folder atau path tersebut terlalu panjang.
InvalidDirName=Nama folder tersebut tidak valid.
BadDirName32=Nama direktori tidak boleh menyertakan karakter berikut:%n%n%1
DirExistsTitle=Direktori tersebut sudah ada
DirExists=Direktori:%n%n%1%n%nsudah ada. Akan melanjutkan instalasi pada direktori tersebut?
DirDoesntExistTitle=Direktori Tidak Valid
DirDoesntExist=Direktori:%n%n%1%n%ntidak valid. Apakah direktori tersebut akan dibuat ulang?

; *** "Select Components" wizard page
WizardSelectComponents=Pilih Komponen
SelectComponentsDesc=Komponen apa saja yang akan anda install?
SelectComponentsLabel2=Pilih komponen yang ingin anda install; hapus komponen yang tidak ingin anda install. Klik Lanjut untuk melanjutkan proses.
FullInstallation=Instalasi Penuh
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Instalasi Ramping
CustomInstallation=Kustomisasi Instalasi
NoUninstallWarningTitle=Komponent Sudah Ada
NoUninstallWarning=Instalasi mendeteksi komponen berikut sudah terinstall pada komputer:%n%n%1%n%nTidak memilih komponen-komponen tersebut tidak akan menghapus keberadaannya.%n%nAnda akan melanjutkan?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Pilihan tersebut membutuhkan paling tidak [mb] MB ruang disk.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Pilih Task Tambahan
SelectTasksDesc=Task mana saja yang akan di jalankan?
SelectTasksLabel2=Pilih task tambahan yang ingin dijalankan dalam prosedur instalasi selama menginstalasikan [name], kemudian klik Lanjut.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Pilih Folder Start Menu
SelectStartMenuFolderDesc=Di folder mana shortcut program akan diletakkan?
SelectStartMenuFolderLabel3=Setup akan membuat shortcut program pada folder Start Menu berikut.
SelectStartMenuFolderBrowseLabel=Untuk melanjutkan, klik Next. Jika ingin memilih folder lainnya, klik Browse.
MustEnterGroupName=Anda harus memasukkan nama foldernya.
GroupNameTooLong=Nama folder atau path tersebut terlalu panjang.
InvalidGroupName=Nama folder tersebut tidak valid.
BadGroupName=Nama direktori tidak boleh menyertakan karakter berikut:%n%n%1
NoProgramGroupCheck2=&Jangan buat folder Start Menu

; *** "Ready to Install" wizard page
WizardReady=Siap Memulai Instalasi
ReadyLabel1=Proses instalasi aplikasi [name] siap dimulai.
ReadyLabel2a=Klik Instal untuk memulai proses, atau klik Kembali jika ingin mereview atau mengubah setting yang ada.
ReadyLabel2b=Klik Instal untuk memulai proses instalasi.
ReadyMemoUserInfo=Informasi User:
ReadyMemoDir=Direktori Tujuan:
ReadyMemoType=Tipe Instalasi:
ReadyMemoComponents=Komponen Terpilih:
ReadyMemoGroup=Folder Start Menu:
ReadyMemoTasks=Task Tambahan:

; *** "Preparing to Install" wizard page
WizardPreparing=Mempersiapkan Proses Instalasi
PreparingDesc=Setup sedang mempersiapkan instalasi [name] pada komputer anda.
PreviousInstallNotCompleted=Instalasi/Deinstalasi program sebelumnya belum selesai. Anda akan memerlukan restart komputer anda untuk memastikan instalasi/deinstalasi selesai.%n%nSetelah merestart komputer, jalankan instalasi lagi untuk menyelesaikan proses instalasi aplikasi [name].
CannotContinue=Instalasi tidak dapat dilanjutkan. Silahkan tekan Batal untuk keluar.

; *** "Installing" wizard page
WizardInstalling=Sedang Menginstal
InstallingLabel=Mohon tunggu selama proses instalasi [name] pada komputer anda.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Finalisasi Instalasi Aplikasi [name]
FinishedLabelNoIcons=Proses instalasi aplikasi [name] telah selesai.
FinishedLabel=Setup telah selesai menginstalkan [name] pada komputer anda. Aplikasi tersebut bisa dijalankan dengan memilih icon yang telah terinstall.
ClickFinish=Klik Selesai untuk keluar dari setup.
FinishedRestartLabel=Untuk menyelesaikan proses instalasi [name], Setup harus merestart komputer anda. Komputer akan direstart sekarang?
FinishedRestartMessage=Untuk menyelesaikan proses instalasi [name], Setup harus merestart komputer anda.%n%nKomputer akan direstart sekarang?
ShowReadmeCheck=Ya, Saya ingin membaca file README
YesRadio=&Ya, Restart komputer sekarang juga
NoRadio=&Tidak, Nanti saja saya restart komputer sendiri
; used for example as 'Run MyProg.exe'
RunEntryExec=Run %1
; used for example as 'View Readme.txt'
RunEntryShellExec=View %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Setup Membutuhkan Disk Berikutnya
SelectDiskLabel2=Silahkan masukkan Disk %1 dan klik OK.%n%nJika file pada disk ditemukan pada folder berbeda dari yang ditampilkan, Masukkan path yang benar atau klik Browse.
PathLabel=&Path:
FileNotInDir2=File "%1" tidak ditemukan di "%2". Masukkan disk yang benar atau pilih folder yang lain.
SelectDirectoryLabel=Spesifikasikan lokasi disk berikutnya.

; *** Installation phase messages
SetupAborted=Setup belum selesai.%n%nSilahkan koreksi kesalahan yang ada dan jalankan Setup lagi.
EntryAbortRetryIgnore=Klik Retry untuk mencoba lagi, Ignore untuk mengindahkannya, atau Abort untuk membatalkan instalasi.

; *** Installation status messages
StatusCreateDirs=Membuat direktori...
StatusExtractFiles=Mengextract file...
StatusCreateIcons=Membuat shortcut program...
StatusCreateIniEntries=Memasukkan entri pada file INI...
StatusCreateRegistryEntries=Memasukkan entri pada registry...
StatusRegisterFiles=Meregistrasikan file...
StatusSavingUninstall=Menyimpan informasi untuk deinstalasi...
StatusRunProgram=Finalisasi instalasi...
StatusRollback=Mengembalikan perubahan yang dilakukan...

; *** Misc. errors
ErrorInternal2=Kesalahan internal: %1
ErrorFunctionFailedNoCode=%1 gagal
ErrorFunctionFailed=%1 gagal; kode %2
ErrorFunctionFailedWithMessage=%1 gagal; kode %2.%n%3
ErrorExecutingProgram=Gagal mengeksekusi file:%n%1

; *** Registry errors
ErrorRegOpenKey=Gagal membuka registry key:%n%1\%2
ErrorRegCreateKey=Gagal membuat registry key:%n%1\%2
ErrorRegWriteKey=Gagal menulis to registry key:%n%1\%2

; *** INI errors
ErrorIniEntry=Gagal membuat entry INI pada file "%1".

; *** File copying errors
FileAbortRetryIgnore=Klik Retry untuk mencoba lagi, Ignore untuk mengindahkan file ini (tidak direkomendasikan), atau Abort untuk membatalkan instalasi.
FileAbortRetryIgnore2=Klik Retry untuk mencoba lagi, Ignore untuk mengindahkan file ini (tidak direkomendasikan), atau Abort untuk membatalkan instalasi.
SourceIsCorrupted=File asal telah rusak
SourceDoesntExist=File asal "%1" tidak valid
ExistingFileReadOnly=File yang ada tercatat sebagai read-only.%n%nKlik Retry untuk membuang catatan atribut read-only dan coba lagi, Ignore untuk mengindahkan file ini, atau Abort untuk membatalkan instalasi.
ErrorReadingExistingDest=Kesalahan muncul saat mencoba membaca file berikut:
FileExists=File sudah ada.%n%nApakah file tersebut akan ditimpa saja?
ExistingFileNewer=File yang sudah ada lebih baru dari yang hendak diinstal. Direkomendasikan untuk membiarkan file tersebut.%n%nApakah akan membiarkan file tersebut?
ErrorChangingAttr=Kesalahan muncul saat mencoba merubah atribut file berikut:
ErrorCreatingTemp=Kesalahan muncul saat mencoba membuat file pada direktori tujuan:
ErrorReadingSource=Kesalahan muncul saat mencoba membaca file asal:
ErrorCopying=Kesalahan muncul saat mencoba meng-copy file:
ErrorReplacingExistingFile=Kesalahan muncul saat mencoba me-replace file:
ErrorRestartReplace=RestartReplace gagal:
ErrorRenamingTemp=Kesalahan muncul saat mencoba me-rename file pada direktori tujuan:
ErrorRegisterServer=Gagal meregister file DLL/OCX: %1
ErrorRegisterServerMissingExport=DllRegisterServer export tidak ditemukan
ErrorRegisterTypeLib=Gagal meregister type library: %1

; *** Post-installation errors
ErrorOpeningReadme=Kesalahan muncul saat mencoba membuka file README.
ErrorRestartingComputer=Setup tidak bisa meresart komputer anda. Silahkan lakukan secara manual.

; *** Uninstaller messages
UninstallNotFound=File "%1" tidak ditemukan. Tidak dapat melanjutkan proses deinstalasi
UninstallOpenError=File "%1" tidak bisa dibuka. Tidak dapat melanjutkan proses deinstalasi
UninstallUnsupportedVer=Arsip log file "%1" tidak dalam format yang dikenali oleh versi uninstaller ini. Deinstalasi tidak dapat dilanjutkan
UninstallUnknownEntry=Entri tidak dikenal (%1) ditemukan pada arsip log deinstalasi
ConfirmUninstall=Yakinkah anda akan menghapus %1 beserta seluruh komponen yang terkait?
UninstallOnlyOnWin64=Instalasi ini hanya bisa melakukan deinstalasi pada Windows 64-bit.
OnlyAdminCanUninstall=Prosesi deinstalasi hanya bisa dilakukan oleh user yang memiliki kewenangan administratif.
UninstallStatusLabel=Mohon tunggu selama aplikasi %1 sedang dihapus dari komputer anda.
UninstalledAll=Aplikasi %1 berhasil dihapus dari komputer anda.
UninstalledMost=Deinstalasi %1 selesai.%n%nBeberapa elemen tidak berhasil dihapus dan harus anda lakukan secara manual.
UninstalledAndNeedsRestart=Untuk menyelesaikan deinstalasi aplikasi %1, komputer anda harus direstart.%n%nKomputer akan direstart sekarang?
UninstallDataCorrupted=File "%1" telah rusak. Tidak dapat melanjutkan proses deinstalasi

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Hapus Shared File?
ConfirmDeleteSharedFile2=Sistem mengindikasikan bahwa shared file berikut tidak digunakan oleh program apapun. Apakah anda akan menghapus shared file ini?%n%nJika ternyata ada program yang masih membutuhkan file tersebut dan ternyata terhapus, Program tersebut tidak akan berjalan sebagaimana mestinya. Jika anda ragu, Pilih Tidak. Membiarkan file pada sistem anda tidak akan menyebabkan kerusakan apapun.
SharedFileNameLabel=Nama file:
SharedFileLocationLabel=Lokasi:
WizardUninstalling=Status Deinstalasi
StatusUninstalling=Deinstalasi %1...

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 versi %2
AdditionalIcons=Tambahan icon:
CreateDesktopIcon=Buat icon di &desktop
CreateQuickLaunchIcon=Buat icon &Quick Launch
ProgramOnTheWeb=%1 di Web
UninstallProgram=Deinstal %1
LaunchProgram=Jalankan %1
AssocFileExtension=&Asosiasikan %1 dengan ekstensi file %2
AssocingFileExtension=Mengasosiasikan %1 dengan ekstensi file %2 ...