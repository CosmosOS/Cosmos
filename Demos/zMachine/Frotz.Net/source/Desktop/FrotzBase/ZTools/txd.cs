// #define TXD_DEBUG

/* txd.c V7/3
 *
 * Z code disassembler for Infocom game files
 *
 * Requires txio.c, getopt.c, showverb.c and tx.h.
 *
 * Works for all V1, V2, V3, V4, V5, V6, V7 and V8 games.
 *
 * Usage: txd story-file-name
 *
 * Mark Howell 25 August 1992 howell_ma@movies.enet.dec.com
 *
 * History:
 *    Merge separate disassemblers for each type into one program
 *    Fix logic error in low routine scan
 *    Force PC past start PC in middle routine scan
 *    Update opcodes
 *    Add pre-action and action verb names to routines
 *    Add print mode descriptions
 *    Wrap long lines of text
 *    Cleanup for 16 bit machines
 *    Change JGE and JLE to JG and JL
 *    Add support for V1 and V2 games
 *    Fix bug in printing of last string
 *    Add symbolic names for routines and labels
 *    Add verb syntax
 *    Improve verb formatting
 *    Add command line options
 *    Fix check for low address
 *    Add a switch to turn off grammar
 *    Add support for V6 games
 *    Make start of code for low scan the end of dictionary data
 *    Generate Inform style syntax as an option
 *    Add dump style and width option
 *    Fix lint warnings
 *    Fix inter-routine backward jumps
 *    Update operand names
 *    Add support for V7 and V8 games
 *    Limit cache size to MAX_CACHE pages
 *    Improve translation of constants to symbols
 *    Distinguish between pre-actions and Inform parsing routines
 *    Introduce indirect operands, eg. load [sp] sp
 *    Fix object 0 problem
 *    Add support for European characters (codes up to 223)
 *    Add support for Inform 6 (helped by Matthew T. Russotto)
 *    Add support for GV2 (MTR)
 *    Add support for Infocom V6 games
 *    Fix dependencies on sizeof(int) == 2 (mostly cosmetic) NOT DONE
 *    Add -S dump-strings at address option
 *    Remove GV2A support
 *    Add unicode disassembly support
 *    Add Inform and user symbol table support
 */

using System;

using zword_t = System.UInt16;
using zbyte_t = System.Byte;

namespace ZTools
{

    public static class txd
    {

        internal const int MAX_PCS = 100;

        private static ulong ROUND_CODE(ulong address)
        {
            return ((address + (txio.code_scaler - 1)) & ~(txio.code_scaler - 1));
        }

        private static ulong ROUND_DATA(ulong address)
        {
            return ((address + (txio.story_scaler - 1)) & ~(txio.story_scaler - 1));
        }

        static ulong[] pctable;
        static int pcindex = 0;

        static ulong start_data_pc, end_data_pc;

        static tx_h.decode_t decode;
        static tx_h.opcode_t opcode;

        static System.Collections.Generic.List<tx_h.cref_item_t> strings_base = null;
        static System.Collections.Generic.List<tx_h.cref_item_t> routines_base = null;
        static tx_h.cref_item_t current_routine = null;

        static int locals_count = 0;
        static ulong start_of_routine = 0;

        static uint verb_count = 0;
        static uint action_count = 0;
        static uint parse_count = 0;
        static uint parser_type = 0;
        static uint prep_type = 0;
        static ulong verb_table_base = 0;
        static ulong verb_data_base = 0;
        static ulong action_table_base = 0;
        static ulong preact_table_base = 0;
        static ulong prep_table_base = 0;
        static ulong prep_table_end = 0;

        static int[] verb_sizes;  // TODO Make this a const or something

        static ulong dict_start = 0;
        static ulong dict_end = 0;
        static ulong word_size = 0;
        static ulong word_count = 0;

        static ulong code_base = 0;

        static int obj_count = 0;
        static ulong obj_table_base = 0, obj_table_end = 0, obj_data_base = 0, obj_data_end = 0;
        static ushort inform_version = 0;
        static ulong class_numbers_base = 0, class_numbers_end = 0;
        static ulong property_names_base = 0, property_names_end = 0;
        static ulong attr_names_base = 0, attr_names_end = 0;

        static int option_labels = 1;
        static int option_grammar = 1;
        static int option_dump = 0;
        static int option_width = 79;
        static int option_symbols = 0;
        static ulong string_location = 0;

        public static String main(byte[] storyData, string[] args)
        {

            verb_sizes = new int[4] { 2, 4, 7, 7 };
            //    int c, errflg = 0;

            //    /* Parse the options */

            //    while ((c = getopt (argc, argv, "abdghnsw:S:u:")) != EOF) {
            //    switch (c) {
            //        case 'a':
            //        option_inform = 6;
            //        break;
            //        case 'd':
            //        option_dump = 1;
            //        break;
            //        case 'g':
            //        option_grammar = 0;
            //        break;
            //        case 'n':
            //        option_labels = 0;
            //        break;
            //        case 'w':
            //        option_width = atoi (optarg);
            //        break;
            //        case 'u':
            //        init_symbols(optarg);
            //        /*FALLTHRU*/
            //        case 's':
            //        option_symbols = 1;
            //        break;
            //        case 'S':
            //#ifdef HAS_STRTOUL
            //            string_location = strtoul(optarg, (char **)NULL, 0);
            //#else
            //            string_location = atoi(optarg);
            //#endif
            //        break;
            //        case 'h':
            //        case '?':
            //        default:
            //        errflg++;
            //    }
            //    }

            //    /* Display usage if unknown flag or no story file */

            //    if (errflg || optind >= argc) {
            //    (void) fprintf (stderr, "usage: %s [options...] story-file [story-file...]\n\n", argv[0]);
            //    (void) fprintf (stderr, "TXD version 7/3 - disassemble Infocom story files. By Mark Howell\n");
            //    (void) fprintf (stderr, "Works with V1 to V8 Infocom games.\n\n");
            //    (void) fprintf (stderr, "\t-a   generate alternate syntax used by Inform\n");
            //    (void) fprintf (stderr, "\t-d   dump hex of opcodes and data\n");
            //    (void) fprintf (stderr, "\t-g   turn off grammar for action routines\n");
            //    (void) fprintf (stderr, "\t-n   use addresses instead of labels\n");
            //    (void) fprintf (stderr, "\t-w n display width (0 = no wrap)\n");
            //    (void) fprintf (stderr, "\t-s   Symbolic mode (Inform 6+ only)\n");
            //    (void) fprintf (stderr, "\t-u <file> Read user symbol table, implies -s for Inform games\n");
            //    (void) fprintf (stderr, "\t-S n Dump high strings only, starting at address n\n");
            //    exit (EXIT_FAILURE);
            //    }

            //    /* Process any story files on the command line */

            //    for (; optind < argc; optind++)
                return process_story(storyData);

            //    exit (EXIT_SUCCESS);


        }/* main */

        internal static String process_story(byte[] storyData)
        {
            routines_base = new System.Collections.Generic.List<tx_h.cref_item_t>();

            decode = new tx_h.decode_t();
            opcode = new tx_h.opcode_t();

            pctable = new ulong[MAX_PCS];

            txio.tx_set_width(option_width);

            txio.open_story(storyData);

            txio.startStringBuilder();

            txio.configure(tx_h.V1, tx_h.V8);

            var header = txio.header;

            //    load_cache ();

            setup_dictionary();

            if (option_grammar > 0)
                showverb.configure_parse_tables(out verb_count, out action_count, out parse_count, out parser_type, out prep_type,
                            out verb_table_base, out verb_data_base,
                            out action_table_base, out preact_table_base,
                            out prep_table_base, out prep_table_end);

            if (option_symbols > 0 && (parser_type >= (int)tx_h.parser_types.inform_gv1))
            {
                showobj.configure_object_tables(out obj_count, out obj_table_base, out obj_table_end,
                                  out obj_data_base, out obj_data_end);
                infinfo.configure_inform_tables(obj_data_end, out inform_version, out class_numbers_base, out class_numbers_end,
                                out property_names_base, out property_names_end, out attr_names_base, out attr_names_end);
            }

            if (header.version != tx_h.V6 && header.version != tx_h.V7)
            {
                decode.pc = code_base = dict_end;
                decode.initial_pc = (ulong)header.start_pc - 1;
            }
            else
            {
                decode.pc = code_base = (ulong)header.routines_offset * txio.story_scaler;
                decode.initial_pc = decode.pc + (ulong)header.start_pc * txio.code_scaler;
            }

            txio.tx_printf("Resident data ends at {0:X}, program starts at {1:X}, file ends at {2:X}\n",
                   (ulong)header.resident_size, (ulong)decode.initial_pc, (ulong)txio.file_size);
            txio.tx_printf("\nStarting analysis pass at address {0:X}\n", (ulong)decode.pc);

#if TXD_DEBUG
        decode.first_pass = 0;
        decode.low_address = decode.initial_pc;
        decode.high_address = (ulong)txio.file_size;
#else
            decode.first_pass = true;
#endif

            if (string_location > 0)
            {
                decode_strings(string_location);
                return txio.getTextFromStringBuilder();
            }
            decode_program();

            scan_strings(decode.pc);

#if !TXD_DEBUG
            txio.tx_printf("\nEnd of analysis pass, low address = {0:X}, high address = {1:X}\n",
                   (ulong)decode.low_address, (ulong)decode.high_address);
            if (start_data_pc > 0)
                txio.tx_printf("\n{0} bytes of data in code from {1:X} to {2:X}\n",
                       (ulong)(end_data_pc - start_data_pc),
                       (ulong)start_data_pc, (ulong)end_data_pc);
            if ((decode.low_address - code_base) >= txio.story_scaler)
            {
                txio.tx_printf("\n{0} bytes of data before code from {1:X} to {2:X}\n",
                       (ulong)(decode.low_address - code_base),
                       (ulong)code_base, (ulong)decode.low_address);
                if (option_dump > 0)
                {
                    txio.tx_printf("\n[Start of data");
                    if (option_labels == 0)
                        txio.tx_printf(" at {0:X}", (ulong)code_base);
                    txio.tx_printf("]\n\n");
                    dump_data(code_base, decode.low_address - 1);
                    txio.tx_printf("\n[End of data");
                    if (option_labels == 0)
                        txio.tx_printf(" at {0:X}", (ulong)(decode.low_address - 1));
                    txio.tx_printf("]\n");
                }
            }

            if (option_labels > 0)
                renumber_cref(routines_base);

            decode.first_pass = false;
            decode_program();

            decode_strings(decode.pc);
#endif

            txio.close_story();

            return txio.getTextFromStringBuilder();

        }/* process_story */

