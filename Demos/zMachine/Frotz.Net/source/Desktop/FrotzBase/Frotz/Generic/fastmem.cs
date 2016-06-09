/* fastmem.c - Memory related functions (fast version without virtual memory)
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

/*
 * New undo mechanism added by Jim Dunleavy <jim.dunleavy@erha.ie>
 */

using zword = System.UInt16;
using zbyte = System.Byte;

using Frotz;
using Frotz.Constants;

namespace Frotz.Generic
{

    internal struct RecordStruct
    {
        internal Story story_id;
        internal zword release;
        internal string serial;

        public RecordStruct(Story story_id, zword release, string serial)
        {
            this.story_id = story_id;
            this.release = release;
            this.serial = serial;

        }
    }

    internal static class FastMem
    {

        internal static RecordStruct[] records = {
        new RecordStruct(Story.SHERLOCK, 97, "871026"),
        new RecordStruct( Story.SHERLOCK,  21, "871214" ),
        new RecordStruct( Story.SHERLOCK,  22, "880112" ),
        new RecordStruct( Story.SHERLOCK,  26, "880127" ),
        new RecordStruct( Story.SHERLOCK,   4, "880324" ),
        new RecordStruct( Story.BEYOND_ZORK,   1, "870412" ),
        new RecordStruct( Story.BEYOND_ZORK,   1, "870715" ),
        new RecordStruct( Story.BEYOND_ZORK,  47, "870915" ),
        new RecordStruct( Story.BEYOND_ZORK,  49, "870917" ),
        new RecordStruct( Story.BEYOND_ZORK,  51, "870923" ),
        new RecordStruct( Story.BEYOND_ZORK,  57, "871221" ),
        new RecordStruct( Story.BEYOND_ZORK,  60, "880610" ),
        new RecordStruct( Story.ZORK_ZERO,   0, "870831" ),
        new RecordStruct( Story.ZORK_ZERO,  96, "880224" ),
        new RecordStruct( Story.ZORK_ZERO, 153, "880510" ),
        new RecordStruct( Story.ZORK_ZERO, 242, "880830" ),
        new RecordStruct( Story.ZORK_ZERO, 242, "880901" ),
        new RecordStruct( Story.ZORK_ZERO, 296, "881019" ),
        new RecordStruct( Story.ZORK_ZERO, 366, "890323" ),
        new RecordStruct( Story.ZORK_ZERO, 383, "890602" ),
        new RecordStruct( Story.ZORK_ZERO, 387, "890612" ),
        new RecordStruct( Story.ZORK_ZERO, 392, "890714" ),
        new RecordStruct( Story.ZORK_ZERO, 393, "890714" ),
        new RecordStruct( Story.SHOGUN, 292, "890314" ),
        new RecordStruct( Story.SHOGUN, 295, "890321" ),
        new RecordStruct( Story.SHOGUN, 311, "890510" ),
        new RecordStruct( Story.SHOGUN, 320, "890627" ),
        new RecordStruct( Story.SHOGUN, 321, "890629" ),
        new RecordStruct( Story.SHOGUN, 322, "890706" ),
        new RecordStruct( Story.ARTHUR,  40, "890502" ),
        new RecordStruct( Story.ARTHUR,  41, "890504" ),
        new RecordStruct( Story.ARTHUR,  54, "890606" ),
        new RecordStruct( Story.ARTHUR,  63, "890622" ),
        new RecordStruct( Story.ARTHUR,  74, "890714" ),
        new RecordStruct( Story.JOURNEY,  46, "880603" ),
        new RecordStruct( Story.JOURNEY,   2, "890303" ),
        new RecordStruct( Story.JOURNEY,  26, "890316" ),
        new RecordStruct( Story.JOURNEY,  30, "890322" ),
        new RecordStruct( Story.JOURNEY,  51, "890522" ),
        new RecordStruct( Story.JOURNEY,  54, "890526" ),
        new RecordStruct( Story.JOURNEY,  77, "890616" ),
        new RecordStruct( Story.JOURNEY,  79, "890627" ),
        new RecordStruct( Story.JOURNEY,  83, "890706" ),
        new RecordStruct( Story.LURKING_HORROR, 203, "870506" ),
        new RecordStruct( Story.LURKING_HORROR, 219, "870912" ),
        new RecordStruct( Story.LURKING_HORROR, 221, "870918" ),
        new RecordStruct( Story.AMFV,  47, "850313" ),
        new RecordStruct( Story.UNKNOWN,   0, "------" )
    };

        internal static string save_name = General.DEFAULT_SAVE_NAME;
        internal static string auxilary_name = General.DEFAULT_AUXILARY_NAME;

