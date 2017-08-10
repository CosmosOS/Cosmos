/* txio.c
 *
 * I/O routines for Z code disassembler and story file dumper.
 *
 * Mark Howell 26 August 1992 howell_ma@movies.enet.dec.com
 *
 */

using zword_t = System.UInt16;
using zbyte_t = System.Byte;

using System;
using System.IO;
namespace ZTools
{
    public static class txio
    {
        internal static tx_h.zheader_t header;

        internal static ulong story_scaler;
        internal static ulong story_shift;
        internal static ulong code_scaler;
        internal static ulong code_shift;
        internal static int property_mask;
        internal static int property_size_mask;

        internal static bool option_inform = false;

        internal static int file_size = 0;

        internal static String[] v1_lookup_table;
        internal static String[] v3_lookup_table;

        static String[] euro_substitute;

        static String[] inform_euro_substitute;

        internal static int lookup_table_loaded = 0;
        internal static char[,] lookup_table;

        static txio()
        {
            lookup_table = new char[3, 26];

            v1_lookup_table = new String[] {
        "abcdefghijklmnopqrstuvwxyz",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        " 0123456789.,!?_#'\"/\\<-:()"
        };

            v3_lookup_table = new string[] {
        "abcdefghijklmnopqrstuvwxyz",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        " \n0123456789.,!?_#'\"/\\-:()"
        };

            euro_substitute = new string[] {
        "ae", "oe", "ue", "Ae", "Oe", "Ue", "ss", ">>", "<<", "e",
        "i",  "y",  "E",  "I",  "a",  "e",  "i",  "o",  "u",  "y",
        "A",  "E",  "I",  "O",  "U",  "Y",  "a",  "e",  "i",  "o",
        "u",  "A",  "E",  "I",  "O",  "U",  "a",  "e",  "i",  "o",
        "u",  "A",  "E",  "I",  "O",  "U",  "a",  "A",  "o",  "O",
        "a",  "n",  "o",  "A",  "N",  "O",  "ae", "AE", "c",  "C",
        "th", "th", "Th", "Th", "L",  "oe", "OE", "!",  "?"
        };

            inform_euro_substitute = new String[] {
        "ae", "oe", "ue", "AE", "OE", "UE", "ss", ">>", "<<", ":e",
        ":i",  ":y",  ":E",  ":I",  "'a",  "'e",  "'i",  "'o",  "'u",  "'y",
        "'A",  "'E",  "'I",  "'O",  "'U",  "'Y",  "`a",  "`e",  "`i",  "`o",
        "`u",  "`A",  "`E",  "`I",  "`O",  "`U",  "^a",  "^e",  "^i",  "^o",
        "^u",  "^A",  "^E",  "^I",  "^O",  "^U",  "oa",  "oA",  "\\o",  "\\O",
        "~a",  "~n",  "~o",  "~A",  "~N",  "~O",  "ae", "AE", "cc",  "cC",
        "th", "et", "Th", "Et", "LL",  "oe", "OE", "!!",  "??"
        };
        }

        const int TX_SCREEN_COLS = 79;

        static char[] tx_line = null;
        internal static int tx_line_pos = 0;
        internal static int tx_col = 1;
        internal static int tx_margin = 0;
        internal static int tx_do_margin = 1;
        internal static int tx_screen_cols = TX_SCREEN_COLS;

        //internal class cache_entry_t
        //{
        //    cache_entry_t flink;
        //    // uint page_number;
        //    zbyte_t[] data = new zbyte_t[tx_h.PAGE_SIZE];
        //}

        static Stream gfp = null;

        //static cache_entry_t *cache = NULL;

        // static uint current_data_page = 0;
        //static cache_entry_t *current_data_cachep = NULL;

        // static uint data_size;

        static zbyte_t[] buffer;

