// #define FULL_HEADER

/*
 * showhead - part of infodump
 *
 * Header display routines.
 */

using zword_t = System.UInt16;
using zbyte_t = System.Byte;

using System;
namespace ZTools
{
    internal class showhead
    {
        static String[] interpreter_flags1 = {
        "Byte swapped data",
        "Display time",
        "Unknown (0x04)",
        "Tandy",
        "No status line",
        "Windows available",
        "Proportional fonts used",
        "Unknown (0x80)"
    };

        static String[] interpreter_flags2 = {
        "Colours",
        "Pictures",
        "Bold font",
        "Emphasis",
        "Fixed space font",
        "Unknown (0x20)",
        "Unknown (0x40)",
        "Timed input"
    };

        static String[] game_flags1 = {
        "Scripting",
        "Use fixed font",
        "Unknown (0x0004)",
        "Unknown (0x0008)",
        "Supports sound",
        "Unknown (0x0010)",
        "Unknown (0x0020)",
        "Unknown (0x0040)",
        "Unknown (0x0080)",
        "Unknown (0x0200)",
        "Unknown (0x0400)",
        "Unknown (0x0800)",
        "Unknown (0x1000)",
        "Unknown (0x2000)",
        "Unknown (0x4000)",
        "Unknown (0x8000)"
    };

        static String[] game_flags2 = {
        "Scripting",
        "Use fixed font",
        "Screen refresh required",
        "Supports graphics",
        "Supports undo",
        "Supports mouse",
        "Supports colour",
        "Supports sound",
        "Supports menus",
        "Unknown (0x0200)",
        "Printer error",
        "Unknown (0x0800)",
        "Unknown (0x1000)",
        "Unknown (0x2000)",
        "Unknown (0x4000)",
        "Unknown (0x8000)"
    };

        /*
         * show_header
         *
         * Format the header which is a 64 byte area at the front of the story file.
         * The format of the header is described by the header structure.
         */

