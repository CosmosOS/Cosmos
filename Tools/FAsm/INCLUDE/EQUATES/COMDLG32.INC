
; COMDLG32.DLL structures and constants

struct OPENFILENAME
  lStructSize       dd ?
  hwndOwner         dd ?
  hInstance         dd ?
  lpstrFilter       dd ?
  lpstrCustomFilter dd ?
  nMaxCustFilter    dd ?
  nFilterIndex      dd ?
  lpstrFile         dd ?
  nMaxFile          dd ?
  lpstrFileTitle    dd ?
  nMaxFileTitle     dd ?
  lpstrInitialDir   dd ?
  lpstrTitle        dd ?
  Flags             dd ?
  nFileOffset       dw ?
  nFileExtension    dw ?
  lpstrDefExt       dd ?
  lCustData         dd ?
  lpfnHook          dd ?
  lpTemplateName    dd ?
ends

struct CHOOSECOLOR
  lStructSize    dd ?
  hwndOwner      dd ?
  hInstance      dd ?
  rgbResult      dd ?
  lpCustColors   dd ?
  Flags          dd ?
  lCustData      dd ?
  lpfnHook       dd ?
  lpTemplateName dd ?
ends

struct FINDREPLACE
  lStructSize      dd ?
  hwndOwner        dd ?
  hInstance        dd ?
  Flags            dd ?
  lpstrFindWhat    dd ?
  lpstrReplaceWith dd ?
  wFindWhatLen     dw ?
  wReplaceWithLen  dw ?
  lCustData        dd ?
  lpfnHook         dd ?
  lpTemplateName   dd ?
ends

struct CHOOSEFONT
  lStructSize    dd ?
  hwndOwner      dd ?
  hDC            dd ?
  lpLogFont      dd ?
  iPointSize     dd ?
  Flags          dd ?
  rgbColors      dd ?
  lCustData      dd ?
  lpfnHook       dd ?
  lpTemplateName dd ?
  hInstance      dd ?
  lpszStyle      dd ?
  nFontType      dw ?
  wReserved      dw ?
  nSizeMin       dd ?
  nSizeMax       dd ?
ends

struct PRINTDLG
  lStructSize         dd ?
  hwndOwner           dd ?
  hDevMode            dd ?
  hDevNames           dd ?
  hDC                 dd ?
  Flags               dd ?
  nFromPage           dw ?
  nToPage             dw ?
  nMinPage            dw ?
  nMaxPage            dw ?
  nCopies             dw ?
  hInstance           dd ?
  lCustData           dd ?
  lpfnPrintHook       dd ?
  lpfnSetupHook       dd ?
  lpPrintTemplateName dd ?
  lpSetupTemplateName dd ?
  hPrintTemplate      dd ?
  hSetupTemplate      dd ?
ends

struct DEVNAMES
  wDriverOffset dw ?
  wDeviceOffset dw ?
  wOutputOffset dw ?
  wDefault      dw ?
ends

struct PAGESETUPDLG
  lStructSize             dd ?
  hwndOwner               dd ?
  hDevMode                dd ?
  hDevNames               dd ?
  Flags                   dd ?
  ptPaperSize             POINT
  rtMinMargin             RECT
  rtMargin                RECT
  hInstance               dd ?
  lCustData               dd ?
  lpfnPageSetupHook       dd ?
  lpfnPagePaintHook       dd ?
  lpPageSetupTemplateName dd ?
  hPageSetupTemplate      dd ?
ends

; OPENFILENAME flags

OFN_READONLY             = 000001h
OFN_OVERWRITEPROMPT      = 000002h
OFN_HIDEREADONLY         = 000004h
OFN_NOCHANGEDIR          = 000008h
OFN_SHOWHELP             = 000010h
OFN_ENABLEHOOK           = 000020h
OFN_ENABLETEMPLATE       = 000040h
OFN_ENABLETEMPLATEHANDLE = 000080h
OFN_NOVALIDATE           = 000100h
OFN_ALLOWMULTISELECT     = 000200h
OFN_EXTENSIONDIFFERENT   = 000400h
OFN_PATHMUSTEXIST        = 000800h
OFN_FILEMUSTEXIST        = 001000h
OFN_CREATEPROMPT         = 002000h
OFN_SHAREAWARE           = 004000h
OFN_NOREADONLYRETURN     = 008000h
OFN_NOTESTFILECREATE     = 010000h
OFN_NONETWORKBUTTON      = 020000h
OFN_NOLONGNAMES          = 040000h
OFN_EXPLORER             = 080000h
OFN_NODEREFERENCELINKS   = 100000h
OFN_LONGNAMES            = 200000h

