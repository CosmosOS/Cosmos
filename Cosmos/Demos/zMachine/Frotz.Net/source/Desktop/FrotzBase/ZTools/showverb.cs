/*
 * showverb - part of infodump
 *
 * Verb and grammar display routines.
 */

using zword_t = System.UInt16;
using zbyte_t = System.Byte;

using System;

enum gv2_tokentype { TT_ILLEGAL, TT_ELEMENTARY, TT_PREPOSITION, TT_NOUNR, TT_ATTRIBUTE, TT_SCOPER, TT_ROUTINE };

namespace ZTools
{
    internal static class showverb
    {
        static int[] verb_sizes;

        static showverb()
        {
            verb_sizes = new int[] { 2, 4, 7, 0 };
        }

        /*
         * configure_parse_tables
         *
         * Determine the start of the parse table, the start of the parse data, the
         * start of the action routine table, the start of the pre-action routine
         * table, the start of the preposition list and other sundry things.
         *
         * Format of the verb parse tables:
         *
         * base:
         *	  Table of pointers (2 bytes) to each verb entry. Each verb in the
         *	  dictionary has an index (1 byte) into this table. The index in the
         *	  dictionary is inverted so an index of 255 = table entry 0, etc.
         *	  In GV2 the index is 2 bytes and it is not inverted -- except that
         *    Inform 6.10 uses the old method.  Thus type GV2A is used internally
         *    to indicate the 2-byte form which will presumably be used in
         *    later Inform versions.
         *
         *	  Next comes the parse data for each verb pointed to by the table of
         *	  pointers. The format of the parse data varies between games. Basically,
         *	  each entry has a count (1 byte) of parse structures corresponding to
         *	  different sentence structures. For example, the verb 'take' in the
         *	  dictionary may have two forms; 'take object' or 'take object with object'.
         *	  Each form has an index (1 byte) into the pre-action and action routine
         *	  tables.
         *
         *	  Next come the action routine tables. There is an entry (2 bytes) for
         *	  each verb form index. The entries are packed addresses of the Z-code
         *	  routines that perform the main verb processing.
         *
         *	  Next come the pre-action routine tables. There is an entry (2 bytes) for
         *	  each verb form index. The entries are packed addresses of the Z-code
         *	  routines that are called before the main verb action routine.
         *
         *	  Finally, there is a list of prepositions that can be used by any verb
         *	  in the verb parse table. This list has a count (2 bytes) followed by
         *	  the address of the preposition in the dictionary and an index.
         *
         * Verb parse entry format:
         *
         *	  The format of the data varies between games. The information in the
         *	  entry is the same though:
         *
         *	  An object count (0, 1 or 2)
         *	  A preposition index for the verb
         *	  Data for object(s) 1
         *	  A preposition index for the objects
         *	  Data for object(s) 2
         *	  A pre-action/action routine table index
         *
         *	  This means a sentence can have the following form:
         *
         *	  verb [+ prep] 							 'look [at]'
         *	  verb [+ prep] + object					 'look [at] book'
         *	  verb [+ prep] + object [+ prep] + object	 'look [at] book [with] glasses'
         *
         *	  The verb and prepositions can only be a single word each. The object
         *	  could be a multiple words 'green book' or a list depending on the object
         *	  data.
         *
         * Notes:
         *
         * Story files produced by Graham Nelson's Inform compiler do not have a
         * concept of pre-actions. Instead, the pre-actions table is filled with
         * pointers to general parsing routines which are related to certain verb
         * parse entries. The format of the parse entries has changed, too. Parse
         * entries have now become a string of tokens. Objects have token values
         * between 0x00 and 0xaf, prepositions correspond to token values from 0xb0
         * to 0xff. Therefore more complicated verb structures with more than two
         * prepositions are possible. See the "Inform Technical Manual" for further
         * information.
         *
         * Inform 6 reduces the size of the pre-action table (which no longer holds
         * any pre-action routines, see above) to the number of parsing routines.
         *
         * Inform 6.10 adds a new grammar form, called GV2, which does away with
         * the pre-action table and preposition table entirely, and completely
         * changes the parse entry format.	Again, see the "Inform Technical Manual"
         * for further information.  Also note that Inform (in both GV1 and GV2)
         * uses the flags bytes in the dictionary entry in a slightly different
         * manner than Infocom games.
         *
         * Graphic (V6) Infocom games use a different grammar format.  The basic  
         * elements are mostly still there, but in a different set of tables.  
         * The table of pointers to the verb entries is gone.  Instead, the
         * first word of 'extra' dictionary information contains a pointer to the
         * verb entry. Pointers to the action and preaction tables are in
         * the next-to-last and last global variables respectively, but in practice 
         * the tables are in their usual location right after the verb grammar table,
         * which is in its usual location at the base of dynamic memory.  The 
         * verb grammar table has the following format
         *
         * Bytes 0-1: Action/pre-action index of the 0-object entry.  That is, action
         *            if this verb is used alone.  $FFFF if this verb cannot be used
         *            as a sentence in itself
         *
         * Bytes 2-3: Doesn't seem to be used.  Might be intended for actions with
         *            no objects but with a preposition.
         *
         * Bytes 4-5: Pointer to grammar entries for 1-object entries -- i.e. those
         *            of the form "verb [+ prep] + object"
         *
         * Bytes 6-7: Pointer to grammar entries for 2-object entries -- i.e. those
         *            of the form "verb [+ prep] + object [+ prep] + object"
         *
         * The grammar entries area is new.  Each item contains data of the following
         * format:
         *
         * Bytes 0-1: Number of entries in this item.  Entries immediately follow
         * For each entry in the item:
         * Bytes 0-1: Action/pre-action index for this item.
         * Bytes 2-3: Byte address of the dictionary word for the preposition
         *            $0000 if no preposition.
         *
         * Byte    4: Attribute # associated with this entry.  This does not appear
         *            to be used by the parser itself, but instead by the helper
         *            which suggests possible commands, in order to suggest
         *            sensible ones.  Actually verifying that the object has the
         *            attribute seems to be left to the action routine.
         *
         * Byte    5: I'm not sure about this one.  $80 appears to mean anything can
         *            be used, particularly including quoted strings.  $0F seems to
         *            mean an object in scope.  $14 may mean an object being held. I
         *            suspect it's a flags byte.
         *
         * Bytes 6-7: (Two object entries only) Same as bytes 2-3, for second preposition
         * Bytes 8-9: (Two object entries only) Same as bytes 4-5, for second object
         *
         *
         * Note also that the dictionary flags have moved from the first to the last byte
         * of each dictionary entry, and that while Zork Zero has only three bytes of
         * extra dictionary data (as with V1-5), Shogun and Arthur have four.
         * Also, I believe there is more grammar data than I've listed here, though how 
         * much is for the parser proper and how much for the helper I don't know -- MTR
         *
         * Journey has no grammar.
         *
         */

