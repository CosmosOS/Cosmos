
/*
 * showdict - part of infodump
 *
 * Dictionary and abbreviation table routines.
 */
namespace ZTools
{
    public static class showdict
    {
        /*
         * show_dictionary
         *
         * List the dictionary in the number of columns specified. If the number of
         * columns is one then also display the data associated with each word.
         */

        internal static void show_dictionary(int columns)
        {
            ulong dict_address, word_address, word_table_base, word_table_end;
            uint separator_count, word_size, word_count, length;
            int i;
            bool inform_flags = false;
            int dictpar1 = 0;

            uint flag;

            var header = txio.header;

            /* Force default column count if none specified */

            if (columns == 0)
                columns = ((uint)header.version < tx_h.V4) ? 5 : 4;

            /* Get dictionary configuration */

            configure_dictionary(out word_count, out word_table_base, out word_table_end);

            if (header.serial[0] >= '0' && header.serial[0] <= '9' &&
                header.serial[1] >= '0' && header.serial[1] <= '9' &&
                header.serial[2] >= '0' && header.serial[2] <= '1' &&
                header.serial[3] >= '0' && header.serial[3] <= '9' &&
                header.serial[4] >= '0' && header.serial[4] <= '3' &&
                header.serial[5] >= '0' && header.serial[5] <= '9' &&
                header.serial[0] != '8')
            {
                inform_flags = true;
            }

            txio.tx_printf("\n    **** Dictionary ****\n\n");

            /* Display the separators */

            dict_address = word_table_base;
            separator_count = txio.read_data_byte(ref dict_address);
            txio.tx_printf("  Word separators = \"");
            for (; separator_count > 0; separator_count--)
                txio.tx_printf("{0:c}", (char)txio.read_data_byte(ref dict_address));
            txio.tx_printf("\"\n");

            /* Get word size and count */

            word_size = txio.read_data_byte(ref dict_address);
            word_count = txio.read_data_word(ref dict_address);

            txio.tx_printf("  Word count = {0}, word size = {0}\n", (int)word_count, (int)word_size);

            /* Display each entry in the dictionary */

            for (i = 1; (uint)i <= word_count; i++)
            {

                /* Set column breaks */

                if (columns == 1 || (i % columns) == 1)
                    txio.tx_printf("\n");

                txio.tx_printf("[{0:d4}] ", (int)i);

                /* Calculate address of next entry */

                word_address = dict_address;
                dict_address += word_size;

                if (columns == 1)
                    txio.tx_printf("@ ${0:X2} ", (uint)word_address);

                /* Display the text for the word */

                for (length = (uint)txio.decode_text(ref word_address); length <= word_size; length++)
                    txio.tx_printf(" ");

                /* For a single column list also display the data for each entry */

                if (columns == 1)
                {
                    txio.tx_printf("[");
                    for (flag = 0; word_address < dict_address; flag++)
                    {
                        if (flag > 0)
                            txio.tx_printf(" ");
                        else
                            dictpar1 = tx_h.get_byte(word_address);

                        txio.tx_printf("{0:X2}", (uint)txio.read_data_byte(ref word_address));
                    }
                    txio.tx_printf("]");

                    if (inform_flags)
                    {
                        if ((dictpar1 & tx_h.NOUN) > 0)
                            txio.tx_printf(" <noun>");
                        if ((dictpar1 & tx_h.PREP) > 0)
                            txio.tx_printf(" <prep>");
                        if ((dictpar1 & tx_h.PLURAL) > 0)
                            txio.tx_printf(" <plural>");
                        if ((dictpar1 & tx_h.META) > 0)
                            txio.tx_printf(" <meta>");
                        if ((dictpar1 & tx_h.VERB_INFORM) > 0)
                            txio.tx_printf(" <verb>");
                    }
                    else if (header.version != tx_h.V6)
                    {
                        flag = (uint)(dictpar1 & tx_h.DATA_FIRST);
                        switch (flag)
                        {
                            case tx_h.DIR_FIRST:
                                if ((dictpar1 & tx_h.DIR) > 0)
                                    txio.tx_printf(" <dir>");
                                break;
                            case tx_h.ADJ_FIRST:
                                if ((dictpar1 & tx_h.DESC) > 0)
                                    txio.tx_printf(" <adj>");
                                break;
                            case tx_h.VERB_FIRST:
                                if ((dictpar1 & tx_h.VERB) > 0)
                                    txio.tx_printf(" <verb>");
                                break;
                            case tx_h.PREP_FIRST:
                                if ((dictpar1 & tx_h.PREP) > 0)
                                    txio.tx_printf(" <prep>");
                                break;
                        }
                        if ((dictpar1 & tx_h.DIR) > 0 && (flag != tx_h.DIR_FIRST))
                            txio.tx_printf(" <dir>");
                        if ((dictpar1 & tx_h.DESC) > 0 && (flag != tx_h.ADJ_FIRST))
                            txio.tx_printf(" <adj>");
                        if ((dictpar1 & tx_h.VERB) > 0 && (flag != tx_h.VERB_FIRST))
                            txio.tx_printf(" <verb>");
                        if ((dictpar1 & tx_h.PREP) > 0 && (flag != tx_h.PREP_FIRST))
                            txio.tx_printf(" <prep>");
                        if ((dictpar1 & tx_h.NOUN) > 0)
                            txio.tx_printf(" <noun>");
                        if ((dictpar1 & tx_h.SPECIAL) > 0)
                            txio.tx_printf(" <special>");
                    }
                }
            }
            txio.tx_printf("\n");

        }/* show_dictionary */