        internal static void show_header()
        {
            ulong address;
            int i, j, list;
            int inform = 0; // TODO Was short

            var header = txio.header;

            if (header.serial[0] >= '0' && header.serial[0] <= '9' &&
                header.serial[1] >= '0' && header.serial[1] <= '9' &&
                header.serial[2] >= '0' && header.serial[2] <= '1' &&
                header.serial[3] >= '0' && header.serial[3] <= '9' &&
                header.serial[4] >= '0' && header.serial[4] <= '3' &&
                header.serial[5] >= '0' && header.serial[5] <= '9' &&
                header.serial[0] != '8')
            {
                inform = 5;

                if (header.name[4] >= '6')
                    inform = header.name[4] - '0';
            }

            txio.tx_printf("\n    **** Story file header ****\n\n");

            /* Z-code version */

            txio.tx_printf("Z-code version:           {0:d}\n", header.version);

            /* Interpreter flags */

            Console.Write("Interpreter flags:        ");
            txio.tx_fix_margin(1);
            list = 0;
            for (i = 0; i < 8; i++)
            {
                if (((uint)header.config & (1 << i)) > 0)
                {
                    txio.tx_printf("{0}{1}", (list++) > 0 ? ", " : "",
                           ((uint)header.version < tx_h.V4) ? interpreter_flags1[i] : interpreter_flags2[i]);
                }
                else
                {
                    if ((uint)header.version < tx_h.V4 && i == 1)
                        txio.tx_printf("{0}Display score/moves", (list++) > 0 ? ", " : "");
                }
            }
            if (list == 0)
                txio.tx_printf("None");

            txio.tx_printf("\n");
            txio.tx_fix_margin(0);

            /* Release number */

            txio.tx_printf("Release number:           {0:d}\n", (int)header.release);

            /* Size of resident memory */

            txio.tx_printf("Size of resident memory:  {0:X4}\n", (uint)header.resident_size);

            /* Start PC */

            if ((uint)header.version != tx_h.V6)
                txio.tx_printf("Start PC:                 {0:X4}\n", (uint)header.start_pc);
            else
                txio.tx_printf("Main routine address:     {0:X5}\n", (ulong)
                       (((ulong)header.start_pc * txio.code_scaler) +
                        ((ulong)header.routines_offset * txio.story_scaler)));

            /* Dictionary address */

            txio.tx_printf("Dictionary address:       {0:X4}\n", (uint)header.dictionary);

            /* Object table address */

            txio.tx_printf("Object table address:     {0:X4}\n", (uint)header.objects);

            /* Global variables address */

            txio.tx_printf("Global variables address: {0:X4}\n", (uint)header.globals);

            /* Size of dynamic memory */

            txio.tx_printf("Size of dynamic memory:   {0:X4}\n", (uint)header.dynamic_size);

            /* Game flags */

            txio.tx_printf("Game flags:               ");
            txio.tx_fix_margin(1);
            list = 0;
            for (i = 0; i < 16; i++)
            {
                if (((uint)header.flags & (1 << i)) > 0)
                {
                    txio.tx_printf("{0}{1}", (list++) > 0 ? ", " : "",
                           ((uint)header.version < tx_h.V4) ? game_flags1[i] : game_flags2[i]);
                }
            }
            if (list == 0)
                txio.tx_printf("None");
            txio.tx_printf("\n");
            txio.tx_fix_margin(0);

            /* Serial number */

            txio.tx_printf("Serial number:            {0}{1}{2}{3}{4}{5}\n",
                (char)header.serial[0], (char)header.serial[1],
                (char)header.serial[2], (char)header.serial[3],
                (char)header.serial[4], (char)header.serial[5]);

            /* Abbreviations address */

            if ((uint)header.abbreviations > 0)
                txio.tx_printf("Abbreviations address:    {0:X4}\n", (uint)header.abbreviations);

            /* File size and checksum */

            if ((uint)header.file_size > 0)
            {
                txio.tx_printf("File size:                {0:X5}\n", (ulong)txio.file_size);
                txio.tx_printf("Checksum:                 {0:X4}\n", (uint)header.checksum);
            }

#if FULL_HEADER

            /* Interpreter */

            txio.tx_printf ("Interpreter number:       {0} ", header.interpreter_number);
            switch ((uint) header.interpreter_number) {
            case 1 : txio.tx_printf ("DEC-20"); break;
            case 2 : txio.tx_printf ("Apple //e"); break;
            case 3 : txio.tx_printf ("Macintosh"); break;
            case 4 : txio.tx_printf ("Amiga"); break;
            case 5 : txio.tx_printf ("Atari ST"); break;
            case 6 : txio.tx_printf ("IBM/MS-DOS"); break;
            case 7 : txio.tx_printf ("Commodore 128"); break;
            case 8 : txio.tx_printf ("C64"); break;
            case 9 : txio.tx_printf ("Apple //c"); break;
            case 10: txio.tx_printf ("Apple //gs"); break;
            case 11: txio.tx_printf ("Tandy Color Computer"); break;
            default: txio.tx_printf("Unknown"); break;
            }
            txio.tx_printf ("\n");

            /* Interpreter version */

            txio.tx_printf ("Interpreter version:      ");
            //if (isprint ((uint) header.interpreter_version))
            //txio.tx_printf ("{0:c}\n", (char) header.interpreter_version);
            //else
            txio.tx_printf ("{0}\n", (int) header.interpreter_version);

            /* Screen dimensions */

            txio.tx_printf ("Screen rows:              {0}\n", (int) header.screen_rows);
            txio.tx_printf ("Screen columns:           {0}\n", (int) header.screen_columns);
            txio.tx_printf ("Screen width:             {0}\n", (int) header.screen_width);
            txio.tx_printf ("Screen height:            {0}\n", (int) header.screen_height);

            /* Font size */

            txio.tx_printf ("Font width:               {0}\n", (int) header.font_width);
            txio.tx_printf ("Font height:              {0}\n", (int) header.font_height);

#endif // defined(FULL_HEADER)

            /* V6 and V7 offsets */

            if ((uint)header.routines_offset > 0)
                txio.tx_printf("Routines offset:          {0:X5}\n", (ulong)header.routines_offset * txio.story_scaler);
            if ((uint)header.strings_offset > 0)
                txio.tx_printf("Strings offset:           {0:X5}\n", (ulong)header.strings_offset * txio.story_scaler);

#if FULL_HEADER

            /* Default colours */

            txio.tx_printf ("Background color:         {0}\n", (int) header.default_background);
            txio.tx_printf ("Foreground color:         {0}\n", (int) header.default_foreground);
        
#endif // defined(FULL_HEADER)

            /* Function keys address */

            if ((uint)header.terminating_keys > 0)
            {
                txio.tx_printf("Terminating keys address: {0:X4}\n", (uint)header.terminating_keys);
                address = (ulong)header.terminating_keys;
                txio.tx_printf("    Keys used: ");
                txio.tx_fix_margin(1);
                list = 0;
                for (i = (int)txio.read_data_byte(ref address); i > 0;
                     i = (int)txio.read_data_byte(ref address))
                {
                    if (list > 0)
                        txio.tx_printf(", ");
                    if (i == 0x81)
                        txio.tx_printf("Up arrow"); /* Arrow keys */
                    else if (i == 0x82)
                        txio.tx_printf("Down arrow");
                    else if (i == 0x83)
                        txio.tx_printf("Left arrow");
                    else if (i == 0x84)
                        txio.tx_printf("Right arrow");
                    else if (i >= 0x85 && i <= 0x90)
                        txio.tx_printf("F{0}", (int)(i - 0x84)); /* Function keys */
                    else if (i >= 0x91 && i <= 0x9a)
                        txio.tx_printf("KP{0}", (int)(i - 0x91)); /* Keypad keys */
                    else if (i == 0xfc)
                        txio.tx_printf("Menu click");
                    else if (i == 0xfd)
                        txio.tx_printf("Single mouse click");
                    else if (i == 0xfe)
                        txio.tx_printf("Double mouse click");
                    else if (i == 0xff)
                        txio.tx_printf("Any function key");
                    else
                        txio.tx_printf("Unknown key (0x{0:X2})", (uint)i);
                    list++;
                }
                txio.tx_printf("\n");
                txio.tx_fix_margin(0);
            }

#if FULL_HEADER

            /* Line width */

            txio.tx_printf ("Line width:               {0}\n", (int) header.line_width);

            /* Specification number */

            if ((uint) header.specification_hi > 0)
            txio.tx_printf ("Specification number:   {0}.{1}",
                   (uint) header.specification_hi,
                   (uint) header.specification_lo);

#endif // defined(FULL_HEADER)

            /* Alphabet address */

            if ((uint)header.alphabet > 0)
            {
                txio.tx_printf("Alphabet address:         {0:4X}\n", (uint)header.alphabet);
                txio.tx_printf("    ");
                txio.tx_fix_margin(1);
                for (i = 0; i < 3; i++)
                {
                    txio.tx_printf("\"");
                    for (j = 0; j < 26; j++)
                        txio.tx_printf("{0}", (char)tx_h.get_byte((ulong)((uint)header.alphabet + (i * 26) + j)));
                    txio.tx_printf("\"\n");
                }
                txio.tx_fix_margin(0);
            }

            /* Mouse table address */

            if ((uint)header.mouse_table > 0)
                txio.tx_printf("Header extension address: {0:X4}\n", (uint)header.mouse_table);

#if FULL_HEADER

            /* Name */

        if ((uint)header.name[0] > 0 || (uint)header.name[1] > 0 || (uint)header.name[2] > 0 || (uint)header.name[3] > 0 ||
            (uint)header.name[4] > 0 || (uint)header.name[5] > 0 || (uint)header.name[6] > 0 || (uint)header.name[7] > 0)
        {
            txio.tx_printf ("Name:                     \"");
            for (i = 0; i < header.name.Length; i++)
                txio.tx_printf ("{0}", (char) header.name[i]);
            txio.tx_printf ("\"\n");
            }

#endif // defined(FULL_HEADER)

            /* Inform version -- overlaps name */
            if (inform >= 6)
            {
                txio.tx_printf("Inform Version:           ");
                for (i = 4; i < header.name.Length; i++)
                    txio.tx_printf("{0}", (char)header.name[i]);
                txio.tx_printf("\n");
            }

            show_header_extension();

        }/* show_header */

        private static void show_header_extension()
        {
            zword_t tlen = 0;

            if ((uint)txio.header.mouse_table > 0)
            {
                tlen = tx_h.get_word(txio.header.mouse_table);
                txio.tx_printf("Header extension length:  {0:X4}\n", tlen);
            }
            else
                return;

#if FULL_HEADER
        if (tlen > 0)
            txio.tx_printf("Mouse Y coordinate:       {0:X4}\n", tx_h.get_word(txio.header.mouse_table + 2));
        if (tlen > 1)
            txio.tx_printf("Mouse X coordinate:       {0:X4}\n", tx_h.get_word(txio.header.mouse_table + 4));
#endif

            if (tlen > 2)
                txio.tx_printf("Unicode table address:    {0:X4}\n", (ulong)tx_h.get_word(txio.header.mouse_table + 6));
        }
    }
}