        internal static void configure(int min_version, int max_version)
        {
            // buffer = new zbyte_t[tx_h.PAGE_SIZE];
            int i;

            buffer = new zbyte_t[gfp.Length];
            gfp.Read(buffer, 0, buffer.Length);

            //#if !defined(lint)
            //    assert (sizeof (zheader_t) == 64);
            //    assert (sizeof (zheader_t) <= PAGE_SIZE);
            //#endif /* !defined(lint) */

            // read_page(0, buffer);
            tx_h.datap = buffer;

            header = new tx_h.zheader_t();

            header.version = tx_h.get_byte(tx_h.H_VERSION);
            header.config = tx_h.get_byte(tx_h.H_CONFIG);
            header.release = tx_h.get_word(tx_h.H_RELEASE);
            header.resident_size = tx_h.get_word(tx_h.H_RESIDENT_SIZE);
            header.start_pc = tx_h.get_word(tx_h.H_START_PC);
            header.dictionary = tx_h.get_word(tx_h.H_DICTIONARY);
            header.objects = tx_h.get_word(tx_h.H_OBJECTS);
            header.globals = tx_h.get_word(tx_h.H_GLOBALS);
            header.dynamic_size = tx_h.get_word(tx_h.H_DYNAMIC_SIZE);
            header.flags = tx_h.get_word(tx_h.H_FLAGS);
            for (i = 0; i < header.serial.Length; i++)
                header.serial[i] = tx_h.get_byte(tx_h.H_SERIAL + i);
            header.abbreviations = tx_h.get_word(tx_h.H_ABBREVIATIONS);
            header.file_size = tx_h.get_word(tx_h.H_FILE_SIZE);
            header.checksum = tx_h.get_word(tx_h.H_CHECKSUM);
            header.interpreter_number = tx_h.get_byte(tx_h.H_INTERPRETER_NUMBER);
            header.interpreter_version = tx_h.get_byte(tx_h.H_INTERPRETER_VERSION);
            header.screen_rows = tx_h.get_byte(tx_h.H_SCREEN_ROWS);
            header.screen_columns = tx_h.get_byte(tx_h.H_SCREEN_COLUMNS);
            header.screen_width = tx_h.get_word(tx_h.H_SCREEN_WIDTH);
            header.screen_height = tx_h.get_word(tx_h.H_SCREEN_HEIGHT);
            if (header.version != tx_h.V6)
            {
                header.font_width = (byte)tx_h.get_word(tx_h.H_FONT_WIDTH);
                header.font_height = tx_h.get_byte(tx_h.H_FONT_HEIGHT);
            }
            else
            {
                header.font_width = (byte)tx_h.get_word(tx_h.H_FONT_HEIGHT);
                header.font_height = tx_h.get_byte(tx_h.H_FONT_WIDTH);
            }
            header.routines_offset = tx_h.get_word(tx_h.H_ROUTINES_OFFSET);
            header.strings_offset = tx_h.get_word(tx_h.H_STRINGS_OFFSET);
            header.default_background = tx_h.get_byte(tx_h.H_DEFAULT_BACKGROUND);
            header.default_foreground = tx_h.get_byte(tx_h.H_DEFAULT_FOREGROUND);
            header.terminating_keys = tx_h.get_word(tx_h.H_TERMINATING_KEYS);
            header.line_width = tx_h.get_word(tx_h.H_LINE_WIDTH);
            header.specification_hi = tx_h.get_byte(tx_h.H_SPECIFICATION_HI);
            header.specification_lo = tx_h.get_byte(tx_h.H_SPECIFICATION_LO);
            header.alphabet = tx_h.get_word(tx_h.H_ALPHABET);
            header.mouse_table = tx_h.get_word(tx_h.H_MOUSE_TABLE);
            for (i = 0; i < header.name.Length; i++)
                header.name[i] = tx_h.get_byte(tx_h.H_NAME + i);

            if ((uint)header.version < (uint)min_version ||
            (uint)header.version > (uint)max_version ||
            ((uint)header.config & tx_h.CONFIG_BYTE_SWAPPED) != 0)
            {
                throw new ArgumentException("\nFatal: wrong game or version\n");
            }

            if ((uint)header.version < tx_h.V4)
            {
                story_scaler = 2;
                story_shift = 1;
                code_scaler = 2;
                code_shift = 1;
                property_mask = tx_h.P3_MAX_PROPERTIES - 1;
                property_size_mask = 0xe0;
            }
            else if ((uint)header.version < tx_h.V6)
            {
                story_scaler = 4;
                story_shift = 2;
                code_scaler = 4;
                code_shift = 2;
                property_mask = tx_h.P4_MAX_PROPERTIES - 1;
                property_size_mask = 0x3f;
            }
            else if ((uint)header.version < tx_h.V8)
            {
                story_scaler = 8;
                story_shift = 3;
                code_scaler = 4;
                code_shift = 2;
                property_mask = tx_h.P4_MAX_PROPERTIES - 1;
                property_size_mask = 0x3f;
            }
            else
            {
                story_scaler = 8;
                story_shift = 3;
                code_scaler = 8;
                code_shift = 3;
                property_mask = tx_h.P4_MAX_PROPERTIES - 1;
                property_size_mask = 0x3f;
            }

            /* Calculate the file size */

            if ((uint)header.file_size == 0)
            {
                throw new ArgumentException("Can't handle files with no length. Giving up!");
                // file_size = get_story_size();
            }
            else if ((uint)header.version <= tx_h.V3)
                file_size = header.file_size * 2;
            else if ((uint)header.version <= tx_h.V5)
                file_size = header.file_size * 4;
            else
                file_size = header.file_size * 8;

        }/* configure */

