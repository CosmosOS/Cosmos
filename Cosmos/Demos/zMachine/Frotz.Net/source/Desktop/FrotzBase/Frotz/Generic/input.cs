/* input.c - High level input functions
 *	Copyright (c) 1995-1997 Stefan Jokisch
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
using zword = System.UInt16;
using zbyte = System.Byte;

using Frotz;
using Frotz.Constants;

namespace Frotz.Generic {
    internal static class Input {

        //zword unicode_tolower (zword);

        /*
         * is_terminator
         *
         * Check if the given key is an input terminator.
         *
         */

        internal static bool is_terminator(zword key)
        {

            if (key == CharCodes.ZC_TIME_OUT)
            return true;
            if (key == CharCodes.ZC_RETURN)
            return true;
            if (key >= CharCodes.ZC_HKEY_MIN && key <= CharCodes.ZC_HKEY_MAX)
            return true;

            if (main.h_terminating_keys != 0)

            if (key >= CharCodes.ZC_ARROW_MIN && key <= CharCodes.ZC_MENU_CLICK) {

                zword addr = main.h_terminating_keys;
                zbyte c;

                do {
                    FastMem.LOW_BYTE(addr, out c);
                if (c == 255 || key == Text.translate_from_zscii (c))
                    return true;
                addr++;
                } while (c != 0);

            }

            return false;

        }/* is_terminator */

        /*
         * z_make_menu, add or remove a menu and branch if successful.
         *
         *	zargs[0] = number of menu
         *	zargs[1] = table of menu entries or 0 to remove menu
         *
         */
        static zword[] menu = new zword[32];

        internal static void z_make_menu ()
        {

            /* This opcode was only used for the Macintosh version of Journey.
               It controls menus with numbers greater than 2 (menus 0, 1 and 2
               are system menus). */

            if (Process.zargs[0] < 3) {
            Process.branch (false);
            return;
            }

            if (Process.zargs[1] != 0) {

            zword items;
            int i;

            FastMem.LOW_WORD (Process.zargs[1], out items);

            for (i = 0; i < items; i++) {

                zword item;
                zbyte length;
                zbyte c;
                int j;

                FastMem.LOW_WORD (Process.zargs[1]+2+(2*i), out item);
               FastMem. LOW_BYTE (item, out length);

                if (length > 31)
                length = 31;
                menu[length] = 0;

                for (j = 0; j < length; j++) {

                    FastMem.LOW_BYTE(item + j + 1, out c);
                menu[j] = Text.translate_from_zscii (c);
                }

                if (i == 0)
                os_.menu(ZMachine.MENU_NEW, Process.zargs[0], menu);
                else
                    os_.menu(ZMachine.MENU_ADD, Process.zargs[0], menu);
            }
            }
            else os_.menu(ZMachine.MENU_REMOVE, Process.zargs[0], new zword[0]);

            Process.branch (true);

        }/* z_make_menu */

        /*
         * read_yes_or_no
         *
         * Ask the user a question; return true if the answer is yes.
         *
         */

        internal static bool read_yes_or_no (string s)
        {
            zword key;

            Text.print_string (s);
            Text.print_string ("? (y/n) >");

            key = Stream.stream_read_key (0, 0, false);

            if (key == 'y' || key == 'Y') {
            Text.print_string ("y\n");
            return true;
            } else {
            Text.print_string ("n\n");
            return false;
            }

        }/* read_yes_or_no */

        /*
         * read_string
         *
         * Read a string from the current input stream.
         *
         */

        internal static void read_string(int max, zword[] buffer)
        {
            zword key;

            buffer[0] = 0;

            do
            {

                key = Stream.stream_read_input(max, buffer, 0, 0, false, false);

            } while (key != CharCodes.ZC_RETURN);

        }/* read_string */

        /*
         * read_number
         *
         * Ask the user to type in a number and return it.
         *
         */

        internal static int read_number ()
        {
            zword[] buffer = new zword[6];
            int value = 0;
            int i;

            Input.read_string (5, buffer);

            for (i = 0; buffer[i] != 0; i++)
            if (buffer[i] >= '0' && buffer[i] <= '9')
                value = 10 * value + buffer[i] - '0';

            return value;

        }/* read_number */

        /*
         * z_read, read a line of input and (in V5+) store the terminating key.
         *
         *	zargs[0] = address of text buffer
         *	zargs[1] = address of token buffer
         *	zargs[2] = timeout in tenths of a second (optional)
         *	zargs[3] = packed address of routine to be called on timeout
         *
         */

        internal static void z_read() {
            zword[] buffer = new zword[General.INPUT_BUFFER_SIZE];
            zword addr;
            zword key;
            zbyte max, size;
            zbyte c;
            int i;

            /* Supply default arguments */

            if (Process.zargc < 3)
                Process.zargs[2] = 0;

            /* Get maximum input size */

            addr = Process.zargs[0];

            FastMem.LOW_BYTE(addr, out max);

            if (main.h_version <= ZMachine.V4)
                max--;

            if (max >= General.INPUT_BUFFER_SIZE)
                max = (zbyte)(General.INPUT_BUFFER_SIZE - 1);

            /* Get initial input size */

            if (main.h_version >= ZMachine.V5) {
                addr++;
                FastMem.LOW_BYTE(addr, out size);
            } else size = 0;

            /* Copy initial input to local buffer */

            for (i = 0; i < size; i++) {
                addr++;
                FastMem.LOW_BYTE(addr, out c);
                buffer[i] = Text.translate_from_zscii(c);
            }

            buffer[i] = 0;

            /* Draw status line for V1 to V3 games */

            if (main.h_version <= ZMachine.V3)
                Screen.z_show_status();

            /* Read input from current input stream */

            key = Stream.stream_read_input(
            max, buffer,		/* buffer and size */
            Process.zargs[2],		/* timeout value   */
            Process.zargs[3],		/* timeout routine */
            true,	     	   	/* enable hot keys */
            main.h_version == ZMachine.V6);	/* no script in V6 */

            if (key == CharCodes.ZC_BAD)
                return;

            /* Perform save_undo for V1 to V4 games */

            if (main.h_version <= ZMachine.V4)
                FastMem.save_undo();

            /* Copy local buffer back to dynamic memory */

            for (i = 0; buffer[i] != 0; i++) {

                if (key == CharCodes.ZC_RETURN) {

                    buffer[i] = Text.unicode_tolower(buffer[i]);

                }

                FastMem.storeb((zword)(Process.zargs[0] + ((main.h_version <= ZMachine.V4) ? 1 : 2) + i), Text.translate_to_zscii(buffer[i]));

            }

            /* Add null character (V1-V4) or write input length into 2nd byte */

            if (main.h_version <= ZMachine.V4)
                FastMem.storeb((zword)(Process.zargs[0] + 1 + i), 0);
            else
                FastMem.storeb((zword)(Process.zargs[0] + 1), (byte)i);

            /* Tokenise line if a token buffer is present */

            if (key == CharCodes.ZC_RETURN && Process.zargs[1] != 0)
                Text.tokenise_line(Process.zargs[0], Process.zargs[1], 0, false);

            /* Store key */

            if (main.h_version >= ZMachine.V5)
                Process.store(Text.translate_to_zscii(key));

        }/* z_read */

        /*
         * z_read_char, read and store a key.
         *
         *	zargs[0] = input device (must be 1)
         *	zargs[1] = timeout in tenths of a second (optional)
         *	zargs[2] = packed address of routine to be called on timeout
         *
         */

        internal static void z_read_char ()
        {
            zword key;

            /* Supply default arguments */

            if (Process.zargc < 2)
            Process.zargs[1] = 0;

            /* Read input from the current input stream */

            key = Stream.stream_read_key (
            Process.zargs[1],	/* timeout value   */
            Process.zargs[2],	/* timeout routine */
            true);  	/* enable hot keys */

            if (key == CharCodes.ZC_BAD)
            return;

            /* Store key */

            Process.store (Text.translate_to_zscii (key));

        }/* z_read_char */

        /*
         * z_read_mouse, write the current mouse status into a table.
         *
         *	zargs[0] = address of table
         *
         */

        internal static void z_read_mouse ()
        {
            zword btn;

            /* Read the mouse position, the last menu click
               and which buttons are down */

            btn = os_.read_mouse ();
            main.hx_mouse_y = main.mouse_y;
            main.hx_mouse_x = main.mouse_x;

            FastMem.storew((zword)(Process.zargs[0] + 0), main.hx_mouse_y);
            FastMem.storew((zword)(Process.zargs[0] + 2), main.hx_mouse_x);
            FastMem.storew((zword)(Process.zargs[0] + 4), btn);		/* mouse button bits */
            FastMem.storew((zword)(Process.zargs[0] + 6), main.menu_selected);	/* menu selection */

        }/* z_read_mouse */
    }
}