        internal static void configure_parse_tables(out uint verb_count,
                                     out uint action_count,
                                     out uint parse_count,
                                     out uint parser_type,
                                     out uint prep_type,
                                     out ulong verb_table_base,
                                     out ulong verb_data_base,
                                     out ulong action_table_base,
                                     out ulong preact_table_base,
                                     out ulong prep_table_base,
                                     out ulong prep_table_end)
        {
            ulong address, first_entry, second_entry, verb_entry;
            uint entry_count, object_count, prep_count, action_index;
            ulong parse_index;
            uint val;
            int i, j;

            verb_table_base = 0;
            verb_data_base = 0;
            action_table_base = 0;
            preact_table_base = 0;
            prep_table_base = 0;
            prep_table_end = 0;
            verb_count = 0;
            action_count = 0;
            parser_type = 0;
            prep_type = 0;
            parse_count = 0;

            var header = txio.header;

            if (header.serial[0] >= '0' && header.serial[0] <= '9' &&
                header.serial[1] >= '0' && header.serial[1] <= '9' &&
                header.serial[2] >= '0' && header.serial[2] <= '1' &&
                header.serial[3] >= '0' && header.serial[3] <= '9' &&
                header.serial[4] >= '0' && header.serial[4] <= '3' &&
                header.serial[5] >= '0' && header.serial[5] <= '9' &&
                header.serial[0] != '8')
            {
                parser_type = (uint)tx_h.parser_types.inform5_grammar;

                if (header.name[4] >= '6')
                    parser_type = (uint)tx_h.parser_types.inform_gv1;
            }

            if ((parser_type < (uint)tx_h.parser_types.inform5_grammar) && (uint)header.version == tx_h.V6)
            {
                // TODO This code does not work!
                ulong word_address, first_word, last_word;
                ushort word_size, word_count;
                ulong vbase, vend;
                ulong area2base, area2end;
                ulong parse_address;

                parser_type = (uint)tx_h.parser_types.infocom6_grammar;
                address = (ulong)(header.objects - 4);
                action_table_base = (ushort)txio.read_data_word(ref address);
                preact_table_base = (ushort)txio.read_data_word(ref address);

                /* Calculate dictionary bounds and entry size */

                address = (ulong)header.dictionary;
                ulong temp = (ulong)txio.read_data_byte(ref address);
                address += temp;
                word_size = txio.read_data_byte(ref address);
                word_count = txio.read_data_word(ref address);
                first_word = address;
                last_word = (address + ((ulong)(word_count - 1) * word_size));

                vbase = area2base = 0xFFFF;
                vend = area2end = 0;

                for (word_address = first_word; word_address <= last_word; word_address += word_size)
                {
                    address = word_address + 6;
                    parse_index = (ushort)txio.read_data_word(ref address);
                    address = word_address + word_size - 1;
                    val = txio.read_data_byte(ref address); /* flags */
                    if ((val & 1) != 0 && (val & 0x80) == 0 && (parse_index != 0) && (parse_index < action_table_base))
                    { /* dictionary verb */
                        if (vbase > parse_index)
                            vbase = parse_index;
                        if (vend <= parse_index)
                            vend = parse_index + 8;
                        address = parse_index + 4;

                        /* retrieve direct-object only parse entries */
                        parse_address = (ushort)txio.read_data_word(ref address);
                        if (parse_address > 0 && (area2base > parse_address))
                            area2base = parse_address;

                        if (parse_address > 0 && (area2end <= parse_address))
                        {
                            val = (ushort)txio.read_data_word(ref parse_address);
                            area2end = (parse_address + 6 * val);
                        }

                        /* retrieve indrect-object parse entries */
                        parse_address = (ushort)txio.read_data_word(ref address);
                        if (parse_address > 0 && (area2base > parse_address))
                            area2base = parse_address;

                        if (parse_address > 0 && (area2end <= parse_address))
                        {
                            val = (ushort)txio.read_data_word(ref parse_address);
                            area2end = (parse_address + 10 * val);
                        }
                    }
                }
                if (vend == 0) /* no verbs */
                    return;
                verb_count = (uint)((vend - vbase) / 8);
                verb_table_base = vbase;
                verb_data_base = area2base;
                /* there is no preposition table, but *prep_table_base bounds the verb data area */
                prep_table_base = area2end;
                prep_table_end = area2end;
                action_count = (uint)(preact_table_base - action_table_base) / 2;
                return;
            }

            /* Start of table comes from the header */

            verb_table_base = (ulong)header.dynamic_size;

            /*
             * Calculate the number of verb entries in the table. This can be done
             * because the verb entries immediately follow the verb table.
             */

            address = verb_table_base;
            first_entry = txio.read_data_word(ref address);
            if (first_entry == 0) /* No verb entries at all */
                return;
            verb_count = (uint)((first_entry - verb_table_base) / sizeof(zword_t));

            /*
             * Calculate the form of the verb parse table entries. Basically,
             * Infocom used two types of table. The first types have 8 bytes per
             * entry, and the second type has a variable sized amount of data per
             * entry. In addition, Inform uses two new types of table. We look at
             * the serial number to distinguish Inform story files from Infocom
             * games, and we look at the last header entry to identify Inform 6
             * story files because Inform 6 writes its version number into the
             * last four bytes of this entry.
             */

            /*
             * Inform 6.10 addes an additional table format, called GV2.  GV1 is the
             * Inform 6.0-6.05 format, and is essentially similar to the Inform 5
             * format except that the parsing routine table is not padded out
             * to the length of the action table.
             * Infocom: parser_type = 0,1
             * Inform 1?-5: parser_type = 2
             * Inform 6 GV1: parser_type = 3
             * Inform 6 GV2: parser_type = 4
             */

            address = verb_table_base;
            first_entry = txio.read_data_word(ref address);
            second_entry = txio.read_data_word(ref address);
            verb_data_base = first_entry;
            entry_count = (uint)txio.read_data_byte(ref first_entry);

            if (parser_type < (uint)tx_h.parser_types.inform5_grammar)
            {
                parser_type = (uint)tx_h.parser_types.infocom_fixed;

                if (((second_entry - first_entry) / entry_count) <= 7)
                    parser_type = (uint)tx_h.parser_types.infocom_variable;
            }

            /* Distinguishing between GV1 and GV2 isn't trivial.
               Here I check the length of the first entry.	It will be 1 mod 3
               for GV2 and 1 mod 8 for GV1. If it's 1 mod 24, first I check to see if
               its length matches the GV1 length.  Then I check for illegal GV1 values. 
               If they aren't found, I assume GV1.  I believe it is actually possible for
               a legal (if somewhat nonsensical) GV1 table to be the same as a legal GV2
               table, but I haven't actually constructed such a weird table.  In practice,
               the ENDIT (15) byte of the GV2 table will probably cause an illegal token
               if the table is interpreted as GV1 -- MTR.
            */

            if (parser_type == (int)tx_h.parser_types.inform_gv1)
            {
                first_entry = verb_data_base;
                if (((second_entry - first_entry) % 3) == 1)
                {
                    entry_count = txio.read_data_byte(ref first_entry);
                    if ((entry_count * 8 + 1) == (second_entry - first_entry))
                    {
                        /* this is the most ambiguous case */
                        for (i = 0; i < entry_count && (parser_type == (int)tx_h.parser_types.inform_gv1); i++)
                        {
                            if (txio.read_data_byte(ref first_entry) > 6)
                            {
                                parser_type = (int)tx_h.parser_types.inform_gv2;
                                break;
                            }
                            for (j = 1; j < 7; j++)
                            {
                                val = txio.read_data_byte(ref first_entry);
                                if ((val >= 9) || (val <= 15) || (val >= 112) || (val <= 127))
                                {
                                    parser_type = (int)tx_h.parser_types.inform_gv2;
                                    break;
                                }
                            }
                            txio.read_data_byte(ref first_entry); /* action number.  This can be anything */
                        }
                    }
                    else
                    {
                        parser_type = (int)tx_h.parser_types.inform_gv2;
                    }
                }
                else if (((second_entry - first_entry) % 8) != 1)
                {
                    Console.Error.WriteLine("Grammar table illegal size!");
                }
            }

            /*
             * Make a pass through the verb parse table looking at the pre-action and
             * action routine indices. We need to know what the highest index is to
             * find the size of the pre-action and action tables. Before Inform 6
             * both tables had the same size. For Inform 6 story files we also need
             * to know the number of parsing routines that occupy the pre-action
             * table (instead of pre-actions).
             */

            verb_entry = 0;
            action_count = 0;
            parse_count = 0;
            address = verb_table_base;
            for (i = 0; (uint)i < verb_count; i++)
            {
                verb_entry = (ulong)txio.read_data_word(ref address);
                entry_count = (uint)txio.read_data_byte(ref verb_entry);
                while (entry_count-- > 0)
                {
                    if (parser_type == (int)tx_h.parser_types.infocom_fixed)
                    {
                        verb_entry += 7;
                        action_index = (uint)txio.read_data_byte(ref verb_entry);
                    }
                    else if (parser_type == (int)tx_h.parser_types.infocom_variable)
                    {
                        object_count = (uint)txio.read_data_byte(ref verb_entry);
                        action_index = (uint)txio.read_data_byte(ref verb_entry);
                        verb_entry += (ulong)(verb_sizes[(object_count >> 6) & 0x03] - 2);
                    }
                    else if ((parser_type == (int)tx_h.parser_types.inform_gv1) || (parser_type == (int)tx_h.parser_types.inform5_grammar))
                    {
                        /* GV1 */
                        object_count = (uint)txio.read_data_byte(ref verb_entry);
                        for (j = 0; j < 6; j++)
                        {
                            val = txio.read_data_byte(ref verb_entry);
                            if (val < 16 || val >= 112)
                                continue;
                            parse_index = (val - 16) % 32;
                            if (parse_index > parse_count)
                                parse_count = (uint)parse_index;
                        }
                        action_index = (uint)txio.read_data_byte(ref verb_entry);
                    }
                    else
                    {
                        /* GV2 */
                        action_index = (uint)(txio.read_data_word(ref verb_entry) & 0x3FF);
                        val = txio.read_data_byte(ref verb_entry);
                        while (val != 15)
                        {
                            txio.read_data_byte(ref verb_entry);
                            txio.read_data_byte(ref verb_entry);
                            val = txio.read_data_byte(ref verb_entry);
                        }
                    }
                    if (action_index > action_count)
                        action_count = action_index;
                }
            }
            (action_count)++;
            if ((parser_type == (int)tx_h.parser_types.inform_gv1) || (parser_type == (int)tx_h.parser_types.inform5_grammar))
                (parse_count)++;

            while ((uint)txio.read_data_byte(ref verb_entry) == 0) /* Skip padding, if any */
                ;

            /*
             * Set the start addresses of the pre-action and action routines tables
             * and the preposition table.
             */

            action_table_base = verb_entry - 1;
            preact_table_base = action_table_base + (action_count * sizeof(zword_t));

            if (parser_type >= (int)tx_h.parser_types.inform_gv2)
            {
                /* GV2 has neither preaction/parse table nor preposition table */
                prep_table_base = preact_table_base;
                prep_table_end = preact_table_base;
            }
            else
            {
                if (parser_type < (int)tx_h.parser_types.inform_gv1)
                    prep_table_base = preact_table_base + (action_count * sizeof(zword_t));
                else
                    prep_table_base = preact_table_base + (parse_count * sizeof(zword_t));

                /*
                 * Set the preposition table type by looking to see if the byte index
                 * is stored in a word (an hence the upper 8 bits are zero).
                 */

                address = prep_table_base;
                prep_count = (uint)txio.read_data_word(ref address);
                address += 2; /* Skip first address */
                if ((uint)txio.read_data_byte(ref address) == 0)
                {
                    prep_type = 0;
                    prep_table_end = prep_table_base + 2 + (4 * prep_count) - 1;
                }
                else
                {
                    prep_type = 1;
                    prep_table_end = prep_table_base + 2 + (3 * prep_count) - 1;
                }
            }

        }/* configure_parse_tables */