        internal static zbyte[] ZMData;
        internal static zword ZMData_checksum = 0;
        internal static byte[] storyData;

        internal static long zmp = 0;
        internal static long pcp = 0;

        private static System.IO.MemoryStream story_fp = null;

        static bool first_restart = true;
        static long init_fp_pos = 0;

        #region zmp & pcp

        internal static byte LO(zword v)
        {
            return (byte)(v & 0xff);
        }

        internal static byte HI(zword v)
        {
            return (byte)(v >> 8);
        }

        internal static void SET_WORD(long addr, zword v)
        {
            ZMData[addr] = HI(v);
            ZMData[addr + 1] = LO(v);

            DebugState.Output("ZMP: {0} -> {1}", addr, v);
        }

        internal static void LOW_WORD(long addr, out byte v)
        {
            v = (byte)((ZMData[addr] << 8) | ZMData[addr + 1]);
        }

        internal static void LOW_WORD(long addr, out zword v)
        {
            v = (ushort)((ZMData[addr] << 8) | ZMData[addr + 1]);
        }

        // TODO I'm suprised that they return the same thing
        internal static void HIGH_WORD(long addr, out zword v)
        {
            LOW_WORD(addr, out v);
        }

        internal static void CODE_WORD(out zword v)
        {
            v = (zword)(ZMData[pcp] << 8 | ZMData[pcp + 1]);
            pcp += 2;
        }

        internal static void SET_BYTE(long addr, byte v)
        {
            ZMData[addr] = v;
            DebugState.Output("ZMP: {0} -> {1}", addr, v);
        }

        internal static void CODE_BYTE(out byte v)
        {
            v = ZMData[pcp++];
        }

        internal static void LOW_BYTE(long addr, out byte v)
        {
            v = ZMData[addr];
        }

        internal static void GET_PC(out long v)
        {
            v = (long)(pcp - zmp);
        }

        internal static void SET_PC(long v)
        {
            pcp = (long)(zmp + v);
        }
        #endregion

        /*
         * Data for the undo mechanism.
         * This undo mechanism is based on the scheme used in Evin Robertson's
         * Nitfol interpreter.
         * Undo blocks are stored as differences between states.
         */

        //typedef struct undo_struct undo_t;
        internal class undo_struct
        {
            internal long pc;
            internal long diff_size;
            internal zword frame_count;
            internal zword stack_size;
            internal zword frame_offset;
            /* undo diff and stack data follow */

            internal long sp;
            internal zword[] stack;
            internal byte[] undo_data;
        };

        // static undo_struct first_undo = null, last_undo = null, curr_undo = null;
        //static zbyte *undo_mem = NULL, *prev_zmp, *undo_diff;

        internal static zbyte[] prev_zmp;
        internal static zbyte[] undo_diff;
        internal static System.Collections.Generic.List<undo_struct> undo_mem;

        static int undo_count = 0;

        /*
         * get_header_extension
         *
         * Read a value from the header extension (former mouse table).
         *
         */

        internal static zword get_header_extension(int entry)
        {
            zword addr;
            zword val;

            if (main.h_extension_table == 0 || entry > main.hx_table_size)
                return 0;

            addr = (zword)(main.h_extension_table + 2 * entry);
            LOW_WORD(addr, out val);

            return val;

        }/* get_header_extension */

        /*
         * set_header_extension
         *
         * Set an entry in the header extension (former mouse table).
         *
         */

        internal static void set_header_extension(int entry, zword val)
        {
            zword addr;

            if (main.h_extension_table == 0 || entry > main.hx_table_size)
                return;

            addr = (zword)(main.h_extension_table + 2 * entry);
            SET_WORD(addr, val);

        }/* set_header_extension */

        /*
         * restart_header
         *
         * Set all header fields which hold information about the interpreter.
         *
         */