        internal static void open_story(byte[] story) {
            gfp = new MemoryStream(story);
        }

        internal static void open_story(string storyname)
        {
            gfp = new FileStream(storyname, FileMode.Open);
            if (gfp == null)
            {
                Console.Error.WriteLine("Fatal: game file not found\n");
            }

        } /* open_story */

        internal static void close_story()
        {
            if (gfp != null)
            {
                gfp.Close();
            }
        }/* close_story */

        internal static void read_page(uint page, byte[] buffer)
        {
            int bytes_to_read;

            if (file_size == 0)
                bytes_to_read = 64;
            else if (page != (uint)(file_size / tx_h.PAGE_SIZE))
                bytes_to_read = tx_h.PAGE_SIZE;
            else
                bytes_to_read = (int)(file_size & tx_h.PAGE_MASK);

            gfp.Position = page * tx_h.PAGE_SIZE;
            gfp.Read(buffer, 0, bytes_to_read);

        } /* read_page */

        //internal static void load_cache()
        //{
        //    //    ulong file_size;
        //    //    uint i, file_pages, data_pages;
        //    //    cache_entry_t *cachep;

        //    //    /* Must have at least one cache page for memory calculation */

        //    //    cachep = (cache_entry_t *) malloc (sizeof (cache_entry_t));
        //    //    if (cachep == NULL) {
        //    //    (void) fprintf (stderr, "\nFatal: insufficient memory\n");
        //    //    exit (EXIT_FAILURE);
        //    //    }
        //    //    cachep->flink = cache;
        //    //    cachep->page_number = 0;
        //    //    cache = cachep;

        //    //    /* Calculate dynamic cache pages required */

        //    //    data_pages = ((uint) header.resident_size + PAGE_MASK) >> PAGE_SHIFT;
        //    //    data_size = data_pages * PAGE_SIZE;
        //    //    file_size = (ulong) header.file_size * story_scaler;
        //    //    file_pages = (uint) ((file_size + PAGE_MASK) >> PAGE_SHIFT);

        //    //    /* Allocate static data area and initialise it */

        //    //    datap = (zbyte_t *) malloc ((size_t) data_size);
        //    //    if (datap == NULL) {
        //    //    (void) fprintf (stderr, "\nFatal: insufficient memory\n");
        //    //    exit (EXIT_FAILURE);
        //    //    }
        //    //    for (i = 0; i < data_pages; i++)
        //    //    read_page (i, &datap[i * PAGE_SIZE]);

        //    //    /* Allocate cache pages and initialise them */

        //    //    for (i = data_pages; cachep != NULL && i < file_pages && i < data_pages + MAX_CACHE; i++) {
        //    //    cachep = (cache_entry_t *) malloc (sizeof (cache_entry_t));
        //    //    if (cachep != NULL) {
        //    //        cachep->flink = cache;
        //    //        cachep->page_number = i;
        //    //        read_page (cachep->page_number, cachep->data);
        //    //        cache = cachep;
        //    //        }
        //    //    }

        //} /* load_cache */

        internal static zword_t read_data_word(ref ulong addr)
        {
            uint w;

            w = (uint)read_data_byte(ref addr) << 8;
            w |= read_data_byte(ref addr);

            return (zword_t)w;

        }/* txio.read_data_word */