        /*
         * show_verbs
         *
         * Display the verb parse tables, sentence structure, action routines and
         * prepositions.
         */

        internal static void show_verbs(int symbolic)
        {
            ulong verb_table_base, verb_data_base;
            ulong action_table_base, preact_table_base;
            ulong prep_table_base, prep_table_end;
            uint verb_count, action_count, parse_count, parser_type, prep_type;

            int obj_count;
            ulong obj_table_base, obj_table_end, obj_data_base, obj_data_end;
            ushort inform_version;
            ulong class_numbers_base, class_numbers_end;
            ulong property_names_base, property_names_end;
            ulong attr_names_base, attr_names_end;
            ulong action_names_base;

            attr_names_end = 0;

            /* Get parse table configuration */

            configure_parse_tables(out verb_count, out action_count, out parse_count, out parser_type, out prep_type,
                                    out verb_table_base, out verb_data_base,
                                    out action_table_base, out preact_table_base,
                                    out prep_table_base, out prep_table_end);

            /* I wonder weather you can guess which author required the following test? */

            if (verb_count == 0)
            {
                txio.tx_printf("\n    **** There are no parse tables ****\n\n");
                txio.tx_printf("  Verb entries = 0\n\n");

                return;
            }

            if (symbolic > 0)
            {
                showobj.configure_object_tables(out obj_count, out obj_table_base, out obj_table_end,
                                  out obj_data_base, out obj_data_end);
                infinfo.configure_inform_tables(obj_data_end, out inform_version, out class_numbers_base, out class_numbers_end,
                                    out property_names_base, out property_names_end, out attr_names_base, out attr_names_end);
            }
            else
            {
                attr_names_base = property_names_base = class_numbers_base = 0;
            }

            action_names_base = attr_names_base > 0 ? attr_names_end + 1 : 0;

            /* Display parse data */

            show_verb_parse_table(verb_table_base, verb_count, parser_type,
                                   prep_type, prep_table_base, attr_names_base);

            /* Display action routines */

            show_action_tables(verb_table_base,
                                verb_count, action_count, parse_count, parser_type, prep_type,
                                action_table_base, preact_table_base,
                                prep_table_base, attr_names_base, action_names_base);

            /* Display prepositions */
            if ((parser_type <= (uint)tx_h.parser_types.inform_gv2) && (parser_type != (int)tx_h.parser_types.infocom6_grammar)) /* no preposition table in GV2 */
                show_preposition_table(prep_type, prep_table_base, parser_type);

        }/* show_verbs */