        internal static void restart_header()
        {
            zword screen_x_size;
            zword screen_y_size;
            zbyte font_x_size;
            zbyte font_y_size;

            int i;

            SET_BYTE(ZMachine.H_CONFIG, main.h_config);
            SET_WORD(ZMachine.H_FLAGS, main.h_flags);

            if (main.h_version >= ZMachine.V4)
            {
                SET_BYTE(ZMachine.H_INTERPRETER_NUMBER, main.h_interpreter_number);
                SET_BYTE(ZMachine.H_INTERPRETER_VERSION, main.h_interpreter_version);
                SET_BYTE(ZMachine.H_SCREEN_ROWS, main.h_screen_rows);
                SET_BYTE(ZMachine.H_SCREEN_COLS, main.h_screen_cols);
            }

            /* It's less trouble to use font size 1x1 for V5 games, especially
               because of a bug in the unreleased German version of "Zork 1" */

            if (main.h_version != ZMachine.V6)
            {
                screen_x_size = main.h_screen_cols;
                screen_y_size = main.h_screen_rows;
                font_x_size = 1;
                font_y_size = 1;
            }
            else
            {
                screen_x_size = main.h_screen_width;
                screen_y_size = main.h_screen_height;
                font_x_size = main.h_font_width;
                font_y_size = main.h_font_height;
            }

            if (main.h_version >= ZMachine.V5)
            {
                SET_WORD(ZMachine.H_SCREEN_WIDTH, screen_x_size);
                SET_WORD(ZMachine.H_SCREEN_HEIGHT, screen_y_size);
                SET_BYTE(ZMachine.H_FONT_HEIGHT, font_y_size);
                SET_BYTE(ZMachine.H_FONT_WIDTH, font_x_size);
                SET_BYTE(ZMachine.H_DEFAULT_BACKGROUND, main.h_default_background);
                SET_BYTE(ZMachine.H_DEFAULT_FOREGROUND, main.h_default_foreground);
            }

            if ((main.h_version >= ZMachine.V3) && (main.h_user_name[0] != 0))
            {
                for (i = 0; i < 8; i++)
                {
                    storeb((zword)(ZMachine.H_USER_NAME + i), main.h_user_name[i]);
                }
            }
            SET_BYTE(ZMachine.H_STANDARD_HIGH, main.h_standard_high);
            SET_BYTE(ZMachine.H_STANDARD_LOW, main.h_standard_low);

            set_header_extension(ZMachine.HX_FLAGS, main.hx_flags);
            set_header_extension(ZMachine.HX_FORE_COLOUR, main.hx_fore_colour);
            set_header_extension(ZMachine.HX_BACK_COLOUR, main.hx_back_colour);
        }/* restart_header */

        /*
         * init_memory
         *
         * Allocate memory and load the story file.
         *
         */