; Common dialog notifications

CDN_FIRST          = -601
CDN_LAST           = -699
CDN_INITDONE       = CDN_FIRST - 0
CDN_SELCHANGE      = CDN_FIRST - 1
CDN_FOLDERCHANGE   = CDN_FIRST - 2
CDN_SHAREVIOLATION = CDN_FIRST - 3
CDN_HELP           = CDN_FIRST - 4
CDN_FILEOK         = CDN_FIRST - 5
CDN_TYPECHANGE     = CDN_FIRST - 6

; Common dialog messages

CDM_FIRST           = WM_USER + 100
CDM_LAST            = WM_USER + 200
CDM_GETSPEC         = CDM_FIRST + 0
CDM_GETFILEPATH     = CDM_FIRST + 1
CDM_GETFOLDERPATH   = CDM_FIRST + 2
CDM_GETFOLDERIDLIST = CDM_FIRST + 3
CDM_SETCONTROLTEXT  = CDM_FIRST + 4
CDM_HIDECONTROL     = CDM_FIRST + 5
CDM_SETDEFEXT       = CDM_FIRST + 6

; CHOOSECOLOR flags

CC_RGBINIT              = 001h
CC_FULLOPEN             = 002h
CC_PREVENTFULLOPEN      = 004h
CC_SHOWHELP             = 008h
CC_ENABLEHOOK           = 010h
CC_ENABLETEMPLATE       = 020h
CC_ENABLETEMPLATEHANDLE = 040h
CC_SOLIDCOLOR           = 080h
CC_ANYCOLOR             = 100h

; FINDREPLACE flags

FR_DOWN                 = 00001h
FR_WHOLEWORD            = 00002h
FR_MATCHCASE            = 00004h
FR_FINDNEXT             = 00008h
FR_REPLACE              = 00010h
FR_REPLACEALL           = 00020h
FR_DIALOGTERM           = 00040h
FR_SHOWHELP             = 00080h
FR_ENABLEHOOK           = 00100h
FR_ENABLETEMPLATE       = 00200h
FR_NOUPDOWN             = 00400h
FR_NOMATCHCASE          = 00800h
FR_NOWHOLEWORD          = 01000h
FR_ENABLETEMPLATEHANDLE = 02000h
FR_HIDEUPDOWN           = 04000h
FR_HIDEMATCHCASE        = 08000h
FR_HIDEWHOLEWORD        = 10000h

; CHOOSEFONT flags

CF_SCREENFONTS          = 0000001h
CF_PRINTERFONTS         = 0000002h
CF_BOTH                 = CF_SCREENFONTS or CF_PRINTERFONTS
CF_SHOWHELP             = 0000004h
CF_ENABLEHOOK           = 0000008h
CF_ENABLETEMPLATE       = 0000010h
CF_ENABLETEMPLATEHANDLE = 0000020h
CF_INITTOLOGFONTSTRUCT  = 0000040h
CF_USESTYLE             = 0000080h
CF_EFFECTS              = 0000100h
CF_APPLY                = 0000200h
CF_ANSIONLY             = 0000400h
CF_SCRIPTSONLY          = CF_ANSIONLY
CF_NOVECTORFONTS        = 0000800h
CF_NOOEMFONTS           = CF_NOVECTORFONTS
CF_NOSIMULATIONS        = 0001000h
CF_LIMITSIZE            = 0002000h
CF_FIXEDPITCHONLY       = 0004000h
CF_WYSIWYG              = 0008000h
CF_FORCEFONTEXIST       = 0010000h
CF_SCALABLEONLY         = 0020000h
CF_TTONLY               = 0040000h
CF_NOFACESEL            = 0080000h
CF_NOSTYLESEL           = 0100000h
CF_NOSIZESEL            = 0200000h
CF_SELECTSCRIPT         = 0400000h
CF_NOSCRIPTSEL          = 0800000h
CF_NOVERTFONTS          = 1000000h

; ChooseFont messages

WM_CHOOSEFONT_GETLOGFONT = WM_USER + 1
WM_CHOOSEFONT_SETLOGFONT = WM_USER + 101
WM_CHOOSEFONT_SETFLAGS   = WM_USER + 102

; PRINTDLG flags

