/* random.c - Z-machine random number generator
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

using Frotz;
using Frotz.Constants;

namespace Frotz.Generic {
    internal static class Random {
        static int  A = 1; // TODO This was a long originally

        static int interval = 0;
        static int counter = 0;

        /*
         * seed_random
         *         * Set the seed value for the random number generator.
         *
         */

        internal static void seed_random (int value)
        {

            if (value == 0) {		/* ask interface for seed value */
            A = os_.random_seed ();
            interval = 0;
            } else if (value < 1000) {	/* special seed value */
            counter = 0;
            interval = value;
            } else {			/* standard seed value */
            A = value;
            interval = 0;
            }

        }/* seed_random */

        /*
         * z_random, store a random number or set the random number seed.
         *
         *	zargs[0] = range (positive) or seed value (negative)
         *
         */

        internal static void z_random() {

            if ((short)Process.zargs[0] <= 0) {	/* set random seed */

                seed_random(-(short)Process.zargs[0]);
                Process.store(0);

            } else {				/* generate random number */

                zword result;

                if (interval != 0) {		/* ...in special mode */
                    result = (zword)counter++;
                    if (counter == interval) counter = 0;
                } else {			/* ...in standard mode */
                    A = 0x015a4e35 * A + 1;
                    result = (zword)((A >> 16) & 0x7fff);
                }

                Process.store((zword)(result % Process.zargs[0] + 1));

            }

        }/* z_random */
    }
}