        internal static void init_memory()
        {
            long size;
            zword addr;
            zword n;
            int i, j;

            // TODO Abstract this part
            /* Open story file */
            // story_fp = new System.IO.FileStream(main.story_name, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            story_fp = os_.path_open(main.story_data);
            if (story_fp == null)
            {
                os_.fatal("Cannot open story file");
            }
            init_fp_pos = story_fp.Position;

            storyData = new byte[story_fp.Length];
            story_fp.Read(storyData, 0, storyData.Length);
            story_fp.Position = 0;

            DebugState.Output("Starting story: {0}", main.story_name);

            /* Allocate memory for story header */

            ZMData = new byte[64];
            Frotz.Other.ZMath.clearArray(ZMData);

            /* Load header into memory */
            if (story_fp.Read(ZMData, 0, 64) != 64)
            {
                os_.fatal("Story file read error");
            }

            /* Copy header fields to global variables */
            LOW_BYTE(ZMachine.H_VERSION, out main.h_version);

            if (main.h_version < ZMachine.V1 || main.h_version > ZMachine.V8)
            {
                os_.fatal("Unknown Z-code version");
            }

            LOW_BYTE(ZMachine.H_CONFIG, out main.h_config);
            if (main.h_version == ZMachine.V3 && ((main.h_config & ZMachine.CONFIG_BYTE_SWAPPED) != 0))
            {
                os_.fatal("Byte swapped story file");
            }

            LOW_WORD(ZMachine.H_RELEASE, out main.h_release);
            LOW_WORD(ZMachine.H_RESIDENT_SIZE, out main.h_resident_size);
            LOW_WORD(ZMachine.H_START_PC, out main.h_start_pc);
            LOW_WORD(ZMachine.H_DICTIONARY, out main.h_dictionary);
            LOW_WORD(ZMachine.H_OBJECTS, out main.h_objects);
            LOW_WORD(ZMachine.H_GLOBALS, out main.h_globals);
            LOW_WORD(ZMachine.H_DYNAMIC_SIZE, out main.h_dynamic_size);
            LOW_WORD(ZMachine.H_FLAGS, out main.h_flags);

            for (i = 0, addr = ZMachine.H_SERIAL; i < 6; i++, addr++)
            {
                LOW_BYTE(addr, out main.h_serial[i]);
            }
            // TODO serial might need to be a char

            /* Auto-detect buggy story files that need special fixes */

            main.story_id = Story.UNKNOWN;

            for (i = 0; records[i].story_id != Story.UNKNOWN; i++)
            {

                if (main.h_release == records[i].release)
                {

                    for (j = 0; j < 6; j++)
                        if (main.h_serial[j] != records[i].serial[j])
                            goto no_match;

                    main.story_id = records[i].story_id;

                }

            no_match: ; /* null statement */

            }

            LOW_WORD(ZMachine.H_ABBREVIATIONS, out main.h_abbreviations);
            LOW_WORD(ZMachine.H_FILE_SIZE, out main.h_file_size);

            /* Calculate story file size in bytes */
            if (main.h_file_size != 0)
            {
                main.story_size = 2 * main.h_file_size;

                if (main.h_version >= ZMachine.V4)
                {
                    main.story_size *= 2;
                }

                if (main.h_version >= ZMachine.V6)
                {
                    main.story_size *= 2;
                }

                if (main.story_id == Story.AMFV && main.h_release == 47)
                {
                    main.story_size = 2 * main.h_file_size;
                }
                else if (main.story_size > 0)
                {/* os_path_open() set the size */
                }
                else
                {/* some old games lack the file size entry */
                    main.story_size = story_fp.Length - init_fp_pos;
                    story_fp.Position = init_fp_pos + 64;
                }

                LOW_WORD(ZMachine.H_CHECKSUM, out main.h_checksum);
                LOW_WORD(ZMachine.H_ALPHABET, out main.h_alphabet);
                LOW_WORD(ZMachine.H_FUNCTIONS_OFFSET, out main.h_functions_offset);
                LOW_WORD(ZMachine.H_STRINGS_OFFSET, out main.h_strings_offset);
                LOW_WORD(ZMachine.H_TERMINATING_KEYS, out main.h_terminating_keys);
                LOW_WORD(ZMachine.H_EXTENSION_TABLE, out main.h_extension_table);

                /* Zork Zero beta and Macintosh versions don't have the graphics flag set */

                if (main.story_id == Story.ZORK_ZERO)
                {
                    if (main.h_release == 96 || main.h_release == 153 ||
                        main.h_release == 242 || main.h_release == 296)
                    {
                        main.h_flags |= ZMachine.GRAPHICS_FLAG;
                    }
                }

                /* Adjust opcode tables */

                if (main.h_version <= ZMachine.V4)
                {
                    Process.op0_opcodes[0x09] = new Process.zinstruction(Variable.z_pop);
                    Process.op0_opcodes[0x0f] = new Process.zinstruction(Math.z_not);
                }
                else
                {
                    Process.op0_opcodes[0x09] = new Process.zinstruction(Process.z_catch);
                    Process.op0_opcodes[0x0f] = new Process.zinstruction(Process.z_call_n);
                }

                /* Allocate memory for story data */

                byte[] temp = new byte[ZMData.Length];
                Frotz.Other.ZMath.clearArray(temp);

                System.Array.Copy(ZMData, temp, ZMData.Length);

                ZMData = new byte[main.story_size];
                Frotz.Other.ZMath.clearArray(ZMData);
                System.Array.Copy(temp, ZMData, temp.Length);

                /* Load story file in chunks of 32KB */

                n = 0x8000;

                for (size = 64; size < main.story_size; size += n)
                {
                    if (main.story_size - size < 0x8000) n = (ushort)(main.story_size - size);
                    SET_PC(size);

                    int read = story_fp.Read(ZMData, (int)pcp, n);

                    if (read != n) os_.fatal("Story file read error");
                }

                // Take a moment to calculate the checksum of the story file in case verify is called
                ZMData_checksum = 0;
                for (int k = 64; k < ZMData.Length; k++)
                {
                    ZMData_checksum += ZMData[k];
                }
            }

            DebugState.Output("Story Size: {0}", main.story_size);

            first_restart = true;

            /* Read header extension table */

            main.hx_table_size = get_header_extension(ZMachine.HX_TABLE_SIZE);
            main.hx_unicode_table = get_header_extension(ZMachine.HX_UNICODE_TABLE);
            main.hx_flags = get_header_extension(ZMachine.HX_FLAGS);
        }/* init_memory */

        /*
         * init_undo
         *
         * Allocate memory for multiple undo. It is important not to occupy
         * all the memory available, since the IO interface may need memory
         * during the game, e.g. for loading sounds or pictures.
         *
         */

        internal static void init_undo()
        {
            prev_zmp = new byte[ZMData.Length];
            undo_diff = new byte[ZMData.Length];

            undo_mem = new System.Collections.Generic.List<undo_struct>();

            System.Array.Copy(ZMData, prev_zmp, main.h_dynamic_size);
        }/* init_undo */

