/* variable.c - Variable and stack related opcodes
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
    internal static class Variable {

        /*
         * z_dec, decrement a variable.
         *
         * 	zargs[0] = variable to decrement
         *
         */

        internal static void z_dec() {
            zword value;

            if (Process.zargs[0] == 0)
                (main.stack[main.sp])--;
            else if (Process.zargs[0] < 16)
                (main.stack[main.fp - Process.zargs[0]])--;
            else {
                zword addr = (zword)(main.h_globals + 2 * (Process.zargs[0] - 16));
                FastMem.LOW_WORD(addr, out value);
                value--;
                FastMem.SET_WORD(addr, value);
            }

        }/* z_dec */

        /*
         * z_dec_chk, decrement a variable and branch if now less than value.
         *
         * 	zargs[0] = variable to decrement
         * 	zargs[1] = value to check variable against
         *
         */

        internal static void z_dec_chk() {
            zword value;

            if (Process.zargs[0] == 0)
                value = --(main.stack[main.sp]);
            else if (Process.zargs[0] < 16)
                value = --(main.stack[main.fp - Process.zargs[0]]);
            else {
                zword addr = (zword)(main.h_globals + 2 * (Process.zargs[0] - 16));
                FastMem.LOW_WORD(addr, out value);
                value--;
                FastMem.SET_WORD(addr, value);
            }

            Process.branch((short)value < (short)Process.zargs[1]);

        }/* z_dec_chk */

        /*
         * z_inc, increment a variable.
         *
         * 	zargs[0] = variable to increment
         *
         */

        internal static void z_inc() {
            zword value;

            if (Process.zargs[0] == 0)
                main.stack[main.sp]++; // (*sp)++;
            else if (Process.zargs[0] < 16)
                (main.stack[main.fp - Process.zargs[0]])++;
            else {
                zword addr = (zword)(main.h_globals + 2 * (Process.zargs[0] - 16));
                FastMem.LOW_WORD(addr, out value);
                value++;
                FastMem.SET_WORD(addr, value);
            }

        }/* z_inc */

        /*
         * z_inc_chk, increment a variable and branch if now greater than value.
         *
         * 	zargs[0] = variable to increment
         * 	zargs[1] = value to check variable against
         *
         */

        internal static void z_inc_chk() {
            zword value;

            if (Process.zargs[0] == 0)
                value = ++(main.stack[main.sp]);
            else if (Process.zargs[0] < 16)
                value = ++(main.stack[main.fp - Process.zargs[0]]);
            else {
                zword addr = (zword)(main.h_globals + 2 * (Process.zargs[0] - 16));
                FastMem.LOW_WORD(addr, out value);
                value++;
                FastMem.SET_WORD(addr, value);
            }

            Process.branch((short)value > (short)Process.zargs[1]);

        }/* z_inc_chk */

        /*
         * z_load, store the value of a variable.
         *
         *	zargs[0] = variable to store
         *
         */

        internal static void z_load ()
        {
            zword value;

            if (Process.zargs[0] == 0)
            value = main.stack[main.sp];
            else if (Process.zargs[0] < 16)
            value = main.stack[main.fp - Process.zargs[0]];
            else {
                zword addr = (zword)(main.h_globals + 2 * (Process.zargs[0] - 16));
            FastMem.LOW_WORD(addr, out value);
            }

            Process.store (value);

        }/* z_load */

        /*
         * z_pop, pop a value off the game stack and discard it.
         *
         *	no zargs used
         *
         */

        internal static void z_pop() {
            main.sp++;

        }/* z_pop */

        /*
         * z_pop_stack, pop n values off the game or user stack and discard them.
         *
         *	zargs[0] = number of values to discard
         *	zargs[1] = address of user stack (optional)
         *
         */

        internal static void z_pop_stack ()
        {

            if (Process.zargc == 2) {		/* it's a user stack */

            zword size;
            zword addr = Process.zargs[1];

            FastMem.LOW_WORD(addr, out size);

            size += Process.zargs[0];
            FastMem.storew (addr, size);

            } else main.sp += Process.zargs[0];	/* it's the game stack */

        }/* z_pop_stack */

        /*
         * z_pull, pop a value off...
         *
         * a) ...the game or a user stack and store it (V6)
         *
         *	zargs[0] = address of user stack (optional)
         *
         * b) ...the game stack and write it to a variable (other than V6)
         *
         *	zargs[0] = variable to write value to
         *
         */

        internal static void z_pull() {
            zword value;

            if (main.h_version != ZMachine.V6) {	/* not a V6 game, pop stack and write */

                value = main.stack[main.sp++];

                if (Process.zargs[0] == 0)
                    main.stack[main.sp] = value;
                else if (Process.zargs[0] < 16)
                    // *(fp - Process.zargs[0]) = value;
                    main.stack[main.fp - Process.zargs[0]] = value;
                else {
                    zword addr = (zword)(main.h_globals + 2 * (Process.zargs[0] - 16));
                    FastMem.SET_WORD(addr, value);
                }

            } else {			/* it's V6, but is there a user stack? */

                if (Process.zargc == 1) {	/* it's a user stack */

                    zword size;
                    zword addr = Process.zargs[0];

                    FastMem.LOW_WORD(addr, out size);

                    size++;
                    FastMem.storew(addr, size);

                    addr += (zword)(2 * size);
                    FastMem.LOW_WORD(addr, out value);

                } else value = main.stack[main.sp++];// value = *sp++;	/* it's the game stack */

                Process.store(value);

            }

        }/* z_pull */

        /*
         * z_push, push a value onto the game stack.
         *
         *	zargs[0] = value to push onto the stack
         *
         */

        internal static void z_push() {

            // *--sp = zargs[0];
            main.stack[--main.sp] = Process.zargs[0];

        }/* z_push */

        /*
         * z_push_stack, push a value onto a user stack then branch if successful.
         *
         *	zargs[0] = value to push onto the stack
         *	zargs[1] = address of user stack
         *
         */

        internal static void z_push_stack ()
        {
            zword size;
            zword addr = Process.zargs[1];

            FastMem.LOW_WORD(addr, out size);

            if (size != 0) {

            FastMem.storew ((zword) (addr + 2 * size), Process.zargs[0]);

            size--;
            FastMem.storew (addr, size);

            }

            Process.branch (size > 0); // TODO I think that's what's expected here

        }/* z_push_stack */

        /*
         * z_store, write a value to a variable.
         *
         * 	zargs[0] = variable to be written to
         *      zargs[1] = value to write
         *
         */

        internal static void z_store() {
            zword value = Process.zargs[1];

            if (Process.zargs[0] == 0)
                main.stack[main.sp] = value;
            else if (Process.zargs[0] < 16)
                main.stack[main.fp - Process.zargs[0]] = value;
            else {
                zword addr = (zword)(main.h_globals + 2 * (Process.zargs[0] - 16));
                FastMem.SET_WORD(addr, value);
            }

        }/* z_store */
    }
}