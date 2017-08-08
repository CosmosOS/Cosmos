/* files.c - Transscription, recording and playback
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

namespace Frotz.Generic
{
    internal static class Files
    {
        internal static string script_name = General.DEFAULT_SCRIPT_NAME;
        internal static string command_name = General.DEFAULT_COMMAND_NAME;

        static int script_width = 0;

        static System.IO.StreamWriter sfp = null;
        static System.IO.StreamWriter rfp = null;
        static System.IO.FileStream pfp = null;

        #region Script
        /*
         * script_open
         *
         * Open the transscript file. 'AMFV' makes this more complicated as it
         * turns transscription on/off several times to exclude some text from
         * the transscription file. This wasn't a problem for the original V4
         * interpreters which always sent transscription to the printer, but it
         * means a problem to modern interpreters that offer to open a new file
         * every time transscription is turned on. Our solution is to append to
         * the old transscription file in V1 to V4, and to ask for a new file
         * name in V5+.
         *
         */

        static bool script_valid = false;
        internal static void script_open()
        {
            string new_name = null;

            main.h_flags &= (zword)~ZMachine.SCRIPTING_FLAG;

            if (main.h_version >= ZMachine.V5 || !script_valid)
            {
                if (os_.read_file_name(out new_name, General.DEFAULT_SCRIPT_NAME, FileTypes.FILE_SCRIPT) != true)
                    goto done;

                script_name = new_name;
            }

            if ((sfp = new System.IO.StreamWriter(script_name, true)) != null)
            {

                main.h_flags |= ZMachine.SCRIPTING_FLAG;

                script_valid = true;
                main.ostream_script = true;

                script_width = 0;

                sfp.AutoFlush = true;
            }
            else Text.print_string("Cannot open file\n");

        done:

            FastMem.SET_WORD(ZMachine.H_FLAGS, main.h_flags);
        }/* script_open */

        /*
         * script_close
         *
         * Stop transscription.
         *
         */

        internal static void script_close()
        {

            main.h_flags &= (ushort)~ZMachine.SCRIPTING_FLAG;
            FastMem.SET_WORD(ZMachine.H_FLAGS, main.h_flags);

            sfp.Close();
            main.ostream_script = false;
        }/* script_close */

        /*
         * script_new_line
         *
         * Write a newline to the transscript file.
         *
         */

        internal static void script_new_line()
        {
            sfp.WriteLine();

            script_width = 0;
        }/* script_new_line */

        /*
         * script_char
         *
         * Write a single character to the transscript file.
         *
         */

        internal static void script_char(zword c)
        {
            if (c == CharCodes.ZC_INDENT && script_width != 0)
                c = ' ';

            if (c == CharCodes.ZC_INDENT)
            { script_char(' '); script_char(' '); script_char(' '); return; }
            if (c == CharCodes.ZC_GAP)
            { script_char(' '); script_char(' '); return; }
            if (c > 0xff)
            { script_char('?'); return; }

            sfp.Write((char)c);
            script_width++;
        }/* script_char */

        /*
         * script_word
         *
         * Write a string to the transscript file.
         *
         */

        internal static void script_word(zword[] s)
        {
            int width;
            int i;

            int pos = 0;

            if (s[pos] == CharCodes.ZC_INDENT && script_width != 0)
            {
                script_char(s[pos++]);
            }

            for (i = pos, width = 0; i < s.Length && s[i] != 0; i++)
            {
                if (s[i] == CharCodes.ZC_NEW_STYLE || s[i] == CharCodes.ZC_NEW_FONT)
                    i++;
                else if (s[i] == CharCodes.ZC_GAP)
                    width += 3;
                else if (s[i] == CharCodes.ZC_INDENT)
                    width += 2;
                else
                    width += 1;
            }

            if (main.option_script_cols != 0 && script_width + width > main.option_script_cols)
            {

                if (s[pos] == ' ' || s[pos] == CharCodes.ZC_INDENT || s[pos] == CharCodes.ZC_GAP)
                    pos++;

                script_new_line();
            }

            for (i = pos; i < s.Length && s[i] != 0; i++)

                if (s[i] == CharCodes.ZC_NEW_FONT || s[i] == CharCodes.ZC_NEW_STYLE)
                    i++;
                else
                    script_char(s[i]);
        }/* script_word */

        /*
         * script_write_input
         *
         * Send an input line to the transscript file.
         *
         */

        internal static void script_write_input(zword[] buf, zword key)
        {
            int width;
            int i;

            for (i = 0, width = 0; buf[i] != 0; i++)
                width++;

            if (main.option_script_cols != 0 && script_width + width > main.option_script_cols)
                script_new_line();

            for (i = 0; buf[i] != 0; i++)
                script_char(buf[i]);

            if (key == CharCodes.ZC_RETURN)
                script_new_line();

        }/* script_write_input */

        /*
         * script_erase_input
         *
         * Remove an input line from the transscript file.
         *
         */

        internal static void script_erase_input(zword[] buf)
        {
            int width;
            int i;

            for (i = 0, width = 0; buf[i] != 0; i++)
                width++;

            sfp.BaseStream.SetLength(sfp.BaseStream.Length - width);
            script_width -= width;
        }/* script_erase_input */

        /*
         * script_mssg_on
         *
         * Start sending a "debugging" message to the transscript file.
         *
         */

        internal static void script_mssg_on()
        {

            if (Files.script_width != 0)
                script_new_line();

            script_char(CharCodes.ZC_INDENT);

        }/* script_mssg_on */

        /*
         * script_mssg_off
         *
         * Stop writing a "debugging" message.
         *
         */

        internal static void script_mssg_off()
        {

            script_new_line();

        }/* script_mssg_off */
        #endregion

        #region Record
        /*
         * record_open
         *
         * Open a file to record the player's input.
         *
         */
        internal static void record_open()
        {
            string new_name = null;

            if (os_.read_file_name(out new_name, command_name, FileTypes.FILE_RECORD) == true)
            {
                command_name = new_name;

                if ((rfp = new System.IO.StreamWriter(command_name, false)) != null)
                {
                    main.ostream_record = true;
                    rfp.AutoFlush = true;
                }
                else
                {
                    Text.print_string("Cannot open file\n");
                }
            }
        }/* record_open */

        /*
         * record_close
         *
         * Stop recording the player's input.
         *
         */

        internal static void record_close()
        {

            rfp.Close();
            main.ostream_record = false;

        }/* record_close */

        ///*
        // * record_code
        // *
        // * Helper function for record_char.
        // *
        // */

        private static void record_code(int c, bool force_encoding)
        {

            if (force_encoding || c == '[' || c < 0x20 || c > 0x7e)
            {
                int i;

                rfp.Write('[');

                for (i = 10000; i != 0; i /= 10)
                    if (c >= i || i == 1)
                        rfp.Write((char)('0' + (c / i) % 10));

                rfp.Write(']');

            }
            else rfp.Write((char)c);


        }/* record_code */

        /*
         * record_char
         *
         * Write a character to the command file.
         *
         */

        private static void record_char(zword c)
        {

            if (c != CharCodes.ZC_RETURN)
            {
                if (c < CharCodes.ZC_HKEY_MIN || c > CharCodes.ZC_HKEY_MAX)
                {
                    record_code(Text.translate_to_zscii(c), false);

                    if (c == CharCodes.ZC_SINGLE_CLICK || c == CharCodes.ZC_DOUBLE_CLICK)
                    {
                        record_code(main.mouse_x, true);
                        record_code(main.mouse_y, true);
                    }
                }
                else record_code(1000 + c - CharCodes.ZC_HKEY_MIN, true);
            }

        }/* record_char */

        /*
         * record_write_key
         *
         * Copy a keystroke to the command file.
         *
         */

        internal static void record_write_key(zword key)
        {
            record_char(key);

            rfp.Write('\n');

        }/* record_write_key */

        /*
         * record_write_input
         *
         * Copy a line of input to a command file.
         *
         */

        internal static void record_write_input(zword[] buf, zword key)
        {
            //zword c;

            for (int i = 0; i < buf.Length && buf[i] != 0; i++ )
            {
                record_char(buf[i]);
            }
            
            record_char (key);

            rfp.Write('\n');

        }/* record_write_input */
        #endregion

        #region Replay

        /*
         * replay_open
         *
         * Open a file of commands for playback.
         *
         */

        internal static void replay_open()
        {
            string new_name = null;

            if (os_.read_file_name(out new_name, command_name, FileTypes.FILE_PLAYBACK))
            {
                command_name = new_name;

                if ( (pfp = new System.IO.FileStream(new_name, System.IO.FileMode.Open)) != null) {

                    Screen.set_more_prompts (Input.read_yes_or_no ("Do you want MORE prompts"));

                    main.istream_replay = true;

                } else Text.print_string ("Cannot open file\n");

            }


        }/* replay_open */

        /*
         * replay_close
         *
         * Stop playback of commands.
         *
         */

        internal static void replay_close()
        {

            Screen.set_more_prompts(true);

            pfp.Close();

            main.istream_replay = false;

        }/* replay_close */

        /*
         * replay_code
         *
         * Helper function for replay_key and replay_line.
         *
         */

        static int replay_code ()
        {
            int c;


            if ( (c = pfp.ReadByte()) == '[') {

            int c2;

            c = 0;

            while ((c2 = pfp.ReadByte()) != -1 && c2 >= '0' && c2 <= '9')
                c = 10 * c + c2 - '0';

            return (c2 == ']') ? c : -1;

            } else return c;

        }/* replay_code */

        /*
         * replay_char
         *
         * Read a character from the command file.
         *
         */

        static zword replay_char ()
        {
            int c;

            if ((c = replay_code ()) != -1) {

            if (c != '\n') {

                if (c < 1000)
                {

                    c = Text.translate_from_zscii((byte)c);

                    if (c == CharCodes.ZC_SINGLE_CLICK || c == CharCodes.ZC_DOUBLE_CLICK)
                    {
                        main.mouse_x = (zword)replay_code();
                        main.mouse_y = (zword)replay_code();
                    }

                    return (zword)c;

                }
                else return (zword)(CharCodes.ZC_HKEY_MIN + c - 1000);
            }

            pfp.Position--;
            pfp.WriteByte((byte)'\n');

            return CharCodes.ZC_RETURN;

            } else return CharCodes.ZC_BAD;

        }/* replay_char */

        /*
         * replay_read_key
         *
         * Read a keystroke from a command file.
         *
         */

        internal static zword replay_read_key()
        {
            zword key = replay_char();

            if (pfp.ReadByte() != '\n')
            {

                replay_close();
                return CharCodes.ZC_BAD;

            }
            else return key;
        
        }/* replay_read_key */

        /*
         * replay_read_input
         *
         * Read a line of input from a command file.
         *
         */

        internal static zword replay_read_input(zword[] buf)
        {
            zword c;

            int pos = 0;

            for (; ; )
            {

                c = replay_char();

                if (c == CharCodes.ZC_BAD || Input.is_terminator(c))
                    break;

                buf[pos++] = c;
            }

            pos = 0;

            //if ( pfp.ReadByte() != '\n') {
            //    replay_close();
            //    return CharCodes.ZC_BAD;

            //} else return c;

            return c;

        }/* replay_read_input */
        #endregion
    }
}