        /*
         * free_undo
         *
         * Free count undo blocks from the beginning of the undo list.
         *
         */

        internal static void free_undo(int count)
        {
            for (int i = 0; i < count; i++)
            {
                undo_mem.RemoveAt(0);
            }

        }/* free_undo */

        /*
         * reset_memory
         *
         * Close the story file and deallocate memory.
         *
         */

        internal static void reset_memory()
        {
            if (story_fp != null) story_fp.Close();

            undo_mem.Clear();

        }/* reset_memory */

        /*
         * storeb
         *
         * Write a byte value to the dynamic Z-machine memory.
         *
         */

        internal static void storeb(zword addr, zbyte value)
        {

            if (addr >= main.h_dynamic_size)
                Err.runtime_error(ErrorCodes.ERR_STORE_RANGE);

            if (addr == ZMachine.H_FLAGS + 1)
            {	/* flags register is modified */

                main.h_flags &= (zword)(~(ZMachine.SCRIPTING_FLAG | ZMachine.FIXED_FONT_FLAG));
                main.h_flags |= (zword)(value & (ZMachine.SCRIPTING_FLAG | ZMachine.FIXED_FONT_FLAG));

                if ((value & ZMachine.SCRIPTING_FLAG) > 0)
                {
                    if (!main.ostream_script)
                        Files.script_open();
                }
                else
                {
                    if (main.ostream_script)
                        Files.script_close();
                }

                Screen.refresh_text_style();

            }

            SET_BYTE(addr, value);

            DebugState.Output("storeb: {0} -> {1}", addr, value);
        }/* storeb */

        /*
         * storew
         *
         * Write a word value to the dynamic Z-machine memory.
         *
         */

        internal static void storew(zword addr, zword value)
        {

            storeb((zword)(addr + 0), HI(value));
            storeb((zword)(addr + 1), LO(value));

        }/* storew */

        /*
         * z_restart, re-load dynamic area, clear the stack and set the PC.
         *
         * 	no zargs used
         *
         */

        internal static void z_restart()
        {
            Buffer.flush_buffer();

            os_.restart_game(ZMachine.RESTART_BEGIN);

            Random.seed_random(0);

            if (!first_restart)
            {
                story_fp.Position = init_fp_pos;

                int read = story_fp.Read(ZMData, 0, main.h_dynamic_size);
                if (read != main.h_dynamic_size)
                {
                    os_.fatal("Story file read error");
                }
            }
            else first_restart = false;

            restart_header();
            Screen.restart_screen();

            main.sp = main.fp = General.STACK_SIZE; // TODO Critical to make sure this logic works; sp = fp = stack + STACK_SIZE;

            main.frame_count = 0;

            if (main.h_version != ZMachine.V6)
            {

                zword pc = main.h_start_pc;
                FastMem.SET_PC((int)pc);

            }
            else
            {
                Process.call(main.h_start_pc, 0, 0, 0);
            }

            os_.restart_game(ZMachine.RESTART_END);

        }/* z_restart */

        /*
         * get_default_name
         *
         * Read a default file name from the memory of the Z-machine and
         * copy it to a string.
         *
         */

        internal static string get_default_name(zword addr)
        {

            if (addr != 0)
            {

                var sb = new System.Text.StringBuilder();

                zbyte len;
                int i;

                FastMem.LOW_BYTE(addr, out len);
                addr++;

                for (i = 0; i < len; i++)
                {

                    zbyte c;

                    FastMem.LOW_BYTE(addr, out c);
                    addr++;

                    if (c >= 'A' && c <= 'Z')
                        c += 'a' - 'A';

                    // default_name[i] = c;
                    sb.Append((char)c);

                }

                // default_name[i] = 0;

                if (sb.ToString().IndexOf(".") == -1)
                {
                    sb.Append(".AUX");
                    return sb.ToString();
                }
                else
                    return auxilary_name;
            }
            return null;

        }/* get_default_name */

        /*
         * z_restore, restore [a part of] a Z-machine state from disk
         *
         *	zargs[0] = address of area to restore (optional)
         *	zargs[1] = number of bytes to restore
         *	zargs[2] = address of suggested file name
         *	zargs[3] = whether to ask for confirmation of the file name
         *
         */

