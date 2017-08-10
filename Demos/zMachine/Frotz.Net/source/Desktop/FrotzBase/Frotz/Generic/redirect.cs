/* redirect.c - Output redirection to Z-machine memory
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

using System;

using Frotz;
using Frotz.Constants;

namespace Frotz.Generic {
    internal static class Redirect {

        private static byte MAX_NESTING = 16;

        static int depth = -1;

        struct RedirectStruct {
            internal zword xsize;
            internal zword table;
            internal zword width;
            internal zword total;
        };
        static RedirectStruct[] redirect = new RedirectStruct[MAX_NESTING];

        /*
         * memory_open
         *
         * Begin output redirection to the memory of the Z-machine.
         *
         */

        internal static void memory_open(zword table, zword xsize, bool buffering) {

            if (++depth < MAX_NESTING) {

                if (!buffering)
                    xsize = 0xffff;
                if (buffering && (short)xsize <= 0)
                    xsize = Screen.get_max_width((zword)(-(short)xsize));

                FastMem.storew(table, 0);

                redirect[depth].table = table;
                redirect[depth].width = 0;
                redirect[depth].total = 0;
                redirect[depth].xsize = xsize;

                main.ostream_memory = true;

            } else Err.runtime_error(ErrorCodes.ERR_STR3_NESTING);

        }/* memory_open */

        /*
         * memory_new_line
         *
         * Redirect a newline to the memory of the Z-machine.
         *
         */

        internal static void memory_new_line ()
        {
            zword size;
            zword addr;

            redirect[depth].total += redirect[depth].width;
            redirect[depth].width = 0;

            addr = redirect[depth].table;

            FastMem.LOW_WORD(addr, out size);
            addr += 2;

            if (redirect[depth].xsize != 0xffff) {

                redirect[depth].table = (zword)(addr + size);
            size = 0;

            } else FastMem.storeb ((zword) (addr + (size++)), 13);

            FastMem.storew (redirect[depth].table, size);
        }/* memory_new_line */

        /*
         * memory_word
         *
         * Redirect a string of characters to the memory of the Z-machine.
         *
         */

        internal static void memory_word (zword[] s) 
        {
            zword size;
            zword addr;
            zword c;

            int pos = 0;

            if (main.h_version == ZMachine.V6) {

            int width = os_.string_width (s);

            if (redirect[depth].xsize != 0xffff)

                if (redirect[depth].width + width > redirect[depth].xsize) {

                if (s[pos] == ' ' || s[pos] == CharCodes.ZC_INDENT || s[pos] == CharCodes.ZC_GAP)
                    width = os_.string_width (s, ++pos);

                memory_new_line ();

                }

            redirect[depth].width += (zword)width;

            }

            addr = redirect[depth].table;

            FastMem.LOW_WORD(addr, out size);
            addr += 2;

            while ((c = s[pos++]) != 0)
            FastMem.storeb ((zword) (addr + (size++)), Text.translate_to_zscii (c));

            FastMem.storew(redirect[depth].table, size);

        }/* memory_word */

        /*
         * memory_close
         *
         * End of output redirection.
         *
         */

        internal static void memory_close ()
        {

            if (depth >= 0) {

            if (redirect[depth].xsize != 0xffff)
                memory_new_line ();

            if (main.h_version == ZMachine.V6) {

                main.h_line_width = (redirect[depth].xsize != 0xffff) ?
                redirect[depth].total : redirect[depth].width;

                FastMem.SET_WORD(ZMachine.H_LINE_WIDTH, main.h_line_width);

            }

            if (depth == 0)
                main.ostream_memory = false;

            depth--;

            }

        }/* memory_close */
    }
}