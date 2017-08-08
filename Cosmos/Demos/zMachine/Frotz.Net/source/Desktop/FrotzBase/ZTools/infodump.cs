/*
 * infodump V7/3
 *
 * Infocom data file dumper, etc. for V1 to V8 games.
 * Works on everything I have, except for parsing information in V6 games.
 *
 * The most useful options are; -i to display the header information, such as
 * game version, serial number and release; -t to display the object tree which
 * shows where all objects start and which item is in which room; -g to show
 * the sentence grammar acceptable to the game; -d to see all the recognised
 * words.
 *
 * Required files:
 *    showhead.c - show header information
 *    showdict.c - show dictionary and abbreviations
 *    showobj.c  - show objects
 *    showverb.c - show verb grammar
 *    txio.c     - I/O support
 *    tx.h       - Include file required by everything above
 *    getopt.c   - The standard getopt function
 *
 * Usage: infodump [options...] story-file [story-file...]
 *     -i   show game information in header (default)
 *     -a   show abbreviations
 *     -m   show data file map
 *     -o   show objects
 *     -t   show object tree
 *     -g   show verb grammar
 *     -d n show dictionary (n = columns)
 *     -a   all of the above
 *     -w n display width (0 = no wrap)
 *
 * Mark Howell 28 August 1992 howell_ma@movies.enet.dec.com
 *
 * History:
 *    Fix verb table display for later V4 and V5 games
 *    Fix property list
 *    Add verb table
 *    Add support for V1 and V2 games
 *    Improve output
 *    Improve verb formatting
 *    Rewrite and add map
 *    Add globals address and V6 start PC
 *    Fix lint warnings and some miscellaneous bugs
 *    Add support for V7 and V8 games
 *    Fix Inform grammar tables
 *    Fix Inform adjectives table
 *    Add support for Inform 6 (helped by Matthew T. Russotto)
 *    Add header flag for "timed input"
 *    Add header extension table and Unicode table
 *    Add Inform and user symbol table support
 */

using System;
namespace ZTools
{
    public static class InfoDump
    {

        /* Options */

        //static short OPTION_A = 0;
        static short OPTION_I = 1;
        //static short OPTION_O = 2;
        //static short OPTION_T = 3;
        //static short OPTION_G = 4;
        //static short OPTION_D = 5;
        //static short OPTION_M = 6;
        static short MAXOPT = 7;

        /*
         * main
         *
         * Process command line arguments and process each story file.
         */

        // TODO Make this internal and just pass it out from txd.main (which I should also rename)
        public class ZToolInfo
        {
            public String Header { get; private set; }
            public String Text { get; private set; }

            public ZToolInfo(String Header, String Text)
            {
                this.Header = Header;
                this.Text = Text;
            }
        }

        public static System.Collections.Generic.List<ZToolInfo> main(byte[] storyFile, String[] args) 
        {
            int i;
            // int c, f, errflg = 0;
            int columns;
            int[] options = new int[MAXOPT];
            int symbolic;

            /* Clear all options */

            for (i = 0; i < MAXOPT; i++)
                options[i] = 0;

            columns = 0;
            symbolic = 0;

            /* Parse the options */

            options[OPTION_I] = 1;

            //    while ((c = getopt (argc, argv, "hafiotgmdsc:w:u:")) != EOF) {
            //    switch (c) {
            //        case 'f':
            //        for (i = 0; i < MAXOPT; i++)
            //            options[i] = 1;
            //        break;
            //        case 'a':
            //        options[OPTION_A] = 1;
            //        break;
            //        case 'i':
            //        options[OPTION_I] = 1;
            //        break;
            //        case 'o':
            //        options[OPTION_O] = 1;
            //        break;
            //        case 't':
            //        options[OPTION_T] = 1;
            //        break;
            //        case 'g':
            //        options[OPTION_G] = 1;
            //        break;
            //        case 'm':
            //        options[OPTION_M] = 1;
            //        break;
            //        case 'd':
            //        options[OPTION_D] = 1;
            //        break;
            //        case 's':
            //        symbolic = 1;
            //        break;
            //        case 'c':
            //        columns = atoi (optarg);
            //        break;
            //        case 'w':
            //        tx_set_width (atoi (optarg));
            //        break;
            //        case 'u':
            //            symbolic = 1;
            //        init_symbols (optarg);
            //        break;
            //        case 'h':
            //        case '?':
            //        default:
            //        errflg++;
            //    }
            //    }

            /* Display usage if unknown flag or no story file */

            //    if (errflg || optind >= argc) {
            //    show_help (argv[0]);
            //    exit (EXIT_FAILURE);
            //    }

            //    /* If no options then force header option information on */

            //    for (f = 0, i = 0; i < MAXOPT; i++)
            //    f += options[i];
            //    if (f == 0)
            //    options[OPTION_I] = 1;

            /* Process any story files on the command line */

                return process_story(storyFile, options, columns, symbolic);

        } /* main */

        /*
         * show_help
         */

