/* tx.h
 *
 * Common I/O support routines for multiple Infocom story file utilities.
 *
 * Mark Howell 26 August 1992 howell_ma@movies.enet.dec.com
 *
 */

using zword_t = System.UInt16;
using zbyte_t = System.Byte;

using System;

namespace ZTools
{

    internal class BufferWithPointer
    {
        internal byte[] Buffer { get; private set; }
        internal int Pointer { get; set; }

        public BufferWithPointer(int size)
        {
            Buffer = new zbyte_t[size];
            Pointer = 0;
        }
    }

    public static class tx_h
    {
        /* Z types */

        /* Data file header format */

        internal class zheader_t
        {
            internal zbyte_t version;
            internal zbyte_t config;
            internal zword_t release;
            internal zword_t resident_size;
            internal zword_t start_pc;
            internal zword_t dictionary;
            internal zword_t objects;
            internal zword_t globals;
            internal zword_t dynamic_size;
            internal zword_t flags;
            internal zbyte_t[] serial = new zbyte_t[6];
            internal zword_t abbreviations;
            internal zword_t file_size;
            internal zword_t checksum;
            internal zbyte_t interpreter_number;
            internal zbyte_t interpreter_version;
            internal zbyte_t screen_rows;
            internal zbyte_t screen_columns;
            internal zword_t screen_width;
            internal zword_t screen_height;
            internal zbyte_t font_width;
            internal zbyte_t font_height;
            internal zword_t routines_offset;
            internal zword_t strings_offset;
            internal zbyte_t default_background;
            internal zbyte_t default_foreground;
            internal zword_t terminating_keys;
            internal zword_t line_width;
            internal zbyte_t specification_hi;
            internal zbyte_t specification_lo;
            internal zword_t alphabet;
            internal zword_t mouse_table;
            internal zbyte_t[] name = new zbyte_t[8];
        };

        internal static int H_VERSION = 0;
        internal static int H_CONFIG = 1;

        internal static int CONFIG_BYTE_SWAPPED = 0x01; /* Game data is byte swapped          - V3  */
        internal static int CONFIG_COLOUR = 0x01; /* Interpreter supports colour        - V5+ */
        internal static int CONFIG_TIME = 0x02; /* Status line displays time          - V3  */
        internal static int CONFIG_PICTURES = 0x02; /* Interpreter supports pictures      - V6  */
        internal static int CONFIG_BOLDFACE = 0x04; /* Interpreter supports bold text     - V4+ */
        internal static int CONFIG_TANDY = 0x08; /* Tandy licensed game                - V3  */
        internal static int CONFIG_EMPHASIS = 0x08; /* Interpreter supports text emphasis - V4+ */
        internal static int CONFIG_NOSTATUSLINE = 0x10; /* Interpreter has no status line     - V3  */
        internal static int CONFIG_FIXED_FONT = 0x10; /* Interpreter supports fixed font    - V4+ */
        internal static int CONFIG_WINDOWS = 0x20; /* Interpreter supports split screen  - V3  */
        internal static int CONFIG_PROPORTIONAL = 0x40; /* Interpreter uses proportional font - V3  */
        internal static int CONFIG_TIMEDINPUT = 0x80; /* Interpreter supports timed input   - V4+ */

        internal static int H_RELEASE = 2;
        internal static int H_RESIDENT_SIZE = 4;
        internal static int H_START_PC = 6;
        internal static int H_DICTIONARY = 8;
        internal static int H_OBJECTS = 10;
        internal static int H_GLOBALS = 12;
        internal static int H_DYNAMIC_SIZE = 14;
        internal static int H_FLAGS = 16;

        internal static int SCRIPTING_FLAG = 0x0001;
        internal static int FIXED_FONT_FLAG = 0x0002;
        internal static int REFRESH_FLAG = 0x0004;
        internal static int GRAPHICS_FLAG = 0x0008;
        internal static int OLD_SOUND_FLAG = 0x0010; /* V3 */
        internal static int UNDO_AVAILABLE_FLAG = 0x0010; /* V5 */
        internal static int MOUSE_FLAG = 0x0020;
        internal static int COLOUR_FLAG = 0x0040;
        internal static int NEW_SOUND_FLAG = 0x0080;
        internal static int MENU_FLAG = 0x0100;

        internal static int H_SERIAL = 18;
        internal static int H_ABBREVIATIONS = 24;
        internal static int H_FILE_SIZE = 26;
        internal static int H_CHECKSUM = 28;
        internal static int H_INTERPRETER_NUMBER = 30;

        internal static int INTERP_GENERIC = 0;
        internal static int INTERP_DEC_20 = 1;
        internal static int INTERP_APPLE_IIE = 2;
        internal static int INTERP_MACINTOSH = 3;
        internal static int INTERP_AMIGA = 4;
        internal static int INTERP_ATARI_ST = 5;
        internal static int INTERP_MSDOS = 6;
        internal static int INTERP_CBM_128 = 7;
        internal static int INTERP_CBM_64 = 8;
        internal static int INTERP_APPLE_IIC = 9;
        internal static int INTERP_APPLE_IIGS = 10;
        internal static int INTERP_TANDY = 11;

