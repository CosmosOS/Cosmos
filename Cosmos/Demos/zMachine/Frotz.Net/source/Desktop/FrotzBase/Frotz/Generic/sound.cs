/* sound.c - Sound effect function
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

using zbyte = System.Byte;
using zword = System.UInt16;

using Frotz.Constants;

namespace Frotz.Generic
{
    internal static class Sound
    {

        const int EFFECT_PREPARE = 1;
        const int EFFECT_PLAY = 2;
        const int EFFECT_STOP = 3;
        const int EFFECT_FINISH_WITH = 4;

        static int next_sample = 0;
        static int next_volume = 0;

        private static bool locked = false;
        private static bool playing = false;

        static zbyte[] lh_repeats = {
            0x00, 0x00, 0x00, 0x01, 0xff,
            0x00, 0x01, 0x01, 0x01, 0x01,
            0xff, 0x01, 0x01, 0xff, 0x00,
            0xff, 0xff, 0xff, 0xff, 0xff
            };

        /*
         * init_sound
         *
         * Initialize sound variables.
         *
         */

        internal static void init_sound()
        {
            locked = false;
            playing = false;
        }

        /*
         * start_sample
         *
         * Call the IO interface to play a sample.
         *
         */

        static void start_sample(int number, int volume, int repeats, zword eos)
        {

            if (main.story_id == Story.LURKING_HORROR)
                repeats = lh_repeats[number];

            os_.start_sample(number, volume, repeats, eos);

            playing = true;

        }/* start_sample */

        /*
         * start_next_sample
         *
         * Play a sample that has been delayed until the previous sound effect has
         * finished.  This is necessary for two samples in The Lurking Horror that
         * immediately follow other samples.
         *
         */

        static void start_next_sample()
        {

            if (next_sample != 0)
                start_sample(next_sample, next_volume, 0, 0);

            next_sample = 0;
            next_volume = 0;

        }/* start_next_sample */

        /*
         * end_of_sound
         *
         * Call the Z-code routine which was given as the last parameter of
         * a sound_effect call. This function may be called from a hardware
         * interrupt (which requires extremely careful programming).
         *
         */

        internal static void end_of_sound(zword routine)
        {

            playing = false;

            if (!locked)
            {

                if (main.story_id == Story.LURKING_HORROR)
                    start_next_sample();

                Process.direct_call(routine);

            }

        }/* end_of_sound */

        /*
         * z_sound_effect, load / play / stop / discard a sound effect.
         *
         *	zargs[0] = number of bleep (1 or 2) or sample
         *	zargs[1] = operation to perform (samples only)
         *	zargs[2] = repeats and volume (play sample only)
         *	zargs[3] = end-of-sound routine (play sample only, optional)
         *
         * Note: Volumes range from 1 to 8, volume 255 is the default volume.
         *	 Repeats are stored in the high byte, 255 is infinite loop.
         *
         */

        internal static void z_sound_effect()
        {
            zword number = Process.zargs[0];
            zword effect = Process.zargs[1];
            zword volume = Process.zargs[2];

            if (Process.zargc < 1)
                number = 0;
            if (Process.zargc < 2)
                effect = EFFECT_PLAY;
            if (Process.zargc < 3)
                volume = 8;

            if (number >= 3 || number == 0)
            {

                locked = true;

                if (main.story_id == Story.LURKING_HORROR && (number == 9 || number == 16))
                {

                    if (effect == EFFECT_PLAY)
                    {

                        next_sample = number;
                        next_volume = volume;

                        locked = false;

                        if (!playing)
                            start_next_sample();

                    }
                    else locked = false;

                    return;

                }

                playing = false;

                switch (effect)
                {

                    case EFFECT_PREPARE:
                        os_.prepare_sample(number);
                        break;
                    case EFFECT_PLAY:
                        start_sample(number, FastMem.LO(volume), FastMem.HI(volume),
                            (zword)((Process.zargc == 4) ? Process.zargs[3] : 0));
                        break;
                    case EFFECT_STOP:
                        os_.stop_sample(number);
                        break;
                    case EFFECT_FINISH_WITH:
                        os_.finish_with_sample(number);
                        break;

                }

                locked = false;

            }
            else os_.beep(number);

        }/* z_sound_effect */
    }
}