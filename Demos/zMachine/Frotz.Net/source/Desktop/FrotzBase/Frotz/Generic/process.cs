/* process.c - Interpreter loop and program control
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

namespace Frotz.Generic
{
    internal static class Process
    {
        internal static zword[] zargs = new zword[8];
        internal static int zargc;

        internal static int finished = 0;

        public delegate void zinstruction();

        internal static zinstruction[] op0_opcodes = new zinstruction[0x10] {
        new zinstruction(z_rtrue),
        new zinstruction(z_rfalse),
        new zinstruction(Text.z_print),
        new zinstruction(Text.z_print_ret),
        new zinstruction(z_nop),
        new zinstruction(FastMem.z_save),
        new zinstruction(FastMem.z_restore),
        new zinstruction(FastMem.z_restart),
        new zinstruction(z_ret_popped),
        new zinstruction(z_catch),
        new zinstruction(z_quit),
        new zinstruction(Text.z_new_line),
        new zinstruction(Screen.z_show_status),
        new zinstruction(FastMem.z_verify), // Not Tested or Implemented
        new zinstruction(__extended__),
        new zinstruction(main.z_piracy)
        };

        internal static zinstruction[] op1_opcodes = new zinstruction[0x10] {
        new zinstruction(Math.z_jz),
        new zinstruction(CObject.z_get_sibling),
        new zinstruction(CObject.z_get_child),
        new zinstruction(CObject.z_get_parent),
        new zinstruction(CObject.z_get_prop_len),
        new zinstruction(Variable.z_inc),
        new zinstruction(Variable.z_dec),
        new zinstruction(Text.z_print_addr),
        new zinstruction(z_call_s),
        new zinstruction(CObject.z_remove_obj),
        new zinstruction(Text.z_print_obj),
        new zinstruction(z_ret),
        new zinstruction(z_jump),
        new zinstruction(Text.z_print_paddr),
        new zinstruction(Variable.z_load),
        new zinstruction(z_call_n),
        };

        internal static zinstruction[] var_opcodes = new zinstruction[0x40] {
        new zinstruction(__illegal__),
        new zinstruction(Math.z_je),
        new zinstruction(Math.z_jl),
        new zinstruction(Math.z_jg),
        new zinstruction(Variable.z_dec_chk),
        new zinstruction(Variable.z_inc_chk),
        new zinstruction(CObject.z_jin),
        new zinstruction(Math.z_test),
        new zinstruction(Math.z_or),
        new zinstruction(Math.z_and),
        new zinstruction(CObject.z_test_attr),
        new zinstruction(CObject.z_set_attr),
        new zinstruction(CObject.z_clear_attr),
        new zinstruction(Variable.z_store),
        new zinstruction(CObject.z_insert_obj),
        new zinstruction(Table.z_loadw),
        new zinstruction(Table.z_loadb),
        new zinstruction(CObject.z_get_prop),
        new zinstruction(CObject.z_get_prop_addr),
        new zinstruction(CObject.z_get_next_prop),
        new zinstruction(Math.z_add),
        new zinstruction(Math.z_sub),
        new zinstruction(Math.z_mul),
        new zinstruction(Math.z_div),
        new zinstruction(Math.z_mod),
        new zinstruction(z_call_s),
        new zinstruction(z_call_n),
        new zinstruction(Screen.z_set_colour),
        new zinstruction(z_throw),
        new zinstruction(__illegal__),
        new zinstruction(__illegal__),
        new zinstruction(__illegal__),
        new zinstruction(z_call_s),
        new zinstruction(Table.z_storew),
        new zinstruction(Table.z_storeb),
        new zinstruction(CObject.z_put_prop),
        new zinstruction(Input.z_read),
        new zinstruction(Text.z_print_char),
        new zinstruction(Text.z_print_num),
        new zinstruction(Random.z_random),
        new zinstruction(Variable.z_push),
        new zinstruction(Variable.z_pull),
        new zinstruction(Screen.z_split_window),
        new zinstruction(Screen.z_set_window),
        new zinstruction(z_call_s),
        new zinstruction(Screen.z_erase_window),
        new zinstruction(Screen.z_erase_line),
        new zinstruction(Screen.z_set_cursor),
        new zinstruction(Screen.z_get_cursor),
        new zinstruction(Screen.z_set_text_style),
        new zinstruction(Screen.z_buffer_mode),
        new zinstruction(Stream.z_output_stream),
        new zinstruction(Stream.z_input_stream),
        new zinstruction(Sound.z_sound_effect),
        new zinstruction(Input.z_read_char),
        new zinstruction(Table.z_scan_table),
        new zinstruction(Math.z_not),
        new zinstruction(z_call_n),
        new zinstruction(z_call_n),
        new zinstruction(Text.z_tokenise),
        new zinstruction(Text.z_encode_text),
        new zinstruction(Table.z_copy_table),
        new zinstruction(Screen.z_print_table),
        new zinstruction(z_check_arg_count),
        };

        internal static zinstruction[] ext_opcodes = new zinstruction[0x1e] {
        new zinstruction(FastMem.z_save),
        new zinstruction(FastMem.z_restore),
        new zinstruction(Math.z_log_shift),
        new zinstruction(Math.z_art_shift), // TODO Not tested
        new zinstruction(Screen.z_set_font),
        new zinstruction(Screen.z_draw_picture),
        new zinstruction(Screen.z_picture_data),
        new zinstruction(Screen.z_erase_picture),
        new zinstruction(Screen.z_set_margins),
        new zinstruction(FastMem.z_save_undo),
        new zinstruction(FastMem.z_restore_undo),//    z_restore_undo, // 10
        new zinstruction(Text.z_print_unicode),
        new zinstruction(Text.z_check_unicode),
        new zinstruction(Screen.z_set_true_colour),	/* spec 1.1 */
        new zinstruction(__illegal__),
        new zinstruction(__illegal__),
        new zinstruction(Screen.z_move_window),
        new zinstruction(Screen.z_window_size),
        new zinstruction(Screen.z_window_style),
        new zinstruction(Screen.z_get_wind_prop),
        new zinstruction(Screen.z_scroll_window), // 20
        new zinstruction(Variable.z_pop_stack),
        new zinstruction(Input.z_read_mouse),//    z_read_mouse,
        new zinstruction(Screen.z_mouse_window),
        new zinstruction(Variable.z_push_stack),
        new zinstruction(Screen.z_put_wind_prop),
        new zinstruction(Text.z_print_form),//    z_print_form,
        new zinstruction(Input.z_make_menu),//    z_make_menu,
        new zinstruction(Screen.z_picture_table),
        new zinstruction(Screen.z_buffer_screen),	/* spec 1.1 */
                };

        static int invokeCount = 0;
        private static void private_invoke(zinstruction instruction, string array, int index, int opcode)
        {
            //DebugState.last_call_made = instruction.Method.Name + ":" + opcode;
            //DebugState.Output(false, "Invoking: {0:X} -> {1} -> {2}", opcode, instruction.Method.Name, invokeCount);
            instruction.Invoke();
            invokeCount++;
        }

        /*
         * init_process
         *
         * Initialize process variables.
         *
         */

        internal static void init_process()
        {
            finished = 0;
        }

        /*
         * load_operand
         *
         * Load an operand, either a variable or a constant.
         *
         */

        static void load_operand(zbyte type)
        {
            zword value;

            if ((type & 2) > 0)
            { 			/* variable */

                zbyte variable;

                FastMem.CODE_BYTE(out variable);

                if (variable == 0)
                    value = main.stack[main.sp++];
                else if (variable < 16)
                    value = main.stack[main.fp - variable];
                else
                {
                    zword addr = (zword)(main.h_globals + 2 * (variable - 16)); // TODO Make sure this logic
                    FastMem.LOW_WORD(addr, out value);
                }

            }
            else if ((type & 1) > 0)
            { 		/* small constant */

                zbyte bvalue;

                FastMem.CODE_BYTE(out bvalue);
                value = bvalue;

            }
            else FastMem.CODE_WORD(out value); 		/* large constant */

            zargs[zargc++] = value;

            DebugState.Output("  Storing operand: {0} -> {1}", zargc - 1, value);

        }/* load_operand */

        /*
         * load_all_operands
         *
         * Given the operand specifier byte, load all (up to four) operands
         * for a VAR or EXT opcode.
         *
         */

        internal static void load_all_operands(zbyte specifier)
        {
            int i;

            for (i = 6; i >= 0; i -= 2)
            {

                zbyte type = (zbyte)((specifier >> i) & 0x03); // TODO Check this conversion

                if (type == 3)
                    break;

                load_operand(type);

            }

        }/* load_all_operands */

        /*
         * interpret
         *
         * Z-code interpreter main loop
         *
         */

        internal static void interpret()
        {
            do
            {
                zbyte opcode;

                FastMem.CODE_BYTE(out opcode);

                DebugState.Output("CODE: {0} -> {1:X}", FastMem.pcp - 1, opcode);

                if (main.abort_game_loop == true)
                {
                    main.abort_game_loop = false;
                    return;
                }

                zargc = 0;
                if (opcode < 0x80)
                {			/* 2OP opcodes */

                    load_operand((zbyte)((opcode & 0x40) > 0 ? 2 : 1));
                    load_operand((zbyte)((opcode & 0x20) > 0 ? 2 : 1));

                    private_invoke(var_opcodes[opcode & 0x1f], "2OP", (opcode & 0x1f), opcode);
                }
                else if (opcode < 0xb0)
                {		/* 1OP opcodes */

                    load_operand((zbyte)(opcode >> 4));

                    private_invoke(op1_opcodes[opcode & 0x0f], "1OP", (opcode & 0x0f), opcode);
                }
                else if (opcode < 0xc0)
                {		/* 0OP opcodes */
                    private_invoke(op0_opcodes[opcode - 0xb0], "0OP", (opcode - 0xb0), opcode);
                }
                else
                {				/* VAR opcodes */

                    zbyte specifier1;
                    zbyte specifier2;

                    if (opcode == 0xec || opcode == 0xfa)
                    {	/* opcodes 0xec */
                        FastMem.CODE_BYTE(out specifier1);                  /* and 0xfa are */
                        FastMem.CODE_BYTE(out specifier2);                  /* call opcodes */
                        load_all_operands(specifier1);		/* with up to 8 */
                        load_all_operands(specifier2);         /* arguments    */
                    }
                    else
                    {
                        FastMem.CODE_BYTE(out specifier1);
                        load_all_operands(specifier1);
                    }

                    private_invoke(var_opcodes[opcode - 0xc0], "VAR", (opcode - 0xc0), opcode);
                }

                os_.tick();
            } while (finished == 0);

            finished--;
        }/* interpret */

        /*
         * call
         *
         * Call a subroutine. Save PC and FP then load new PC and initialise
         * new stack frame. Note that the caller may legally provide less or
         * more arguments than the function actually has. The call type "ct"
         * can be 0 (z_call_s), 1 (z_call_n) or 2 (direct call).
         *
         */

        internal static void call(zword routine, int argc, int args_offset, int ct)
        {
            long pc;
            zword value;
            zbyte count;
            int i;

            if (main.sp < 4)//if (sp - stack < 4)
                Err.runtime_error(ErrorCodes.ERR_STK_OVF);

            FastMem.GET_PC(out pc);

            main.stack[--main.sp] = (zword)(pc >> 9);
            main.stack[--main.sp] = (zword)(pc & 0x1ff);
            main.stack[--main.sp] = (zword)(main.fp - 1); // *--sp = (zword) (fp - stack - 1);
            main.stack[--main.sp] = (zword)(argc | (ct << (main.option_save_quetzal == true ? 12 : 8)));

            main.fp = main.sp;
            main.frame_count++;

            DebugState.Output("Added Frame: {0} -> {1}:{2}:{3}:{4}",
                main.frame_count,
                main.stack[main.sp + 0],
                main.stack[main.sp + 1],
                main.stack[main.sp + 2],
                main.stack[main.sp + 3]);

            /* Calculate byte address of routine */

            if (main.h_version <= ZMachine.V3)
                pc = (long)routine << 1;
            else if (main.h_version <= ZMachine.V5)
                pc = (long)routine << 2;
            else if (main.h_version <= ZMachine.V7)
                pc = ((long)routine << 2) + ((long)main.h_functions_offset << 3);
            else /* (h_version <= V8) */
                pc = (long)routine << 3;

            if (pc >= main.story_size)
                Err.runtime_error(ErrorCodes.ERR_ILL_CALL_ADDR);

            FastMem.SET_PC(pc);

            /* Initialise local variables */

            FastMem.CODE_BYTE(out count);

            if (count > 15)
                Err.runtime_error(ErrorCodes.ERR_CALL_NON_RTN);
            if (main.sp < count)
                Err.runtime_error(ErrorCodes.ERR_STK_OVF);

            if (main.option_save_quetzal == true)
                main.stack[main.fp] |= (zword)(count << 8);	/* Save local var count for Quetzal. */

            value = 0;

            for (i = 0; i < count; i++)
            {

                if (main.h_version <= ZMachine.V4)		/* V1 to V4 games provide default */
                    FastMem.CODE_WORD(out value);		/* values for all local variables */

                main.stack[--main.sp] = (zword)((argc-- > 0) ? zargs[args_offset + i] : value);
                //*--sp = (zword) ((argc-- > 0) ? args[i] : value);

            }

            /* Start main loop for direct calls */

            if (ct == 2)
                interpret();
        }/* call */

        /*
         * ret
         *
         * Return from the current subroutine and restore the previous stack
         * frame. The result may be stored (0), thrown away (1) or pushed on
         * the stack (2). In the latter case a direct call has been finished
         * and we must exit the interpreter loop.
         *
         */

        internal static void ret(zword value)
        {
            long pc;
            int ct;

            if (main.sp > main.fp)
                Err.runtime_error(ErrorCodes.ERR_STK_UNDF);

            main.sp = main.fp;

            DebugState.Output("Removing Frame: {0}", main.frame_count);

            ct = main.stack[main.sp++] >> (main.option_save_quetzal == true ? 12 : 8);
            main.frame_count--;
            main.fp = 1 + main.stack[main.sp++]; // fp = stack + 1 + *sp++;
            pc = main.stack[main.sp++];
            pc = (main.stack[main.sp++] << 9) | (int)pc; // TODO Really don't trust casting PC to int

            FastMem.SET_PC(pc);

            /* Handle resulting value */

            if (ct == 0)
                store(value);
            if (ct == 2)
                main.stack[--main.sp] = value;

            /* Stop main loop for direct calls */

            if (ct == 2)
                finished++;

        }/* ret */

        /*
         * branch
         *
         * Take a jump after an instruction based on the flag, either true or
         * false. The branch can be short or long; it is encoded in one or two
         * bytes respectively. When bit 7 of the first byte is set, the jump
         * takes place if the flag is true; otherwise it is taken if the flag
         * is false. When bit 6 of the first byte is set, the branch is short;
         * otherwise it is long. The offset occupies the bottom 6 bits of the
         * first byte plus all the bits in the second byte for long branches.
         * Uniquely, an offset of 0 means return false, and an offset of 1 is
         * return true.
         *
         */

        internal static void branch(bool flag)
        {
            long pc;
            zword offset;
            zbyte specifier;
            zbyte off1;
            zbyte off2;

            FastMem.CODE_BYTE(out specifier);

            off1 = (zbyte)(specifier & 0x3f);

            if (!flag)
                specifier ^= 0x80;

            if ((specifier & 0x40) == 0)
            { // if (!(specifier & 0x40)) {		/* it's a long branch */

                if ((off1 & 0x20) > 0)		/* propagate sign bit */
                    off1 |= 0xc0;

                FastMem.CODE_BYTE(out off2);

                offset = (zword)((off1 << 8) | off2);

            }
            else offset = off1;		/* it's a short branch */

            if ((specifier & 0x80) > 0)
            {

                if (offset > 1)
                {		/* normal branch */

                    FastMem.GET_PC(out pc);
                    pc += (short)offset - 2;
                    FastMem.SET_PC(pc);

                }
                else ret(offset);		/* special case, return 0 or 1 */
            }

        }/* branch */

        /*
         * store
         *
         * Store an operand, either as a variable or pushed on the stack.
         *
         */

        internal static void store(zword value)
        {
            zbyte variable;

            FastMem.CODE_BYTE(out variable);

            if (variable == 0)
            {
                main.stack[--main.sp] = value; // *--sp = value;
                DebugState.Output("  Storing {0} on stack at {1}", value, main.sp);
            }
            else if (variable < 16)
            {
                main.stack[main.fp - variable] = value;  // *(fp - variable) = value;
                DebugState.Output("  Storing {0} on stack as Variable {1} at {2}", value, variable, main.sp);
            }
            else
            {
                zword addr = (zword)(main.h_globals + 2 * (variable - 16));
                FastMem.SET_WORD(addr, value);
                DebugState.Output("  Storing {0} at {1}", value, addr);
            }

        }/* store */

        /*
         * direct_call
         *
         * Call the interpreter loop directly. This is necessary when
         *
         * - a sound effect has been finished
         * - a read instruction has timed out
         * - a newline countdown has hit zero
         *
         * The interpreter returns the result value on the stack.
         *
         */

        internal static int direct_call(zword addr)
        {
            zword[] saved_zargs = new zword[8];
            int saved_zargc;
            int i;

            /* Calls to address 0 return false */

            if (addr == 0)
                return 0;

            /* Save operands and operand count */

            for (i = 0; i < 8; i++)
                saved_zargs[i] = zargs[i];

            saved_zargc = zargc;

            /* Call routine directly */

            call(addr, 0, 0, 2);

            /* Restore operands and operand count */

            for (i = 0; i < 8; i++)
                zargs[i] = saved_zargs[i];

            zargc = saved_zargc;

            /* Resulting value lies on top of the stack */

            return (short)main.stack[main.sp++];

        }/* direct_call */

        /*
         * __extended__
         *
         * Load and execute an extended opcode.
         *
         */

        static void __extended__()
        {
            zbyte opcode;
            zbyte specifier;

            FastMem.CODE_BYTE(out opcode);
            FastMem.CODE_BYTE(out specifier);

            load_all_operands(specifier);

            if (opcode < 0x1e)			/* extended opcodes from 0x1e on */
                // ext_opcodes[opcode] ();		/* are reserved for future spec' */
                private_invoke(ext_opcodes[opcode], "Extended", opcode, opcode);

        }/* __extended__ */

        /*
         * __illegal__
         *
         * Exit game because an unknown opcode has been hit.
         *
         */

        static void __illegal__()
        {

            Err.runtime_error(ErrorCodes.ERR_ILL_OPCODE);

        }/* __illegal__ */

        /*
         * z_catch, store the current stack frame for later use with z_throw.
         *
         *	no zargs used
         *
         */

        internal static void z_catch()
        {
            Process.store((zword)(main.option_save_quetzal == true ? main.frame_count : main.fp));

        }/* z_catch */

        /*
         * z_throw, go back to the given stack frame and return the given value.
         *
         *	zargs[0] = value to return
         *	zargs[1] = stack frame
         *
         */

        internal static void z_throw()
        {
            // TODO This has never been tested
            if (main.option_save_quetzal == true)
            {
                if (zargs[1] > main.frame_count)
                    Err.runtime_error(ErrorCodes.ERR_BAD_FRAME);

                /* Unwind the stack a frame at a time. */
                for (; main.frame_count > zargs[1]; --main.frame_count)
                    //fp = stack + 1 + fp[1];
                    main.fp = 1 + main.stack[main.fp + 1]; // TODO I think this is correct
            }
            else
            {
                if (zargs[1] > General.STACK_SIZE)
                    Err.runtime_error(ErrorCodes.ERR_BAD_FRAME);

                main.fp = zargs[1]; // fp = stack + zargs[1];
            }

            ret(zargs[0]);

        }/* z_throw */

        /*
         * z_call_n, call a subroutine and discard its result.
         *
         * 	zargs[0] = packed address of subroutine
         *	zargs[1] = first argument (optional)
         *	...
         *	zargs[7] = seventh argument (optional)
         *
         */

        internal static void z_call_n()
        {

            if (Process.zargs[0] != 0)
                Process.call(zargs[0], zargc - 1, 1, 1);

        }/* z_call_n */

        /*
         * z_call_s, call a subroutine and store its result.
         *
         * 	zargs[0] = packed address of subroutine
         *	zargs[1] = first argument (optional)
         *	...
         *	zargs[7] = seventh argument (optional)
         *
         */

        internal static void z_call_s()
        {

            if (zargs[0] != 0)
                call(zargs[0], zargc - 1, 1, 0); // TODO Was "call (zargs[0], zargc - 1, zargs + 1, 0);"
            else
                store(0);

        }/* z_call_s */

        /*
         * z_check_arg_count, branch if subroutine was called with >= n arg's.
         *
         * 	zargs[0] = number of arguments
         *
         */

        internal static void z_check_arg_count()
        {

            if (main.fp == General.STACK_SIZE)
                branch(zargs[0] == 0);
            else
                branch(zargs[0] <= (zword)(main.stack[main.fp] & 0xff)); //   (*fp & 0xff));

        }/* z_check_arg_count */

        /*
         * z_jump, jump unconditionally to the given address.
         *
         *	zargs[0] = PC relative address
         *
         */

        internal static void z_jump()
        {
            long pc;

            FastMem.GET_PC(out pc);

            pc += (short)zargs[0] - 2; // TODO This actually counts on an overflow to work

            if (pc >= main.story_size)
                Err.runtime_error(ErrorCodes.ERR_ILL_JUMP_ADDR);

            FastMem.SET_PC(pc);

        }/* z_jump */

        /*
         * z_nop, no operation.
         *
         *	no zargs used
         *
         */

        internal static void z_nop()
        {

            /* Do nothing */

        }/* z_nop */

        /*
         * z_quit, stop game and exit interpreter.
         *
         *	no zargs used
         *
         */

        internal static void z_quit()
        {

            finished = 9999;

        }/* z_quit */

        /*
         * z_ret, return from a subroutine with the given value.
         *
         *	zargs[0] = value to return
         *
         */

        internal static void z_ret()
        {

            ret(zargs[0]);

        }/* z_ret */

        /*
         * z_ret_popped, return from a subroutine with a value popped off the stack.
         *
         *	no zargs used
         *
         */

        internal static void z_ret_popped()
        {

            ret(main.stack[main.sp++]);
            // ret (*sp++);

        }/* z_ret_popped */

        /*
         * z_rfalse, return from a subroutine with false (0).
         *
         * 	no zargs used
         *
         */

        internal static void z_rfalse()
        {

            ret(0);

        }/* z_rfalse */

        /*
         * z_rtrue, return from a subroutine with true (1).
         *
         * 	no zargs used
         *
         */

        internal static void z_rtrue()
        {

            ret(1);

        }/* z_rtrue */
    }
}