        static void show_help(String program)
        {
            Console.Error.WriteLine("usage: %s [options...] story-file [story-file...]\n\n", program);
            Console.Error.WriteLine("INFODUMP version 7/3 - display Infocom story file information. By Mark Howell\n");
            Console.Error.WriteLine("Works with V1 to V8 Infocom games.\n\n");
            Console.Error.WriteLine("\t-i   show game information in header (default)\n");
            Console.Error.WriteLine("\t-a   show abbreviations\n");
            Console.Error.WriteLine("\t-m   show data file map\n");
            Console.Error.WriteLine("\t-o   show objects\n");
            Console.Error.WriteLine("\t-t   show object tree\n");
            Console.Error.WriteLine("\t-g   show verb grammar\n");
            Console.Error.WriteLine("\t-d   show dictionary\n");
            Console.Error.WriteLine("\t-f   full report (all of the above)\n");
            Console.Error.WriteLine("\t-c n number of columns for dictionary display\n");
            Console.Error.WriteLine("\t-w n display width (0 = no wrap)\n");
            Console.Error.WriteLine("\t-s Display Inform symbolic names in object and grammar displays\n");
            Console.Error.WriteLine("\t-u <file> Display symbols from file in object and grammar displays (implies -s)\n");

        }/* show_help */

        /*
         * process_story
         *
         * Load the story and display all parts of the data file requested.
         */

        static System.Collections.Generic.List<ZToolInfo> process_story(byte[] storyFile, int[] options, int columns, int symbolic)
        {
            var _areas = new System.Collections.Generic.List<ZToolInfo>();

            txio.open_story(storyFile);

            txio.configure(tx_h.V1, tx_h.V8);

            // txio.load_cache ();

            fix_dictionary();

            txio.startStringBuilder();
            showhead.show_header();
            _areas.Add(new ZToolInfo("Header", txio.getTextFromStringBuilder()));

            show_map();
            _areas.Add(new ZToolInfo("Map", txio.getTextFromStringBuilder()));

            showdict.show_abbreviations();
            _areas.Add(new ZToolInfo("Abbreviations", txio.getTextFromStringBuilder()));

            showobj.show_objects(symbolic);
            _areas.Add(new ZToolInfo("Objects", txio.getTextFromStringBuilder()));

            showobj.show_tree();
            _areas.Add(new ZToolInfo("Tree", txio.getTextFromStringBuilder()));

            showverb.show_verbs(symbolic);
            _areas.Add(new ZToolInfo("Verbs", txio.getTextFromStringBuilder()));

            showdict.show_dictionary(columns);
            _areas.Add(new ZToolInfo("Dictionary", txio.getTextFromStringBuilder()));

            txio.close_story();

            return _areas;

        } /* process_story */

        /*
         * fix_dictionary
         *
         * Fix the end of text flag for each word in the dictionary. Some older games
         * are missing the end of text flag on some words. All the words are fixed up
         * so that they can be printed.
         */

        internal static void fix_dictionary()
        {
            ulong address;
            int word_count, i;
            ulong separator_count, word_size;

            address = txio.header.dictionary;
            separator_count = txio.read_data_byte(ref address);
            address += separator_count;
            word_size = txio.read_data_byte(ref address);
            word_count = txio.read_data_word(ref address);

            for (i = 1; i <= word_count; i++)
            {

                /* Check that the word is in non-paged memory before writing */

                if ((address + 4) < (ulong)txio.header.resident_size)
                    if ((uint)txio.header.version <= tx_h.V3)
                        tx_h.set_byte(address + 2, (uint)tx_h.get_byte(address + 2) | 0x80);
                    else
                        tx_h.set_byte(address + 4, (uint)tx_h.get_byte(address + 4) | 0x80);

                address += word_size;
            }

        } /* fix_dictionary */

        private const int MAX_AREA = 20;

        private static void set_area(ulong base_addr, ulong end_addr, String name_string)
        {
            area_t a = new area_t();
            a.areabase = base_addr;
            a.end = end_addr;
            a.name = name_string;
            areas.Add(a);
        }

        struct area_t
        {
            internal ulong areabase;
            internal ulong end;
            internal String name;
        };

        /*
         * show_map
         *
         * Show the map of the data area. This is done by calling the configure routine
         * for each area. Each area is then sorted in ascending order and displayed.
         */

        private static System.Collections.Generic.List<area_t> areas;

