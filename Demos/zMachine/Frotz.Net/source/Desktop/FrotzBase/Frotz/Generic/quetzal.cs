/* quetzal.c  - Saving and restoring of Quetzal files.
 *	Written by Martin Frost <mdf@doc.ic.ac.uk>
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
using zlong = System.UInt32;

using System.IO;

using Frotz;
using Frotz.Constants;

namespace Frotz.Generic {
    internal static class Quetzal {


        //typedef unsigned long zlong;

        /*
         * This is used only by save_quetzal. It probably should be allocated
         * dynamically rather than statically.
         */

        private static zword[] frames = new zword[General.STACK_SIZE / 4 + 1];

        ///*
        // * ID types.
        // */

        //#define makeid(a,b,c,d) 

        // TODO Remove this in favor of zmath
        private static zlong makeid(char a, char b, char c, char d) {
            return ((zlong)(((a) << 24) | ((b) << 16) | ((c) << 8) | (d)));
        }

        private static zlong ID_FORM = makeid('F', 'O', 'R', 'M');
        private static zlong ID_IFZS = makeid('I', 'F', 'Z', 'S');
        private static zlong ID_IFhd = makeid('I', 'F', 'h', 'd');
        private static zlong ID_UMem = makeid('U', 'M', 'e', 'm');
        private static zlong ID_CMem = makeid('C', 'M', 'e', 'm');
        private static zlong ID_Stks = makeid('S', 't', 'k', 's');
        private static zlong ID_ANNO = makeid('A', 'N', 'N', 'O');

        /*
         * Various parsing states within restoration.
         */

        internal static byte GOT_HEADER = 0x01;
        internal static byte GOT_STACK = 0x02;
        internal static byte GOT_MEMORY = 0x04;
        internal static byte GOT_NONE = 0x00;
        internal static byte GOT_ALL = 0x07;
        internal static byte GOT_ERROR = 0x80;

        ///*
        // * Macros used to write the files.
        // */

        private static bool write_run(FileStream fp, byte run) {
            write_byte(fp, 0);
            write_byte(fp, run);
            return true;
        }

        private static bool write_byte(FileStream fp, int b) {
            fp.WriteByte((byte)b);
            return true;
        }

        private static bool write_bytx(FileStream fp, int b) {
            write_byte(fp, (b) & 0xFF);
            return true;
        }

        //private static bool write_word(FileStream fp, zword w) {
        //    write_bytx(fp, (w) >> 8);
        //    write_bytx(fp, (w));
        //    return true;
        //}

        private static bool write_word(FileStream fp, int w) {
            write_bytx(fp, (w) >> 8);
            write_bytx(fp, (w));
            return true;
        }

        private static bool write_bytx(FileStream fp, long b) {
            write_byte(fp, (byte)((b) & 0xFF));
            return true;
        }

        private static bool write_long(FileStream fp, long l) {
            write_bytx(fp, (l) >> 24);
            write_bytx(fp, (l) >> 16);
            write_bytx(fp, (l) >> 8);
            write_bytx(fp, (l));
            return true;
        }

        internal static bool write_chnk(FileStream fs, long id, long len) {
            write_long(fs, id);
            write_long(fs, len);

            return true;
        }


        /* Read one word from file; return TRUE if OK. */
        static bool read_word(FileStream fs, out zword result) {
            int a, b;

            result = zword.MaxValue;

            if ((a = fs.ReadByte()) == -1) return false;
            if ((b = fs.ReadByte()) == -1) return false;


            result = (zword)(((zword)a << 8) | (zword)b);
            return true;
        }

        /* Read one long from file; return TRUE if OK. */
        static bool read_long(FileStream fs, out zlong result) {
            result = zlong.MaxValue;
            int a, b, c, d;
            byte[] buffer = new byte[4];

            if ((a = fs.ReadByte()) == -1) return false;
            if ((b = fs.ReadByte()) == -1) return false;
            if ((c = fs.ReadByte()) == -1) return false;
            if ((d = fs.ReadByte()) == -1) return false;

            // TODO Replace this in favor of zmath
            result = ((zlong)a << 24) | ((zlong)b << 16) |
                  ((zlong)c << 8) | (zlong)d;
            return true;
        }

        /*
         * Restore a saved game using Quetzal format. Return 2 if OK, 0 if an error
         * occurred before any damage was done, -1 on a fatal error.
         */

        internal static zword restore_quetzal(FileStream svf, MemoryStream stf) {
            zlong ifzslen, currlen, tmpl;
            zlong pc;
            zword i, tmpw;
            zword fatal = 0;	/* Set to -1 when errors must be fatal. */
            zbyte skip, progress = GOT_NONE;
            int x, y;

            /* Check it's really an `IFZS' file. */


            if (!read_long(svf, out tmpl)
            || !read_long(svf, out ifzslen)
            || !read_long(svf, out currlen)) return 0;
            if (tmpl != ID_FORM || currlen != ID_IFZS) {
                Text.print_string("This is not a saved game file!\n");
                return 0;
            }
            if (((ifzslen & 1) > 0) || ifzslen < 4) /* Sanity checks. */	return 0;
            ifzslen -= 4;

            /* Read each chunk and process it. */
            while (ifzslen > 0) {
                /* Read chunk header. */
                if (ifzslen < 8) /* Couldn't contain a chunk. */	return 0;
                if (!read_long(svf, out tmpl)
                    || !read_long(svf, out currlen)) return 0;
                ifzslen -= 8;	/* Reduce remaining by size of header. */

                /* Handle chunk body. */
                if (ifzslen < currlen) /* Chunk goes past EOF?! */	return 0;
                skip = (byte)(currlen & 1);
                ifzslen -= currlen + (zlong)skip;

                switch (tmpl) {
                    /* `IFhd' header chunk; must be first in file. */
                    case 1229351012: // IFhd
                        if ((progress & GOT_HEADER) > 0) {
                            Text.print_string("Save file has two IFZS chunks!\n");
                            return fatal;
                        }
                        progress |= GOT_HEADER;
                        if (currlen < 13
                            || !read_word(svf, out tmpw)) return fatal;
                        if (tmpw != main.h_release)
                            progress = GOT_ERROR;

                        for (i = ZMachine.H_SERIAL; i < ZMachine.H_SERIAL + 6; ++i) {
                            if ((x = svf.ReadByte()) == -1) return fatal;
                            if (x != FastMem.ZMData[FastMem.zmp + i])
                                progress = GOT_ERROR;
                        }

                        if (!read_word(svf, out tmpw)) return fatal;
                        if (tmpw != main.h_checksum)
                            progress = GOT_ERROR;

                        if ((progress & GOT_ERROR) > 0) {
                            Text.print_string("File was not saved from this story!\n");
                            return fatal;
                        }
                        if ((x = svf.ReadByte()) == -1) return fatal;
                        pc = (zlong)x << 16;
                        if ((x = svf.ReadByte()) == -1) return fatal;
                        pc |= (zlong)x << 8;
                        if ((x = svf.ReadByte()) == -1) return fatal;
                        pc |= (zlong)x;
                        fatal = zword.MaxValue;	/* Setting PC means errors must be fatal. */ // TODO make sure this works
                        FastMem.SET_PC(pc);

                        for (i = 13; i < currlen; ++i)
                            svf.ReadByte();	/* Skip rest of chunk. */
                        break;
                    /* `Stks' stacks chunk; restoring this is quite complex. ;) */
                    case 1400138611: // ID_Stks:
                        if ((progress & GOT_STACK) > 0) {
                            Text.print_string("File contains two stack chunks!\n");
                            break;
                        }
                        progress |= GOT_STACK;

                        fatal = zword.MaxValue; ;	/* Setting SP means errors must be fatal. */
                        // sp = stack + General.STACK_SIZE;
                        main.sp = main.stack.Length;

                        /*
                         * All versions other than V6 may use evaluation stack outside
                         * any function context. As a result a faked function context
                         * will be present in the file here. We skip this context, but
                         * load the associated stack onto the stack proper...
                         */
                        if (main.h_version != ZMachine.V6) {
                            if (currlen < 8) return fatal;
                            for (i = 0; i < 6; ++i)
                                if (svf.ReadByte() != 0) return fatal;
                            if (!read_word(svf, out tmpw)) return fatal;
                            if (tmpw > General.STACK_SIZE) {
                                Text.print_string("Save-file has too much stack (and I can't cope).\n");
                                return fatal;
                            }
                            currlen -= 8;
                            if (currlen < tmpw * 2) return fatal;
                            for (i = 0; i < tmpw; ++i)
                                // if (!read_word(svf, --sp)) return fatal;
                                if (!read_word(svf, out main.stack[--main.sp])) return fatal;
                            currlen -= (zword)(tmpw * 2);
                        }

                        /* We now proceed to load the main block of stack frames. */
                        for (main.fp = main.stack.Length, main.frame_count = 0;
                             currlen > 0;
                             currlen -= 8, ++main.frame_count) {
                            if (currlen < 8) return fatal;
                            if (main.sp < 4)	/* No space for frame. */ {
                                Text.print_string("Save-file has too much stack (and I can't cope).\n");
                                return fatal;
                            }

                            /* Read PC, procedure flag and formal param count. */
                            if (!read_long(svf, out tmpl)) return fatal;
                            y = (int)(tmpl & 0x0F);	/* Number of formals. */
                            tmpw = (zword)(y << 8);

                            /* Read result variable. */
                            if ((x = svf.ReadByte()) == -1) return fatal;

                            /* Check the procedure flag... */
                            if ((tmpl & 0x10) > 0) {
                                tmpw |= 0x1000;	/* It's a procedure. */
                                tmpl >>= 8;	/* Shift to get PC value. */
                            } else {
                                /* Functions have type 0, so no need to or anything. */
                                tmpl >>= 8;	/* Shift to get PC value. */
                                --tmpl;		/* Point at result byte. */
                                /* Sanity check on result variable... */
                                if (FastMem.ZMData[FastMem.zmp + tmpl] != (zbyte)x) {
                                    Text.print_string("Save-file has wrong variable number on stack (possibly wrong game version?)\n");
                                    return fatal;
                                }
                            }
                        
                            main.stack[--main.sp] = (zword)(tmpl >> 9);	/* High part of PC */
                            main.stack[--main.sp] = (zword)(tmpl & 0x1FF);	/* Low part of PC */
                            main.stack[--main.sp] = (zword)(main.fp - 1);	/* FP */

                            /* Read and process argument mask. */
                            if ((x = svf.ReadByte()) == -1) return fatal;
                            ++x;	/* Should now be a power of 2 */
                            for (i = 0; i < 8; ++i)
                                if ((x & (1 << i)) > 0)
                                    break;
                            if ((x ^ (1 << i)) > 0)	/* Not a power of 2 */ {
                                Text.print_string("Save-file uses incomplete argument lists (which I can't handle)\n");
                                return fatal;
                            }
                            // *--sp = tmpw | i;
                            main.stack[--main.sp] = (zword)(tmpw | i);
                            main.fp = main.sp;	/* FP for next frame. */

                            /* Read amount of eval stack used. */
                            if (!read_word(svf, out tmpw)) return fatal;

                            tmpw += (zword)y;	/* Amount of stack + number of locals. */
                            // if (sp - stack <= tmpw) {
                            if (main.sp <= tmpw) {
                                Text.print_string("Save-file has too much stack (and I can't cope).\n");
                                return fatal;
                            }
                            if (currlen < tmpw * 2) return fatal;
                            for (i = 0; i < tmpw; ++i)
                                if (!read_word(svf, out main.stack[--main.sp])) return fatal;
                            currlen -= (zword)(tmpw * 2);
                        }
                        /* End of `Stks' processing... */
                        break;
                    /* Any more special chunk types must go in HERE or ABOVE. */
                    /* `CMem' compressed memory chunk; uncompress it. */
                    case 1129145709: // CMem
                        if ((progress & GOT_MEMORY) == 0)	/* Don't complain if two. */ {
                            stf.Position = 0; // (void) fseek (stf, 0, SEEK_SET);
                            i = 0;	/* Bytes written to data area. */
                            for (; currlen > 0; --currlen) {
                                if ((x = svf.ReadByte()) == -1) return fatal;
                                if (x == 0)	/* Start run. */ {
                                    /* Check for bogus run. */
                                    if (currlen < 2) {
                                        Text.print_string("File contains bogus `CMem' chunk.\n");
                                        for (; currlen > 0; --currlen)
                                            svf.ReadByte();	/* Skip rest. */
                                        currlen = 1;
                                        i = 0xFFFF;
                                        break; /* Keep going; may be a `UMem' too. */
                                    }
                                    /* Copy story file to memory during the run. */
                                    --currlen;
                                    if ((x = svf.ReadByte()) == -1) return fatal;
                                    for (; x >= 0 && i < main.h_dynamic_size; --x, ++i)
                                        if ((y = stf.ReadByte()) == -1) return fatal;
                                        else
                                            FastMem.ZMData[FastMem.zmp + i] = (zbyte)y;
                                } else	/* Not a run. */ {
                                    if ((y = stf.ReadByte()) == -1) return fatal;
                                    FastMem.ZMData[FastMem.zmp + i] = (zbyte)(x ^ y);
                                    ++i;
                                }
                                /* Make sure we don't load too much. */
                                if (i > main.h_dynamic_size) {
                                    Text.print_string("warning: `CMem' chunk too long!\n");
                                    for (; currlen > 1; --currlen)
                                        svf.ReadByte();	/* Skip rest. */
                                    break;	/* Keep going; there may be a `UMem' too. */
                                }
                            }
                            /* If chunk is short, assume a run. */
                            for (; i < main.h_dynamic_size; ++i)
                                if ((y = stf.ReadByte()) == -1) return fatal;
                                else
                                    FastMem.ZMData[FastMem.zmp + i] = (zbyte)y;
                            if (currlen == 0)
                                progress |= GOT_MEMORY;	/* Only if succeeded. */
                            break;
                        }
                        goto default;
                    /* Fall right thru (to default) if already GOT_MEMORY */
                    /* `UMem' uncompressed memory chunk; load it. */
                    case 1431135597: // ID_UMem:
                        if ((progress & GOT_MEMORY) == 0)	/* Don't complain if two. */ {
                            /* Must be exactly the right size. */
                            if (currlen == main.h_dynamic_size) {
                                byte[] buffer = new byte[currlen];
                                int read = svf.Read(FastMem.ZMData, (int)FastMem.zmp, (int)currlen);
                                if (read == currlen) {
                                    progress |= GOT_MEMORY;	/* Only on success. */
                                    break;
                                }
                                //if (fread(zmp, currlen, 1, svf) == 1) {
                                //}
                            } else
                                Text.print_string("`UMem' chunk wrong size!\n");
                            /* Fall into default action (skip chunk) on errors. */
                        }
                        goto default;
                    /* Fall thru (to default) if already GOT_MEMORY */
                    /* Unrecognised chunk type; skip it. */
                    default:
                        // (void) fseek (svf, currlen, SEEK_CUR);	/* Skip chunk. */
                        svf.Position += currlen;
                        break;
                }
                if (skip > 0)
                    svf.ReadByte();	/* Skip pad byte. */
            }

            /*
             * We've reached the end of the file. For the restoration to have been a
             * success, we must have had one of each of the required chunks.
             */
            if ((progress & GOT_HEADER) == 0)
                Text.print_string("error: no valid header (`IFhd') chunk in file.\n");
            if ((progress & GOT_STACK) == 0)
                Text.print_string("error: no valid stack (`Stks') chunk in file.\n");
            if ((progress & GOT_MEMORY) == 0)
                Text.print_string("error: no valid memory (`CMem' or `UMem') chunk in file.\n");

            return (ushort)(progress == GOT_ALL ? 2 : fatal);
        }

        /*
         * Save a game using Quetzal format. Return 1 if OK, 0 if failed.
         */

        internal static zword save_quetzal(FileStream svf, MemoryStream stf) {
            zlong ifzslen = 0, cmemlen = 0, stkslen = 0;
            long pc;
            zword i, j, n;
            int nvars, nargs, nstk, p;
            zbyte var;
            long cmempos, stkspos;
            int c;

            /* Write `IFZS' header. */
            if (!write_chnk(svf, ID_FORM, 0)) return 0;
            if (!write_long(svf, ID_IFZS)) return 0;

            /* Write `IFhd' chunk. */
            FastMem.GET_PC(out pc);
            if (!write_chnk(svf, ID_IFhd, 13)) return 0;
            if (!write_word(svf, main.h_release)) return 0;
            for (i = ZMachine.H_SERIAL; i < ZMachine.H_SERIAL + 6; ++i)
                if (!write_byte(svf, FastMem.ZMData[FastMem.zmp + i])) return 0;
            if (!write_word(svf, main.h_checksum)) return 0;
            if (!write_long(svf, pc << 8)) /* Includes pad. */	return 0;

            /* Write `CMem' chunk. */
            if ((cmempos = svf.Position) < 0) return 0;
            if (!write_chnk(svf, ID_CMem, 0)) return 0;
            // (void) fseek (stf, 0, SEEK_SET);
            stf.Position = 0;
            /* j holds current run length. */
            for (i = 0, j = 0, cmemlen = 0; i < main.h_dynamic_size; ++i) {
                if ((c = stf.ReadByte()) == -1) return 0;
                c ^= (int)FastMem.ZMData[i];
                if (c == 0)
                    ++j;	/* It's a run of equal bytes. */
                else {
                    /* Write out any run there may be. */
                    if (j > 0) {
                        for (; j > 0x100; j -= 0x100) {
                            if (!write_run(svf, 0xFF)) return 0;
                            cmemlen += 2;
                        }
                        if (!write_run(svf, (byte)(j - 1))) return 0;
                        cmemlen += 2;
                        j = 0;
                    }
                    /* Any runs are now written. Write this (nonzero) byte. */
                    if (!write_byte(svf, (zbyte)c)) return 0;
                    ++cmemlen;
                }
            }
            /*
             * Reached end of dynamic memory. We ignore any unwritten run there may be
             * at this point.
             */
            if ((cmemlen & 1) > 0)	/* Chunk length must be even. */
                if (!write_byte(svf, 0)) return 0;

            /* Write `Stks' chunk. You are not expected to understand this. ;) */
            if ((stkspos = svf.Position) < 0) return 0;
            if (!write_chnk(svf, ID_Stks, 0)) return 0;

            /*
             * We construct a list of frame indices, most recent first, in `frames'.
             * These indices are the offsets into the `stack' array of the word before
             * the first word pushed in each frame.
             */
            frames[0] = (zword)main.sp;	/* The frame we'd get by doing a call now. */
            for (i = (zword)(main.fp + 4), n = 0; i < General.STACK_SIZE + 4; i = (zword)(main.stack[i - 3] + 5))
                frames[++n] = i;

            /*
             * All versions other than V6 can use evaluation stack outside a function
             * context. We write a faked stack frame (most fields zero) to cater for
             * this.
             */
            if (main.h_version != ZMachine.V6) {
                for (i = 0; i < 6; ++i)
                    if (!write_byte(svf, 0)) return 0;
                nstk = General.STACK_SIZE - frames[n];
                if (!write_word(svf, nstk)) return 0;
                for (j = (zword)(General.STACK_SIZE - 1); j >= frames[n]; --j)
                    if (!write_word(svf, main.stack[j])) return 0;
                stkslen = (zword)(8 + 2 * nstk);
            }

            /* Write out the rest of the stack frames. */
            for (i = n; i > 0; --i) {
                p = frames[i] - 4;  // p = stack + frames[i] - 4;	/* Points to call frame. */
                nvars = (main.stack[p] & 0x0F00) >> 8;
                nargs = main.stack[p] & 0x00FF;
                nstk = (zword)(frames[i] - frames[i - 1] - nvars - 4);
                pc = ((zlong)main.stack[p + 3] << 9) | main.stack[p + 2];

                switch (main.stack[p] & 0xF000)	/* Check type of call. */
                {
                    case 0x0000:	/* Function. */
                        var = FastMem.ZMData[FastMem.zmp + pc];
                        pc = ((pc + 1) << 8) | (zlong)nvars;
                        break;
                    case 0x1000:	/* Procedure. */
                        var = 0;
                        pc = (pc << 8) | 0x10 | (zlong)nvars;	/* Set procedure flag. */
                        break;
                    /* case 0x2000: */
                    default:
                        Err.runtime_error(ErrorCodes.ERR_SAVE_IN_INTER);
                        return 0;
                }
                if (nargs != 0)
                    nargs = (zword)((1 << nargs) - 1);	/* Make args into bitmap. */

                /* Write the main part of the frame... */
                if (!write_long(svf, pc)
                    || !write_byte(svf, var)
                    || !write_byte(svf, nargs)
                    || !write_word(svf, nstk)) return 0;

                /* Write the variables and eval stack. */
                for (j = 0, --p; j < nvars + nstk; ++j, --p)
                    if (!write_word(svf, main.stack[p])) return 0;

                /* Calculate length written thus far. */
                stkslen += (zword)(8 + 2 * (nvars + nstk));
            }

            /* Fill in variable chunk lengths. */
            ifzslen = 3 * 8 + 4 + 14 + cmemlen + stkslen;
            if ((cmemlen & 1) > 0)
                ++ifzslen;
            svf.Position = 4;
            if (!write_long(svf, ifzslen)) return 0;
            svf.Position = cmempos + 4;
            if (!write_long(svf, cmemlen)) return 0;
            svf.Position = stkspos + 4;
            if (!write_long(svf, stkslen)) return 0;

            /* After all that, still nothing went wrong! */
            return 1;
        }
    }
}