/* hotkey.c - Hot key functions
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
    internal static class Hotkey {

        /*
         * hot_key_debugging
         *
         * ...allows user to toggle cheating options on/off.
         *
         */

        internal static bool hot_key_debugging ()
        {

            Text.print_string ("Debugging options\n");

            main.option_attribute_assignment = Input.read_yes_or_no("Watch attribute assignment");
            main.option_attribute_testing = Input.read_yes_or_no("Watch attribute testing");
            main.option_object_movement = Input.read_yes_or_no("Watch object movement");
            main.option_object_locating = Input.read_yes_or_no("Watch object locating");

            return false;

        }/* hot_key_debugging */

        /*
         * hot_key_help
         *
         * ...displays a list of all hot keys.
         *
         */

        static bool hot_key_help () {

            Text.print_string ("Help\n");

            Text.print_string (
            "\n" +
            "Alt-D  debugging options\n" +
            "Alt-H  help\n" +
            "Alt-N  new game\n" +
            "Alt-P  playback on\n" +
            "Alt-R  recording on/off\n" +
            "Alt-S  seed random numbers\n" +
            "Alt-U  undo one turn\n" +
            "Alt-X  exit game\n");

            return false;

        }/* hot_key_help */

        /*
         * hot_key_playback
         *
         * ...allows user to turn playback on.
         *
         */

        internal static bool hot_key_playback ()
        {

            Text.print_string ("Playback on\n");

            if (!main.istream_replay)
            Files.replay_open ();

            return false;

        }/* hot_key_playback */

        /*
         * hot_key_recording
         *
         * ...allows user to turn recording on/off.
         *
         */

        internal static bool hot_key_recording ()
        {

            if (main.istream_replay) {
            Text.print_string ("Playback off\n");
            Files.replay_close ();
            } else if (main.ostream_record) {
            Text.print_string ("Recording off\n");
            Files.record_close ();
            } else {
            Text.print_string ("Recording on\n");
            Files.record_open ();
            }

            return false;

        }/* hot_key_recording */

        /*
         * hot_key_seed
         *
         * ...allows user to seed the random number seed.
         *
         */

        internal static bool hot_key_seed ()
        {

            Text.print_string ("Seed random numbers\n");

            Text.print_string ("Enter seed value (or return to randomize): ");
            Random.seed_random (Input.read_number ());

            return false;

        }/* hot_key_seed */

        /*
         * hot_key_undo
         *
         * ...allows user to undo the previous turn.
         *
         */

        internal static bool hot_key_undo ()
        {

            Text.print_string ("Undo one turn\n");

            if (FastMem.restore_undo () > 0) {

            if (main.h_version >= ZMachine.V5) {		/* for V5+ games we must */
                Process.store (2);			/* store 2 (for success) */
                return true;		/* and abort the input   */
            }

            if (main.h_version <= ZMachine.V3) {		/* for V3- games we must */
                Screen.z_show_status ();		/* draw the status line  */
                return false;		/* and continue input    */
            }

            } else Text.print_string ("No more undo information available.\n");

            return false;

        }/* hot_key_undo */

        /*
         * hot_key_restart
         *
         * ...allows user to start a new game.
         *
         */

        internal static bool hot_key_restart ()
        {

            Text.print_string ("New game\n");

            if (Input.read_yes_or_no ("Do you wish to restart")) {

            FastMem.z_restart ();
            return true;

            } else return false;

        }/* hot_key_restart */

        /*
         * hot_key_quit
         *
         * ...allows user to exit the game.
         *
         */

        static bool hot_key_quit ()
        {

            Text.print_string ("Exit game\n");

            if (Input.read_yes_or_no ("Do you wish to quit")) {

            Process.z_quit ();
            return true;

            } else return false;

        }/* hot_key_quit */

        /*
         * handle_hot_key
         *
         * Perform the action associated with a so-called hot key. Return
         * true to abort the current input action.
         *
         */

        public static bool handle_hot_key(zword key)
        {

            if (main.cwin == 0)
            {

                bool aborting;

                aborting = false;

                Text.print_string("\nHot key -- ");

                switch (key)
                {
                    case CharCodes.ZC_HKEY_RECORD: aborting = hot_key_recording(); break;
                    case CharCodes.ZC_HKEY_PLAYBACK: aborting = hot_key_playback(); break;
                    case CharCodes.ZC_HKEY_SEED: aborting = hot_key_seed(); break;
                    case CharCodes.ZC_HKEY_UNDO: aborting = hot_key_undo(); break;
                    case CharCodes.ZC_HKEY_RESTART: aborting = hot_key_restart(); break;
                    case CharCodes.ZC_HKEY_QUIT: aborting = hot_key_quit(); break;
                    case CharCodes.ZC_HKEY_DEBUG: aborting = hot_key_debugging(); break;
                    case CharCodes.ZC_HKEY_HELP: aborting = hot_key_help(); break;
                }

                if (aborting)
                    return false;

                Text.print_string("\nContinue input...\n");

            }

            return false;

        }/* handle_hot_key */
    }
}