        internal static zbyte_t read_data_byte(ref ulong addr)
        {
            // uint page_number, page_offset;
            zbyte_t value;

            //if (addr < (ulong)txio.data_size)
            //    value = buffer[addr];
            //else
            //{
            //    page_number = (uint)(addr >> tx_h.PAGE_SHIFT);
            //    page_offset = (uint)(addr & (ulong)tx_h.PAGE_MASK);
            //    if (page_number != current_data_page)
            //    {
            //        current_data_cachep = update_cache(page_number);
            //        current_data_page = page_number;
            //    }
            //    value = current_data_cachep->data[page_offset];
            //}

            value = buffer[addr];
            addr++;

            return (value);
        }/* txio.read_data_byte */

        internal static int decode_text(ref ulong address)
        {
            int i, j, char_count, synonym_flag, synonym = 0, ascii_flag, ascii = 0;
            int data, code, shift_state, shift_lock;
            ulong addr;

            /*
             * Load correct character translation table for this game.
             */

            if (lookup_table_loaded == 0)
            {
                for (i = 0; i < 3; i++)
                {
                    for (j = 0; j < 26; j++)
                    {
                        if ((uint)header.alphabet > 0)
                        {
                            lookup_table[i, j] = (char)tx_h.get_byte(txio.header.alphabet + (i * 26) + j);
                        }
                        else
                        {
                            if ((uint)txio.header.version == tx_h.V1)
                                lookup_table[i, j] = v1_lookup_table[i][j];
                            else
                                lookup_table[i, j] = v3_lookup_table[i][j];
                        }

                        if (option_inform && lookup_table[i, j] == '\"')
                        {
                            lookup_table[i, j] = '~';
                        }
                    }
                    lookup_table_loaded = 1;
                }
            }

            /* Set state variables */

            shift_state = 0;
            shift_lock = 0;
            char_count = 0;
            ascii_flag = 0;
            synonym_flag = 0;

            do
            {

                /*
                 * Read one 16 bit word. Each word contains three 5 bit codes. If the
                 * high bit is set then this is the last word in the string.
                 */
                data = txio.read_data_word(ref address);

                for (i = 10; i >= 0; i -= 5)
                {
                    /* Get code, high bits first */

                    code = (data >> i) & 0x1f;

                    /* Synonym codes */

                    if (synonym_flag > 0)
                    {

                        synonym_flag = 0;
                        synonym = (synonym - 1) * 64;
                        addr = (ulong)tx_h.get_word(header.abbreviations + synonym + (code * 2)) * 2;
                        char_count += txio.decode_text(ref addr);
                        shift_state = shift_lock;

                        /* ASCII codes */

                    }
                    else if (ascii_flag > 0)
                    {

                        /*
                         * If this is the first part ASCII code then remember it.
                         * Because the codes are only 5 bits you need two codes to make
                         * one eight bit ASCII character. The first code contains the
                         * top 3 bits. The second code contains the bottom 5 bits.
                         */

                        if (ascii_flag++ == 1)

                            ascii = code << 5;

                        /*
                         * If this is the second part ASCII code then assemble the
                         * character from the two codes and output it.
                         */

                        else
                        {

                            ascii_flag = 0;
                            txio.tx_printf("{0}", (char)(ascii | code));
                            char_count++;

                        }

                        /* Character codes */

                    }
                    else if (code > 5)
                    {

                        code -= 6;

                        /*
                         * If this is character 0 in the punctuation set then the next two
                         * codes make an ASCII character.
                         */

                        if (shift_state == 2 && code == 0)

                            ascii_flag = 1;

                        /*
                         * If this is character 1 in the punctuation set then this
                         * is a new line.
                         */

                        else if (shift_state == 2 && code == 1 && (uint)header.version > tx_h.V1)

                            txio.tx_printf("{0}", (option_inform) ? '^' : '\n');

                                /*
                                 * This is a normal character so select it from the character
                                 * table appropriate for the current shift state.
                         */

                        else
                        {

                            txio.tx_printf("{0}", (char)lookup_table[shift_state, code]);
                            char_count++;

                        }

                        shift_state = shift_lock;

                        /* Special codes 0 to 5 */

                    }
                    else
                    {

                        /*
                         * Space: 0
                         *
                         * Output a space character.
                         *
                         */

                        if (code == 0)
                        {

                            txio.tx_printf(" ");
                            char_count++;

                        }
                        else
                        {

                            /*
                             * The use of the synonym and shift codes is the only difference between
                             * the different versions.
                             */

                            if ((uint)header.version < tx_h.V3)
                            {

                                /*
                                 * Newline or synonym: 1
                                 *
                                 * Output a newline character or set synonym flag.
                                 *
                                 */

                                if (code == 1)
                                {

                                    if ((uint)header.version == tx_h.V1)
                                    {
                                        txio.tx_printf("{0}", (option_inform) ? '^' : '\n');
                                        char_count++;
                                    }
                                    else
                                    {
                                        synonym_flag = 1;
                                        synonym = code;
                                    }

                                    /*
                                     * Shift keys: 2, 3, 4 or 5
                                     *
                                     * Shift keys 2 & 3 only shift the next character and can be used regardless of
                                     * the state of the shift lock. Shift keys 4 & 5 lock the shift until reset.
                                     *
                                     * The following code implements the the shift code state transitions:
                                     *
                                     *               +-------------+-------------+-------------+-------------+
                                     *               |       Shift   State       |        Lock   State       |
                                     * +-------------+-------------+-------------+-------------+-------------+
                                     * | Code        |      2      |       3     |      4      |      5      |
                                     * +-------------+-------------+-------------+-------------+-------------+
                                     * | lowercase   | uppercase   | punctuation | uppercase   | punctuation |
                                     * | uppercase   | punctuation | lowercase   | punctuation | lowercase   |
                                     * | punctuation | lowercase   | uppercase   | lowercase   | uppercase   |
                                     * +-------------+-------------+-------------+-------------+-------------+
                                     *
                                     */

                                }
                                else
                                {
                                    if (code < 4)
                                        shift_state = (shift_lock + code + 2) % 3;
                                    else
                                        shift_lock = shift_state = (shift_lock + code) % 3;
                                }

                            }
                            else
                            {

                                /*
                                 * Synonym table: 1, 2 or 3
                                 *
                                 * Selects which of three synonym tables the synonym
                                 * code following in the next code is to use.
                                 *
                                 */

                                if (code < 4)
                                {

                                    synonym_flag = 1;
                                    synonym = code;
                                    /*
                                     * Shift key: 4 or 5
                                     *
                                     * Selects the shift state for the next character,
                                     * either uppercase (4) or punctuation (5). The shift
                                     * state automatically gets reset back to lowercase for
                                     * V3+ games after the next character is output.
                                     *
                                     */

                                }
                                else
                                {
                                    shift_state = code - 3;
                                    shift_lock = 0;

                                }
                            }
                        }
                    }
                }
            } while ((data & 0x8000) == 0);

            return (char_count);

        }/* txio.decode_text */