        internal static void z_restore()
        {
            string new_name;
            //            string default_name;
            System.IO.FileStream gfp;

            zword success = 0;

            if (Process.zargc != 0)
            {
                os_.fail("Need to implement optional args in z_restore");
                gfp = null;
                ///* Get the file name */

                //get_default_name (default_name, (FastMem.zargc >= 3) ? FastMem.zargs[2] : 0);

                //if ((FastMem.zargc >= 4) ? FastMem.zargs[3] : 1) {

                //    if (os_read_file_name (new_name, default_name, ZMachine.FILE_LOAD_AUX) == 0)
                //    goto finished;

                //    strcpy (auxilary_name, new_name);

                //} else strcpy (new_name, default_name);

                ///* Open auxilary file */

                //if ((gfp = fopen (new_name, "rb")) == NULL)
                //    goto finished;

                ///* Load auxilary file */

                //success = fread (zmp + zargs[0], 1, zargs[1], gfp);

                ///* Close auxilary file */

                //fclose (gfp);

            }
            else
            {

                /* Get the file name */

                if (!os_.read_file_name(out new_name, save_name, FileTypes.FILE_RESTORE))
                    goto finished;

                save_name = new_name;

                /* Open game file */
                gfp = new System.IO.FileStream(new_name, System.IO.FileMode.Open);
                if (gfp == null) goto finished;

                if (main.option_save_quetzal == true)
                {
                    success = Quetzal.restore_quetzal(gfp, story_fp);

                }
                else
                {
                    os_.fail("Need to implement old style save");
                    /* Load game file */

                    //    release = (unsigned) fgetc (gfp) << 8;
                    //    release |= fgetc (gfp);

                    //    (void) fgetc (gfp);
                    //    (void) fgetc (gfp);

                    //    /* Check the release number */

                    //    if (release == h_release) {

                    //    pc = (long) fgetc (gfp) << 16;
                    //    pc |= (unsigned) fgetc (gfp) << 8;
                    //    pc |= fgetc (gfp);

                    //    SET_PC (pc)

                    //    sp = stack + (fgetc (gfp) << 8);
                    //    sp += fgetc (gfp);
                    //    fp = stack + (fgetc (gfp) << 8);
                    //    fp += fgetc (gfp);

                    //    for (i = (int) (sp - stack); i < STACK_SIZE; i++) {
                    //        stack[i] = (unsigned) fgetc (gfp) << 8;
                    //        stack[i] |= fgetc (gfp);
                    //    }

                    //    fseek (story_fp, init_fp_pos, SEEK_SET);

                    //    for (addr = 0; addr < h_dynamic_size; addr++) {
                    //        int skip = fgetc (gfp);
                    //        for (i = 0; i < skip; i++)
                    //        zmp[addr++] = fgetc (story_fp);
                    //        zmp[addr] = fgetc (gfp);
                    //        (void) fgetc (story_fp);
                    //    }

                    //    /* Check for errors */

                    //    if (ferror (gfp) || ferror (story_fp) || addr != h_dynamic_size)
                    //        success = -1;
                    //    else

                    //        /* Success */

                    //        success = 2;

                    //    } else print_string ("Invalid save file\n");
                }
            }
            if ((short)success >= 0 && success != zword.MaxValue)
            {
                /* Close game file */

                gfp.Close();

                if ((short)success > 0)
                {
                    zbyte old_screen_rows;
                    zbyte old_screen_cols;

                    /* In V3, reset the upper window. */
                    if (main.h_version == ZMachine.V3)
                        Screen.split_window(0);

                    LOW_BYTE(ZMachine.H_SCREEN_ROWS, out old_screen_rows);
                    LOW_BYTE(ZMachine.H_SCREEN_COLS, out old_screen_cols);

                    /* Reload cached header fields. */
                    restart_header();

                    /*
                     * Since QUETZAL files may be saved on many different machines,
                     * the screen sizes may vary a lot. Erasing the status window
                     * seems to cover up most of the resulting badness.
                     */
                    if (main.h_version > ZMachine.V3 && main.h_version != ZMachine.V6
                        && (main.h_screen_rows != old_screen_rows
                        || main.h_screen_cols != old_screen_cols))
                        Screen.erase_window(1);
                }
            }
            else
                os_.fatal("Error reading save file");


        finished:

            if (main.h_version <= ZMachine.V3)
                Process.branch(success > 0);
            else
                Process.store(success);
        }/* z_restore */

        /*
         * mem_diff
         *
         * Set diff to a Quetzal-like difference between a and b,
         * copying a to b as we go.  It is assumed that diff points to a
         * buffer which is large enough to hold the diff.
         * mem_size is the number of bytes to compare.
         * Returns the number of bytes copied to diff.
         *
         */