        /*
         * show_verb_parse_table
         *
         * Display the parse information associated with each verb. The entry into the
         * table is found from the dictionary. Each verb has a parse table entry index.
         * These indices range from 255 to 0*. Each parse table entry can have one or
         * more sentence formats associated with the verb. Once the verb and sentence
         * structure match, an index taken from the parse data is used to index into the
         * pre-action and action routine tables. This format allows multiple similar
         * verb and sentence structures to parse to the same action routine.
         * * 0 to 65535 in GV2
         *
         * Synonyms for each verb are also show. The first verb in the dictionary is
         * used in the sentence structure text. This can lead to bizarre looking
         * sentences, but they all work!
         *
         * The index used to find the action routine is the same number printed when
         * debugging is turned on in games that support this. The number is printed as
         * performing: nn
         */

        private static void show_verb_parse_table(ulong verb_table_base,
                                           uint verb_count,
                                           uint parser_type,
                                           uint prep_type,
                                           ulong prep_table_base,
                                           ulong attr_names_base)
        {
            ulong address;
            uint entry_count, object_count;
            uint parse_data = 0;
            int i, j, verb_size;

            ulong parse_entry = 0;
            ulong verb_entry = 0;

            txio.tx_printf("\n    **** Parse tables ****\n\n");
            txio.tx_printf("  Verb entries = {0}\n", (int)verb_count);

            /* Go through each verb and print its parse information and grammar */

            address = verb_table_base;
            for (i = 0; (uint)i < verb_count; i++)
            {

                /* Get start of verb entry and number of entries */

                if (parser_type == (uint)tx_h.parser_types.infocom6_grammar)
                {
                    ulong do_address, doio_address;
                    uint verb_address;

                    verb_address = (uint)address; /* cast is guaranteed to work provided uint >= 16 bits */
                    txio.tx_printf("\n{0,3:d}. @ ${1:X4}, verb = ", i, address);
                    show_words((uint)address, 0L, tx_h.VERB_V6, parser_type);
                    txio.tx_printf("\n    Main data");
                    txio.tx_printf("\n    [");
                    parse_data = (uint)txio.read_data_word(ref address);
                    txio.tx_printf("{0:X4} ", (uint)parse_data);
                    txio.read_data_word(ref address);	/* I don't know what this word does */
                    txio.tx_printf("{0:X4} ", (uint)parse_data);
                    do_address = (uint)txio.read_data_word(ref address);
                    txio.tx_printf("{0:X4} ", (uint)do_address);
                    doio_address = (uint)txio.read_data_word(ref address);
                    txio.tx_printf("{0:X4}", (uint)doio_address);
                    txio.tx_printf("] ");
                    if (verb_entry != 0xFFFF)
                        show_verb_grammar(parse_entry, verb_address, (int)parser_type, 0, 0, 0L, 0L);

                    if (do_address > 0)
                    {
                        txio.tx_printf("\n    One object entries:\n");
                        verb_entry = do_address;
                        entry_count = (uint)txio.read_data_word(ref verb_entry);
                        verb_size = 3; /* words */
                        while (entry_count-- > 0)
                        {
                            parse_entry = verb_entry;
                            txio.tx_printf("    [");
                            for (j = 0; j < verb_size; j++)
                            {
                                parse_data = (uint)txio.read_data_word(ref verb_entry);
                                txio.tx_printf("{0:X4}", (uint)parse_data);
                                if (j < (verb_size - 1))
                                    txio.tx_printf(" ");
                            }
                            txio.tx_printf("] ");
                            show_verb_grammar(parse_entry, verb_address, (int)parser_type, 1,
                                       0, 0L, 0L);
                            txio.tx_printf("\n");
                        }
                    }
                    if (doio_address > 0)
                    {
                        txio.tx_printf("\n    Two object entries:\n");
                        verb_entry = doio_address;
                        entry_count = (uint)txio.read_data_word(ref verb_entry);
                        verb_size = 5; /* words */
                        while (entry_count-- > 0)
                        {
                            parse_entry = verb_entry;
                            txio.tx_printf("    [");
                            for (j = 0; j < verb_size; j++)
                            {
                                parse_data = (uint)txio.read_data_word(ref verb_entry);
                                txio.tx_printf("{0:X4}", (uint)parse_data);
                                if (j < (verb_size - 1))
                                    txio.tx_printf(" ");
                            }
                            txio.tx_printf("] ");
                            show_verb_grammar(parse_entry, verb_address, (int)parser_type, 2,
                                       0, 0L, 0L);
                            txio.tx_printf("\n");
                        }
                    }
                }
                else
                { /* everything but Zork Zero, Shogun, and Arthur */
                    verb_entry = (ulong)txio.read_data_word(ref address);
                    entry_count = (uint)txio.read_data_byte(ref verb_entry);

                    /* Show the verb index, entry count, verb and synonyms */

                    txio.tx_printf("\n{0:d3}. {1} entr{2}, verb = ", (int)tx_h.VERB_NUM(i, parser_type),
                               (int)entry_count, (entry_count == 1) ? "y" : "ies");
                    show_words(tx_h.VERB_NUM(i, parser_type), 0L, tx_h.VERB, parser_type);
                    txio.tx_printf("\n");

                    /* Show parse data and grammar for each verb entry */

                    while (entry_count-- > 0)
                    {
                        parse_entry = verb_entry;

                        /* Calculate the amount of verb data */

                        if (parser_type != (int)tx_h.parser_types.infocom_variable)
                        {
                            verb_size = 8;
                        }
                        else
                        {
                            object_count = (uint)txio.read_data_byte(ref parse_entry);
                            verb_size = verb_sizes[(object_count >> 6) & 0x03];
                            parse_entry = verb_entry;
                        }

                        /* Show parse data for each verb */

                        txio.tx_printf("    [");

                        if (parser_type < (int)tx_h.parser_types.inform_gv2)
                        {
                            for (j = 0; j < verb_size; j++)
                            {
                                parse_data = (uint)txio.read_data_byte(ref verb_entry);
                                txio.tx_printf("{0:X2}", (uint)parse_data);
                                if (j < (verb_size - 1))
                                    txio.tx_printf(" ");
                            }
                        }
                        else
                        {
                            /* GV2 variable entry format
                               <flags and action high> <action low> n*(<token type> <token data 1> <token data 2>) <ENDIT>*/
                            for (j = 0; (j == 0) || (j % 3 != 0) || (parse_data != tx_h.ENDIT); j++)
                            {
                                if (j != 0)
                                    txio.tx_printf(" ");
                                parse_data = (uint)txio.read_data_byte(ref verb_entry);
                                txio.tx_printf("{0:X2}", (uint)parse_data);
                            }
                            verb_size = j;
                        }
                        txio.tx_printf("] ");
                        for (; j < 8; j++)
                            txio.tx_printf("   ");

                        /* Show the verb grammar for this entry */

                        show_verb_grammar(parse_entry, tx_h.VERB_NUM(i, parser_type), (int)parser_type, 0,
                                           (int)prep_type, prep_table_base, attr_names_base);
                        txio.tx_printf("\n");
                    }
                }
            }

        }/* show_verb_parse_table */

