/* buffer.c - Text buffering and word wrapping
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
    internal static class Buffer {
        internal static zword[] buffer_var; // TODO Was a zword
        internal static int bufpos = 0;
        internal static bool locked = false;

        internal static zword prev_c = 0;

        /*
         * init_buffer
         *
         * Initialize buffer variables.
         *
         */

        internal static void init_buffer()
        {
            buffer_var = new zword[General.TEXT_BUFFER_SIZE];
            bufpos = 0;
            prev_c = 0;
            locked = false;
        }

        ///*
        // * flush_buffer
        // *
        // * Copy the contents of the text buffer to the output streams.
        // *
        // */

        internal static void flush_buffer ()
        {
            /* Make sure we stop when flush_buffer is called from flush_buffer.
               Note that this is difficult to avoid as we might print a newline
               during flush_buffer, which might cause a newline interrupt, that
               might execute any arbitrary opcode, which might flush the buffer. */

            if (locked || bufpos == 0)
            return;

            /* Send the buffer to the output streams */

            buffer_var[bufpos] = '\0';
            locked = true; Stream.stream_word (buffer_var); locked = false;

            /* Reset the buffer */

            bufpos = 0;
            prev_c = 0;

        }/* flush_buffer */

        /*
         * print_char
         *
         * High level output function.
         *
         */

        static bool print_char_flag = false;
        internal static void print_char(zword c)
        {
            if (main.message || main.ostream_memory || main.enable_buffering) {

                if (!print_char_flag) {

                    /* Characters 0 and ZC_RETURN are special cases */

                    if (c == CharCodes.ZC_RETURN) { new_line(); return; }
                    if (c == 0)
                        return;

                    /* Flush the buffer before a whitespace or after a hyphen */

                    if (c == ' ' || c == CharCodes.ZC_INDENT || c == CharCodes.ZC_GAP || (prev_c == '-' && c != '-'))


                        flush_buffer();

                    /* Set the flag if this is part one of a style or font change */

                    if (c == CharCodes.ZC_NEW_FONT || c == CharCodes.ZC_NEW_STYLE)
                        print_char_flag = true;

                    /* Remember the current character code */

                    prev_c = c;

                } else print_char_flag = false;

                /* Insert the character into the buffer */

                buffer_var[bufpos++] = (char)c;

                if (bufpos == General.TEXT_BUFFER_SIZE)
                    Err.runtime_error(ErrorCodes.ERR_TEXT_BUF_OVF);

            } else Stream.stream_char (c);

        }/* print_char */

        /*
         * new_line
         *
         * High level newline function.
         *
         */

        internal static void new_line ()
        {

            flush_buffer (); Stream.stream_new_line ();

        }/* new_line */
    }
}