        static long mem_diff(zbyte[] a, zbyte[] b, zword mem_size, zbyte[] diff)
        {
            zword size = mem_size;
            int dPtr = 0;
            uint j;
            zbyte c = 0;

            int aPtr = 0;
            int bPtr = 0;

            for (; ; )
            {
                for (j = 0; size > 0 && (c = (zbyte)(a[aPtr++] ^ b[bPtr++])) == 0; j++)
                    size--;
                if (size == 0) break;

                size--;

                if (j > 0x8000)
                {
                    diff[dPtr++] = 0;
                    diff[dPtr++] = 0xff;
                    diff[dPtr++] = 0xff;
                    j -= 0x8000;
                }

                if (j > 0)
                {
                    diff[dPtr++] = 0;
                    j--;

                    if (j <= 0x7f)
                    {
                        diff[dPtr++] = (byte)j;
                    }
                    else
                    {
                        diff[dPtr++] = (byte)((j & 0x7f) | 0x80);
                        diff[dPtr++] = (byte)((j & 0x7f80) >> 7);
                    }
                }
                diff[dPtr++] = c;
                b[bPtr - 1] ^= c;
            }
            return dPtr;

        }/* mem_diff */

        /*
         * mem_undiff
         *
         * Applies a quetzal-like diff to dest
         *
         */

        static void mem_undiff(zbyte[] diff, long diff_length, zbyte[] dest)
        {
            zbyte c;
            uint diffPtr = 0;
            uint destPtr = 0;

            while (diff_length > 0)
            {
                c = diff[diffPtr++];
                diff_length--;
                if (c == 0)
                {
                    uint runlen;

                    if (diff_length == 0) // TODO I'm not sure about this logic
                        return;  /* Incomplete run */
                    runlen = diff[diffPtr++];
                    diff_length--;
                    if ((runlen & 0x80) > 0)
                    {
                        if (diff_length == 0)
                            return; /* Incomplete extended run */
                        c = diff[diffPtr++];
                        diff_length--;
                        runlen = (runlen & 0x7f) | (((uint)c) << 7);
                    }

                    destPtr += runlen + 1;
                }
                else
                {
                    dest[destPtr++] ^= c;
                }
            }

        }/* mem_undiff */

        /*
         * restore_undo
         *
         * This function does the dirty work for z_restore_undo.
         *
         */

        internal static int restore_undo()
        {
            if (main.option_undo_slots == 0)	/* undo feature unavailable */
                return -1;

            if (undo_mem.Count == 0) return 0;

            /* undo possible */

            undo_struct undo = undo_mem[undo_mem.Count - 1];
            
            System.Array.Copy(prev_zmp, ZMData, main.h_dynamic_size);
            SET_PC(undo.pc);
            main.sp = undo.sp;
            main.fp = undo.frame_offset;
            main.frame_count = undo.frame_count;
             
            mem_undiff(undo.undo_data, undo.diff_size, prev_zmp);

            // System.Array.Copy(undo.stack, 0, main.stack, undo.sp, undo.stack.Length);
            Frotz.Other.ArrayCopy.Copy(undo.stack, 0, main.stack, undo.sp, undo.stack.Length);

            undo_mem.Remove(undo);

            restart_header();

            return 2;

        }/* restore_undo */

        /*
         * z_restore_undo, restore a Z-machine state from memory.
         *
         *	no zargs used
         *
         */

        internal static void z_restore_undo()
        {

            Process.store((zword)restore_undo());

        }/* z_restore_undo */

        /*
         * z_save, save [a part of] the Z-machine state to disk.
         *
         *	zargs[0] = address of memory area to save (optional)
         *	zargs[1] = number of bytes to save
         *	zargs[2] = address of suggested file name
         *	zargs[3] = whether to ask for confirmation of the file name
         *
         */