        //#ifdef __STDC__
        //static cache_entry_t *update_cache (uint page_number)
        //#else
        //static cache_entry_t *update_cache (page_number)
        //uint page_number;
        //#endif
        //{
        //    cache_entry_t *cachep, *lastp;

        //    for (lastp = cache, cachep = cache;
        //         cachep->flink != NULL &&
        //         cachep->page_number &&
        //         cachep->page_number != page_number;
        //         lastp = cachep, cachep = cachep->flink)
        //        ;
        //    if (cachep->page_number != page_number) {
        //        if (cachep->flink == NULL && cachep->page_number) {
        //            if (current_data_page == (uint) cachep->page_number)
        //                current_data_page = 0;
        //    }
        //        cachep->page_number = page_number;
        //        read_page (page_number, cachep->data);
        //    }
        //    if (lastp != cache) {
        //        lastp->flink = cachep->flink;
        //        cachep->flink = cache;
        //        cache = cachep;
        //    }

        //    return (cachep);

        //}/* update_cache */

        ///*
        // * get_story_size
        // *
        // * Calculate the size of the game file. Only used for very old games that do not
        // * have the game file size in the header.
        // *
        // */

        //#ifdef __STDC__
        //static ulong get_story_size (void)
        //#else
        //static ulong get_story_size ()
        //#endif
        //{
        //    ulong file_length;

        //    /* Read whole file to calculate file size */

        //    rewind (gfp);
        //    for (file_length = 0; fgetc (gfp) != EOF; file_length++)
        //    ;
        //    rewind (gfp);

        //    return (file_length);

        //}/* get_story_size */