        /* show_syntax_of_action
         *
         * Display the syntax entries for a given action number.  Used by
         * txd as well as by show_action_tables.  A pre-action number works as well
         * (because they are the same as action numbers), but not a parsing routine
         * number (see show_syntax_of_parsing_routine)
         *
         */

        internal static void show_syntax_of_action(uint action,
                                        ulong verb_table_base,
                                        uint verb_count,
                                        uint parser_type,
                                        uint prep_type,
                                        ulong prep_table_base,
                                        ulong attr_names_base)
        {
            ulong address;
            ulong verb_entry, parse_entry;
            uint entry_count, object_count, val, action_index;
            int i;
            bool matched = false;

            address = verb_table_base;
            for (i = 0; (uint)i < verb_count; i++)
            {

                if (parser_type == (uint)tx_h.parser_types.infocom6_grammar)
                {
                    ulong do_address, doio_address;
                    uint verb_address;

                    verb_address = (uint)address;
                    parse_entry = address;
                    action_index = txio.read_data_word(ref address);
                    if (action_index == (uint)action)
                    {
                        show_verb_grammar(parse_entry, verb_address, (int)parser_type, 0,
                                           (int)0, 0L, 0L);
                        txio.tx_printf("\n");
                        matched = true;
                    }
                    txio.read_data_word(ref address);
                    do_address = txio.read_data_word(ref address);
                    doio_address = txio.read_data_word(ref address);

                    if (do_address > 0)
                    {
                        verb_entry = do_address;
                        entry_count = (uint)txio.read_data_word(ref verb_entry);
                        while (entry_count-- > 0)
                        {
                            parse_entry = verb_entry;
                            action_index = txio.read_data_word(ref verb_entry);
                            if (action_index == (uint)action)
                            {
                                show_verb_grammar(parse_entry, verb_address, (int)parser_type, 1,
                                       0, 0L, 0L);
                                txio.tx_printf("\n");
                                matched = true;
                            }
                            verb_entry += 4; /* skip preposition and object */
                        }
                    }

                    if (doio_address > 0)
                    {
                        verb_entry = doio_address;
                        entry_count = (uint)txio.read_data_word(ref verb_entry);
                        while (entry_count-- > 0)
                        {
                            parse_entry = verb_entry;
                            action_index = txio.read_data_word(ref verb_entry);
                            if (action_index == (uint)action)
                            {
                                show_verb_grammar(parse_entry, verb_address, (int)parser_type, 2,
                                       0, 0L, 0L);
                                txio.tx_printf("\n");
                                matched = true;
                            }
                            verb_entry += 8; /* skip preposition and direct object and preposition and indirect object*/
                        }
                    }
                }
                else
                {
                    /* Get the parse data address for this entry */

                    verb_entry = (ulong)txio.read_data_word(ref address);
                    entry_count = (uint)txio.read_data_byte(ref verb_entry);

                    /* Look through the sentence structures looking for a match */

                    while (entry_count-- > 0)
                    {
                        parse_entry = verb_entry;
                        if (parser_type >= (uint)tx_h.parser_types.inform_gv2)
                        { /* GV2, variable length with terminator */
                            action_index = (uint)txio.read_data_word(ref verb_entry) & 0x3FF;
                            val = txio.read_data_byte(ref verb_entry);
                            while (val != tx_h.ENDIT)
                            {
                                txio.read_data_word(ref verb_entry);
                                val = txio.read_data_byte(ref verb_entry);
                            }
                        }
                        else if (parser_type != (uint)tx_h.parser_types.infocom_variable)
                        { /* Index is in last (8th) byte */
                            verb_entry += 7;
                            action_index = (uint)txio.read_data_byte(ref verb_entry);
                        }
                        else
                        { /* Index is in second byte */
                            object_count = (uint)txio.read_data_byte(ref verb_entry);
                            action_index = (uint)txio.read_data_byte(ref verb_entry);
                            verb_entry += (ulong)(verb_sizes[(object_count >> 6) & 0x03] - 2);
                        }

                        /* Check if this verb/sentence structure uses the action routine */

                        if (action_index == (uint)action)
                        {
                            show_verb_grammar(parse_entry, tx_h.VERB_NUM(i, parser_type), (int)parser_type, 0,
                                               (int)prep_type, prep_table_base, attr_names_base);
                            txio.tx_printf("\n");
                            matched = true;
                        }
                    }
                }
            }

            if (!matched)
            {
                txio.tx_printf("\n");
            }
        }

        internal static bool is_gv2_parsing_routine(ulong parsing_routine,
                                            ulong verb_table_base,
                                            uint verb_count)
        {
            ulong address;
            ulong verb_entry;
            ushort token_data;
            uint entry_count, val;
            int i;
            ulong parsing_routine_packed = (parsing_routine - (ulong)txio.story_scaler * txio.header.routines_offset) / txio.code_scaler;

            bool found = false;

            address = verb_table_base;
            found = false;
            for (i = 0; !found && (uint)i < verb_count; i++)
            {

                /* Get the parse data address for this entry */

                verb_entry = (ulong)txio.read_data_word(ref address);
                entry_count = (uint)txio.read_data_byte(ref verb_entry);
                while (!found && entry_count-- > 0)
                {
                    txio.read_data_word(ref verb_entry); /* skip action # and flags */
                    val = (uint)txio.read_data_byte(ref verb_entry);
                    while (val != tx_h.ENDIT)
                    {
                        token_data = txio.read_data_word(ref verb_entry);
                        if (((val & 0xC0) == 0x80) && (token_data == parsing_routine_packed))
                            found = true;
                        val = (uint)txio.read_data_byte(ref verb_entry);
                    }
                }
            }
            return found;
        }

        /* show_syntax_of_parsing_routine
         *
         * Display the syntax entries for a given parsing routine number or address.  Used by
         * txd as well as by show_action_tables. For Inform 5 and GV1, the input should be
         * the parsing routine number.	For GV2, it should be the parsing routine address.
         *
         */

