/* table.c - Table handling opcodes
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

namespace Frotz.Generic {
    internal static class Table {

        /*
         * z_copy_table, copy a table or fill it with zeroes.
         *
         *	zargs[0] = address of table
         * 	zargs[1] = destination address or 0 for fill
         *	zargs[2] = size of table
         *
         * Note: Copying is safe even when source and destination overlap; but
         *       if zargs[1] is negative the table _must_ be copied forwards.
         *
         */

        internal static void z_copy_table ()
        {
            zword addr;
            zword size = Process.zargs[2];
            zbyte value;
            int i;

            if (Process.zargs[1] == 0)      				/* zero table */

            for (i = 0; i < size; i++)
                FastMem.storeb ((zword) (Process.zargs[0] + i), 0);

            else if ((short) size < 0 || Process.zargs[0] > Process.zargs[1])	/* copy forwards */

            for (i = 0; i < (((short) size < 0) ? - (short) size : size); i++) {
                addr = (zword)(Process.zargs[0] + i);
                FastMem.LOW_BYTE (addr, out value);
                FastMem.storeb ((zword) (Process.zargs[1] + i), value);
            }

            else						/* copy backwards */

            for (i = size - 1; i >= 0; i--) {
                addr = (zword)(Process.zargs[0] + i);
                FastMem.LOW_BYTE(addr, out value);
                FastMem.storeb ((zword) (Process.zargs[1] + i), value);
            }

        }/* z_copy_table */

        /*
         * z_loadb, store a value from a table of bytes.
         *
         *	zargs[0] = address of table
         *	zargs[1] = index of table entry to store
         *
         */

        internal static void z_loadb() {
            zword addr = (zword)(Process.zargs[0] + Process.zargs[1]);
            zbyte value;

            FastMem.LOW_BYTE(addr, out value);

            Process.store(value);

        }/* z_loadb */

        /*
         * z_loadw, store a value from a table of words.
         *
         *	zargs[0] = address of table
         *	zargs[1] = index of table entry to store
         *
         */

        internal static void z_loadw() {
            zword addr = (zword)(Process.zargs[0] + 2 * Process.zargs[1]);
            zword value;

            FastMem.LOW_WORD(addr, out value);

            Process.store(value);

        }/* z_loadw */

        /*
         * z_scan_table, find and store the address of a target within a table.
         *
         *	zargs[0] = target value to be searched for
         *	zargs[1] = address of table
         *	zargs[2] = number of table entries to check value against
         *	zargs[3] = type of table (optional, defaults to 0x82)
         *
         * Note: The table is a word array if bit 7 of zargs[3] is set; otherwise
         *       it's a byte array. The lower bits hold the address step.
         *
         */

        internal static void z_scan_table ()
        {
            zword addr = Process.zargs[1];
            int i;

            /* Supply default arguments */

            if (Process.zargc < 4)
            Process.zargs[3] = 0x82;

            /* Scan byte or word array */

            for (i = 0; i < Process.zargs[2]; i++) {

            if ( (Process.zargs[3] & 0x80) > 0) {	/* scan word array */

                zword wvalue;

                FastMem.LOW_WORD(addr, out wvalue);

                if (wvalue == Process.zargs[0])
                goto finished;

            } else {		/* scan byte array */

                zbyte bvalue;

                FastMem.LOW_BYTE (addr, out bvalue);

                if (bvalue == Process.zargs[0])
                goto finished;

            }

            addr += (zword)(Process.zargs[3] & 0x7f);

            }

            addr = 0;

        finished:

            Process.store (addr);
            Process.branch (addr > 0);

        }/* z_scan_table */

        /*
         * z_storeb, write a byte into a table of bytes.
         *
         *	zargs[0] = address of table
         *	zargs[1] = index of table entry
         *	zargs[2] = value to be written
         *
         */

        internal static void z_storeb() {

            FastMem.storeb((zword)(Process.zargs[0] + Process.zargs[1]), (byte)Process.zargs[2]);

        }/* z_storeb */

        /*
         * z_storew, write a word into a table of words.
         *
         *	zargs[0] = address of table
         *	zargs[1] = index of table entry
         *	zargs[2] = value to be written
         *
         */

        internal static void z_storew() {

            FastMem.storew((zword)(Process.zargs[0] + 2 * Process.zargs[1]), Process.zargs[2]);

        }/* z_storew */
    }
}