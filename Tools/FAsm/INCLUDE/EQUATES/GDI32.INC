
; GDI32.DLL structures and constants

struct SIZE
  cx dd ?
  cy dd ?
ends

struct BITMAP
  bmType       dd ?
  bmWidth      dd ?
  bmHeight     dd ?
  bmWidthBytes dd ?
  bmPlanes     dw ?
  bmBitsPixel  dw ?
  bmBits       dd ?
ends

struct BITMAPCOREHEADER
  bcSize     dd ?
  bcWidth    dw ?
  bcHeight   dw ?
  bcPlanes   dw ?
  bcBitCount dw ?
ends

struct BITMAPINFOHEADER
  biSize          dd ?
  biWidth         dd ?
  biHeight        dd ?
  biPlanes        dw ?
  biBitCount      dw ?
  biCompression   dd ?
  biSizeImage     dd ?
  biXPelsPerMeter dd ?
  biYPelsPerMeter dd ?
  biClrUsed       dd ?
  biClrImportant  dd ?
ends

struct BITMAPFILEHEADER
  bfType      dw ?
  bfSize      dd ?
  bfReserved1 dw ?
  bfReserved2 dw ?
  bfOffBits   dd ?
ends

struct TEXTMETRIC
  tmHeight           dd ?
  tmAscent           dd ?
  tmDescent          dd ?
  tmInternalLeading  dd ?
  tmExternalLeading  dd ?
  tmAveCharWidth     dd ?
  tmMaxCharWidth     dd ?
  tmWeight           dd ?
  tmOverhang         dd ?
  tmDigitizedAspectX dd ?
  tmDigitizedAspectY dd ?
  tmFirstChar        db ?
  tmLastChar         db ?
  tmDefaultChar      db ?
  tmBreakChar        db ?
  tmItalic           db ?
  tmUnderlined       db ?
  tmStruckOut        db ?
  tmPitchAndFamily   db ?
  tmCharSet          db ?
ends

struct LOGBRUSH
  lbStyle dd ?
  lbColor dd ?
  lbHatch dd ?
ends

struct LOGPEN
  lopnStyle dd ?
  lopnWidth POINT
  lopnColor dd ?
ends

struct EXTLOGPEN
  elpPenStyle   dd ?
  elpWidth      dd ?
  elpBrushStyle dd ?
  elpColor      dd ?
  elpHatch      dd ?
  elpNumEntries dd ?
  elpStyleEntry dd ?
ends

struct LOGFONT
  lfHeight         dd ?
  lfWidth          dd ?
  lfEscapement     dd ?
  lfOrientation    dd ?
  lfWeight         dd ?
  lfItalic         db ?
  lfUnderline      db ?
  lfStrikeOut      db ?
  lfCharSet        db ?
  lfOutPrecision   db ?
  lfClipPrecision  db ?
  lfQuality        db ?
  lfPitchAndFamily db ?
  lfFaceName       db 32 dup (?)
ends

struct ENUMLOGFONT
  elfLogFont  LOGFONT
  elfFullName db 64 dup (?)
  elfStyle    db 32 dup (?)
ends

struct ENUMLOGFONTEX
  elfLogFont  LOGFONT
  elfFullName db 64 dup (?)
  elfStyle    db 32 dup (?)
  elfScript   db 32 dup (?)
ends

struct PIXELFORMATDESCRIPTOR
  nSize           dw ?
  nVersion        dw ?
  dwFlags         dd ?
  iPixelType      db ?
  cColorBits      db ?
  cRedBits        db ?
  cRedShift       db ?
  cGreenBits      db ?
  cGreenShift     db ?
  cBlueBits       db ?
  cBlueShift      db ?
  cAlphaBits      db ?
  cAlphaShift     db ?
  cAccumBits      db ?
  cAccumRedBits   db ?
  cAccumGreenBits db ?
  cAccumBlueBits  db ?
  cAccumAlphaBits db ?
  cDepthBits      db ?
  cStencilBits    db ?
  cAuxBuffers     db ?
  iLayerType      db ?
  bReserved       db ?
  dwLayerMask     dd ?
  dwVisibleMask   dd ?
  dwDamageMask    dd ?
ends

; General constants

GDI_ERROR  = 0FFFFFFFFh
HGDI_ERROR = 0FFFFFFFFh

; Binary raster operations

R2_BLACK       = 1
R2_NOTMERGEPEN = 2
R2_MASKNOTPEN  = 3
R2_NOTCOPYPEN  = 4
R2_MASKPENNOT  = 5
R2_NOT         = 6
R2_XORPEN      = 7
R2_NOTMASKPEN  = 8
R2_MASKPEN     = 9
R2_NOTXORPEN   = 10
R2_NOP         = 11
R2_MERGENOTPEN = 12
R2_COPYPEN     = 13
R2_MERGEPENNOT = 14
R2_MERGEPEN    = 15
R2_WHITE       = 16