PD_ALLPAGES                   = 000000h
PD_SELECTION                  = 000001h
PD_PAGENUMS                   = 000002h
PD_NOSELECTION                = 000004h
PD_NOPAGENUMS                 = 000008h
PD_COLLATE                    = 000010h
PD_PRINTTOFILE                = 000020h
PD_PRINTSETUP                 = 000040h
PD_NOWARNING                  = 000080h
PD_RETURNDC                   = 000100h
PD_RETURNIC                   = 000200h
PD_RETURNDEFAULT              = 000400h
PD_SHOWHELP                   = 000800h
PD_ENABLEPRINTHOOK            = 001000h
PD_ENABLESETUPHOOK            = 002000h
PD_ENABLEPRINTTEMPLATE        = 004000h
PD_ENABLESETUPTEMPLATE        = 008000h
PD_ENABLEPRINTTEMPLATEHANDLE  = 010000h
PD_ENABLESETUPTEMPLATEHANDLE  = 020000h
PD_USEDEVMODECOPIES           = 040000h
PD_USEDEVMODECOPIESANDCOLLATE = 040000h
PD_DISABLEPRINTTOFILE         = 080000h
PD_HIDEPRINTTOFILE            = 100000h
PD_NONETWORKBUTTON            = 200000h

; PAGESETUPDLG flags

PSD_DEFAULTMINMARGINS             = 000000h
PSD_INWININIINTLMEASURE           = 000000h
PSD_MINMARGINS                    = 000001h
PSD_MARGINS                       = 000002h
PSD_INTHOUSANDTHSOFINCHES         = 000004h
PSD_INHUNDREDTHSOFMILLIMETERS     = 000008h
PSD_DISABLEMARGINS                = 000010h
PSD_DISABLEPRINTER                = 000020h
PSD_NOWARNING                     = 000080h
PSD_DISABLEORIENTATION            = 000100h
PSD_RETURNDEFAULT                 = 000400h
PSD_DISABLEPAPER                  = 000200h
PSD_SHOWHELP                      = 000800h
PSD_ENABLEPAGESETUPHOOK           = 002000h
PSD_ENABLEPAGESETUPTEMPLATE       = 008000h
PSD_ENABLEPAGESETUPTEMPLATEHANDLE = 020000h
PSD_ENABLEPAGEPAINTHOOK           = 040000h
PSD_DISABLEPAGEPAINTING           = 080000h
PSD_NONETWORKBUTTON               = 200000h

; PageSetupDlg messages

WM_PSD_PAGESETUPDLG   = WM_USER
WM_PSD_FULLPAGERECT   = WM_USER + 1
WM_PSD_MINMARGINRECT  = WM_USER + 2
WM_PSD_MARGINRECT     = WM_USER + 3
WM_PSD_GREEKTEXTRECT  = WM_USER + 4
WM_PSD_ENVSTAMPRECT   = WM_USER + 5
WM_PSD_YAFULLPAGERECT = WM_USER + 6

; Common dialog error codes

CDERR_DIALOGFAILURE    = 0FFFFh
CDERR_GENERALCODES     = 00000h
CDERR_STRUCTSIZE       = 00001h
CDERR_INITIALIZATION   = 00002h
CDERR_NOTEMPLATE       = 00003h
CDERR_NOHINSTANCE      = 00004h
CDERR_LOADSTRFAILURE   = 00005h
CDERR_FINDRESFAILURE   = 00006h
CDERR_LOADRESFAILURE   = 00007h
CDERR_LOCKRESFAILURE   = 00008h
CDERR_MEMALLOCFAILURE  = 00009h
CDERR_MEMLOCKFAILURE   = 0000Ah
CDERR_NOHOOK           = 0000Bh
CDERR_REGISTERMSGFAIL  = 0000Ch
PDERR_PRINTERCODES     = 01000h
PDERR_SETUPFAILURE     = 01001h
PDERR_PARSEFAILURE     = 01002h
PDERR_RETDEFFAILURE    = 01003h
PDERR_LOADDRVFAILURE   = 01004h
PDERR_GETDEVMODEFAIL   = 01005h
PDERR_INITFAILURE      = 01006h
PDERR_NODEVICES        = 01007h
PDERR_NODEFAULTPRN     = 01008h
PDERR_DNDMMISMATCH     = 01009h
PDERR_CREATEICFAILURE  = 0100Ah
PDERR_PRINTERNOTFOUND  = 0100Bh
PDERR_DEFAULTDIFFERENT = 0100Ch
CFERR_CHOOSEFONTCODES  = 02000h
CFERR_NOFONTS          = 02001h
CFERR_MAXLESSTHANMIN   = 02002h
FNERR_FILENAMECODES    = 03000h
FNERR_SUBCLASSFAILURE  = 03001h
FNERR_INVALIDFILENAME  = 03002h
FNERR_BUFFERTOOSMALL   = 03003h
FRERR_FINDREPLACECODES = 04000h
FRERR_BUFFERLENGTHZERO = 04001h
CCERR_CHOOSECOLORCODES = 05000h