        internal static void show_syntax_of_parsing_routine(ulong parsing_routine,
                                            ulong verb_table_base,
                                            uint verb_count,
                                            uint parser_type,
                                            uint prep_type,
                                            ulong prep_table_base,
                                            ulong attr_names_base)
        {
            ulong address;
            ulong verb_entry, parse_entry;
            ushort token_data;
            uint entry_count, object_count, val;
            ulong parsing_routine_packed = (parsing_routine - (ulong)txio.story_scaler * txio.header.routines_offset) / txio.code_scaler;
            int i;
            bool found = false;

            address = verb_table_base;
            for (i = 0; (uint)i < verb_count; i++)
            {

                /* Get the parse data address for this entry */

                verb_entry = (ulong)txio.read_data_word(ref address);
                entry_count = (uint)txio.read_data_byte(ref verb_entry);
                while (entry_count-- > 0)
                {
                    parse_entry = verb_entry;
                    found = false;
                    if (parser_type < (int)tx_h.parser_types.inform_gv2)
                    {
                        object_count = (uint)txio.read_data_byte(ref verb_entry);
                        while (object_count > 0)
                        {
                            val = (uint)txio.read_data_byte(ref verb_entry);
                            if (val < 0xb0)
                            {
                                object_count--;
                                if (val >= 0x10 && val < 0x70 && ((val - 0x10) & 0x1f) == (uint)parsing_routine)
                                    found = true;
                            }
                        }
                        verb_entry = parse_entry + 8;
                    }
                    else
                    {
                        txio.read_data_word(ref verb_entry); /* skip action # and flags */
                        val = (uint)txio.read_data_byte(ref verb_entry);
                        while (val != tx_h.ENDIT)
                        {
                            token_data = txio.read_data_word(ref verb_entry);
                            if (((val & 0xC0) == 0x80) && (token_data == parsing_routine_packed)) /* V7/V6 issue here */
                                found = true;
                            val = (uint)txio.read_data_byte(ref verb_entry);
                        }
                    }
                    if (found)
                    {
                        show_verb_grammar(parse_entry, tx_h.VERB_NUM(i, parser_type), (int)parser_type, (int)prep_type, 0,
                                           prep_table_base, attr_names_base);
                        txio.tx_printf("\n");
                    }
                }
            }
        }

        /*
         * show_action_tables
         *
         * Display the pre-action and action routine addresses. The list of
         * verb/sentence structures is displayed with each routine. A list of
         * verb/sentence structures against each routine indicate that the routine
         * is called when any of the verb/sentence structures are typed. Inform
         * written games, however, do not have a concept of pre-actions. Their
         * pre-actions table is filled with so-called parsing routines which
         * are linked to single objects within verb/sentence structures. Usually
         * these routines decide if a specific object or text matches these
         * sentence structures.
         */

        static void show_action_tables(ulong verb_table_base,
                                        uint verb_count,
                                        uint action_count,
                                        uint parse_count,
                                        uint parser_type,
                                        uint prep_type,
                                        ulong action_table_base,
                                        ulong preact_table_base,
                                        ulong prep_table_base,
                                        ulong attr_names_base,
                                        ulong action_names_base)
        {

            ulong actions_address, preacts_address;
            ulong routine_address;
            int action;

            var header = txio.header;

            txio.tx_printf("\n    **** Verb action routines ****\n\n");
            txio.tx_printf("  Action table entries = {0:d}\n\n", (int)action_count);
            txio.tx_printf("action# ");
            if (parser_type <= (uint)tx_h.parser_types.infocom6_grammar)
                txio.tx_printf("pre-action-routine ");

            txio.tx_printf("action-routine \"verb...\"\n\n");

            actions_address = action_table_base;
            preacts_address = preact_table_base;

            /* Iterate through all routine entries for pre-action and action routines */

            for (action = 0; (uint)action < action_count; action++)
            {

                /* Display the routine index and addresses */

                txio.tx_printf("{0:d3}. ", (int)action);
                if (parser_type <= (uint)tx_h.parser_types.infocom6_grammar)
                {
                    routine_address = (ulong)txio.read_data_word(ref preacts_address) * txio.code_scaler;
                    if (routine_address > 0)
                        routine_address += (ulong)txio.story_scaler * header.routines_offset;
                    txio.tx_printf("{0:X5} ", routine_address);
                }
                routine_address = (ulong)txio.read_data_word(ref actions_address) * txio.code_scaler;
                if (routine_address > 0)
                    routine_address += (ulong)txio.story_scaler * header.routines_offset;
                txio.tx_printf("{0:X5} ", routine_address);
                txio.tx_printf(" ");
                txio.tx_fix_margin(1);
                if (action_names_base > 0)
                {
                    txio.tx_printf("<");
                    infinfo.print_inform_action_name(action_names_base, action);
                    txio.tx_printf(">\n");
                }

                /*
                 * Now scan down the parse table looking for all verb/sentence formats
                 * that cause this action routine to be called.
                 */

                show_syntax_of_action((uint)action,
                                        verb_table_base,
                                        verb_count,
                                        parser_type,
                                        prep_type,
                                        prep_table_base,
                                        attr_names_base);

                txio.tx_fix_margin(0);
            }

            if ((parser_type >= (uint)tx_h.parser_types.inform5_grammar) && (parser_type < (uint)tx_h.parser_types.inform_gv2))
            {

                /* Determine number of parsing routines (ie. the number of
                   non-zero entries in the former pre-actions table) */

                txio.tx_printf("\n    **** Parsing routines ****\n\n");
                txio.tx_printf("  Number of parsing routines = %d\n\n", (int)parse_count);
                txio.tx_printf("parse# parsing-routine \"verb...\"\n\n");

                for (action = 0; (uint)action < parse_count; action++)
                {

                    /* Display the routine index and addresses */

                    txio.tx_printf("{0:d3}. ", (int)action);
                    txio.tx_printf("{0:X5} ", (ulong)txio.read_data_word(ref preacts_address) * txio.code_scaler + (ulong)txio.story_scaler * header.routines_offset);
                    txio.tx_printf(" ");
                    txio.tx_fix_margin(1);
                    /*
                     * Now scan down the parse table looking for all verb/sentence formats
                     * that this parsing routine applies to.
                     */

                    show_syntax_of_parsing_routine((ulong)action,
                                                   verb_table_base,
                                                   verb_count,
                                                   parser_type,
                                                   prep_type,
                                                   prep_table_base,
                                                   attr_names_base);
                    txio.tx_fix_margin(0);
                }
            }

        }/* show_action_tables */

        /*
         * show_preposition table
         *
         * Displays all the prepositions and their synonyms. The preposition index can
         * be found in the sentence structure data in the parse tables.
         */

        internal static void show_preposition_table(uint prep_type,
                                            ulong prep_table_base,
                                            uint parser_type)
        {
            ulong address, prep_address;
            uint count, prep_index;
            int i;

            /* Get the base address and count of prepositions */

            address = prep_table_base;
            count = (uint)txio.read_data_word(ref address);

            txio.tx_printf("\n    **** Prepositions ****\n\n");
            txio.tx_printf("  Table entries = {0}\n\n", (int)count);

            /* Iterate through all prepositions */

            for (i = 0; (uint)i < count; i++)
            {

                /* Read the dictionary address of the text for this entry */

                prep_address = txio.read_data_word(ref address);

                /* Pick up the index */

                if (prep_type == 0)
                    prep_index = (uint)txio.read_data_word(ref address);
                else
                    prep_index = (uint)txio.read_data_byte(ref address);

                /* Display index and word */

                txio.tx_printf("{0:d3}. ", (int)prep_index);
                show_words(prep_index, prep_address, tx_h.PREP, parser_type);
                txio.tx_printf("\n");
            }

        }/* show_preposition_table */

        /*
         * show_words
         *
         * Display any verb/preposition and synonyms by index. Inform written games
         * do not have synonyms for prepositions.
         */

