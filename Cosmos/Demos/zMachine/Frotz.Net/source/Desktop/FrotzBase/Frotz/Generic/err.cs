/* err.c - Runtime error reporting functions
 *	Written by Jim Dunleavy <jim.dunleavy@erha.ie>
 *
 * This file is part of Frotz.
 *
 * Frotz is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * Frotz is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA
 */

using Frotz.Constants;

namespace Frotz.Generic {
    internal static class Err {
        /* Define stuff for stricter Z-code error checking, for the generic
           Unix/DOS/etc terminal-window interface. Feel free to change the way
           player prefs are specified, or replace report_zstrict_error() 
           completely if you want to change the way errors are reported. */

        internal static int err_report_mode = ErrorCodes.ERR_DEFAULT_REPORT_MODE;
        static int[] error_count = new int[ErrorCodes.ERR_NUM_ERRORS];

        static string[] err_messages = {
            "Text buffer overflow",
            "Store out of dynamic memory",
            "Division by zero",
            "Illegal object",
            "Illegal attribute",
            "No such property",
            "Stack overflow",
            "Call to illegal address",
            "Call to non-routine",
            "Stack underflow",
            "Illegal opcode",
            "Bad stack frame",
            "Jump to illegal address",
            "Can't save while in interrupt",
            "Nesting stream #3 too deep",
            "Illegal window",
            "Illegal window property",
            "Print at illegal address",
            "Illegal dictionary word length",
            "@jin called with object 0",
            "@get_child called with object 0",
            "@get_parent called with object 0",
            "@get_sibling called with object 0",
            "@get_prop_addr called with object 0",
            "@get_prop called with object 0",
            "@put_prop called with object 0",
            "@clear_attr called with object 0",
            "@set_attr called with object 0",
            "@test_attr called with object 0",
            "@move_object called moving object 0",
            "@move_object called moving into object 0",
            "@remove_object called with object 0",
            "@get_next_prop called with object 0"
        };

        //static void print_long (unsigned long value, int base);

        /*
         * init_err
         *
         * Initialise error reporting.
         *
         */

        internal static void init_err ()
        {
            int i;

            /* Initialize the counters. */

            for (i = 0; i < ErrorCodes.ERR_NUM_ERRORS; i++)
                error_count[i] = 0;
        }

        /*
         * runtime_error
         *
         * An error has occurred. Ignore it, pass it to os_fatal or report
         * it according to err_report_mode.
         *
         * errnum : Numeric code for error (1 to ERR_NUM_ERRORS)
         *
         */

        internal static void runtime_error(int errnum) {
            bool wasfirst;

            if (errnum <= 0 || errnum > ErrorCodes.ERR_NUM_ERRORS)
                return;

            if (err_report_mode == ErrorCodes.ERR_REPORT_FATAL
            || (main.option_ignore_errors == false && errnum <= ErrorCodes.ERR_MAX_FATAL)) {
                Buffer.flush_buffer();
                os_.fatal(err_messages[errnum - 1]);
                return;
            }

            wasfirst = (error_count[errnum - 1] == 0);
            error_count[errnum - 1]++;

            if ((err_report_mode == ErrorCodes.ERR_REPORT_ALWAYS)
            || (err_report_mode == ErrorCodes.ERR_REPORT_ONCE && wasfirst)) {
                long pc;

                FastMem.GET_PC(out pc);
                Text.print_string("Warning: ");
                Text.print_string(err_messages[errnum - 1]);
                Text.print_string(" (PC = ");
                print_long(pc, 16);
                Buffer.print_char(')');

                if (err_report_mode == ErrorCodes.ERR_REPORT_ONCE) {
                    Text.print_string(" (will ignore further occurrences)");
                } else {
                    Text.print_string(" (occurence ");
                    print_long(error_count[errnum - 1], 10);
                    Buffer.print_char(')');
                }
                Buffer.new_line();
            }

        } /* report_error */

        /*
         * print_long
         *
         * Print an unsigned 32bit number in decimal or hex.
         *
         */

        static void print_long (long value, int base_val)
        {
            string s = "";
            switch (base_val) {
                case 10:  s = value.ToString(); break;
                case 16: s = value.ToString("X"); break;
                default: os_.fail("Unsupported print_long base"); break;
            }
            Text.print_string(s);
        }/* print_long */
    }
}