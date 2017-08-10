/* math.c - Arithmetic, compare and logical opcodes
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
    internal static class Math {
        /*
         * z_add, 16bit addition.
         *
         *	zargs[0] = first value
         *	zargs[1] = second value
         *
         */

        internal static void z_add() {

            Process.store((zword)((short)Process.zargs[0] + (short)Process.zargs[1]));

        }/* z_add */

        /*
         * z_and, bitwise AND operation.
         *
         *	zargs[0] = first value
         *	zargs[1] = second value
         *
         */

        internal static void z_and() {

            Process.store((zword)(Process.zargs[0] & Process.zargs[1]));

        }/* z_and */

        /*
         * z_art_shift, arithmetic SHIFT operation.
         *
         *	zargs[0] = value
         *	zargs[1] = #positions to shift left (positive) or right
         *
         */

        internal static void z_art_shift() {
            // TODO This code has never been hit... I need to find something that will hit it
            if ((short)Process.zargs[1] > 0)
                Process.store((zword)((short)Process.zargs[0] << (short)Process.zargs[1]));
            else
                Process.store((zword)((short)Process.zargs[0] >> -(short)Process.zargs[1]));

        }/* z_art_shift */

        /*
         * z_div, signed 16bit division.
         *
         *	zargs[0] = first value
         *	zargs[1] = second value
         *
         */

        internal static void z_div() {

            if (Process.zargs[1] == 0)
                Err.runtime_error(ErrorCodes.ERR_DIV_ZERO);

            Process.store((zword)((short)Process.zargs[0] / (short)Process.zargs[1]));

        }/* z_div */

        /*
         * z_je, branch if the first value equals any of the following.
         *
         *	zargs[0] = first value
         *	zargs[1] = second value (optional)
         *	...
         *	zargs[3] = fourth value (optional)
         *
         */

        internal static void z_je() {

            Process.branch(
            Process.zargc > 1 && (Process.zargs[0] == Process.zargs[1] || (
            Process.zargc > 2 && (Process.zargs[0] == Process.zargs[2] || (
            Process.zargc > 3 && (Process.zargs[0] == Process.zargs[3]))))));

        }/* z_je */

        /*
         * z_jg, branch if the first value is greater than the second.
         *
         *	zargs[0] = first value
         *	zargs[1] = second value
         *
         */

        internal static void z_jg() {

            Process.branch((short)Process.zargs[0] > (short)Process.zargs[1]);

        }/* z_jg */

        /*
         * z_jl, branch if the first value is less than the second.
         *
         *	zargs[0] = first value
         *	zargs[1] = second value
         *
         */

        internal static void z_jl() {

            Process.branch((short)Process.zargs[0] < (short)Process.zargs[1]);

        }/* z_jl */

        /*
         * z_jz, branch if value is zero.
         *
         * 	zargs[0] = value
         *
         */

        internal static void z_jz() {

            Process.branch((short)Process.zargs[0] == 0);

        }/* z_jz */

        /*
         * z_log_shift, logical SHIFT operation.
         *
         * 	zargs[0] = value
         *	zargs[1] = #positions to shift left (positive) or right (negative)
         *
         */

        internal static void z_log_shift() {

            if ((short)Process.zargs[1] > 0)
                Process.store((zword)(Process.zargs[0] << (short)Process.zargs[1]));
            else
                Process.store((zword)(Process.zargs[0] >> -(short)Process.zargs[1]));

        }/* z_log_shift */

        /*
         * z_mod, remainder after signed 16bit division.
         *
         * 	zargs[0] = first value
         *	zargs[1] = second value
         *
         */

        internal static void z_mod() {

            if (Process.zargs[1] == 0)
                Err.runtime_error(ErrorCodes.ERR_DIV_ZERO);

            Process.store((zword)((short)Process.zargs[0] % (short)Process.zargs[1]));

        }/* z_mod */

        /*
         * z_mul, 16bit multiplication.
         *
         * 	zargs[0] = first value
         *	zargs[1] = second value
         *
         */

        internal static void z_mul() {

            Process.store((zword)((short)Process.zargs[0] * (short)Process.zargs[1]));

        }/* z_mul */

        /*
         * z_not, bitwise NOT operation.
         *
         * 	zargs[0] = value
         *
         */

        internal static void z_not() {

            Process.store ((zword) ~Process.zargs[0]);

        }/* z_not */

        /*
         * z_or, bitwise OR operation.
         *
         *	zargs[0] = first value
         *	zargs[1] = second value
         *
         */

        internal static void z_or() {

            Process.store((zword)(Process.zargs[0] | Process.zargs[1]));

        }/* z_or */

        /*
         * z_sub, 16bit substraction.
         *
         *	zargs[0] = first value
         *	zargs[1] = second value
         *
         */

        internal static void z_sub() {

            Process.store((zword)((short)Process.zargs[0] - (short)Process.zargs[1]));

        }/* z_sub */

        /*
         * z_test, branch if all the flags of a bit mask are set in a value.
         *
         *	zargs[0] = value to be examined
         *	zargs[1] = bit mask
         *
         */

        internal static void z_test() {

            Process.branch((Process.zargs[0] & Process.zargs[1]) == Process.zargs[1]);

        }/* z_test */
    }
}