        private static void show_words(uint indx,
                                ulong prep_address,
                                uint type,
                                uint parser_type)
        {
            ulong address, word_address;
            int flag = 0;

            /* If this is a preposition then we have an address */

            if (type == tx_h.PREP)
                word_address = prep_address;
            else
                word_address = lookup_word(0L, indx, type, parser_type);

            /* If the word address is NULL then there are no entries */

            if (word_address == 0)
            {
                txio.tx_printf(" no-");
                if ((type == tx_h.VERB) || (type == tx_h.VERB_V6))
                    txio.tx_printf("verb");
                if (type == tx_h.PREP)
                    txio.tx_printf("preposition");
            }

            /* Display all synonyms for the verb or preposition */

            for (flag = 0; word_address > 0; flag++)
            {
                if (flag > 0)
                    txio.tx_printf(", ");
                if (flag == 1)
                {
                    txio.tx_printf("synonyms = ");
                    txio.tx_fix_margin(1);
                }

                /* Display the current word */

                address = word_address;
                txio.tx_printf("\"");
                txio.decode_text(ref address);
                txio.tx_printf("\"");

                /* Lookup the next synonym (but skip the word itself) */

                if (type == tx_h.PREP && flag == 0)
                    word_address = 0;
                if (type != tx_h.PREP || parser_type <= (int)tx_h.parser_types.infocom_variable)
                {
                    word_address = lookup_word(word_address, indx, type, parser_type);
                    if (type == tx_h.PREP && word_address == prep_address)
                        word_address = lookup_word(word_address, indx, type, parser_type);
                }
            }
            if (flag > 0)
                txio.tx_fix_margin(0);

        }/* show_words */

        /*
         * show_verb_grammar
         *
         * Display the sentence structure associated with a parse table entry.
         */

        internal static void show_verb_grammar(ulong verb_entry,
                                uint verb_index,
                                int parser_type,
                                int v6_number_objects,
                                int prep_type,
                                ulong prep_table_base,
                                ulong attr_names_base)
        {

            ulong address, verb_address, prep_address;
            uint parse_data, objs, val;
            uint token_type, token_data, action;
            int i;
            uint[] preps = new uint[2];

            string[] GV2_elementary = new String[] {"noun" ,"held", "multi", "multiheld",
                                            "multiexcept", "multiinside", "creature",
                                            "special", "number", "topic"};
            address = verb_entry;

            if (parser_type == (int)tx_h.parser_types.infocom6_grammar)
            {
                txio.tx_printf("\"");
                verb_address = lookup_word(0L, verb_index, tx_h.VERB_V6, (uint)parser_type);
                if (verb_address > 0)
                    txio.decode_text(ref verb_address);
                else
                    txio.tx_printf("no-verb");

                if (v6_number_objects > 0)
                {
                    txio.tx_printf(" ");

                    action = txio.read_data_word(ref address);
                    while (v6_number_objects-- > 0)
                    {
                        token_data = txio.read_data_word(ref address);
                        token_type = txio.read_data_word(ref address);
                        if (token_data > 0)
                        {
                            prep_address = token_data;
                            txio.decode_text(ref prep_address);
                            txio.tx_printf(" ");
                        }
                        //                            txio.tx_printf("${0:X4}", token_type);  /* turn this on if you want to see the attribute and flag? info for the object */

                        txio.tx_printf("OBJ");

                        if (v6_number_objects > 0)
                            txio.tx_printf(" ");
                    }
                }
                txio.tx_printf("\"");
            }
            else if (parser_type >= (int)tx_h.parser_types.inform_gv2)
            {
                /* Inform 6 GV2 verb entry */

                txio.tx_printf("\"");

                /* Print verb if one is present */

                verb_address = lookup_word(0L, verb_index, tx_h.VERB, (uint)parser_type);

                if (verb_address > 0)
                    txio.decode_text(ref verb_address);
                else
                    txio.tx_printf("no-verb");

                action = txio.read_data_word(ref address); /* Action # and flags*/

                val = txio.read_data_byte(ref address);
                while (val != tx_h.ENDIT)
                {
                    if (((val & 0x30) == 0x10) || ((val & 0x30) == 0x30)) /* 2nd ... nth byte of alternative list */
                        txio.tx_printf(" /");
                    txio.tx_printf(" ");
                    token_type = val & 0xF;
                    token_data = txio.read_data_word(ref address);
                    switch ((gv2_tokentype)token_type)
                    {
                        case gv2_tokentype.TT_ELEMENTARY:
                            if (token_data < GV2_elementary.Length)
                                txio.tx_printf(GV2_elementary[token_data]);
                            else
                                txio.tx_printf("UNKNOWN_ELEMENTARY");
                            break;
                        case gv2_tokentype.TT_PREPOSITION:
                            prep_address = token_data;
                            txio.decode_text(ref prep_address);
                            break;
                        case gv2_tokentype.TT_NOUNR:
                            txio.tx_printf("noun = [parse ${0:X4}]", token_data);
                            break;
                        case gv2_tokentype.TT_ATTRIBUTE:
                            txio.tx_printf("ATTRIBUTE(");
                            if (symbols.print_attribute_name(attr_names_base, (int)token_data) == 0)
                            {
                                txio.tx_printf("{0}", token_data);
                            }
                            txio.tx_printf(")", token_data);
                            break;
                        case gv2_tokentype.TT_SCOPER:
                            txio.tx_printf("scope = [parse ${0:X4}]", token_data);
                            break;
                        case gv2_tokentype.TT_ROUTINE:
                            txio.tx_printf("[parse ${0:X4}]", token_data);
                            break;
                        default:
                            txio.tx_printf("UNKNOWN");
                            break;
                    }
                    val = txio.read_data_byte(ref address);
                }
                txio.tx_printf("\"");
                if ((action & 0x0400) > 0)
                    txio.tx_printf(" REVERSE");
            }
            else if (parser_type >= (int)tx_h.parser_types.inform5_grammar)
            {
                /* Inform 5 and GV1 verb entries are just a series of tokens */

                txio.tx_printf("\"");

                /* Print verb if one is present */

                verb_address = lookup_word(0L, verb_index, tx_h.VERB, (uint)parser_type);

                if (verb_address > 0)
                    txio.decode_text(ref verb_address);
                else
                    txio.tx_printf("no-verb");

                objs = txio.read_data_byte(ref address);

                for (i = 0; i < 8; i++)
                {
                    val = txio.read_data_byte(ref address);
                    if (val < 0xb0)
                    {
                        if (val == 0 && objs == 0)
                            break;
                        txio.tx_printf(" ");
                        if (val == 0)
                            txio.tx_printf("NOUN");
                        else if (val == 1)
                            txio.tx_printf("HELD");
                        else if (val == 2)
                            txio.tx_printf("MULTI");
                        else if (val == 3)
                            txio.tx_printf("MULTIHELD");
                        else if (val == 4)
                            txio.tx_printf("MULTIEXCEPT");
                        else if (val == 5)
                            txio.tx_printf("MULTIINSIDE");
                        else if (val == 6)
                            txio.tx_printf("CREATURE");
                        else if (val == 7)
                            txio.tx_printf("SPECIAL");
                        else if (val == 8)
                            txio.tx_printf("NUMBER");
                        else if (val >= 16 && val < 48)
                            txio.tx_printf("NOUN [parse {0}]", val - 16);
                        else if (val >= 48 && val < 80)
                            txio.tx_printf("TEXT [parse {0}]", val - 48);
                        else if (val >= 80 && val < 112)
                            txio.tx_printf("SCOPE [parse {0}]", val - 80);
                        else if (val >= 128 && val < 176)
                        {
                            txio.tx_printf("ATTRIBUTE(");
                            if (symbols.print_attribute_name(attr_names_base, (int)(val - 128)) == 0)
                            {
                                txio.tx_printf("{0}", val - 128);
                            }
                            txio.tx_printf(")");
                        }
                        else
                            txio.tx_printf("UNKNOWN");
                        objs--;
                    }
                    else
                    {
                        txio.tx_printf(" ");
                        show_preposition(val, prep_type, prep_table_base);
                    }
                }

                txio.tx_printf("\"");
            }
            else
            {

                address = verb_entry;
                preps[0] = preps[1] = 0;

                /* Calculate noun count and prepositions */

                if (parser_type == (int)tx_h.parser_types.infocom_fixed)
                {

                    /* Fixed length parse table format */

                    /* Object count in 1st byte, preposition indices in next two bytes */

                    objs = (uint)txio.read_data_byte(ref address);
                    preps[0] = (uint)txio.read_data_byte(ref address);
                    preps[0] = (preps[0] >= 0x80) ? preps[0] : 0;
                    preps[1] = (uint)txio.read_data_byte(ref address);
                    preps[1] = (preps[1] >= 0x80) ? preps[1] : 0;
                }
                else
                {

                    /* Variable length parse table format */

                    /* Object count in top two bits of first byte */

                    parse_data = (uint)txio.read_data_byte(ref address);
                    objs = (parse_data >> 6) & 0x03;

                    /* 1st preposition in bottom 6 bits of first byte. Fill in top two bits */

                    preps[0] = (parse_data & 0x3f) > 0 ? parse_data | 0xc0 : 0;
                    parse_data = (uint)txio.read_data_byte(ref address);

                    /* Check for more than one object */

                    if (objs > 0)
                    {

                        /* Skip object data */

                        parse_data = (uint)txio.read_data_byte(ref address);
                        parse_data = (uint)txio.read_data_byte(ref address);

                        /* Check for more than two objects */

                        if (objs > 1)
                        {

                            /* 2nd preposition in bottom 6 bits of byte. Fill in top two bits */

                            parse_data = (uint)txio.read_data_byte(ref address);
                            preps[1] = (parse_data & 0x3f) > 0 ? parse_data | 0xc0 : 0;
                        }
                    }
                }

                /* Check that there are 0 - 2 objects only */

                if (objs > 2)
                {

                    txio.tx_printf("Bad object count (%d)", (int)objs);

                }
                else
                {

                    txio.tx_printf("\"");

                    /* Print verb if one is present */

                    verb_address = lookup_word(0L, verb_index, tx_h.VERB, (uint)parser_type);

                    if (verb_address > 0)
                        txio.decode_text(ref verb_address);
                    else
                        txio.tx_printf("no-verb");

                    /* Display any prepositions and objects if present */

                    for (i = 0; i < 2; i++)
                    {
                        if (preps[i] != 0)
                        {
                            txio.tx_printf(" ");
                            show_preposition(preps[i], prep_type, prep_table_base);
                        }
                        if (objs > (uint)i)
                            txio.tx_printf(" OBJ");
                    }

                    txio.tx_printf("\"");

                }
            }

        }/* show_verb_grammar */