        private static void show_map()
        {
            uint abbr_count;
            ulong abbr_table_base, abbr_table_end, abbr_data_base, abbr_data_end;
            uint word_count;
            ulong word_table_base, word_table_end;
            int obj_count;
            ulong obj_table_base, obj_table_end, obj_data_base, obj_data_end;
            uint verb_count, action_count, verb_type, prep_type;
            uint parse_count;
            ulong verb_table_base, verb_data_base;
            ulong action_table_base, preact_table_base;
            ulong prep_table_base, prep_table_end;
            uint ext_table_size;
            ulong ext_table_base, ext_table_end;
            ulong unicode_table_base, unicode_table_end;
            ushort inform_version;
            ulong class_numbers_base, class_numbers_end;
            ulong property_names_base, property_names_end;
            ulong attr_names_base, attr_names_end;
            int i;

            areas = new System.Collections.Generic.List<area_t>();

            var header = txio.header;

            /* Configure areas */

            set_area(0, 63, "Story file header");

            ext_table_base = header.mouse_table;
            if (ext_table_base > 0)
            {
                ext_table_size = tx_h.get_word((int)ext_table_base);
                ext_table_end = ext_table_base + 2 + ext_table_size * 2 - 1;
                set_area(ext_table_base, ext_table_end, "Header extension table");
                if (ext_table_size > 2)
                {
                    unicode_table_base = tx_h.get_word((int)ext_table_base + 6);
                    if (unicode_table_base > 0)
                    {
                        unicode_table_end = unicode_table_base + (ulong)tx_h.get_byte((int)unicode_table_base) * 2;
                        set_area(unicode_table_base, unicode_table_end, "Unicode table");
                    }
                }
            }

            showdict.configure_abbreviations(out abbr_count, out abbr_table_base, out abbr_table_end,
                         out abbr_data_base, out abbr_data_end);

            if (abbr_count > 0)
            {
                set_area(abbr_table_base, abbr_table_end, "Abbreviation pointer table");
                set_area(abbr_data_base, abbr_data_end, "Abbreviation data");
            }

            showdict.configure_dictionary(out word_count, out word_table_base, out word_table_end);

            set_area(word_table_base, word_table_end, "Dictionary");

            showobj.configure_object_tables(out obj_count, out obj_table_base, out obj_table_end,
                         out obj_data_base, out obj_data_end);

            set_area(obj_table_base, obj_table_end, "Object table");
            set_area(obj_data_base, obj_data_end, "Property data");

            showverb.configure_parse_tables(out verb_count, out action_count, out parse_count, out verb_type, out prep_type,
                        out verb_table_base, out verb_data_base,
                        out action_table_base, out preact_table_base,
                        out prep_table_base, out prep_table_end);

            if ((verb_count > 0) && (verb_type != (int)tx_h.parser_types.infocom6_grammar))
            {
                set_area(verb_table_base, verb_data_base - 1, "Grammar pointer table");
                set_area(verb_data_base, action_table_base - 1, "Grammar data");
                set_area(action_table_base, preact_table_base - 1, "Action routine table");
                if (verb_type < (int)tx_h.parser_types.inform_gv2)
                {
                    set_area(preact_table_base, prep_table_base - 1, (verb_type >= (int)tx_h.parser_types.inform5_grammar) ? "Parsing routine table" : "Pre-action routine table");
                    set_area(prep_table_base, prep_table_end, "Preposition table");
                }
            }
            else if (verb_count > 0)
            {
                set_area(verb_table_base, verb_table_base + 8 * verb_count - 1, "Verb grammar table");
                set_area(verb_data_base, prep_table_base - 1, "Grammar entries");
                set_area(action_table_base, preact_table_base - 1, "Action routine table");
                set_area(preact_table_base, preact_table_base + action_count * 2 - 1, "Pre-action routine table");
            }

            infinfo.configure_inform_tables(obj_data_end, out inform_version, out class_numbers_base, out class_numbers_end,
                            out property_names_base, out property_names_end, out attr_names_base, out attr_names_end);

            if (inform_version >= tx_h.INFORM_6)
            {
                set_area(class_numbers_base, class_numbers_end, "Class Prototype Object Numbers");
                set_area(property_names_base, property_names_end, "Property Names Table");
                if (inform_version >= tx_h.INFORM_610)
                {
                    set_area(attr_names_base, attr_names_end, "Attribute Names Table");
                }
            }

            set_area((ulong)header.globals,
                  (ulong)header.globals + (240 * 2) - 1,
                  "Global variables");

            set_area((ulong)header.resident_size,
                  (ulong)txio.file_size - 1,
                  "Paged memory");

            if (header.alphabet > 0)
                set_area((ulong)header.alphabet,
                      (ulong)header.alphabet + (26 * 3) - 1,
                      "Alphabet");

            /* Sort areas */

            areas.Sort(new area_comparer());

            /* Print area map */

            txio.tx_printf("\n    **** Story file map ****\n\n");

            txio.tx_printf(" Base    End   Size\n");
            for (i = 0; i < areas.Count; i++)
            {
                if (i > 0 && (areas[i].areabase - 1) > areas[i - 1].end)
                    txio.tx_printf("{0,5:X}  {1,5:X}  {2,5:X}\n",
                           (ulong)(areas[i - 1].end + 1), (ulong)(areas[i].areabase - 1),
                           (ulong)((areas[i].areabase - 1) - (areas[i - 1].end + 1) + 1));
                txio.tx_printf("{0,5:X}  {1,5:X}  {2,5:X}  {3}\n",
                       (ulong)areas[i].areabase, (ulong)areas[i].end,
                       (ulong)(areas[i].end - areas[i].areabase + 1),
                       areas[i].name);
            }


        }/* show_map */

        private class area_comparer : System.Collections.Generic.Comparer<area_t>
        {
            public override int Compare(area_t x, area_t y)
            {
                return (int)(x.areabase - y.areabase);
            }
        }

    }
}