        internal static int H_INTERPRETER_VERSION = 31;
        internal static int H_SCREEN_ROWS = 32;
        internal static int H_SCREEN_COLUMNS = 33;
        internal static int H_SCREEN_WIDTH = 34;
        internal static int H_SCREEN_HEIGHT = 36;
        internal static int H_FONT_WIDTH = 38; /* this is the font height in V6 */
        internal static int H_FONT_HEIGHT = 39; /* this is the font width in V6 */
        internal static int H_ROUTINES_OFFSET = 40;
        internal static int H_STRINGS_OFFSET = 42;
        internal static int H_DEFAULT_BACKGROUND = 44;
        internal static int H_DEFAULT_FOREGROUND = 45;
        internal static int H_TERMINATING_KEYS = 46;
        internal static int H_LINE_WIDTH = 48;
        internal static int H_SPECIFICATION_HI = 50;
        internal static int H_SPECIFICATION_LO = 51;
        internal static int H_ALPHABET = 52;
        internal static int H_MOUSE_TABLE = 54;
        internal static int H_NAME = 56;

        internal const int V1 = 1;

        internal const int V2 = 2;

        ///* Version 3 object format */

        internal const int V3 = 3;

        //typedef struct zobjectv3 {
        //    zword_t attributes[2];
        //    zbyte_t parent;
        //    zbyte_t next;
        //    zbyte_t child;
        //    zword_t property_offset;
        //} zobjectv3_t;

        internal static int O3_ATTRIBUTES = 0;
        internal static ulong O3_PARENT = 4;
        internal static ulong O3_NEXT = 5;
        internal static ulong O3_CHILD = 6;
        internal static ulong O3_PROPERTY_OFFSET = 7;

        internal static int O3_SIZE = 9;

        //internal static int PARENT3(offset) = (offset + O3_PARENT);
        //internal static int NEXT3(offset) = (offset + O3_NEXT);
        //internal static int CHILD3(offset) = (offset + O3_CHILD);

        internal static int P3_MAX_PROPERTIES = 0x20;

        ///* Version 4 object format */

        internal const int V4 = 4;

        //typedef struct zobjectv4 {
        //    zword_t attributes[3];
        //    zword_t parent;
        //    zword_t next;
        //    zword_t child;
        //    zword_t property_offset;
        //} zobjectv4_t;

        internal static int O4_ATTRIBUTES = 0;
        internal static ulong O4_PARENT = 6;
        internal static ulong O4_NEXT = 8;
        internal static ulong O4_CHILD = 10;
        internal static ulong O4_PROPERTY_OFFSET = 12;

        internal static int O4_SIZE = 14;

        //internal static int PARENT4(offset) = (offset + O4_PARENT);
        //internal static int NEXT4(offset) = (offset + O4_NEXT);
        //internal static int CHILD4(offset) = (offset + O4_CHILD);

        internal static int P4_MAX_PROPERTIES = 0x40;

        internal const int V5 = 5;

        internal const int V6 = 6;

        internal const int V7 = 7;

        internal const int V8 = 8;

        ///* Local defines */

        internal static int PAGE_SIZE = 512;
        internal static int PAGE_MASK = 511;
        internal static int PAGE_SHIFT = 9;

        internal const int NIL = 0;
        internal const int ANYTHING = 1;
        internal const int VAR = 2;
        internal const int NUMBER = 3;
        internal const int LOW_ADDR = 4;
        internal const int ROUTINE = 5;
        internal const int OBJECT = 6;
        internal const int STATIC = 7;
        internal const int LABEL = 8;
        internal const int PCHAR = 9;
        internal const int VATTR = 10;
        internal const int PATTR = 11;
        internal const int INDIRECT = 12;
        internal const int PROPNUM = 13;
        internal const int ATTRNUM = 14;

        internal static int NONE = 0;
        internal static int TEXT = 1;
        internal static int STORE = 2;
        internal static int BRANCH = 3;
        internal static int BOTH = 4;

        internal static int PLAIN = 0;
        internal static int CALL = 1;
        internal static int RETURN = 2;
        internal static int ILLEGAL = 3;

        internal const int TWO_OPERAND = 0;
        internal const int ONE_OPERAND = 1;
        internal const int ZERO_OPERAND = 2;
        internal const int VARIABLE_OPERAND = 3;
        internal const int EXTENDED_OPERAND = 4;

        internal const int WORD_IMMED = 0;
        internal const int BYTE_IMMED = 1;
        internal const int VARIABLE = 2;
        internal const int NO_OPERAND = 3;

        internal static int END_OF_CODE = 1;
        internal static int END_OF_ROUTINE = 2;
        internal static int END_OF_INSTRUCTION = 3;
        internal static int BAD_ENTRY = 4;
        internal static int BAD_OPCODE = 5;