        /*
         * show_preposition
         *
         * Display a preposition by index.
         */

        static void show_preposition(uint prep,
                                      int prep_type,
                                      ulong prep_table_base)
        {
            ulong address, text_address;
            uint prep_count, prep_num;
            int i;

            address = prep_table_base;
            prep_count = (uint)txio.read_data_word(ref address);

            /* Iterate through the preposition table looking for a match */

            for (i = 0; (uint)i < prep_count; i++)
            {
                text_address = txio.read_data_word(ref address);
                if (prep_type == 0)
                    prep_num = (uint)txio.read_data_word(ref address);
                else
                    prep_num = (uint)txio.read_data_byte(ref address);

                /* If the indices match then print the preposition text */

                if (prep == prep_num)
                {
                    txio.decode_text(ref text_address);
                    return;
                }
            }

        }/* show_preposition */

        /*
         * lookup_word
         *
         * Look up a word in the dictionary based on its type; verb, preposition, etc.
         * The return entry is used to restart the search from the last word found.
         */

        private static ulong lookup_word(ulong entry,
                                          uint number,
                                          uint mask,
                                          uint parser_type)
        {
            ulong address, word_address, first_word, last_word;
            uint word_count, word_size, flags, data;

            /* Calculate dictionary bounds and entry size */

            address = (ulong)txio.header.dictionary;
            ulong temp = (ulong)txio.read_data_byte(ref address);
            address += temp;
            word_size = txio.read_data_byte(ref address);
            word_count = txio.read_data_word(ref address);
            first_word = address;
            last_word = address + ((word_count - 1) * word_size);

            /* If entry is 0 then set to first word, otherwise advance to next word */

            if (entry == 0)
                entry = first_word;
            else
                entry += word_size;

            /* Correct Inform verb mask -- Inform sets both 0x40 and 0x01, but only 0x01 is documented */
            if ((mask == tx_h.VERB) && (parser_type >= (int)tx_h.parser_types.inform5_grammar))
                mask = (uint)tx_h.VERB_INFORM;

            /* Scan down the dictionary from entry looking for a match */

            for (word_address = entry; word_address <= last_word; word_address += word_size)
            {

                /* Skip to flags byte and read it */

                if (parser_type != (int)tx_h.parser_types.infocom6_grammar)
                {
                    address = word_address + (ulong)(((uint)txio.header.version < tx_h.V4) ? 4 : 6);
                    flags = txio.read_data_byte(ref address);
                }
                else
                {
                    address = word_address + word_size - 1;
                    flags = txio.read_data_byte(ref address);
                    address = word_address + 6;
                }

                /* Check if this word is the type we are looking for */

                if ((flags & mask) > 0)
                {

                    if ((parser_type == (int)tx_h.parser_types.infocom6_grammar) || (parser_type >= (int)tx_h.parser_types.inform_gv2a))
                    {
                        data = (uint)txio.read_data_word(ref address);
                    }
                    else if (parser_type <= (int)tx_h.parser_types.inform_gv1)
                    {
                        /* Infocom, Inform 5, GV1.	Verbs only for Inform */
                        /* Read the data for the word */

                        data = (uint)txio.read_data_byte(ref  address);

                        /* Skip to next byte under some circumstances */

                        if (((mask == tx_h.VERB) && (flags & tx_h.DATA_FIRST) != tx_h.VERB_FIRST) ||
                            ((mask == tx_h.DESC) && (flags & tx_h.DATA_FIRST) != tx_h.ADJ_FIRST))
                            data = (uint)txio.read_data_byte(ref address);
                    }
                    else
                    {
                        /* GV2, Inform 6.10 version */
                        data = (uint)txio.read_data_byte(ref address);
                    }

                    /* If this word matches the type and index then return its address */

                    if (data == number)
                        return (word_address);
                }
            }

            /* Return 0 if no more words found */

            return 0;

        }/* lookup_word */
    }
}