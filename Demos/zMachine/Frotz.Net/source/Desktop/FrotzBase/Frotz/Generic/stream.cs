/* stream.c - IO stream implementation
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
    internal static class Stream {


        /*
         * scrollback_char
         *
         * Write a single character to the scrollback buffer.
         *
         */

        internal static void scrollback_char(zword c) {

            if (c == CharCodes.ZC_INDENT) { scrollback_char(' '); scrollback_char(' '); scrollback_char(' '); return; }
            if (c == CharCodes.ZC_GAP) { scrollback_char(' '); scrollback_char(' '); return; }

            os_.scrollback_char(c);
        }/* scrollback_char */

        /*
         * scrollback_word
         *
         * Write a string to the scrollback buffer.
         *
         */

        internal static void scrollback_word(zword[] s) {
            // for (i = 0; s[i] != 0; i++)
            for (int i = 0; i < s.Length; i++) {
                if (s[i] == CharCodes.ZC_NEW_FONT || s[i] == CharCodes.ZC_NEW_STYLE)
                    i++;
                else
                    scrollback_char(s[i]);
            }
        }/* scrollback_word */

        /*
         * scrollback_write_input
         *
         * Send an input line to the scrollback buffer.
         *
         */

        internal static void scrollback_write_input(zword[] buf, zword key) {
            int i;

            for (i = 0; buf[i] != 0; i++)
                scrollback_char(buf[i]);

            if (key == CharCodes.ZC_RETURN)
                scrollback_char('\n');
        }/* scrollback_write_input */

        /*
         * scrollback_erase_input
         *
         * Remove an input line from the scrollback buffer.
         *
         */

        internal static void scrollback_erase_input(zword[] buf) {
            int width;
            int i;

            for (i = 0, width = 0; buf[i] != 0; i++)
                width++;

            os_.scrollback_erase(width);
        }/* scrollback_erase_input */

        /*
         * stream_mssg_on
         *
         * Start printing a "debugging" message.
         *
         */

        internal static void stream_mssg_on ()
        {

            Buffer.flush_buffer ();

            if (main.ostream_screen)
            Screen.screen_mssg_on ();
            if (main.ostream_script && main.enable_scripting)
            Files.script_mssg_on ();

            main.message = true;

        }/* stream_mssg_on */

        /*
         * stream_mssg_off
         *
         * Stop printing a "debugging" message.
         *
         */

        internal static void stream_mssg_off ()
        {

            Buffer.flush_buffer ();

            if (main.ostream_screen)
            Screen.screen_mssg_off ();
            if (main.ostream_script && main.enable_scripting)
            Files.script_mssg_off ();

            main.message = false;

        }/* stream_mssg_off */

        /*
         * z_output_stream, open or close an output stream.
         *
         *	zargs[0] = stream to open (positive) or close (negative)
         *	zargs[1] = address to redirect output to (stream 3 only)
         *	zargs[2] = width of redirected output (stream 3 only, optional)
         *
         */

        internal static void z_output_stream() {

            Buffer.flush_buffer();

            switch ((short)Process.zargs[0]) {

                case 1: main.ostream_screen = true;
                    break;
                case -1: main.ostream_screen = false;
                    break;
                case 2: if (!main.ostream_script) Files.script_open();
                    break;
                case -2: if (main.ostream_script) Files.script_close();
                    break;
                case 3: Redirect.memory_open(Process.zargs[1], Process.zargs[2], Process.zargc >= 3);
                    break;
                case -3: Redirect.memory_close();
                    break;
                case 4: if (!main.ostream_record) Files.record_open();
                    break;
                case -4: if (main.ostream_record) Files.record_close();
                    break;

            }

        }/* z_output_stream */

        /*
         * stream_char
         *
         * Send a single character to the output stream.
         *
         */

        internal static void stream_char(zword c) {
            if (main.ostream_screen)
                Screen.screen_char(c);
            if (main.ostream_script && main.enable_scripting)
                Files.script_char(c);
            if (main.enable_scripting)
                scrollback_char(c);

        }/* stream_char */

        /*
         * stream_word
         *
         * Send a string of characters to the output streams.
         *
         */

        internal static void stream_word(zword[] s) {

            if (main.ostream_memory && !main.message)

                Redirect.memory_word(s);

            else {

                if (main.ostream_screen)
                    Screen.screen_word(s);
                if (main.ostream_script && main.enable_scripting)
                    Files.script_word(s);
                if (main.enable_scripting)
                    Stream.scrollback_word(s);
            }
        }/* stream_word */

        /*
         * stream_new_line
         *
         * Send a newline to the output streams.
         *
         */

        internal static void stream_new_line() {

            if (main.ostream_memory && !main.message)

                Redirect.memory_new_line();

            else {

                if (main.ostream_screen)
                    Screen.screen_new_line();
                if (main.ostream_script && main.enable_scripting)
                    Files.script_new_line();
                if (main.enable_scripting)
                    os_.scrollback_char('\n');

            }
        }/* stream_new_line */

        /*
         * z_input_stream, select an input stream.
         *
         *	zargs[0] = input stream to be selected
         *
         */

        internal static void z_input_stream ()
        {

            Buffer.flush_buffer ();

            if (Process.zargs[0] == 0 && main.istream_replay)
            Files.replay_close ();
            if (Process.zargs[0] == 1 && !main.istream_replay)
            Files.replay_open ();

        }/* z_input_stream */

        /*
         * stream_read_key
         *
         * Read a single keystroke from the current input stream.
         *
         */

        internal static zword stream_read_key(zword timeout, zword routine,
                    bool hot_keys) {
            zword key = CharCodes.ZC_BAD;

            Buffer.flush_buffer();

            /* Read key from current input stream */

        continue_input:

            do {

                if (main.istream_replay)
                    key = Files.replay_read_key();
                else
                    key = Screen.console_read_key(timeout);

            } while (key == CharCodes.ZC_BAD);

            /* Verify mouse clicks */

            if (key == CharCodes.ZC_SINGLE_CLICK || key == CharCodes.ZC_DOUBLE_CLICK)
                if (!Screen.validate_click())
                    goto continue_input;

            /* Copy key to the command file */

            if (main.ostream_record && !main.istream_replay)
                Files.record_write_key(key);

            /* Handle timeouts */

            if (key == CharCodes.ZC_TIME_OUT)
                if (Process.direct_call(routine) == 0)
                    goto continue_input;

            /* Handle hot keys */

            if (hot_keys && key >= CharCodes.ZC_HKEY_MIN && key <= CharCodes.ZC_HKEY_MAX) {

                if (main.h_version == ZMachine.V4 && key == CharCodes.ZC_HKEY_UNDO)
                    goto continue_input;
                if (!Hotkey.handle_hot_key(key))
                    goto continue_input;

            }

            /* Return key */

            return key;
        }/* stream_read_key */

        /*
         * stream_read_input
         *
         * Read a line of input from the current input stream.
         *
         */

        internal static zword stream_read_input(int max, zword[] buf,// zword* buf,
                      zword timeout, zword routine,
                      bool hot_keys,
                      bool no_scripting) {
            zword key = CharCodes.ZC_BAD;
            bool no_scrollback = no_scripting;

            if (main.h_version == ZMachine.V6 && main.story_id == Story.UNKNOWN && !main.ostream_script)
                no_scrollback = false;

            Buffer.flush_buffer();

            /* Remove initial input from the transscript file or from the screen */

            if (main.ostream_script && main.enable_scripting && !no_scripting)
                Files.script_erase_input(buf);
            if (main.enable_scripting && !no_scrollback)
                Stream.scrollback_erase_input(buf);
            if (main.istream_replay)
                Screen.screen_erase_input(buf);

            /* Read input line from current input stream */

        continue_input:

            do {

                if (main.istream_replay)
                    key = Files.replay_read_input(buf);
                else
                    key = Screen.console_read_input(max, buf, timeout, key != CharCodes.ZC_BAD);

            } while (key == CharCodes.ZC_BAD);

            /* Verify mouse clicks */

            if (key == CharCodes.ZC_SINGLE_CLICK || key == CharCodes.ZC_DOUBLE_CLICK)
                if (!Screen.validate_click())
                    goto continue_input;

            /* Copy input line to the command file */

            if (main.ostream_record && !main.istream_replay)
                Files.record_write_input(buf, key);

            /* Handle timeouts */

            if (key == CharCodes.ZC_TIME_OUT)
                if (Process.direct_call(routine) == 0)
                    goto continue_input;

            /* Handle hot keys */

            if (hot_keys && key >= CharCodes.ZC_HKEY_MIN && key <= CharCodes.ZC_HKEY_MAX) {

                if (!Hotkey.handle_hot_key(key))
                    goto continue_input;

                return CharCodes.ZC_BAD;
            }

            /* Copy input line to transscript file or to the screen */

            if (main.ostream_script && main.enable_scripting && !no_scripting)
                Files.script_write_input(buf, key);
            if (main.enable_scripting && !no_scrollback)
                scrollback_write_input(buf, key);
            if (main.istream_replay)
                Screen.screen_write_input(buf, key);

            /* Return terminating key */

            return key;

        }/* stream_read_input */
    }
}