        internal static int ROMAN = 0;
        internal static int REVERSE = 1;
        internal static int BOLDFACE = 2;
        internal static int EMPHASIS = 4;
        internal static int FIXED_FONT = 8;

        ///* Grammar related defines */

        internal enum parser_types
        {
            infocom_fixed,
            infocom_variable,
            infocom6_grammar,
            inform5_grammar,
            inform_gv1,
            inform_gv2,
            inform_gv2a
        };

        internal static uint VERB_NUM(int index, uint parser_type)
        {
            return (((parser_type) >= (int)tx_h.parser_types.inform_gv2a) ? (uint)(index) : ((uint)(255 - (index))));
        }

        // internal static int VERB_NUM(index, = parser_type) (((parser_type) >= inform_gv2a)?(index):((uint)(255-(index))));

        internal static uint PREP = 0x08;
        internal static uint DESC = 0x20;	/* infocom V1-5 only -- actually an adjective. */
        internal static uint NOUN = 0x80;
        internal static uint VERB = 0x40;	/* infocom V1-5 only */
        internal static uint DIR = 0x10; 	/* infocom V1-5 only */
        internal static uint VERB_INFORM = 0x01;
        internal static uint VERB_V6 = 0x01;
        internal static uint PLURAL = 0x04; 	/* inform only */
        internal static uint SPECIAL = 0x04; 	/* infocom V1-5 only */
        internal static uint META = 0x02; 	/* infocom V1-5 only */
        internal const uint DATA_FIRST = 0x03; 	/* infocom V1-5 only */
        internal const uint DIR_FIRST = 0x03;  	/* infocom V1-5 only */
        internal const uint ADJ_FIRST = 0x02;  	/* infocom V1-5 only */
        internal const uint VERB_FIRST = 0x01;  	/* infocom V1-5 only */
        internal const uint PREP_FIRST = 0x00;  	/* infocom V1-5 only */
        internal static uint ENDIT = 0x0F;

        ///* txd-specific defines? */

        internal static int MAX_CACHE = 10;

        internal class decode_t
        {
            internal bool first_pass;   /* Code pass flag                   */
            internal ulong pc;           /* Current PC                       */
            internal ulong initial_pc;   /* Initial PC                       */
            internal ulong high_pc;      /* Highest PC in current subroutine */
            internal ulong low_address;  /* Lowest subroutine address        */
            internal ulong high_address; /* Highest code address             */
        } ;

        internal class opcode_t
        {
            internal int opcode;  /* Current opcode  */
            internal int opclass;   /* Class of opcode */
            internal int[] par = new int[4];  /* Types of parameters */
            internal int extra;   /* Branch/store/text */
            internal int type;    /* Opcode type */
        };

        internal class cref_item_t
        {
            internal ulong address;
            internal int number;
            internal System.Collections.Generic.List<cref_item_t> child = new System.Collections.Generic.List<cref_item_t>();

            public override string ToString()
            {
                return String.Format("{0} -> {1}", address, number);
            }
        }

        /* Data access macros */
        internal static byte[] datap = null;

        internal static byte get_byte(ulong offset)
        {
            return get_byte((int)offset);
        }

        internal static byte get_byte(int offset)
        {
            return datap[offset];
        }

        internal static ushort get_word(int offset)
        {
            return (ushort)((datap[offset] << 8) + (datap[offset + 1]));
        }

        internal static void set_byte(ulong offset, uint value)
        {
            datap[offset] = (zbyte_t)value;
        }

        internal static void set_word(ulong offset, uint value)
        {
            datap[offset] = (zbyte_t)(value >> 8);
            datap[offset + 1] = (zbyte_t)(value & 0xff);
        }

        ///* Inform version codes */
        internal static int INFORM_5 = 500;
        internal static int INFORM_6 = 600;
        internal static int INFORM_610 = 610;

        ///* Grammar prototypes */;
        //#ifdef __STDC__
        //void configure_parse_tables
        //    (uint *, uint *, uint *, uint *, uint *,
        //     ulong *, ulong *, ulong *, ulong *,
        //     ulong *, ulong *);
        //void show_verb_grammar
        //    (ulong, uint, int, int, int, ulong, ulong);
        //void show_syntax_of_action(int action,
        //            ulong verb_table_base,
        //            uint verb_count,
        //            uint parser_type,
        //            uint prep_type,
        //            ulong attr_names_base,
        //            ulong prep_table_base);

        //void show_syntax_of_parsing_routine(ulong parsing_routine,
        //                    ulong verb_table_base,
        //                    uint verb_count,
        //                    uint parser_type,
        //                    uint prep_type,
        //                    ulong prep_table_base,
        //                    ulong attr_names_base);

        //int is_gv2_parsing_routine(ulong parsing_routine,
        //                    ulong verb_table_base,
        //                    uint verb_count);
        //#else
        //void configure_parse_tables ();
        //void show_verb_grammar ();
        //void show_syntax_of_action();
        //void show_syntax_of_parsing_routine();
        //int is_gv2_parsing_routine();
        //#endif
    }
}