        internal static void z_save()
        {
            string new_name;
            string default_name;
            System.IO.FileStream gfp;

            zword success = 0;

            if (Process.zargc != 0)
            {

                /* Get the file name */

                default_name = get_default_name((zword)((Process.zargc >= 3) ? Process.zargs[2] : 0));

                //    if ((zargc >= 4) ? zargs[3] : 1) {

                //        if (os_read_file_name (new_name, default_name, FILE_SAVE_AUX) == 0)
                //        goto finished;

                //        strcpy (auxilary_name, new_name);

                //    } else strcpy (new_name, default_name);

                //    /* Open auxilary file */

                //    if ((gfp = fopen (new_name, "wb")) == NULL)
                //        goto finished;

                //    /* Write auxilary file */

                //    success = fwrite (zmp + zargs[0], zargs[1], 1, gfp);

                //    /* Close auxilary file */

                //    fclose (gfp);
                os_.fail("need to implement option save arguments");
            }
            else
            {
                if (!os_.read_file_name(out new_name, save_name, FileTypes.FILE_SAVE))
                    goto finished;

                save_name = new_name;

                /* Open game file */

                if ((gfp = new System.IO.FileStream(new_name, System.IO.FileMode.OpenOrCreate)) == null)
                    goto finished;

                if (main.option_save_quetzal == true)
                {
                    success = Quetzal.save_quetzal(gfp, story_fp);

                }
                else
                {
                    os_.fail("Need to implement old style save");

                    //        /* Write game file */

                    //        fputc ((int) hi (h_release), gfp);
                    //        fputc ((int) lo (h_release), gfp);
                    //        fputc ((int) hi (h_checksum), gfp);
                    //        fputc ((int) lo (h_checksum), gfp);

                    //        GET_PC (pc)

                    //        fputc ((int) (pc >> 16) & 0xff, gfp);
                    //        fputc ((int) (pc >> 8) & 0xff, gfp);
                    //        fputc ((int) (pc) & 0xff, gfp);

                    //        nsp = (int) (sp - stack);
                    //        nfp = (int) (fp - stack);

                    //        fputc ((int) hi (nsp), gfp);
                    //        fputc ((int) lo (nsp), gfp);
                    //        fputc ((int) hi (nfp), gfp);
                    //        fputc ((int) lo (nfp), gfp);

                    //        for (i = nsp; i < STACK_SIZE; i++) {
                    //        fputc ((int) hi (stack[i]), gfp);
                    //        fputc ((int) lo (stack[i]), gfp);
                    //        }

                    //        fseek (story_fp, init_fp_pos, SEEK_SET);

                    //        for (addr = 0, skip = 0; addr < h_dynamic_size; addr++)
                    //        if (zmp[addr] != fgetc (story_fp) || skip == 255 || addr + 1 == h_dynamic_size) {
                    //            fputc (skip, gfp);
                    //            fputc (zmp[addr], gfp);
                    //            skip = 0;
                    //        } else skip++;
                }

                /* Close game file and check for errors */

                gfp.Close();
                // TODO Not sure what to do with these
                //    if (gfp.Close() ) { // || ferror(story_fp)) {
                //    Text.print_string("Error writing save file\n");
                //    goto finished;
                //}

                /* Success */

                success = 1;

            }

        finished:

            if (main.h_version <= ZMachine.V3)
                Process.branch(success > 0);
            else
                Process.store(success);
        }/* z_save */

        /*
         * save_undo
         *
         * This function does the dirty work for z_save_undo.
         *
         */

        internal static int save_undo()
        {
            long diff_size;
            int stack_size;
            undo_struct p;

            if (main.option_undo_slots == 0)		/* undo feature unavailable */
                return -1;

            /* save undo possible */

            if (undo_count == main.option_undo_slots)
                free_undo(1);

            p = new undo_struct();

            diff_size = mem_diff(ZMData, prev_zmp, main.h_dynamic_size, undo_diff);
            stack_size = main.stack.Length;

            GET_PC(out p.pc);
            p.frame_count = main.frame_count;
            p.diff_size = diff_size;
            p.stack_size = (zword)stack_size;
            p.frame_offset = (zword)main.fp; //    p->frame_offset = fp - stack;

            // p.undo_data = undo_diff;
            p.undo_data = new zbyte[diff_size];
            // System.Array.Copy(undo_diff, p.undo_data, diff_size);
            Frotz.Other.ArrayCopy.Copy(undo_diff, 0, p.undo_data, 0, diff_size);

            p.stack = new zword[main.stack.Length - main.sp];
            // System.Array.Copy(main.stack, main.sp, p.stack, 0, main.stack.Length - main.sp);
            Frotz.Other.ArrayCopy.Copy(main.stack, main.sp, p.stack, 0, main.stack.Length - main.sp);
            p.sp = main.sp;

            undo_mem.Add(p);

            return 1;
        }/* save_undo */

        /*
         * z_save_undo, save the current Z-machine state for a future undo.
         *
         *	no zargs used
         *
         */

        internal static void z_save_undo()
        {

            Process.store((zword)save_undo());

        }/* z_save_undo */

        /*
         * z_verify, check the story file integrity.
         *
         *	no zargs used
         *
         */

        internal static void z_verify()
        {
            //zword checksum = 0;
            //long i;

            /* Sum all bytes in story file except header bytes */

            //for (i = 64; i < main.story_size; i++)
            //{
            //    checksum += FastMem.ZMData_original[i];
            //}

            //for (i = 64; i < story_size; i++)
            //    checksum += fgetc(story_fp);

            /* Branch if the checksums are equal */

            Process.branch(ZMData_checksum == main.h_checksum);

        }/* z_verify */
    }
}