; Raster operations

SRCCOPY     = 00CC0020h
SRCPAINT    = 00EE0086h
SRCAND      = 008800C6h
SRCINVERT   = 00660046h
SRCERASE    = 00440328h
NOTSRCCOPY  = 00330008h
NOTSRCERASE = 001100A6h
MERGECOPY   = 00C000CAh
MERGEPAINT  = 00BB0226h
PATCOPY     = 00F00021h
PATPAINT    = 00FB0A09h
PATINVERT   = 005A0049h
DSTINVERT   = 00550009h
BLACKNESS   = 00000042h
WHITENESS   = 00FF0062h

; Region flags

ERROR         = 0
NULLREGION    = 1
SIMPLEREGION  = 2
COMPLEXREGION = 3

; CombineRgn styles

RGN_AND  = 1
RGN_OR   = 2
RGN_XOR  = 3
RGN_DIFF = 4
RGN_COPY = 5

; StretchBlt modes

BLACKONWHITE = 1
WHITEONBLACK = 2
COLORONCOLOR = 3
HALFTONE     = 4
STRETCH_ANDSCANS    = BLACKONWHITE
STRETCH_ORSCANS     = WHITEONBLACK
STRETCH_DELETESCANS = COLORONCOLOR
STRETCH_HALFTONE    = HALFTONE

; PolyFill modes

ALTERNATE = 1
WINDING   = 2

; Background modes

TRANSPARENT = 1
OPAQUE      = 2

; Point types

PT_CLOSEFIGURE = 1
PT_LINETO      = 2
PT_BEZIERTO    = 4
PT_MOVETO      = 6

; Mapping modes

MM_TEXT        = 1
MM_LOMETRIC    = 2
MM_HIMETRIC    = 3
MM_LOENGLISH   = 4
MM_HIENGLISH   = 5
MM_TWIPS       = 6
MM_ISOTROPIC   = 7
MM_ANISOTROPIC = 8

; Coordinate modes

ABSOLUTE = 1
RELATIVE = 2

; Stock logical objects

WHITE_BRUSH         = 0
LTGRAY_BRUSH        = 1
GRAY_BRUSH          = 2
DKGRAY_BRUSH        = 3
BLACK_BRUSH         = 4
NULL_BRUSH          = 5
HOLLOW_BRUSH        = NULL_BRUSH
WHITE_PEN           = 6
BLACK_PEN           = 7
NULL_PEN            = 8
OEM_FIXED_FONT      = 10
ANSI_FIXED_FONT     = 11
ANSI_VAR_FONT       = 12
SYSTEM_FONT         = 13
DEVICE_DEFAULT_FONT = 14
DEFAULT_PALETTE     = 15
SYSTEM_FIXED_FONT   = 16
DEFAULT_GUI_FONT    = 17

; Brush styles

BS_SOLID         = 0
BS_NULL          = 1
BS_HOLLOW        = BS_NULL
BS_HATCHED       = 2
BS_PATTERN       = 3
BS_INDEXED       = 4
BS_DIBPATTERN    = 5
BS_DIBPATTERNPT  = 6
BS_PATTERN8X8    = 7
BS_DIBPATTERN8X8 = 8
BS_MONOPATTERN   = 9

; Hatch styles

HS_HORIZONTAL = 0
HS_VERTICAL   = 1
HS_FDIAGONAL  = 2
HS_BDIAGONAL  = 3
HS_CROSS      = 4
HS_DIAGCROSS  = 5

; Pen styles

PS_SOLID         = 0
PS_DASH          = 1
PS_DOT           = 2
PS_DASHDOT       = 3
PS_DASHDOTDOT    = 4
PS_NULL          = 5
PS_INSIDEFRAME   = 6
PS_USERSTYLE     = 7
PS_ALTERNATE     = 8
PS_ENDCAP_ROUND  = 0
PS_ENDCAP_SQUARE = 100h
PS_ENDCAP_FLAT   = 200h
PS_JOIN_ROUND    = 0
PS_JOIN_BEVEL    = 1000h
PS_JOIN_MITER    = 2000h
PS_COSMETIC      = 0
PS_GEOMETRIC     = 010000h

; Arc directions

AD_COUNTERCLOCKWISE = 1
AD_CLOCKWISE        = 2

; Text alignment options

TA_NOUPDATECP = 0
TA_UPDATECP   = 1
TA_LEFT       = 0
TA_RIGHT      = 2
TA_CENTER     = 6
TA_TOP        = 0
TA_BOTTOM     = 8
TA_BASELINE   = 24
TA_RTLREADING = 100h
VTA_BASELINE  = TA_BASELINE
VTA_LEFT      = TA_BOTTOM
VTA_RIGHT     = TA_TOP
VTA_CENTER    = TA_CENTER
VTA_BOTTOM    = TA_RIGHT
VTA_TOP       = TA_LEFT