        ///* decode_program - Decode Z code in two passes */

        // static int count = 0;

        internal static void decode_program()
        {
            ulong pc, low_pc, high_pc, prev_low_pc, prev_high_pc;
            int i, vars;
            bool flag = false;

            var header = txio.header;

            if (decode.first_pass)
            {
                if (decode.pc < decode.initial_pc)
                {
                    /* Scan for low routines */
                    decode.pc = ROUND_CODE(decode.pc);
                    for (pc = decode.pc, flag = false; pc < decode.initial_pc && !flag; pc += txio.code_scaler)
                    {
                        for (vars = (char)txio.read_data_byte(ref pc); vars < 0 || vars > 15; vars = (char)txio.read_data_byte(ref pc))
                            pc = ROUND_CODE(pc);
                        decode.pc = pc - 1;
                        for (i = 0, flag = true; i < 3 && flag; i++)
                        {
                            pcindex = 0;
                            decode.pc = ROUND_CODE(decode.pc);
                            if (decode_routine() != tx_h.END_OF_ROUTINE || pcindex > 0)
                                flag = false;
                        }
                        decode.pc = pc - 1;
                    }
                    if (flag && (uint)header.version < tx_h.V5)
                    {
                        pc = decode.pc;
                        vars = (char)txio.read_data_byte(ref pc);
                        low_pc = decode.pc;
                        for (pc = (ulong)(pc + ((ulong)vars * 2) - 1), flag = false; pc >= low_pc && !flag; pc -= txio.story_scaler)
                        {
                            decode.pc = pc;
                            for (i = 0, flag = true; i < 3 && flag; i++)
                            {
                                pcindex = 0;
                                decode.pc = ROUND_CODE(decode.pc);
                                if (decode_routine() != tx_h.END_OF_ROUTINE || pcindex > 0)
                                    flag = false;
                            }
                            decode.pc = pc;
                        }
                    }
                    if (!flag || decode.pc > decode.initial_pc)
                        decode.pc = decode.initial_pc;
                }
                /* Fill in middle routines */
                decode.low_address = decode.pc;
                decode.high_address = decode.pc;
                start_data_pc = 0;
                end_data_pc = 0;
                do
                {
                    if (option_labels > 0)
                    {
                        routines_base = null;
                    }
                    prev_low_pc = decode.low_address;
                    prev_high_pc = decode.high_address;
                    flag = false;
                    pcindex = 0;
                    low_pc = decode.low_address;
                    high_pc = decode.high_address;
                    pc = decode.pc = decode.low_address;
                    while (decode.pc <= high_pc || decode.pc <= decode.initial_pc)
                    {
                        if (start_data_pc == decode.pc)
                            decode.pc = end_data_pc;
                        if (decode_routine() != tx_h.END_OF_ROUTINE)
                        {
                            if (start_data_pc == 0)
                                start_data_pc = decode.pc;
                            flag = true;
                            end_data_pc = 0;
                            pcindex = 0;
                            pc = ROUND_CODE(pc);
                            do
                            {
                                pc += txio.code_scaler;
                                vars = (char)txio.read_data_byte(ref pc);
                                pc--;
                            } while (vars < 0 || vars > 15);
                            decode.pc = pc;
                        }
                        else
                        {
                            if (pc < decode.initial_pc && decode.pc > decode.initial_pc)
                            {
                                pc = decode.pc = decode.initial_pc;
                                decode.low_address = low_pc;
                                decode.high_address = high_pc;
                            }
                            else
                            {
                                if (start_data_pc > 0 && end_data_pc == 0)
                                    end_data_pc = pc;
                                pc = ROUND_CODE(decode.pc);
                                if (!flag)
                                {
                                    low_pc = decode.low_address;
                                    high_pc = decode.high_address;
                                }
                            }
                        }
                    }
                    decode.low_address = low_pc;
                    decode.high_address = high_pc;
                    // Console.WriteLine("{0} < {1} : {2} > {3}", low_pc, prev_low_pc, high_pc, prev_high_pc);

                } while (low_pc < prev_low_pc || high_pc > prev_high_pc);
                /* Scan for high routines */
                pc = decode.pc;
                while (decode_routine() == tx_h.END_OF_ROUTINE)
                {
                    decode.high_address = pc;
                    pc = decode.pc;
                }
            }
            else
            {
                txio.tx_printf("\n[Start of code");
                if (option_labels == 0)
                    txio.tx_printf(" at {0:X}", (ulong)decode.low_address);
                txio.tx_printf("]\n");
                for (decode.pc = decode.low_address;
                     decode.pc <= decode.high_address; )
                    decode_routine();
                txio.tx_printf("\n[End of code");
                if (option_labels == 0)
                    txio.tx_printf(" at {0:X}", (ulong)decode.pc);
                txio.tx_printf("]\n");
            }

        }/* decode_program */

        /* decode_routine - Decode a routine from start address to last instruction */

        static int decode_routine()
        {
            var header = txio.header;

            ulong old_pc, old_start;
            tx_h.cref_item_t cref_item;
            int vars, status, i, locals;

            if (decode.first_pass)
            {
                cref_item = null;
                if (option_labels > 0)
                    cref_item = current_routine;
                old_start = start_of_routine;
                locals = locals_count;
                old_pc = decode.pc;
                decode.pc = ROUND_CODE(decode.pc);
                vars = txio.read_data_byte(ref decode.pc);
                if (vars >= 0 && vars <= 15)
                {
                    if (option_labels > 0)
                        add_routine(decode.pc - 1);
                    locals_count = vars;
                    if ((uint)header.version < tx_h.V5)
                        for (; vars > 0; vars--)
                            txio.read_data_word(ref decode.pc);
                    start_of_routine = decode.pc;
                    if (decode_code() == tx_h.END_OF_ROUTINE)
                        return (tx_h.END_OF_ROUTINE);
                    if (option_labels > 0)
                        current_routine = cref_item;
                    start_of_routine = old_start;
                    locals_count = locals;
                }
                decode.pc = old_pc;
                if ((status = decode_code()) != tx_h.END_OF_ROUTINE)
                {
                    decode.pc = old_pc;
                }
                else
                {
                    pctable[pcindex++] = old_pc;
                    if (pcindex == MAX_PCS)
                    {
                        throw new NotImplementedException("\nFatal: too many orphan code fragments\n");
                    }
                }
            }
            else
            {
                if (decode.pc == start_data_pc)
                {
                    if (option_dump > 0)
                    {
                        txio.tx_printf("\n[Start of data");
                        if (option_labels == 0)
                            txio.tx_printf(" at {0:X}", (ulong)start_data_pc);
                        txio.tx_printf("]\n\n");
                        dump_data(start_data_pc, end_data_pc - 1);
                        txio.tx_printf("\n[End of data");
                        if (option_labels == 0)
                            txio.tx_printf(" at {0:X}", (ulong)(end_data_pc - 1));
                        txio.tx_printf("]\n");
                    }
                    decode.pc = end_data_pc;
                }
                for (i = 0; i < pcindex && decode.pc != pctable[i]; i++)
                    ;
                if (i == pcindex)
                {
                    decode.pc = ROUND_CODE(decode.pc);
                    start_of_routine = decode.pc;
                    vars = txio.read_data_byte(ref decode.pc);
                    if (option_labels > 0)
                    {
                        txio.tx_printf("{0}outine {1}{2:d4}, {3} local",
                                           (decode.pc - 1 == decode.initial_pc) ? "\nMain r" : "\nR",
                                           (txio.option_inform) ? 'r' : 'R',
                               (int)lookup_routine(decode.pc - 1, 1),
                                           (int)vars);
                    }
                    else
                    {
                        txio.tx_printf("{0}outine {1:X}, {2} local",
                               (decode.pc - 1 == decode.initial_pc) ? "\nMain r" : "\nR",
                                           (ulong)(decode.pc - 1),
                                           (int)vars);
                    }
                    if (vars != 1)
                        txio.tx_printf("s");
                    if ((uint)header.version < tx_h.V5)
                    {
                        txio.tx_printf(" (");
                        txio.tx_fix_margin(1);
                        for (; vars > 0; vars--)
                        {
                            txio.tx_printf("{0:X4}", (uint)txio.read_data_word(ref decode.pc));
                            if (vars > 1)
                                txio.tx_printf(", ");
                        }
                        txio.tx_fix_margin(0);
                        txio.tx_printf(")");
                    }
                    txio.tx_printf("\n");
                    lookup_verb(start_of_routine);
                    txio.tx_printf("\n");
                }
                else
                    txio.tx_printf("\norphan code fragment:\n\n");
                status = decode_code();
            }

            return (status);

        }/* decode_routine */