        internal static void tx_printf(String format, params Object[] args)
        {
            int count, i;

            if (tx_screen_cols != 0)
            {
                if (tx_line == null || tx_line.Length == 0)
                {
                    tx_line = new char[TX_SCREEN_COLS];
                }


                String temp = String.Format(format, args);
                count = temp.Length;
                if (count > TX_SCREEN_COLS)
                {
                    throw new ArgumentException("\nFatal: buffer space overflow\n");
                }
                for (i = 0; i < count; i++)
                {
                    tx_write_char(temp[i]);
                }
            }
            else
                sb.AppendFormat(format, args);

        }/* txio.tx_printf */

        private static void write_high_zscii(int c)
        {
            //    static zword_t unicode_table[256];
            //    static int unicode_table_loaded;
            //    int unicode_table_addr;
            //    int length, i;

            //    if (!unicode_table_loaded) {
            //        if (header.mouse_table && (tx_h.get_word(header.mouse_table) > 2)) {
            //        unicode_table_addr = tx_h.get_word(header.mouse_table + 6);
            //        if (unicode_table_addr) {
            //            length = tx_h.get_byte(unicode_table_addr);
            //        for (i = 0; i < unicode_table_addr; i++)
            //                unicode_table[i + 155] = tx_h.get_word(unicode_table_addr + 1 + i*2);
            //        }
            //    }
            //    unicode_table_loaded = 1;
            //    }

            //    if ((c <= 0xdf) && !unicode_table[c]) {
            //        if (option_inform)
            //        txio.tx_printf("@%s", inform_euro_substitute[c - 0x9b]);
            //    else
            //        txio.tx_printf (euro_substitute[c - 0x9b]);
            //    }
            //    else /* no non-inform version of these.  */
            //        txio.tx_printf("@{%x}", unicode_table[c]);
        }

        static void tx_write_char(int c)
        {
            int i;
            int cp;

            /* In V6 games a tab is a paragraph indent gap and a vertical tab is
               an inter-sentence gap. Both can be set to a space for readability */

            if (c == '\v' || c == '\t')
                c = ' ';

            //    /* European characters should be substituted by their replacements. */

            if (c >= 0x9b && c <= 0xfb)
            {
                write_high_zscii(c);
                return;
            }

            if (tx_col == tx_screen_cols + 1 || c == '\n')
            {
                tx_do_margin = 1;
                if (tx_line_pos < tx_line.Length) tx_line[tx_line_pos++] = '\0';
                int eol = tx_line.ToString().IndexOf('\0');
                if (eol == -1) eol = tx_line_pos;
                String temp = new String(tx_line, 0, tx_line_pos);
                cp = temp.LastIndexOf(' ');
                if (c == ' ' || c == '\n' || cp == -1)
                {
                    sb.Append(temp);
                    sb.Append('\n');
                    tx_line_pos = 0;
                    tx_col = 1;

                    clear_tx_line();
                    return;
                }
                else
                {
                    tx_line[cp++] = '\0';

                    sb.Append(temp.Substring(0, cp));
                    sb.Append('\n');
                    tx_line_pos = 0;
                    tx_col = 1;
                    txio.tx_printf("{0}", temp.Substring(cp));
                }
            }

            if (tx_do_margin > 0)
            {
                tx_do_margin = 0;
                for (i = 1; i < tx_margin; i++)
                    tx_write_char(' ');
            }

            tx_line[tx_line_pos++] = (char)c;
            tx_col++;

        }/* tx_write_char */

        private static void clear_tx_line()
        {
            for (int i = 0; i < tx_line.Length; i++)
            {
                tx_line[i] = ' ';
            }
        }

        internal static void tx_fix_margin(int flag)
        {
            txio.tx_margin = (flag) > 0 ? txio.tx_col : 0;

        }/* txio.tx_fix_margin */

        internal static void tx_set_width(int width)
        {

            if (width > tx_screen_cols)
            {
                if (tx_line != null)
                {
                    tx_line[tx_line_pos++] = '\0';
                    sb.Append(tx_line);
                }
                tx_line_pos = 0;
                // free (tx_line);
                // tx_line = NULL;
            }
            tx_screen_cols = width;

        }/* tx_set_width */

        internal static System.Text.StringBuilder sb;
        internal static void startStringBuilder()
        {
            sb = new System.Text.StringBuilder();
        }

        internal static String getTextFromStringBuilder()
        {
            String text = sb.ToString();
            sb.Length = 0;

            foreach (char c in text)
            {
                if (c != '\0')
                {
                    sb.Append(c);
                }
            }

            text = sb.ToString();
            sb.Length = 0;

            return text;
        }

    }
}