        /*
         * configure_dictionary
         *
         * Determine the dictionary start and end addresses, together with the number
         * of word entries.
         *
         * Format:
         *
         * As ASCIC string listing the punctuation to be treated as words. Correct
         * recognition of punctuation is important for parsing.
         *
         * A byte word size. Not the size of the displayed word, but the amount of data
         * occupied by each word entry in the dictionary.
         *
         * A word word count. Total size of dictionary is word count * word size.
         *
         * Word count word entries. The format of the textual part of the word is fixed
         * by the Z machine, but the data following each word can vary. The text for
         * the word starts each entry. It is a packed string. The data
         * associated with each word is used in parsing a sentence. It includes flags
         * to identify the type of word (verb, noun, etc.) and data specific to each
         * word type.
         */

        internal static void configure_dictionary(out uint word_count,
                                   out ulong word_table_base,
                                   out ulong word_table_end)
        {
            ulong dict_address;
            uint separator_count, word_size;

            word_table_base = 0;
            word_table_end = 0;
            word_count = 0;

            /* Dictionary base address comes from the header */

            word_table_base = (ulong)txio.header.dictionary;

            /* Skip the separator list */

            dict_address = word_table_base;
            separator_count = txio.read_data_byte(ref dict_address);
            dict_address += separator_count;

            /* Get entry size and count */

            word_size = (uint)txio.read_data_byte(ref dict_address);
            word_count = (uint)txio.read_data_word(ref dict_address);

            /* Calculate dictionary end address */

            word_table_end = (dict_address + (word_size * word_count)) - 1;

        }/* configure_dictionary */

        /*
         * show_abbreviations
         *
         * Display the list of abbreviations used to compress text strings.
         */

        internal static void show_abbreviations()
        {
            ulong table_address, abbreviation_address;
            ulong abbr_table_base, abbr_table_end, abbr_data_base, abbr_data_end;
            uint abbr_count;
            int i;

            /* Get abbreviations configuration */

            configure_abbreviations(out abbr_count, out abbr_table_base, out abbr_table_end,
                         out abbr_data_base, out abbr_data_end);

            txio.tx_printf("\n    **** Abbreviations ****\n\n");

            /* No abbreviations if count is zero (V1 games only) */

            if (abbr_count == 0)
            {
                txio.tx_printf("No abbreviation information.\n");
            }
            else
            {

                /* Display each abbreviation */

                table_address = abbr_table_base;

                for (i = 0; (uint)i < abbr_count; i++)
                {

                    /* Get address of abbreviation text from table */

                    abbreviation_address = (ulong)txio.read_data_word(ref table_address) * 2;
                    txio.tx_printf("[{0:d2}] \"", (int)i);
                    txio.decode_text(ref abbreviation_address);
                    txio.tx_printf("\"\n");
                }
            }
        }

        //}/* show_abbreviations */

        /*
         * configure_abbreviations
         *
         * Determine the abbreviation table start and end addresses, together
         * with the abbreviation text start and end addresses, and the number
         * of abbreviation entries.
         *
         * Format:
         *
         * The abbreviation information consists of two parts. Firstly a table of
         * word sized pointers corresponding to the abbreviation number, and
         * secondly, the packed string data for each abbreviation.
         *
         * Note: the pointers have to be multiplied by 2 *regardless* of the game
         * version to get the byte address for each abbreviation.
         */

        internal static void configure_abbreviations(
            out uint abbr_count, out ulong abbr_table_base, out ulong abbr_table_end,
            out ulong abbr_data_base, out ulong abbr_data_end)
        {
            ulong table_address, address;
            int i, tables;

            abbr_table_base = 0;
            abbr_table_end = 0;
            abbr_data_base = 0;
            abbr_data_end = 0;
            abbr_count = 0;

            /* The abbreviation table address comes from the header */

            abbr_table_base = (ulong)txio.header.abbreviations;

            /* Check if there is any abbreviation table (V2 games and above) */

            if (abbr_table_base > 0)
            {

                /* Calculate the number of abbreviation tables (V2 = 1, V3+ = 3) */

                tables = ((uint)txio.header.version < tx_h.V3) ? 1 : 3;

                /* Calculate abbreviation count and table end address */

                abbr_count = (uint)(tables * 32);
                abbr_table_end = abbr_table_base + (abbr_count * 2) - 1;

                /* Calculate the high and low address for the abbreviation strings */

                table_address = abbr_table_base;
                for (i = 0; (uint)i < abbr_count; i++)
                {
                    address = (ulong)txio.read_data_word(ref table_address) * 2;
                    if (abbr_data_base == 0 || address < abbr_data_base)
                        abbr_data_base = address;
                    if (abbr_data_end == 0 || address > abbr_data_end)
                        abbr_data_end = address;
                }

                /* Scan last string to get the actual end of the string */

                while (((uint)txio.read_data_word(ref abbr_data_end) & 0x8000) == 0)
                    ;

                (abbr_data_end)--;
            }

        }/* configure_abbreviations */
    }
}