        /* decode_code - grab opcode and determine the class */

        static int decode_code()
        {
            int status;
            int label;

            var header = txio.header;

            decode.high_pc = decode.pc;
            do
            {
                if (!decode.first_pass)
                {
                    if (option_labels > 0)
                    {
                        label = lookup_label(decode.pc, 0);
                        if (label != 0)
                            txio.tx_printf("{0}{1:d4}: ", (txio.option_inform) ? 'l' : 'L', (int)label);
                        else
                            txio.tx_printf("       ");
                    }
                    else
                        txio.tx_printf("{0:X5}:  ", (ulong)decode.pc);
                }
                opcode.opcode = txio.read_data_byte(ref decode.pc);

                if ((uint)header.version > tx_h.V4 && opcode.opcode == 0xbe)
                {
                    opcode.opcode = txio.read_data_byte(ref decode.pc);
                    opcode.opclass = tx_h.EXTENDED_OPERAND;
                }
                else if (opcode.opcode < 0x80)
                    opcode.opclass = tx_h.TWO_OPERAND;
                else
                    if (opcode.opcode < 0xb0)
                        opcode.opclass = tx_h.ONE_OPERAND;
                    else
                        if (opcode.opcode < 0xc0)
                            opcode.opclass = tx_h.ZERO_OPERAND;
                        else
                            opcode.opclass = tx_h.VARIABLE_OPERAND;

                status = decode_opcode();
            } while (status == tx_h.END_OF_INSTRUCTION);

            return (status);

        }/* decode_code */


        /* decode_opcode - Check and decode the opcode itself */