; ExtTextOut options

ETO_OPAQUE         = 0002h
ETO_CLIPPED        = 0004h
ETO_GLYPH_INDEX    = 0010h
ETO_RTLREADING     = 0080h
ETO_IGNORELANGUAGE = 1000h

; Bitmap compression types

BI_RGB       = 0
BI_RLE8      = 1
BI_RLE4      = 2
BI_BITFIELDS = 3

; tmPitchAndFamily flags

TMPF_FIXED_PITCH = 1
TMPF_VECTOR      = 2
TMPF_TRUETYPE    = 4
TMPF_DEVICE      = 8

; Font output precision values

OUT_DEFAULT_PRECIS        = 0
OUT_STRING_PRECIS         = 1
OUT_CHARACTER_PRECIS      = 2
OUT_STROKE_PRECIS         = 3
OUT_TT_PRECIS             = 4
OUT_DEVICE_PRECIS         = 5
OUT_RASTER_PRECIS         = 6
OUT_TT_ONLY_PRECIS        = 7
OUT_OUTLINE_PRECIS        = 8
OUT_SCREEN_OUTLINE_PRECIS = 9

; Font clipping precision values

CLIP_DEFAULT_PRECIS   = 0
CLIP_CHARACTER_PRECIS = 1
CLIP_STROKE_PRECIS    = 2
CLIP_LH_ANGLES        = 10h
CLIP_TT_ALWAYS        = 20h
CLIP_EMBEDDED         = 80h

; Font output quality values

DEFAULT_QUALITY        = 0
DRAFT_QUALITY          = 1
PROOF_QUALITY          = 2
NONANTIALIASED_QUALITY = 3
ANTIALIASED_QUALITY    = 4

; Font pitch values

DEFAULT_PITCH  = 0
FIXED_PITCH    = 1
VARIABLE_PITCH = 2
MONO_FONT      = 8

; Font families

FF_DONTCARE   = 00h
FF_ROMAN      = 10h
FF_SWISS      = 20h
FF_MODERN     = 30h
FF_SCRIPT     = 40h
FF_DECORATIVE = 50h

; Font weights

FW_DONTCARE   = 0
FW_THIN       = 100
FW_EXTRALIGHT = 200
FW_LIGHT      = 300
FW_NORMAL     = 400
FW_MEDIUM     = 500
FW_SEMIBOLD   = 600
FW_BOLD       = 700
FW_EXTRABOLD  = 800
FW_HEAVY      = 900
FW_ULTRALIGHT = FW_EXTRALIGHT
FW_REGULAR    = FW_NORMAL
FW_DEMIBOLD   = FW_SEMIBOLD
FW_ULTRABOLD  = FW_EXTRABOLD
FW_BLACK      = FW_HEAVY

; Character set values

ANSI_CHARSET        = 0
DEFAULT_CHARSET     = 1
SYMBOL_CHARSET      = 2
SHIFTJIS_CHARSET    = 128
HANGEUL_CHARSET     = 129
GB2312_CHARSET      = 134
CHINESEBIG5_CHARSET = 136
OEM_CHARSET         = 255
JOHAB_CHARSET       = 130
HEBREW_CHARSET      = 177
ARABIC_CHARSET      = 178
GREEK_CHARSET       = 161
TURKISH_CHARSET     = 162
VIETNAMESE_CHARSET  = 163
THAI_CHARSET        = 222
EASTEUROPE_CHARSET  = 238
RUSSIAN_CHARSET     = 204
MAC_CHARSET         = 77
BALTIC_CHARSET      = 186

; Pixel format constants

PFD_TYPE_RGBA             = 0
PFD_TYPE_COLORINDEX       = 1
PFD_MAIN_PLANE            = 0
PFD_OVERLAY_PLANE         = 1
PFD_UNDERLAY_PLANE        = -1
PFD_DOUBLEBUFFER          = 1
PFD_STEREO                = 2
PFD_DRAW_TO_WINDOW        = 4
PFD_DRAW_TO_BITMAP        = 8
PFD_SUPPORT_GDI           = 10h
PFD_SUPPORT_OPENGL        = 20h
PFD_GENERIC_FORMAT        = 40h
PFD_NEED_PALETTE          = 80h
PFD_NEED_SYSTEM_PALETTE   = 100h
PFD_SWAP_EXCHANGE         = 200h
PFD_SWAP_COPY             = 400h
PFD_SWAP_LAYER_BUFFERS    = 800h
PFD_GENERIC_ACCELERATED   = 1000h
PFD_DEPTH_DONTCARE        = 20000000h
PFD_DOUBLEBUFFER_DONTCARE = 40000000h
PFD_STEREO_DONTCARE       = 80000000h