        static int decode_opcode()
        {
            int code;

            var header = txio.header;

            code = opcode.opcode;

            switch (opcode.opclass)
            {

                case tx_h.EXTENDED_OPERAND:
                    code &= 0x3f;
                    switch (code)
                    {
                        case 0x00: return decode_operands("SAVE", tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.LOW_ADDR, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x01: return decode_operands("RESTORE", tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.LOW_ADDR, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x02: return decode_operands("LOG_SHIFT", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x03: return decode_operands("ART_SHIFT", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x04: return decode_operands("SET_FONT", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x05: return decode_operands("DRAW_PICTURE", tx_h.NUMBER, tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x06: return decode_operands("PICTURE_DATA", tx_h.NUMBER, tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                        case 0x07: return decode_operands("ERASE_PICTURE", tx_h.NUMBER, tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x08: return decode_operands("SET_MARGINS", tx_h.NUMBER, tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x09: return decode_operands("SAVE_UNDO", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x0A: return decode_operands("RESTORE_UNDO", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);

                        case 0x10: return decode_operands("MOVE_WINDOW", tx_h.NUMBER, tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x11: return decode_operands("WINDOW_SIZE", tx_h.NUMBER, tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x12: return decode_operands("WINDOW_STYLE", tx_h.NUMBER, tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x13: return decode_operands("GET_WIND_PROP", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x14: return decode_operands("SCROLL_WINDOW", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x15: return decode_operands("POP_STACK", tx_h.NUMBER, tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x16: return decode_operands("READ_MOUSE", tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x17: return decode_operands("MOUSE_WINDOW", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x18: return decode_operands("PUSH_STACK", tx_h.NUMBER, tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                        case 0x19: return decode_operands("PUT_WIND_PROP", tx_h.NUMBER, tx_h.NUMBER, tx_h.ANYTHING, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x1A: return decode_operands("PRINT_FORM", tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x1B: return decode_operands("MAKE_MENU", tx_h.NUMBER, tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                        case 0x1C: return decode_operands("PICTURE_TABLE", tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                        default:
                            return (decode_operands("ILLEGAL", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.ILLEGAL));
                    }

                case tx_h.TWO_OPERAND:
                case tx_h.VARIABLE_OPERAND:
                    if (opcode.opclass == tx_h.TWO_OPERAND)
                    {
                        code &= 0x1f;
                    }

                    code &= 0x3f;
                    switch (code)
                    {
                        case 0x01: return decode_operands("JE", tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.BRANCH, tx_h.PLAIN);
                        case 0x02: return decode_operands("JL", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                        case 0x03: return decode_operands("JG", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                        case 0x04: return decode_operands("DEC_CHK", tx_h.VAR, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                        case 0x05: return decode_operands("INC_CHK", tx_h.VAR, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                        case 0x06: return decode_operands("JIN", tx_h.OBJECT, tx_h.OBJECT, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                        case 0x07: return decode_operands("TEST", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                        case 0x08: return decode_operands("OR", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x09: return decode_operands("AND", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x0A: return decode_operands("TEST_ATTR", tx_h.OBJECT, tx_h.ATTRNUM, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                        case 0x0B: return decode_operands("SET_ATTR", tx_h.OBJECT, tx_h.ATTRNUM, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x0C: return decode_operands("CLEAR_ATTR", tx_h.OBJECT, tx_h.ATTRNUM, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x0D: return decode_operands("STORE", tx_h.VAR, tx_h.ANYTHING, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x0E: return decode_operands("INSERT_OBJ", tx_h.OBJECT, tx_h.OBJECT, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x0F: return decode_operands("LOADW", tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x10: return decode_operands("LOADB", tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x11: return decode_operands("GET_PROP", tx_h.OBJECT, tx_h.PROPNUM, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x12: return decode_operands("GET_PROP_ADDR", tx_h.OBJECT, tx_h.PROPNUM, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x13: return decode_operands("GET_NEXT_PROP", tx_h.OBJECT, tx_h.PROPNUM, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x14: return decode_operands("ADD", tx_h.LOW_ADDR, tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x15: return decode_operands("SUB", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x16: return decode_operands("MUL", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x17: return decode_operands("DIV", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x18: return decode_operands("MOD", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);

                        case 0x21: return decode_operands("STOREW", tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.ANYTHING, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x22: return decode_operands("STOREB", tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.ANYTHING, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x23: return decode_operands("PUT_PROP", tx_h.OBJECT, tx_h.NUMBER, tx_h.ANYTHING, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                        case 0x25: return decode_operands("PRINT_CHAR", tx_h.PCHAR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x26: return decode_operands("PRINT_NUM", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x27: return decode_operands("RANDOM", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x28: return decode_operands("PUSH", tx_h.ANYTHING, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                        case 0x2A: return decode_operands("SPLIT_WINDOW", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x2B: return decode_operands("SET_WINDOW", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                        case 0x33: return decode_operands("OUTPUT_STREAM", tx_h.PATTR, tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x34: return decode_operands("INPUT_STREAM", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x35: return decode_operands("SOUND_EFFECT", tx_h.NUMBER, tx_h.NUMBER, tx_h.NUMBER, tx_h.ROUTINE, tx_h.NONE, tx_h.PLAIN);

                        default:
                            switch ((int)header.version)
                            {
                                case tx_h.V1:
                                case tx_h.V2:
                                case tx_h.V3:
                                    switch (code)
                                    {
                                        case 0x20: return decode_operands("CALL", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.STORE, tx_h.CALL);

                                        case 0x24: return decode_operands("READ", tx_h.LOW_ADDR, tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                                        case 0x29: return decode_operands("PULL", tx_h.VAR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                    }
                                    break;
                                case tx_h.V4:
                                    switch (code)
                                    {
                                        case 0x19: return decode_operands("CALL_2S", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.CALL);

                                        case 0x20: return decode_operands("CALL_VS", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.STORE, tx_h.CALL);

                                        case 0x24: return decode_operands("READ", tx_h.LOW_ADDR, tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.ROUTINE, tx_h.NONE, tx_h.PLAIN);

                                        case 0x29: return decode_operands("PULL", tx_h.VAR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                                        case 0x2C: return decode_operands("CALL_VS2", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.STORE, tx_h.CALL);
                                        case 0x2D: return decode_operands("ERASE_WINDOW", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x2E: return decode_operands("ERASE_LINE", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x2F: return decode_operands("SET_CURSOR", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                                        case 0x31: return decode_operands("SET_TEXT_STYLE", tx_h.VATTR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x32: return decode_operands("BUFFER_MODE", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                                        case 0x36: return decode_operands("READ_CHAR", tx_h.NUMBER, tx_h.NUMBER, tx_h.ROUTINE, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                                        case 0x37: return decode_operands("SCAN_TABLE", tx_h.ANYTHING, tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NUMBER, tx_h.BOTH, tx_h.PLAIN);
                                    }
                                    break;
                                case tx_h.V5:
                                case tx_h.V7:
                                case tx_h.V8:
                                    switch (code)
                                    {
                                        case 0x19: return decode_operands("CALL_2S", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.CALL);
                                        case 0x1A: return decode_operands("CALL_2N", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.CALL);
                                        case 0x1B: return decode_operands("SET_COLOUR", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                                        case 0x20: return decode_operands("CALL_VS", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.STORE, tx_h.CALL);

                                        case 0x24: return decode_operands("READ", tx_h.LOW_ADDR, tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.ROUTINE, tx_h.STORE, tx_h.PLAIN);

                                        case 0x29: return decode_operands("PULL", tx_h.VAR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                                        case 0x2C: return decode_operands("CALL_VS2", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.STORE, tx_h.CALL);
                                        case 0x2D: return decode_operands("ERASE_WINDOW", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x2E: return decode_operands("ERASE_LINE", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x2F: return decode_operands("SET_CURSOR", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                                        case 0x31: return decode_operands("SET_TEXT_STYLE", tx_h.VATTR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x32: return decode_operands("BUFFER_MODE", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                                        case 0x36: return decode_operands("READ_CHAR", tx_h.NUMBER, tx_h.NUMBER, tx_h.ROUTINE, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                                        case 0x37: return decode_operands("SCAN_TABLE", tx_h.ANYTHING, tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NUMBER, tx_h.BOTH, tx_h.PLAIN);

                                        case 0x39: return decode_operands("CALL_VN", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.NONE, tx_h.CALL);
                                        case 0x3A: return decode_operands("CALL_VN2", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.NONE, tx_h.CALL);
                                        case 0x3B: return decode_operands("TOKENISE", tx_h.LOW_ADDR, tx_h.LOW_ADDR, tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NONE, tx_h.PLAIN);
                                        case 0x3C: return decode_operands("ENCODE_TEXT", tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NUMBER, tx_h.LOW_ADDR, tx_h.NONE, tx_h.PLAIN);
                                        case 0x3D: return decode_operands("COPY_TABLE", tx_h.LOW_ADDR, tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x3E: return decode_operands("PRINT_TABLE", tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NUMBER, tx_h.NUMBER, tx_h.NONE, tx_h.PLAIN);
                                        case 0x3F: return decode_operands("CHECK_ARG_COUNT", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                                    }
                                    break;
                                case tx_h.V6:
                                    switch (code)
                                    {
                                        case 0x19: return decode_operands("CALL_2S", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.CALL);
                                        case 0x1A: return decode_operands("CALL_2N", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.CALL);
                                        case 0x1B: return decode_operands("SET_COLOUR", tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x1C: return decode_operands("THROW", tx_h.ANYTHING, tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                                        case 0x20: return decode_operands("CALL_VS", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.STORE, tx_h.CALL);

                                        case 0x24: return decode_operands("READ", tx_h.LOW_ADDR, tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.ROUTINE, tx_h.STORE, tx_h.PLAIN);

                                        case 0x29: return decode_operands("PULL", tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);

                                        case 0x2C: return decode_operands("CALL_VS2", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.STORE, tx_h.CALL);
                                        case 0x2D: return decode_operands("ERASE_WINDOW", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x2E: return decode_operands("ERASE_LINE", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x2F: return decode_operands("SET_CURSOR", tx_h.NUMBER, tx_h.NUMBER, tx_h.NUMBER, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x30: return decode_operands("GET_CURSOR", tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x31: return decode_operands("SET_TEXT_STYLE", tx_h.VATTR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x32: return decode_operands("BUFFER_MODE", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                                        case 0x36: return decode_operands("READ_CHAR", tx_h.NUMBER, tx_h.NUMBER, tx_h.ROUTINE, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                                        case 0x37: return decode_operands("SCAN_TABLE", tx_h.ANYTHING, tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NUMBER, tx_h.BOTH, tx_h.PLAIN);
                                        case 0x38: return decode_operands("NOT", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                                        case 0x39: return decode_operands("CALL_VN", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.NONE, tx_h.CALL);
                                        case 0x3A: return decode_operands("CALL_VN2", tx_h.ROUTINE, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.ANYTHING, tx_h.NONE, tx_h.CALL);
                                        case 0x3B: return decode_operands("TOKENISE", tx_h.LOW_ADDR, tx_h.LOW_ADDR, tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NONE, tx_h.PLAIN);
                                        case 0x3C: return decode_operands("ENCODE_TEXT", tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NUMBER, tx_h.LOW_ADDR, tx_h.NONE, tx_h.PLAIN);
                                        case 0x3D: return decode_operands("COPY_TABLE", tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.LOW_ADDR, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                        case 0x3E: return decode_operands("PRINT_TABLE", tx_h.LOW_ADDR, tx_h.NUMBER, tx_h.NUMBER, tx_h.NUMBER, tx_h.NONE, tx_h.PLAIN);
                                        case 0x3F: return decode_operands("CHECK_ARG_COUNT", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                                    }
                                    break;
                            }
                            return (decode_operands("ILLEGAL", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.ILLEGAL));
                    }

                case tx_h.ONE_OPERAND:
                    code &= 0x0f;
                    switch (code)
                    {
                        case 0x00: return decode_operands("JZ", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                        case 0x01: return decode_operands("GET_SIBLING", tx_h.OBJECT, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.BOTH, tx_h.PLAIN);
                        case 0x02: return decode_operands("GET_CHILD", tx_h.OBJECT, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.BOTH, tx_h.PLAIN);
                        case 0x03: return decode_operands("GET_PARENT", tx_h.OBJECT, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x04: return decode_operands("GET_PROP_LEN", tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                        case 0x05: return decode_operands("INC", tx_h.VAR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x06: return decode_operands("DEC", tx_h.VAR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x07: return decode_operands("PRINT_ADDR", tx_h.LOW_ADDR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                        case 0x09: return decode_operands("REMOVE_OBJ", tx_h.OBJECT, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x0A: return decode_operands("PRINT_OBJ", tx_h.OBJECT, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x0B: return decode_operands("RET", tx_h.ANYTHING, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.RETURN);
                        case 0x0C: return decode_operands("JUMP", tx_h.LABEL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.RETURN);
                        case 0x0D: return decode_operands("PRINT_PADDR", tx_h.STATIC, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x0E: return decode_operands("LOAD", tx_h.VAR, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);

                        default:
                            switch ((int)header.version)
                            {
                                case tx_h.V1:
                                case tx_h.V2:
                                case tx_h.V3:
                                    switch (code)
                                    {
                                        case 0x0F: return decode_operands("NOT", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                                    }
                                    break;
                                case tx_h.V4:
                                    switch (code)
                                    {
                                        case 0x08: return decode_operands("CALL_1S", tx_h.ROUTINE, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.CALL);

                                        case 0x0F: return decode_operands("NOT", tx_h.NUMBER, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                                    }
                                    break;
                                case tx_h.V5:
                                case tx_h.V6:
                                case tx_h.V7:
                                case tx_h.V8:
                                    switch (code)
                                    {
                                        case 0x08: return decode_operands(".CALL_1S", tx_h.ROUTINE, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.CALL);

                                        case 0x0F: return decode_operands("CALL_1N", tx_h.ROUTINE, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.CALL);
                                    }
                                    break;
                            }
                            return (decode_operands("ILLEGAL", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.ILLEGAL));
                    }

                case tx_h.ZERO_OPERAND:
                    code &= 0x0f;
                    switch (code)
                    {
                        case 0x00: return decode_operands("RTRUE", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.RETURN);
                        case 0x01: return decode_operands("RFALSE", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.RETURN);
                        case 0x02: return decode_operands("PRINT", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.TEXT, tx_h.PLAIN);
                        case 0x03: return decode_operands("PRINT_RET", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.TEXT, tx_h.RETURN);
                        case 0x04: return decode_operands("NOP", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                        case 0x07: return decode_operands("RESTART", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                        case 0x08: return decode_operands("RET_POPPED", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.RETURN);

                        case 0x0A: return decode_operands("QUIT", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.RETURN);
                        case 0x0B: return decode_operands("NEW_LINE", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                        case 0x0D: return decode_operands("VERIFY", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);

                        default:
                            switch (header.version)
                            {
                                case tx_h.V1:
                                case tx_h.V2:
                                case tx_h.V3:
                                    switch (code)
                                    {
                                        case 0x05: return decode_operands("SAVE", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                                        case 0x06: return decode_operands("RESTORE", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);

                                        case 0x09: return decode_operands("POP", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                                        case 0x0C: return decode_operands("SHOW_STATUS", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                    }
                                    break;
                                case tx_h.V4:
                                    switch (code)
                                    {
                                        case 0x09: return decode_operands("POP", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);

                                        case 0x05: return decode_operands("SAVE", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                                        case 0x06: return decode_operands("RESTORE", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                                    }
                                    break;
                                case tx_h.V5:
                                case tx_h.V7:
                                case tx_h.V8:
                                    switch (code)
                                    {
                                        case 0x09: return decode_operands("CATCH", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);
                                        /* From a bug in Wishbringer V23 */
                                        case 0x0C: return decode_operands("SHOW_STATUS", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.PLAIN);
                                    }
                                    break;
                                case tx_h.V6:
                                    switch (code)
                                    {
                                        case 0x09: return decode_operands("CATCH", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.STORE, tx_h.PLAIN);

                                        case 0x0F: return decode_operands("PIRACY", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.BRANCH, tx_h.PLAIN);
                                    }
                                    break;
                            }
                            return (decode_operands("ILLEGAL", tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NIL, tx_h.NONE, tx_h.ILLEGAL));
                    }

                default:
                    throw new NotImplementedException(String.Format("\nFatal: bad class ({0})\n", (int)opcode.opclass));
            }

        }/* decode_opcode */

        /* decode_operands - Decode operands of opcode */

        static int decode_operands(String opcode_name, int par1, int par2, int par3, int par4, int extra, int type)
        {
            int len;
            int i, opers, status;

            opcode.par[0] = par1;
            opcode.par[1] = par2;
            opcode.par[2] = par3;
            opcode.par[3] = par4;
            opcode.extra = extra;
            opcode.type = type;

            if (opcode.type == tx_h.ILLEGAL)
                return (tx_h.BAD_OPCODE);

            if (decode.first_pass)
            {
                status = decode_parameters(out opers);
                if (status > 0)
                    return (tx_h.BAD_OPCODE);
                status = decode_extra();
            }
            else
            {
                if (option_dump > 0)
                    dump_opcode(decode.pc, opcode.opcode, opcode.opclass, opcode.par, opcode.extra);
                if (txio.option_inform)
                {
                    len = opcode_name.Length;
                    for (i = 0; i < len; i++)
                        txio.tx_printf("{0}", Char.ToLower(opcode_name[i]));
                }
                else
                {
                    txio.tx_printf(opcode_name);
                    // len = strlen (opcode_name);
                    len = opcode_name.Length;
                }
                for (; len < 16; len++)
                    txio.tx_printf(" ");
                decode_parameters(out opers);
                if (opers > 0 && opcode.extra != tx_h.NONE)
                    txio.tx_printf(" ");
                status = decode_extra();
                txio.tx_printf("\n");
            }
            if (decode.pc > decode.high_pc)
                decode.high_pc = decode.pc;

            return (status);
        }/* decode_operands */

        /* decode_parameters - Decode input parameters */

        static int decode_parameters(out int opers)
        {

            int status, modes, addr_mode, maxopers;

            opers = 0;

            switch (opcode.opclass)
            {

                case tx_h.ONE_OPERAND:
                    status = decode_parameter((opcode.opcode >> 4) & 0x03, 0);
                    if (status > 0)
                        return (status);
                    opers = 1;
                    break;

                case tx_h.TWO_OPERAND:
                    status = decode_parameter((opcode.opcode & 0x40) > 0 ? tx_h.VARIABLE : tx_h.BYTE_IMMED, 0);
                    if (status > 0)
                        return (status);
                    if (!decode.first_pass)
                    {
                        if (!txio.option_inform && opcode.type == tx_h.CALL)
                            txio.tx_printf(" (");
                        else
                            txio.tx_printf("{0}", (txio.option_inform) ? ' ' : ',');
                    }
                    status = decode_parameter((opcode.opcode & 0x20) > 0 ? tx_h.VARIABLE : tx_h.BYTE_IMMED, 1);
                    if (status > 0)
                        return (status);
                    opers = 2;
                    if (!txio.option_inform && !decode.first_pass && opcode.type == tx_h.CALL && opers > 1)
                        txio.tx_printf(")");
                    break;

                case tx_h.VARIABLE_OPERAND:
                case tx_h.EXTENDED_OPERAND:
                    if ((opcode.opcode & 0x3f) == 0x2c ||
                    (opcode.opcode & 0x3f) == 0x3a)
                    {
                        modes = txio.read_data_word(ref decode.pc);
                        maxopers = 8;
                    }
                    else
                    {
                        modes = txio.read_data_byte(ref decode.pc);
                        maxopers = 4;
                    }
                    for (addr_mode = 0, opers = 0;
                     (addr_mode != tx_h.NO_OPERAND) && maxopers > 0; maxopers--)
                    {
                        addr_mode = (modes >> ((maxopers - 1) * 2)) & 0x03;
                        if (addr_mode != tx_h.NO_OPERAND)
                        {
                            if (!decode.first_pass && opers > 0)
                            {
                                if (!txio.option_inform && opcode.type == tx_h.CALL && opers == 1)
                                    txio.tx_printf(" (");
                                else
                                    txio.tx_printf("{0}", (txio.option_inform) ? ' ' : ',');
                            }
                            status = decode_parameter(addr_mode, opers);
                            if (status > 0)
                                return (status);
                            opers++;
                        }
                    }
                    if (!txio.option_inform && !decode.first_pass && opcode.type == tx_h.CALL && opers > 1)
                        txio.tx_printf(")");
                    break;

                case tx_h.ZERO_OPERAND:
                    break;

                default:
                    throw new ArgumentException(String.Format("\nFatal: bad class ({0})\n", (int)opcode.opclass));
            }

            return (0);
        }/* decode_parameters */

        /* decode_parameter - Decode one input parameter */

        static int decode_parameter(int addr_mode, int opers)
        {
            var header = txio.header;

            ulong addr;
            uint value = 0;
            int routine, vars, par, dictionary, s;

            par = (opers < 4) ? opcode.par[opers] : tx_h.ANYTHING;

            switch (addr_mode)
            {

                case tx_h.WORD_IMMED:
                    value = (uint)txio.read_data_word(ref decode.pc);
                    break;

                case tx_h.BYTE_IMMED:
                    value = (uint)txio.read_data_byte(ref decode.pc);
                    break;

                case tx_h.VARIABLE:
                    value = (uint)txio.read_data_byte(ref decode.pc);
                    par = tx_h.VAR;
                    break;

                case tx_h.NO_OPERAND:
                    return (0);

                default:
                    throw new ArgumentException(String.Format("\nFatal: bad addressing mode ({0})\n", (int)addr_mode));
            }

            /*
             * To make the code more readable, VAR type operands are not translated
             * as constants, eg. INC 5 is actually printed as INC L05. However, if
             * the VAR type operand _is_ a variable, the translation should look like
             * INC [L05], ie. increment the variable which is given by the contents
             * of local variable #5. Earlier versions of "txd" translated both cases
             * as INC L05. This bug was finally detected by Graham Nelson.
             */

            if (opers < 4 && opcode.par[opers] == tx_h.VAR)
                par = (addr_mode == tx_h.VARIABLE) ? tx_h.INDIRECT : tx_h.VAR;

            switch (par)
            {

                case tx_h.NIL:
                    if (!decode.first_pass)
                    {
                        Console.Error.Write("\nWarning: Unexpected Parameter #{0} near {1:X5}\n", opers, decode.pc);
                        print_integer(value, addr_mode == tx_h.BYTE_IMMED);
                    }
                    break;

                case tx_h.ANYTHING:
                    if (!decode.first_pass)
                    {
                        addr = (ulong)txio.code_scaler * value + (ulong)txio.story_scaler * header.strings_offset;
                        s = lookup_string(addr);
                        if (s > 0)
                            txio.tx_printf("{0}{1,3:d}", (txio.option_inform) ? 's' : 'S', s);
                        addr = (ulong)value;
                        dictionary = in_dictionary(addr);
                        if (dictionary > 0)
                        {
                            if (s > 0)
                                txio.tx_printf(" {0} ", (txio.option_inform) ? "or" : "OR");
                            txio.tx_printf("\"");
                            txio.decode_text(ref addr);
                            txio.tx_printf("\"");
                        }
                        if (dictionary == 0 && s == 0)
                            print_integer(value, addr_mode == tx_h.BYTE_IMMED);
                    }
                    break;

                case tx_h.VAR:
                    if (!decode.first_pass)
                    {
                        if (value == 0)
                            txio.tx_printf("{0}", (txio.option_inform) ? "sp" : "(SP)+");
                        else
                            print_variable(value);
                    }
                    else
                    {
                        if ((int)value > 0 && (int)value < 16 && (int)value > locals_count)
                            return (1);
                    }
                    break;

                case tx_h.NUMBER:
                    if (!decode.first_pass)
                        print_integer(value, addr_mode == tx_h.BYTE_IMMED);
                    break;

                case tx_h.PROPNUM:
                    if (!decode.first_pass)
                        if (symbols.print_property_name(property_names_base, (int)value) == 0)
                            print_integer(value, addr_mode == tx_h.BYTE_IMMED);
                    break;

                case tx_h.ATTRNUM:
                    if (!decode.first_pass)
                        if (symbols.print_attribute_name(attr_names_base, (int)value) == 0)
                            print_integer(value, addr_mode == tx_h.BYTE_IMMED);
                    break;

                case tx_h.LOW_ADDR:
                    if (!decode.first_pass)
                    {
                        addr = (ulong)value;
                        dictionary = in_dictionary(addr);
                        if (dictionary > 0)
                        {
                            txio.tx_printf("\"");
                            txio.decode_text(ref addr);
                            txio.tx_printf("\"");
                        }
                        else
                            print_integer(value, addr_mode == tx_h.BYTE_IMMED);
                    }
                    break;

                case tx_h.ROUTINE:
                    addr = (ulong)txio.code_scaler * value + (ulong)header.routines_offset * txio.story_scaler;
                    if (!decode.first_pass)
                    {
                        if (option_labels > 0)
                            if (value != 0)
                            {
                                routine = lookup_routine(addr, 0);
                                if (routine != 0)
                                    txio.tx_printf("{0}{1,4:d}", (txio.option_inform) ? 'r' : 'R', routine);
                                else
                                {
                                    Console.Error.Write("\nWarning: found call to nonexistent routine!\n");
                                    txio.tx_printf("{0:X}", addr);
                                }
                            }
                            else
                                print_integer(value, addr_mode == tx_h.BYTE_IMMED);
                        else
                            txio.tx_printf("{0:X}", addr);
                    }
                    else
                    {
                        if (addr < decode.low_address &&
                            addr >= code_base)
                        {
                            vars = txio.read_data_byte(ref addr);
                            if (vars >= 0 && vars <= 15)
                                decode.low_address = addr - 1;
                        }
                        if (addr > decode.high_address &&
                            addr < (ulong)txio.file_size)
                        {
                            vars = txio.read_data_byte(ref addr);
                            if (vars >= 0 && vars <= 15)
                                decode.high_address = addr - 1;
                        }
                    }
                    break;

                case tx_h.OBJECT:
                    if (!decode.first_pass)
                    {
                        // if (value == 0 || showobj.print_object_desc ((int)value) == 0) // TODO I don't like this section
                        if (value == 0)
                        {
                            showobj.print_object_desc((int)value);
                            print_integer(value, addr_mode == tx_h.BYTE_IMMED);
                        }
                    }
                    break;

                case tx_h.STATIC:
                    if (!decode.first_pass)
                    {
                        addr = (ulong)txio.code_scaler * value + (ulong)txio.story_scaler * header.strings_offset;
                        s = lookup_string(addr);
                        if (s != 0)
                            txio.tx_printf("{0}{1,3:d}", (txio.option_inform) ? 's' : 'S', (int)s);
                        else
                        {
#if !TXD_DEBUG
                            Console.Error.WriteLine("\nWarning: printing of nonexistent string\n");
#endif
                            txio.tx_printf("{0:X}", addr);
                        }
                    }
                    break;

                case tx_h.LABEL:
                    addr = decode.pc + (ulong)(short)value - 2; // TODO Check this math somehow
                    if (decode.first_pass && addr < decode.low_address)
                        return (1);
                    if (option_labels > 0)
                    {
                        if (decode.first_pass)
                            add_label(addr);
                        else
                            txio.tx_printf("{0}{1,4:d}", (txio.option_inform) ? 'l' : 'L', lookup_label(addr, 1));
                    }
                    else
                    {
                        if (!decode.first_pass)
                            txio.tx_printf("{0:X}", addr);
                    }
                    if (addr > decode.high_pc)
                        decode.high_pc = addr;
                    break;

                case tx_h.PCHAR:
                    if (!decode.first_pass)
                        if (isprint((char)value))
                            txio.tx_printf("\'{0}\'", (char)value);
                        else
                            print_integer(value, addr_mode == tx_h.BYTE_IMMED);
                    break;

                case tx_h.VATTR:
                    if (!decode.first_pass)
                    {
                        if (value == tx_h.ROMAN)
                            txio.tx_printf("{0}", (txio.option_inform) ? "roman" : "ROMAN");
                        else if (value == tx_h.REVERSE)
                            txio.tx_printf("{0}", (txio.option_inform) ? "reverse" : "REVERSE");
                        else if (value == tx_h.BOLDFACE)
                            txio.tx_printf("{0}", (txio.option_inform) ? "boldface" : "BOLDFACE");
                        else if (value == tx_h.EMPHASIS)
                            txio.tx_printf("{0}", (txio.option_inform) ? "emphasis" : "EMPHASIS");
                        else if (value == tx_h.FIXED_FONT)
                            txio.tx_printf("{0}", (txio.option_inform) ? "fixed_font" : "FIXED_FONT");
                        else
                            print_integer(value, addr_mode == tx_h.BYTE_IMMED);
                    }
                    break;

                case tx_h.PATTR:
                    if (!decode.first_pass)
                    {
                        if ((int)value == 1)
                            txio.tx_printf("{0}", (txio.option_inform) ? "output_enable" : "OUTPUT_ENABLE");
                        else if ((int)value == 2)
                            txio.tx_printf("{0}", (txio.option_inform) ? "scripting_enable" : "SCRIPTING_ENABLE");
                        else if ((int)value == 3)
                            txio.tx_printf("{0}", (txio.option_inform) ? "redirect_enable" : "REDIRECT_ENABLE");
                        else if ((int)value == 4)
                            txio.tx_printf("{0}", (txio.option_inform) ? "record_enable" : "RECORD_ENABLE");
                        else if ((int)value == -1)
                            txio.tx_printf("{0}", (txio.option_inform) ? "output_disable" : "OUTPUT_DISABLE");
                        else if ((int)value == -2)
                            txio.tx_printf("{0}", (txio.option_inform) ? "scripting_disable" : "SCRIPTING_DISABLE");
                        else if ((int)value == -3)
                            txio.tx_printf("{0}", (txio.option_inform) ? "redirect_disable" : "REDIRECT_DISABLE");
                        else if ((int)value == -4)
                            txio.tx_printf("{0}", (txio.option_inform) ? "record_disable" : "RECORD_DISABLE");
                        else
                            print_integer(value, addr_mode == tx_h.BYTE_IMMED);
                    }
                    break;

                case tx_h.INDIRECT:
                    if (!decode.first_pass)
                    {
                        if (value == 0)
                            txio.tx_printf("[{0}]", (txio.option_inform) ? "sp" : "(SP)+");
                        else
                        {
                            txio.tx_printf("[");
                            print_variable(value);
                            txio.tx_printf("]");
                        }
                    }
                    break;

                default:
                    throw new ArgumentException(String.Format("\nFatal: bad operand type ({0})\n", (int)par));
            }

            return (0);
        }/* decode_parameter */

        /* decode_extra - Decode branches, stores and text */

        static int decode_extra()
        {
            ulong addr;
            zbyte_t firstbyte;

            if (opcode.extra == tx_h.STORE || opcode.extra == tx_h.BOTH)
            {
                addr = (zbyte_t)txio.read_data_byte(ref decode.pc);
                if (!decode.first_pass)
                {
                    if (!txio.option_inform) // || (txio.option_inform >= 6))
                        txio.tx_printf("-> ");
                    if (addr == 0)
                        txio.tx_printf("{0}", (txio.option_inform) ? "sp" : "-(SP)");
                    else
                        print_variable((uint)addr);

                    if (opcode.extra == tx_h.BOTH)
                        txio.tx_printf(" ");
                }
                else
                {
                    if (addr > 0 && addr < 16 && addr > (ulong)locals_count)
                        return (tx_h.BAD_OPCODE);
                }
            }

            if (opcode.extra == tx_h.BRANCH || opcode.extra == tx_h.BOTH)
            {
                addr = firstbyte = (zbyte_t)txio.read_data_byte(ref decode.pc);
                addr &= 0x7f;
                if ((addr & 0x40) > 0)
                    addr &= 0x3f;
                else
                {
                    addr = (addr << 8) | (ulong)txio.read_data_byte(ref decode.pc);
                    if ((addr & 0x2000) > 0)
                    {
                        addr &= 0x1fff;
                        unchecked
                        {
                            addr |= (ulong)~0x1fff; // TODO Is there a better way to handle this?
                        }
                    }
                }
                if (!decode.first_pass)
                {
                    if ((addr > 1) && (firstbyte & 0x40) == 0 && (txio.option_inform) && (option_labels > 0)) // TODO Option_inform >= 6
                    {
                        txio.tx_printf("?");	/* Inform 6 long branch */
                    }
                    if ((firstbyte & 0x80) > 0)
                        txio.tx_printf("{0}", (txio.option_inform) ? "" : "[TRUE]");
                    else
                        txio.tx_printf("{0}", (txio.option_inform) ? "~" : "[FALSE]");
                }
                if (addr == 0)
                {
                    if (!decode.first_pass)
                        txio.tx_printf("{0}", (txio.option_inform) ? "rfalse" : " RFALSE");
                }
                else if (addr == 1)
                {
                    if (!decode.first_pass)
                        txio.tx_printf("{0}", (txio.option_inform) ? "rtrue" : " RTRUE");
                }
                else
                {
                    addr = decode.pc + addr - 2;
                    if (decode.first_pass && addr < start_of_routine)
                        return (tx_h.BAD_OPCODE);
                    if (option_labels > 0)
                    {
                        if (decode.first_pass)
                            add_label(addr);
                        else
                            txio.tx_printf("{0}{1:d4}", (txio.option_inform) ? "l" : " L", (int)lookup_label(addr, 1));
                    }
                    else
                    {
                        if (!decode.first_pass)
                            txio.tx_printf("{0}{1:X}", (txio.option_inform) ? "" : " ", (ulong)addr);
                    }
                    if (addr > decode.high_pc)
                        decode.high_pc = addr;
                }
            }

            if (opcode.extra == tx_h.TEXT)
            {
                if (decode.first_pass)
                {
                    while ((short)txio.read_data_word(ref decode.pc) >= 0)
                        ;
                }
                else
                    print_text(ref decode.pc);
            }

            if (opcode.type == tx_h.RETURN)
                if (decode.pc > decode.high_pc)
                    return (tx_h.END_OF_ROUTINE);

            return (tx_h.END_OF_INSTRUCTION);

        }/* decode_outputs */

        ///* decode_strings - Dump text after end of code */

        internal static void decode_strings(ulong pc)
        {
            int count = 1;

            pc = ROUND_DATA(pc);
            txio.tx_printf("\n[Start of text");
            if (option_labels == 0)
                txio.tx_printf(" at {0:X}", (ulong)pc);
            txio.tx_printf("]\n\n");
            while (pc < (ulong)txio.file_size)
            {
                if (option_labels > 0)
                    txio.tx_printf("{0}{1:d3}: ", (txio.option_inform) ? 's' : 'S', (int)count++);
                else
                    txio.tx_printf("{0:X}: S{1:d3} ", (ulong)pc, (int)count++);
                print_text(ref pc);
                txio.tx_printf("\n");
                pc = ROUND_CODE(pc);
            }
            txio.tx_printf("\n[End of text");
            if (option_labels == 0)
                txio.tx_printf(" at %lx", (ulong)pc);
            txio.tx_printf("]\n\n[End of file]\n");

        }/* decode_strings */

        /* scan_strings - build string address table */

        internal static void scan_strings(ulong pc)
        {
            ulong old_pc;
            int count = 1;
            zword_t data;

            strings_base = new System.Collections.Generic.List<tx_h.cref_item_t>();

            pc = ROUND_DATA(pc);
            old_pc = pc;
            while (pc < (ulong)txio.file_size)
            {
                tx_h.cref_item_t cref_item = new tx_h.cref_item_t();
                cref_item.address = pc;
                cref_item.number = count++;
                strings_base.Insert(0, cref_item);
                old_pc = pc;
                do
                    data = (zword_t)txio.read_data_word(ref pc);
                while (pc < (ulong)txio.file_size && ((uint)data & 0x8000) == 0);
                pc = ROUND_CODE(pc);
                if ((uint)(data & 0x8000) > 0)
                    old_pc = pc;
            }
            txio.file_size = (int)old_pc;

        }/* scan_strings */

        /* lookup_string - lookup a string address */

        internal static int lookup_string(ulong addr)
        {

            if (addr <= decode.high_address || addr >= (ulong)txio.file_size)
                return 0;

            for (int i = 0; i < strings_base.Count; i++)
            {
                if (strings_base[i].address == addr) return strings_base[i].number;
            }

            return 0;

        }/* lookup_string */

        static void lookup_verb(ulong addr)
        {
            ulong address, routine;
            uint i;

            bool first = true;

            var header = txio.header;

            address = action_table_base;
            for (i = 0; i < action_count; i++)
            {
                routine = (ulong)txio.read_data_word(ref address) * txio.code_scaler + (ulong)txio.story_scaler * header.routines_offset;
                if (routine == addr)
                {
                    if (first)
                    {
                        txio.tx_printf("    Action routine for:\n");
                        txio.tx_printf("        ");
                        txio.tx_fix_margin(1);
                        first = false;
                    }
                    showverb.show_syntax_of_action(i,
                              verb_table_base,
                              verb_count,
                              parser_type,
                              prep_type,
                              prep_table_base,
                              attr_names_base);
                }
            }
            txio.tx_fix_margin(0);

            first = true;
            address = preact_table_base;
            if (parser_type >= (int)tx_h.parser_types.inform_gv2)
            {
                if (showverb.is_gv2_parsing_routine(addr, verb_table_base,
                               verb_count))
                {
                    txio.tx_printf("    Parsing routine for:\n");
                    txio.tx_printf("        ");
                    txio.tx_fix_margin(1);
                    first = false;
                    showverb.show_syntax_of_parsing_routine(addr,
                                    verb_table_base,
                                    verb_count,
                                    parser_type,
                                    prep_type,
                                    prep_table_base,
                                    attr_names_base);
                }
            }
            else if (parser_type >= (int)tx_h.parser_types.inform5_grammar)
            {
                for (i = 0; i < parse_count; i++)
                {
                    routine = (ulong)txio.read_data_word(ref address) * txio.code_scaler + (ulong)txio.story_scaler * header.routines_offset;
                    if (routine == addr)
                    {
                        if (first)
                        {
                            txio.tx_printf("    Parsing routine for:\n");
                            txio.tx_printf("        ");
                            txio.tx_fix_margin(1);
                            first = false;
                        }
                        showverb.show_syntax_of_parsing_routine(i,
                                  verb_table_base,
                                  verb_count,
                                  parser_type,
                                  prep_type,
                                  prep_table_base,
                                  attr_names_base);
                    }
                }
            }
            else
            {
                for (i = 0; i < action_count; i++)
                {
                    routine = (ulong)txio.read_data_word(ref address) * txio.code_scaler + (ulong)txio.story_scaler * header.routines_offset;
                    if (routine == addr)
                    {
                        if (first)
                        {
                            txio.tx_printf("    Pre-action routine for:\n");
                            txio.tx_printf("        ");
                            txio.tx_fix_margin(1);
                            first = false;
                        }
                        showverb.show_syntax_of_action(i,
                                  verb_table_base,
                                  verb_count,
                                  parser_type,
                                  prep_type,
                                  prep_table_base,
                                  attr_names_base);
                    }
                }
            }
            txio.tx_fix_margin(0);

        }/* lookup_verb */

        internal static void setup_dictionary()
        {

            dict_start = (ulong)txio.header.dictionary;
            ulong temp = (ulong)txio.read_data_byte(ref dict_start);
            dict_start += temp;
            word_size = (ulong)txio.read_data_byte(ref dict_start);
            word_count = (ulong)txio.read_data_word(ref dict_start);
            dict_end = dict_start + (word_count * word_size);

        }/* setup_dictionary */

        internal static int in_dictionary(ulong word_address)
        {

            if (word_address < dict_start || word_address > dict_end)
                return (0);

            if ((word_address - dict_start) % word_size == 0)
                return (1);

            return (0);

        }/* in_dictionary */

        internal static void add_label(ulong addr)
        {
            //    cref_item_t *cref_item, **prev_item, *next_item;

            if (current_routine == null)
                return;

            tx_h.cref_item_t child = new tx_h.cref_item_t();
            child.address = addr;
            child.number = 0;
            current_routine.child.Add(child);

            //    prev_item = &current_routine->child;
            //    next_item = current_routine->child;
            //    while (next_item != NULL && next_item->address < addr) {
            //    prev_item = &(next_item->next);
            //    next_item = next_item->next;
            //    }

            //    if (next_item == NULL || next_item->address != addr) {
            //    cref_item = (cref_item_t *) malloc (sizeof (cref_item_t));
            //    if (cref_item == NULL) {
            //        (void) fprintf (stderr, "\nFatal: insufficient memory\n");
            //        exit (EXIT_FAILURE);
            //    }
            //    cref_item->next = next_item;
            //    *prev_item = cref_item;
            //    cref_item->child = NULL;
            //    cref_item->address = addr;
            //    cref_item->number = 0;
            //    }

            // add_routine(addr); // TODO I'm not sure this is correct

        }/* add_label */

        static void add_routine(ulong addr)
        {
            //    cref_item_t *cref_item, **prev_item, *next_item;

            //    prev_item = &routines_base;
            //    next_item = routines_base;
            //    while (next_item != NULL && next_item->address < addr) {
            //    prev_item = &(next_item->next);
            //    next_item = next_item->next;
            //    }

            //    if (next_item == NULL || next_item->address != addr) {
            //    cref_item = (cref_item_t *) malloc (sizeof (cref_item_t));
            //    if (cref_item == NULL) {
            //        (void) fprintf (stderr, "\nFatal: insufficient memory\n");
            //        exit (EXIT_FAILURE);
            //    }
            //    cref_item->next = next_item;
            //    *prev_item = cref_item;
            //    cref_item->child = NULL;
            //    cref_item->address = addr;
            //    cref_item->number = 0;
            //    } else
            //    cref_item = next_item;

            //    current_routine = cref_item;

            if (routines_base == null)
            {
                routines_base = new System.Collections.Generic.List<tx_h.cref_item_t>();
            }

            tx_h.cref_item_t cref_item = new tx_h.cref_item_t();

            cref_item.address = addr;
            cref_item.number = 0;

            routines_base.Insert(0, cref_item);
            current_routine = cref_item;
        }/* add_routine */

        static int lookup_label(ulong addr, int flag)
        {
            //cref_item_t *cref_item = current_routine->child;
            //int label;

            //while (cref_item != NULL && cref_item->address != addr)
            //cref_item = cref_item->next;

            //if (cref_item == NULL) {
            //label = 0;
            //if (flag) {
            //    (void) fprintf (stderr, "\nFatal: cannot find label!\n");
            //    exit (EXIT_FAILURE);
            //}
            //} else
            //label = cref_item->number;

            // return (label);

            for (int i = 0; i < current_routine.child.Count; i++)
            {
                if (current_routine.child[i].address == addr)
                    return routines_base[i].number;
            }

            if (flag == 1)
            {
                Console.Error.Write("\nFatal: cannot find label!\n");
                throw new ArgumentException(String.Format("Can't find label for addr: {0}", +addr));
            }
            else
            {
                return 0;
            }
            throw new NotImplementedException();

        }/* lookup_label */

        static int lookup_routine(ulong addr, int flag)
        {
            //    cref_item_t *cref_item = routines_base;

            //    while (cref_item != NULL && cref_item->address != addr)
            //    cref_item = cref_item->next;

            //    if (cref_item == NULL) {
            //    if (flag) {
            //        (void) fprintf (stderr, "\nFatal: cannot find routine!\n");
            //        exit (EXIT_FAILURE);
            //    } else
            //        return (0);
            //    }

            //    if (flag)
            //    current_routine = cref_item;

            //    return (cref_item->number);

            for (int i = 0; i < routines_base.Count; i++)
            {
                if (routines_base[i].address == addr)
                {
                    if (flag == 1)
                    {
                        current_routine = routines_base[i];
                    }
                    return routines_base[i].number;
                }
            }
            if (flag == 1)
            {
                throw new ArgumentException("\nFatal: Cannot find routine!\n");
            }
            else
            {
                return 0;
            }
        }/* lookup_routine */

        internal static void renumber_cref(System.Collections.Generic.List<tx_h.cref_item_t> items)
        {
            int number = 1;

            for (int i = 0; i < items.Count; i++)
            {
                var cref_item = items[i];
                if (start_data_pc == 0 ||
                   cref_item.address < start_data_pc ||
                   cref_item.address >= end_data_pc)
                    cref_item.number = number++;

                renumber_cref(cref_item.child);
            }

        }/* renumber_cref */



        //#ifdef __STDC__
        //static int print_object_desc (uint obj)
        //#else
        //static int print_object_desc (obj)
        //uint obj;
        //#endif
        //{
        //    ulong address;

        //    address = (ulong) header.objects;
        //    if ((uint) header.version < V4)
        //    address += ((P3_MAX_PROPERTIES - 1) * 2) + ((obj - 1) * O3_SIZE) + O3_PROPERTY_OFFSET;
        //    else
        //    address += ((P4_MAX_PROPERTIES - 1) * 2) + ((obj - 1) * O4_SIZE) + O4_PROPERTY_OFFSET;

        //    address = (ulong) txio.read_data_word (&address);
        //    if ((uint) txio.read_data_byte (&address)) {
        //    txio.tx_printf ("\"");
        //    (void) txio.decode_text (&address);
        //    txio.tx_printf ("\"");
        //    } else
        //    obj = 0;

        //    return (obj);

        //}/* print_object_desc */

        internal static void print_text(ref ulong addr)
        {
            txio.tx_printf("\"");
            txio.decode_text(ref addr);
            txio.tx_printf("\"");
        }/* print_text */

        static void print_integer(uint value, bool flag)
        {

            if (flag)
                txio.tx_printf("#{0:X2}", value);
            else
                txio.tx_printf("#{0:X4}", value);

        }

        internal static void dump_data(ulong start_addr, ulong end_addr)
        {

            //    int i, c;
            //    ulong addr, save_addr, tx_h.LOW_ADDR, high_addr;

            //    tx_h.LOW_ADDR = start_addr & ~15;
            //    high_addr = (end_addr + 15) & ~15;

            //    for (addr = tx_h.LOW_ADDR; addr < high_addr; ) {
            //    txio.tx_printf ("%5lx: ", (ulong) addr);
            //    save_addr = addr;
            //    for (i = 0; i < 16; i++) {
            //        if (addr < start_addr || addr > end_addr) {
            //        txio.tx_printf ("   ");
            //        addr++;
            //        } else
            //        txio.tx_printf ("%02x ", (uint) txio.read_data_byte (&addr));
            //    }
            //    addr = save_addr;
            //    for (i = 0; i < 16; i++) {
            //        if (addr < start_addr || addr > end_addr) {
            //        txio.tx_printf (" ");
            //        addr++;
            //        } else {
            //        c = txio.read_data_byte (&addr);
            //        txio.tx_printf ("%c", (char) ((isprint (c)) ? c : '.'));
            //        }
            //    }
            //    txio.tx_printf ("\n");
            //    }
            throw new NotImplementedException();
        }/* dump_data */

        static void dump_opcode(ulong addr, int op, int opclass, int[] par, int extra)
        {
            //    int opers, modes, addr_mode, maxopers, count;
            //    unsigned char t;
            //    ulong save_addr;

            //    count = 0;

            //    addr--;
            //    if (class == EXTENDED_OPERAND) {
            //    addr--;
            //    txio.tx_printf ("%02x ", (uint) txio.read_data_byte (&addr));
            //    count++;
            //    }
            //    txio.tx_printf ("%02x ", (uint) txio.read_data_byte (&addr));
            //    count++;

            //    if (class == ONE_OPERAND)
            //    dump_operand (&addr, (op >> 4) & 0x03, 0, par, &count);

            //    if (class == TWO_OPERAND) {
            //    dump_operand (&addr, (op & 0x40) ? VARIABLE : BYTE_IMMED, 0, par, &count);
            //    dump_operand (&addr, (op & 0x20) ? VARIABLE : BYTE_IMMED, 1, par, &count);
            //    }

            //    if (class == VARIABLE_OPERAND || class == EXTENDED_OPERAND) {
            //    if ((op & 0x3f) == 0x2c || (op & 0x3f) == 0x3a) {
            //        save_addr = addr;
            //        modes = txio.read_data_word (&addr);
            //        addr = save_addr;
            //        txio.tx_printf ("%02x ", (uint) txio.read_data_byte (&addr));
            //        txio.tx_printf ("%02x ", (uint) txio.read_data_byte (&addr));
            //        count += 2;
            //        maxopers = 8;
            //    } else {
            //        save_addr = addr;
            //        modes = txio.read_data_byte (&addr);
            //        addr = save_addr;
            //        txio.tx_printf ("%02x ", (uint) txio.read_data_byte (&addr));
            //        count++;
            //        maxopers = 4;
            //    }
            //    for (addr_mode = 0, opers = 0; (addr_mode != NO_OPERAND) && maxopers; maxopers--) {
            //        addr_mode = (modes >> ((maxopers - 1) * 2)) & 0x03;
            //        if (addr_mode != NO_OPERAND) {
            //        dump_operand (&addr, addr_mode, opers, par, &count);
            //        opers++;
            //        }
            //    }
            //    }

            //    if (extra == TEXT) {
            //    txio.tx_printf ("...");
            //    count++;
            //    }

            //    if (extra == STORE || extra == BOTH) {
            //    txio.tx_printf ("%02x ", (uint) txio.read_data_byte (&addr));
            //    count++;
            //    }

            //    if (extra == BRANCH || extra == BOTH) {
            //    t = (unsigned char) txio.read_data_byte (&addr);
            //    txio.tx_printf ("%02x ", (uint) t);
            //    count++;
            //    if (((uint) t & 0x40) == 0) {
            //        txio.tx_printf ("%02x ", (uint) txio.read_data_byte (&addr));
            //        count++;
            //    }
            //    }

            //    if (count > 8)
            //    txio.tx_printf ("\n                               ");
            //    else
            //    for (; count < 8; count++)
            //        txio.tx_printf ("   ");

            throw new NotImplementedException();
        }/* dump_opcode */

        static void dump_operand(ulong addr, int addr_mode, int opers, int[] par, out int count)
        {
            //    if (opers < 4 && par[opers] == VAR)
            //    addr_mode = VARIABLE;

            //    if (addr_mode == WORD_IMMED) {
            //    txio.tx_printf ("%02x ", (uint) txio.read_data_byte (addr));
            //    txio.tx_printf ("%02x ", (uint) txio.read_data_byte (addr));
            //    *count += 2;
            //    }

            //    if (addr_mode == BYTE_IMMED || addr_mode == VARIABLE) {
            //    txio.tx_printf ("%02x ", (uint) txio.read_data_byte (addr));
            //    (*count)++;
            //    }

            throw new NotImplementedException();
        }/* dump_operand */

        static void print_variable(uint varnum)
        {
            if (varnum < 16)
            {
                if (option_symbols > 0 && symbols.print_local_name(start_of_routine, (int)(varnum - 1)) > 0) /* null */
                { }
                else if (txio.option_inform)
                    txio.tx_printf("local{0}", varnum - 1);
                else
                    txio.tx_printf("L{0:X2}", varnum - 1);
            }
            else
                if (option_symbols > 0 && symbols.print_global_name(start_of_routine, (int)(varnum - 16)) > 0) /* null */{ }
                else txio.tx_printf("{0}{1:X2}", (txio.option_inform) ? 'g' : 'G', varnum - 16);
        }

        private static bool isprint(char c)
        {
            return true;
        }
    }
}