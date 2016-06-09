/* screen.c - Generic screen manipulation
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
using Frotz.Other;

namespace Frotz.Generic
{

    

    internal static class Screen
    {

        const int current_window = 100;

        private struct StoryInfo
        {
            internal Story story_id;
            internal int pic;
            internal int pic1;
            internal int pic2;

            internal StoryInfo(Story story_id, int pic, int pic1, int pic2)
            {
                this.story_id = story_id;
                this.pic = pic;
                this.pic1 = pic2;
                this.pic2 = pic2;
            }

        }

        private static StoryInfo[] mapper = {
                                 new StoryInfo(Story.ZORK_ZERO, 5, 497, 498),
                                 new StoryInfo(Story.ZORK_ZERO, 6, 501, 502),
                                 new StoryInfo(Story.ZORK_ZERO, 7, 499, 500),
                                 new StoryInfo(Story.ZORK_ZERO, 8, 503, 504),
                                 new StoryInfo(Story.ARTHUR, 54, 170, 171),
                                 new StoryInfo(Story.SHOGUN, 50, 61,62),
                                 new StoryInfo(Story.UNKNOWN, 0,0,0),
                             };

        static zword font_height = 1;
        static zword font_width = 1;

        static bool input_redraw = false;
        static bool more_prompts = true;
        static bool discarding = false;
        static bool cursor = true;

        static int input_window = 0;


        internal static ZWindow[] wp = new ZWindow[8];
        private static ZWindow _cwp;
        internal static ZWindow cwp
        {
            set { _cwp = value; }
            get { return _cwp; }
        }


        /*
         * winarg0
         *
         * Return the window number in zargs[0]. In V6 only, -3 refers to the
         * current window.
         *
         */

        internal static zword winarg0()
        {

            if (main.h_version == ZMachine.V6 && (short)Process.zargs[0] == -3)
                return (zword)main.cwin;

            if (Process.zargs[0] >= ((main.h_version == ZMachine.V6) ? 8 : 2))
                Err.runtime_error(ErrorCodes.ERR_ILL_WIN);

            return Process.zargs[0];

        }/* winarg0 */

        /*
         * winarg2
         *
         * Return the (optional) window number in zargs[2]. -3 refers to the
         * current window. This optional window number was only used by some
         * V6 opcodes: set_cursor, set_margins, set_colour.
         *
         */

        internal static zword winarg2()
        {

            if (Process.zargc < 3 || (short)Process.zargs[2] == -3)
                return main.cwin;

            if (Process.zargs[2] >= 8)
                Err.runtime_error(ErrorCodes.ERR_ILL_WIN);

            return Process.zargs[2];

        }/* winarg2 */

        /*
         * update_cursor
         *
         * Move the hardware cursor to make it match the window properties.
         *
         */

        internal static void update_cursor()
        {

            os_.set_cursor(
            cwp.y_pos + cwp.y_cursor - 1,
            cwp.x_pos + cwp.x_cursor - 1);

        }/* update_cursor */

        /*
         * reset_cursor
         *
         * Reset the cursor of a given window to its initial position.
         *
         */

        static void reset_cursor(zword win)
        {
            int lines = 0;

            if (main.h_version <= ZMachine.V4 && win == 0)
                lines = wp[0].y_size / FastMem.HI(wp[0].font_size) - 1;

            wp[win].y_cursor = (zword)(FastMem.HI(wp[0].font_size) * lines + 1);
            wp[win].x_cursor = (zword)(wp[win].left + 1);

            if (win == main.cwin)
                update_cursor();

        }/* reset_cursor */

        /*
         * amiga_screen_model
         *
         * Check if the Amiga screen model should be used, required for
         * some Infocom games.
         *
         */

        internal static bool amiga_screen_model()
        {
            if (main.h_interpreter_number == ZMachine.INTERP_AMIGA)
            {

                switch (main.story_id)
                {
                    case Story.BEYOND_ZORK:
                    case Story.ZORK_ZERO:
                    case Story.SHOGUN:
                    case Story.ARTHUR:
                    case Story.JOURNEY:
                        return true;
                }
            }

            return false;

        }/* amiga_screen_model */


        /*
         * set_more_prompts
         *
         * Turn more prompts on/off.
         *
         */

        internal static void set_more_prompts(bool flag)
        {

            if (flag && !more_prompts)
                cwp.line_count = 0;

            more_prompts = flag;

        }/* set_more_prompts */

        /*
         * units_left
         *
         * Return the #screen units from the cursor to the end of the line.
         *
         */

        internal static int units_left()
        {
            if (os_.wrap_window(cwp_index()) == 0) return 999;

            return cwp.x_size - cwp.right - cwp.x_cursor + 1;

        }/* units_left */

        /*
         * get_max_width
         *
         * Return maximum width of a line in the given window. This is used in
         * connection with the extended output stream #3 call in V6.
         *
         */

        internal static zword get_max_width(zword win)
        {

            if (main.h_version == ZMachine.V6)
            {

                if (win >= 8)
                    Err.runtime_error(ErrorCodes.ERR_ILL_WIN);

                return (zword)(wp[win].x_size - wp[win].left - wp[win].right);

            }
            else return 0xffff;

        }/* get_max_width */

        /*
         * countdown
         *
         * Decrement the newline counter. Call the newline interrupt when the
         * counter hits zero. This is a helper function for screen_new_line.
         *
         */

        internal static void countdown()
        {

            if (cwp.nl_countdown != 0)
                if (--cwp.nl_countdown == 0)
                    Process.direct_call(cwp.nl_routine);

        }/* countdown */

        /*
         * screen_new_line
         *
         * Print a newline to the screen.
         *
         */

        internal static void screen_new_line()
        {

            if (discarding) return;

            if (os_.wrap_window(cwp_index()) == 0)
                os_.display_char('\n');

            /* Handle newline interrupts at the start (for most cases) */

            if (main.h_interpreter_number != ZMachine.INTERP_MSDOS || main.story_id != Story.ZORK_ZERO || main.h_release != 393)
                countdown();

            /* Check whether the last input line gets destroyed */

            if (input_window == main.cwin)
                input_redraw = true;

            /* If the cursor has not reached the bottom line, then move it to
               the next line; otherwise scroll the window or reset the cursor
               to the top left. */

            cwp.x_cursor = (zword)((cwp.left + 1));

            if (cwp.y_cursor + 2 * font_height - 1 > cwp.y_size)

                if (main.enable_scrolling)
                {

                    zword y = cwp.y_pos;
                    zword x = cwp.x_pos;

                    os_.scroll_area(y,
                            x,
                            y + cwp.y_size - 1,
                            x + cwp.x_size - 1,
                            font_height);

                }
                else cwp.y_cursor = 1;

            else cwp.y_cursor += font_height;

            update_cursor();

            /* See if we need to print a more prompt (unless the game has set
               the line counter to -999 in order to suppress more prompts). */

            if (main.enable_scrolling && (short)cwp.line_count != -999)
            {

                zword above = (zword)((cwp.y_cursor - 1) / font_height);
                zword below = (zword)((cwp.y_size - cwp.y_cursor + 1) / font_height);

                cwp.line_count++;

                if ((short)cwp.line_count >= (short)above + below - 1)
                {

                    if (more_prompts)
                        os_.more_prompt();

                    cwp.line_count = main.option_context_lines;

                }

            }

            /* Handle newline interrupts at the end for Zork Zero under DOS */

            if (main.h_interpreter_number == ZMachine.INTERP_MSDOS && main.story_id == Story.ZORK_ZERO && main.h_release == 393)
                countdown();

        }/* screen_new_line */

        /*
         * screen_char
         *
         * Display a single character on the screen.
         *
         */

        internal static void screen_char(zword c)
        {
            int width;

            if (discarding) return;

            if (c == CharCodes.ZC_INDENT && cwp.x_cursor != cwp.left + 1)
                c = ' ';

            if (units_left() < (width = os_.char_width(c)))
            {

                if (!main.enable_wrapping) { cwp.x_cursor = (zword)(cwp.x_size - cwp.right); return; }

                screen_new_line();

            }

            os_.display_char(c); cwp.x_cursor += (zword)width;

        }/* screen_char */

        /*
         * screen_word
         *
         * Display a string of characters on the screen. If the word doesn't fit
         * then use wrapping or clipping depending on the current setting of the
         * enable_wrapping flag.
         *
         */

        internal static void screen_word(zword[] buf)
        {
            int width;
            int pos = 0;

            if (discarding) return;

            if (buf[pos] == CharCodes.ZC_INDENT && cwp.x_cursor != cwp.left + 1)
                screen_char(buf[pos++]);

            if (units_left() < (width = os_.string_width(buf)))
            {

                if (!main.enable_wrapping)
                {

                    zword c;

                    while ((c = buf[pos++]) != 0)

                        if (c == CharCodes.ZC_NEW_FONT || c == CharCodes.ZC_NEW_STYLE)
                        {

                            int arg = (int)buf[pos++];

                            if (c == CharCodes.ZC_NEW_FONT)
                                os_.set_font(arg);
                            if (c == CharCodes.ZC_NEW_STYLE)
                                os_.set_text_style(arg);

                        }
                        else screen_char(c);

                    return;

                }

                if (buf[pos] == ' ' || buf[pos] == CharCodes.ZC_INDENT || buf[pos] == CharCodes.ZC_GAP)
                    width = os_.string_width(buf, ++pos);

                screen_new_line();
            }

            os_.display_string(buf, pos); cwp.x_cursor += (zword)width;

        }/* screen_word */

        /*
         * screen_write_input
         *
         * Display an input line on the screen. This is required during playback.
         *
         */

        internal static void screen_write_input(zword[] buf, zword key)
        {
            int width;

            if (units_left() < (width = os_.string_width(buf)))
                screen_new_line();

            os_.display_string(buf, 0); cwp.x_cursor += (zword)width;

            if (key == CharCodes.ZC_RETURN)
                screen_new_line();

        }/* screen_write_input */

        /*
         * screen_erase_input
         *
         * Remove an input line that has already been printed from the screen
         * as if it was deleted by the player. This could be necessary during
         * playback.
         *
         */

        internal static void screen_erase_input(zword[] buf)
        {

            if (buf[0] != 0)
            {

                int width = os_.string_width(buf);

                zword y;
                zword x;

                cwp.x_cursor -= (zword)width;

                y = (zword)(cwp.y_pos + cwp.y_cursor - 1);
                x = (zword)(cwp.x_pos + cwp.x_cursor - 1);

                os_.erase_area(y, x, y + font_height - 1, x + width - 1, -1);
                os_.set_cursor(y, x);

            }

        }/* screen_erase_input */

        /*
         * console_read_input
         *
         * Read an input line from the keyboard and return the terminating key.
         *
         */

        internal static zword console_read_input(int max, zword[] buf, zword timeout, bool continued)
        {
            zword key;
            int i;

            /* Make sure there is some space for input */

            if (main.cwin == 0 && units_left() + os_.string_width(buf) < 10 * font_width)
                screen_new_line();

            /* Make sure the input line is visible */

            if (continued && input_redraw)
                screen_write_input(buf, zword.MaxValue); // TODO second value was -1, interestingly enough

            input_window = main.cwin;
            input_redraw = false;

            /* Get input line from IO interface */

            cwp.x_cursor -= (zword)os_.string_width(buf);
            key = os_.read_line(max, buf, timeout, units_left(), continued);
            cwp.x_cursor += (zword)os_.string_width(buf);

            if (key != CharCodes.ZC_TIME_OUT)
                for (i = 0; i < 8; i++)
                    wp[i].line_count = 0;

            /* Add a newline if the input was terminated normally */

            if (key == CharCodes.ZC_RETURN)
                screen_new_line();

            return key;

        }/* console_read_input */

        /*
         * console_read_key
         *
         * Read a single keystroke and return it.
         *
         */

        internal static zword console_read_key(zword timeout)
        {
            zword key;
            int i;

            key = os_.read_key(timeout, cursor);

            if (key != CharCodes.ZC_TIME_OUT)
                for (i = 0; i < 8; i++)
                    wp[i].line_count = 0;

            return key;

        }/* console_read_key */

        /*
         * update_attributes
         *
         * Set the three enable_*** variables to make them match the attributes
         * of the current window.
         *
         */

        internal static void update_attributes()
        {
            zword attr = cwp.attribute;

            main.enable_wrapping = (attr & 1) > 0;
            main.enable_scrolling = (attr & 2) > 0;
            main.enable_scripting = (attr & 4) > 0;
            main.enable_buffering = (attr & 8) > 0;

            /* Some story files forget to select wrapping for printing hints */

            if (main.story_id == Story.ZORK_ZERO && main.h_release == 366)
                if (main.cwin == 0)
                    main.enable_wrapping = true;
            if (main.story_id == Story.SHOGUN && main.h_release <= 295)
                if (main.cwin == 0)
                    main.enable_wrapping = true;

        }/* update_attributes */

        /*
         * refresh_text_style
         *
         * Set the right text style. This can be necessary when the fixed font
         * flag is changed, or when a new window is selected, or when the game
         * uses the set_text_style opcode.
         *
         */

        internal static void refresh_text_style()
        {
            zword style;

            if (main.h_version != ZMachine.V6)
            {

                style = wp[0].style;

                if (main.cwin != 0 || (main.h_flags & ZMachine.FIXED_FONT_FLAG) > 0)
                    style |= ZStyles.FIXED_WIDTH_STYLE;

            }
            else style = cwp.style;

            if (!main.ostream_memory && main.ostream_screen && main.enable_buffering)
            {

                Buffer.print_char(CharCodes.ZC_NEW_STYLE);
                Buffer.print_char(style);

            }
            else os_.set_text_style(style);

        }/* refresh_text_style */

        /*
         * set_window
         *
         * Set the current window. In V6 every window has its own set of window
         * properties such as colours, text style, cursor position and size.
         *
         */

        static void set_window(zword win)
        {

            Buffer.flush_buffer();

            main.cwin = win; cwp = wp[win];

            update_attributes();

            if (main.h_version == ZMachine.V6)
            {

                os_.set_colour(FastMem.LO(cwp.colour), FastMem.HI(cwp.colour));

                if (os_.font_data(cwp.font, ref font_height, ref font_width))
                    os_.set_font(cwp.font);

                os_.set_text_style(cwp.style);

            }
            else refresh_text_style();

            if (main.h_version != ZMachine.V6 && win != 0)
            {
                wp[win].y_cursor = 1;
                wp[win].x_cursor = 1;
            }

            update_cursor();

            os_.set_active_window(win);

        }/* set_window */

        /*
         * erase_window
         *
         * Erase a window to background colour.
         *
         */

        internal static void erase_window(zword win)
        {
            zword y = wp[win].y_pos;
            zword x = wp[win].x_pos;

            if (main.h_version == ZMachine.V6 && win != main.cwin && !Screen.amiga_screen_model())
                os_.set_colour(FastMem.LO(wp[win].colour), FastMem.HI(wp[win].colour));

            if (FastMem.HI(wp[win].colour) != ZColor.TRANSPARENT_COLOUR)
            {

                os_.erase_area(y,
                           x,
                           y + wp[win].y_size - 1,
                           x + wp[win].x_size - 1,
                           win);

            }

            if (main.h_version == ZMachine.V6 && win != main.cwin && !amiga_screen_model())
                os_.set_colour(FastMem.LO(cwp.colour), FastMem.HI(cwp.colour));

            reset_cursor(win);

            wp[win].line_count = 0;

        }/* erase_window */

        /*
         * split_window
         *
         * Divide the screen into upper (1) and lower (0) windows. In V3 the upper
         * window appears below the status line.
         *
         */

        internal static void split_window(zword height)
        {
            zword stat_height = 0;

            Buffer.flush_buffer();

            /* Calculate height of status line and upper window */

            if (main.h_version != ZMachine.V6)
                height *= FastMem.HI(wp[1].font_size);

            if (main.h_version <= ZMachine.V3)
            {
                stat_height = FastMem.HI(wp[7].font_size);
                wp[7].y_size = stat_height;
                os_.set_window_size(7, wp[7]);
            }
            else
            {
                wp[7].y_size = 0;
            }

            /* Cursor of upper window mustn't be swallowed by the lower window */

            wp[1].y_cursor += (zword)(wp[1].y_pos - 1 - stat_height);

            wp[1].y_pos = (zword)(1 + stat_height);
            wp[1].y_size = height;

            if ((short)wp[1].y_cursor > (short)wp[1].y_size)
                reset_cursor(1);

            /* Cursor of lower window mustn't be swallowed by the upper window */

            wp[0].y_cursor += (zword)(wp[0].y_pos - 1 - stat_height - height);

            wp[0].y_pos = (zword)(1 + stat_height + height);
            wp[0].y_size = (zword)(main.h_screen_height - stat_height - height);

            if ((short)wp[0].y_cursor < 1)
                reset_cursor(0);

            /* Erase the upper window in V3 only */

            if (main.h_version == ZMachine.V3 && height != 0)
                erase_window(1);

            os_.set_window_size(0, wp[0]);
            os_.set_window_size(1, wp[1]);

        }/* split_window */

        /*
         * erase_screen
         *
         * Erase the entire screen to background colour.
         *
         */

        static void erase_screen(zword win)
        {
            int i;

            if (FastMem.HI(cwp.colour) != ZColor.TRANSPARENT_COLOUR)
                os_.erase_area(1, 1, main.h_screen_height, main.h_screen_width, -2);

            if (win == zword.MaxValue)
            {
                split_window(0);
                set_window(0);
                reset_cursor(0);
            }

            for (i = 0; i < 8; i++)
                wp[i].line_count = 0;

        }/* erase_screen */

        ///*
        // * resize_screen
        // *
        // * Try to adapt the window properties to a new screen size.
        // *
        // */

        //void resize_screen (void)
        //{

        //    if (h_version != V6) {

        //    int h = wp[0].y_pos + wp[0].y_size;

        //    wp[0].x_size = h_screen_width;
        //    wp[1].x_size = h_screen_width;
        //    wp[7].x_size = h_screen_width;

        //    wp[0].y_size = h_screen_height - wp[1].y_size;
        //    if (h_version <= V3)
        //        wp[0].y_size -= hi (wp[7].font_size);

        //    if (os_font_data (TEXT_FONT, &font_height, &font_width)) {

        //        int i;
        //        for (i = 0; i < 8; i++)
        //        wp[i].font_size = (font_height << 8) | font_width;
        //    }

        //    if (cwin == 0) {

        //        int lines = wp[0].y_cursor + font_height - wp[0].y_size - 1;

        //        if (lines > 0) {

        //        if (lines % font_height != 0)
        //            lines += font_height;
        //        lines /= font_height;

        //        if (wp[0].y_cursor > (font_height * lines)) {

        //            os_scroll_area (wp[0].y_pos,
        //                    wp[0].x_pos,
        //                    h - 1,
        //                    wp[0].x_pos + wp[0].x_size - 1,
        //                    font_height * lines);
        //            wp[0].y_cursor -= (font_height * lines);
        //            update_cursor ();
        //        }
        //        }
        //    }

        //    os_window_height (0, wp[0].y_size);

        //    }

        //}/* resize_screen */

        /*
         * restart_screen
         *
         * Prepare the screen for a new game.
         *
         */

        internal static void restart_screen()
        {
            /* Use default settings */

            os_.set_colour(main.h_default_foreground, main.h_default_background);

            if (os_.font_data(ZFont.TEXT_FONT, ref font_height, ref font_width))
                os_.set_font(ZFont.TEXT_FONT);

            os_.set_text_style(0);

            cursor = true;

            /* Initialise window properties */

            //mwin = 1;

            for (int i = 0; i < 8; i++)
            {
                wp[i] = new ZWindow();
                wp[i].y_pos = 1;
                wp[i].x_pos = 1;
                wp[i].y_size = 0;
                wp[i].x_size = 0;
                wp[i].y_cursor = 1;
                wp[i].x_cursor = 1;
                wp[i].left = 0;
                wp[i].right = 0;
                wp[i].nl_routine = 0;
                wp[i].nl_countdown = 0;
                wp[i].style = 0;
                wp[i].colour = (ushort)((main.h_default_background << 8) | main.h_default_foreground);
                wp[i].font = ZFont.TEXT_FONT;
                wp[i].font_size = (ushort)((font_height << 8) | font_width);
                wp[i].attribute = 8;
                wp[i].true_fore = main.hx_fore_colour;
                wp[i].true_back = main.hx_back_colour;

                wp[i].index = i;
            }

            cwp = wp[0];

            /* Prepare lower/upper windows and status line */

            wp[0].attribute = 15;

            wp[0].left = main.option_left_margin;
            wp[0].right = main.option_right_margin;

            wp[0].x_size = main.h_screen_width;
            wp[1].x_size = main.h_screen_width;

            if (main.h_version <= ZMachine.V3)
                wp[7].x_size = main.h_screen_width;

            os_.restart_game(ZMachine.RESTART_WPROP_SET);
            /* Clear the screen, unsplit it and select window 0 */

            Screen.erase_screen(ushort.MaxValue);
        }/* restart_screen */

        /*
         * validate_click
         *
         * Return false if the last mouse click occured outside the current
         * mouse window; otherwise write the mouse arrow coordinates to the
         * memory of the header extension table and return true.
         *
         */

        internal static bool validate_click()
        {

            if (main.mwin >= 0)
            {

                if (main.mouse_y < wp[main.mwin].y_pos || main.mouse_y >= wp[main.mwin].y_pos + wp[main.mwin].y_size)
                    return false;
                if (main.mouse_x < wp[main.mwin].x_pos || main.mouse_x >= wp[main.mwin].x_pos + wp[main.mwin].x_size)
                    return false;

            }
            else
            {

                if (main.mouse_y < 1 || main.mouse_y > main.h_screen_height)
                    return false;
                if (main.mouse_x < 1 || main.mouse_x > main.h_screen_width)
                    return false;
            }

            main.hx_mouse_y = main.mouse_y;
            main.hx_mouse_x = main.mouse_x;

            if (main.h_version != ZMachine.V6)
            {
                main.hx_mouse_y = (ushort)((main.hx_mouse_y - 1) / main.h_font_height + 1);
                main.hx_mouse_x = (ushort)((main.hx_mouse_x - 1) / main.h_font_width + 1);
            }

            FastMem.set_header_extension(ZMachine.HX_MOUSE_Y, main.hx_mouse_y);
            FastMem.set_header_extension(ZMachine.HX_MOUSE_X, main.hx_mouse_x);

            return true;
        }/* validate_click */

        /*
         * screen_mssg_on
         *
         * Start printing a so-called debugging message. The contents of the
         * message are passed to the message stream, a Frotz specific output
         * stream with maximum priority.
         *
         */

        internal static void screen_mssg_on()
        {

            if (main.cwin == 0)
            {		/* messages in window 0 only */

                os_.set_text_style(0);

                if (cwp.x_cursor != cwp.left + 1)
                    screen_new_line();

                screen_char(CharCodes.ZC_INDENT);

            }
            else discarding = true; 	/* discard messages in other windows */

        }/* screen_mssg_on */

        /*
         * screen_mssg_off
         *
         * Stop printing a "debugging" message.
         *
         */

        internal static void screen_mssg_off()
        {

            if (main.cwin == 0)
            {		/* messages in window 0 only */

                screen_new_line();

                refresh_text_style();

            }
            else discarding = false; 	/* message has been discarded */

        }/* screen_mssg_off */

        /*
         * z_buffer_mode, turn text buffering on/off.
         *
         *	zargs[0] = new text buffering flag (0 or 1)
         *
         */

        internal static void z_buffer_mode()
        {

            /* Infocom's V6 games rarely use the buffer_mode opcode. If they do
               then only to print text immediately, without any delay. This was
               used to give the player some sign of life while the game was
               spending much time on parsing a complicated input line. (To turn
               off word wrapping, V6 games use the window_style opcode instead.)
               Today we can afford to ignore buffer_mode in V6. */

            if (main.h_version != ZMachine.V6)
            {

                Buffer.flush_buffer();

                zword temp = 8; // TODO No idea what this math will be like //'

                wp[0].attribute &= (zword)(~temp);

                if (Process.zargs[0] != 0)
                    wp[0].attribute |= 8;

                update_attributes();

            }

        }/* z_buffer_mode */

        /*
         * z_draw_picture, draw a picture.
         *
         *	zargs[0] = number of picture to draw
         *	zargs[1] = y-coordinate of top left corner
         *	zargs[2] = x-coordinate of top left corner
         *
         */

        internal static void z_draw_picture()
        {
            zword pic = Process.zargs[0];

            zword y = Process.zargs[1];
            zword x = Process.zargs[2];

            int i;

            Buffer.flush_buffer();

            if (y == 0)			/* use cursor line if y-coordinate is 0 */
                y = cwp.y_cursor;
            if (x == 0)    		/* use cursor column if x-coordinate is 0 */
                x = cwp.x_cursor;

            y += (zword)(cwp.y_pos - 1);
            x += (zword)(cwp.x_pos - 1);

            /* The following is necessary to make Amiga and Macintosh story
               files work with MCGA graphics files.  Some screen-filling
               pictures of the original Amiga release like the borders of
               Zork Zero were split into several MCGA pictures (left, right
               and top borders).  We pretend this has not happened. */

            for (i = 0; mapper[i].story_id != Story.UNKNOWN; i++)
            {
                if (main.story_id == mapper[i].story_id && pic == mapper[i].pic)
                {

                    int height1, width1;
                    int height2, width2;

                    int delta = 0;

                    os_.picture_data(pic, out height1, out width1);
                    os_.picture_data(mapper[i].pic2, out height2, out width2);

                    if (main.story_id == Story.ARTHUR && pic == 54)
                        delta = main.h_screen_width / 160;

                    os_.draw_picture(mapper[i].pic1, y + height1, x + delta);
                    os_.draw_picture(mapper[i].pic2, y + height1, x + width1 - width2 - delta);
                }
            }

            os_.draw_picture(pic, y, x);

            if (main.story_id == Story.SHOGUN)

                if (pic == 3)
                {

                    int height, width;

                    os_.picture_data(59, out height, out width);
                }

        }/* z_draw_picture */

        /*
         * z_erase_line, erase the line starting at the cursor position.
         *
         *	zargs[0] = 1 + #units to erase (1 clears to the end of the line)
         *
         */

        internal static void z_erase_line()
        {
            // TODO This has never been hit...
            zword pixels = Process.zargs[0];
            zword y, x;

            Buffer.flush_buffer();

            /* Do nothing if the background is transparent */

            if (FastMem.HI(cwp.colour) == ZColor.TRANSPARENT_COLOUR)
                return;

            /* Clipping at the right margin of the current window */

            if (--pixels == 0 || pixels > units_left())
                pixels = (zword)units_left();

            /* Erase from cursor position */

            y = (zword)(cwp.y_pos + cwp.y_cursor - 1);
            x = (zword)(cwp.x_pos + cwp.x_cursor - 1);

            os_.erase_area(y, x, y + font_height - 1, x + pixels - 1, -1);

        }/* z_erase_line */

        /*
         * z_erase_picture, erase a picture with background colour.
         *
         *	zargs[0] = number of picture to erase
         *	zargs[1] = y-coordinate of top left corner (optional)
         *	zargs[2] = x-coordinate of top left corner (optional)
         *
         */

        internal static void z_erase_picture()
        {
            int height, width;

            zword y = Process.zargs[1];
            zword x = Process.zargs[2];

            Buffer.flush_buffer();

            /* Do nothing if the background is transparent */

            if (FastMem.HI(cwp.colour) == ZColor.TRANSPARENT_COLOUR)
                return;

            if (y == 0)		/* use cursor line if y-coordinate is 0 */
                y = cwp.y_cursor;
            if (x == 0)    	/* use cursor column if x-coordinate is 0 */
                x = cwp.x_cursor;

            os_.picture_data(Process.zargs[0], out height, out width);

            y += (zword)(cwp.y_pos - 1);
            x += (zword)(cwp.x_pos - 1);

            os_.erase_area(y, x, y + height - 1, x + width - 1, -1);

        }/* z_erase_picture */

        /*
         * z_erase_window, erase a window or the screen to background colour.
         *
         *	zargs[0] = window (-3 current, -2 screen, -1 screen & unsplit)
         *
         */

        internal static void z_erase_window()
        {

            Buffer.flush_buffer();

            if ((short)Process.zargs[0] == -1 || (short)Process.zargs[0] == -2)
                erase_screen(Process.zargs[0]);
            else
                erase_window(winarg0());

        }/* z_erase_window */

        /*
         * z_get_cursor, write the cursor coordinates into a table.
         *
         *	zargs[0] = address to write information to
         *
         */

        internal static void z_get_cursor()
        {
            zword y, x;

            Buffer.flush_buffer();

            y = cwp.y_cursor;
            x = cwp.x_cursor;

            if (main.h_version != ZMachine.V6)
            {	/* convert to grid positions */
                y = (zword)((y - 1) / main.h_font_height + 1);
                x = (zword)((x - 1) / main.h_font_width + 1);
            }

            FastMem.storew((zword)(Process.zargs[0] + 0), y);
            FastMem.storew((zword)(Process.zargs[0] + 2), x);

        }/* z_get_cursor */

        /*
         * z_get_wind_prop, store the value of a window property.
         *
         *	zargs[0] = window (-3 is the current one)
         *	zargs[1] = number of window property to be stored
         *
         */

        internal static void z_get_wind_prop()
        {
            Buffer.flush_buffer();

            if (Process.zargs[1] < 16)
            {
                // Process.store(((zword*)(wp + winarg0()))[Process.zargs[1]]);
                // This is a nasty, nasty piece of code
                Process.store(wp[winarg0()][Process.zargs[1]]);
                // Process.store((wp[winarg0()].union[Process.zargs[1]]

            }
            else if (Process.zargs[1] == 16)
                Process.store(os_.to_true_colour(FastMem.LO(wp[winarg0()].colour)));

            else if (Process.zargs[1] == 17)
            {

                zword bg = FastMem.HI(wp[winarg0()].colour);

                if (bg == ZColor.TRANSPARENT_COLOUR)
                {
                    unchecked
                    {
                        Process.store((zword)(-4));
                    }
                }
                else
                    Process.store(os_.to_true_colour(bg));

            }
            else
                Err.runtime_error(ErrorCodes.ERR_ILL_WIN_PROP);

        }/* z_get_wind_prop */

        /*
         * z_mouse_window, select a window as mouse window.
         *
         *	zargs[0] = window number (-3 is the current) or -1 for the screen
         *
         */

        internal static void z_mouse_window()
        {

            main.mwin = ((short)Process.zargs[0] == -1) ? -1 : Screen.winarg0();

        }/* z_mouse_window */

        /*
         * z_move_window, place a window on the screen.
         *
         *	zargs[0] = window (-3 is the current one)
         *	zargs[1] = y-coordinate
         *	zargs[2] = x-coordinate
         *
         */

        internal static void z_move_window()
        {
            zword win = winarg0();

            Buffer.flush_buffer();

            wp[win].y_pos = Process.zargs[1];
            wp[win].x_pos = Process.zargs[2];

            if (win == main.cwin)
                update_cursor();

        }/* z_move_window */

        /*
         * z_picture_data, get information on a picture or the graphics file.
         *
         *	zargs[0] = number of picture or 0 for the graphics file
         *	zargs[1] = address to write information to
         *
         */

        internal static void z_picture_data()
        {
            zword pic = Process.zargs[0];
            zword table = Process.zargs[1];

            int height, width;
            int i;

            bool avail = os_.picture_data(pic, out height, out width);

            for (i = 0; mapper[i].story_id != Story.UNKNOWN; i++)

                if (main.story_id == mapper[i].story_id)
                {

                    if (pic == mapper[i].pic)
                    {

                        int height2, width2;

                        avail &= os_.picture_data(mapper[i].pic1, out height2, out width2);
                        avail &= os_.picture_data(mapper[i].pic2, out height2, out width2);

                        height += height2;

                    }
                    else if (pic == mapper[i].pic1 || pic == mapper[i].pic2)

                        avail = false;
                }

            FastMem.storew((zword)(table + 0), (zword)(height));
            FastMem.storew((zword)(table + 2), (zword)(width));

            Process.branch(avail);

        }/* z_picture_data */

        /*
         * z_picture_table, prepare a group of pictures for faster display.
         *
         *	zargs[0] = address of table holding the picture numbers
         *
         */

        internal static void z_picture_table()
        {

            /* This opcode is used by Shogun and Zork Zero when the player
               encounters built-in games such as Peggleboz. Nowadays it is
               not very helpful to hold the picture data in memory because
               even a small disk cache avoids re-loading of data. */

        }/* z_picture_table */

        /*
         * z_buffer_screen, set the screen buffering mode.
         *
         *	zargs[0] = mode
         *
         */

        internal static void z_buffer_screen()
        {
            // TODO This wants to cast the zword to a negative... I'd like to know why
            unchecked
            {
                Process.store((zword)os_.buffer_screen((Process.zargs[0] == (zword)(-1)) ? -1 : Process.zargs[0]));
            }

        }/* z_buffer_screen */

        /*
         * z_print_table, print ASCII text in a rectangular area.
         *
         *	zargs[0] = address of text to be printed
         *	zargs[1] = width of rectangular area
         *	zargs[2] = height of rectangular area (optional)
         *	zargs[3] = number of char's to skip between lines (optional)
         *
         */

        internal static void z_print_table()
        {
            zword addr = Process.zargs[0];
            zword x;
            int i, j;

            Buffer.flush_buffer();

            /* Supply default arguments */

            if (Process.zargc < 3)
                Process.zargs[2] = 1;
            if (Process.zargc < 4)
                Process.zargs[3] = 0;

            /* Write text in width x height rectangle */

            x = cwp.x_cursor;

            for (i = 0; i < Process.zargs[2]; i++)
            {

                if (i != 0)
                {

                    Buffer.flush_buffer();

                    cwp.y_cursor += font_height;
                    cwp.x_cursor = x;

                    update_cursor();

                }

                for (j = 0; j < Process.zargs[1]; j++)
                {

                    zbyte c;

                    FastMem.LOW_BYTE(addr, out c);
                    addr++;

                    Buffer.print_char(c);

                }

                addr += Process.zargs[3];

            }

        }/* z_print_table */

        /*
         * z_put_wind_prop, set the value of a window property.
         *
         *	zargs[0] = window (-3 is the current one)
         *	zargs[1] = number of window property to set
         *	zargs[2] = value to set window property to
         *
         */

        internal static void z_put_wind_prop()
        {

            Buffer.flush_buffer();

            if (Process.zargs[1] >= 16)
                Err.runtime_error(ErrorCodes.ERR_ILL_WIN_PROP);

            // ((zword *) (wp + winarg0 ())) [zargs[1]] = zargs[2];

            // This is still a wicked evil piece of codee
            wp[winarg0()][Process.zargs[1]] = Process.zargs[2];


        }/* z_put_wind_prop */

        /*
         * z_scroll_window, scroll a window up or down.
         *
         *	zargs[0] = window (-3 is the current one)
         *	zargs[1] = #screen units to scroll up (positive) or down (negative)
         *
         */

        internal static void z_scroll_window()
        {
            zword win = winarg0();
            zword y, x;

            Buffer.flush_buffer();

            /* Use the correct set of colours when scrolling the window */

            if (win != main.cwin && !amiga_screen_model())
                os_.set_colour(FastMem.LO(wp[win].colour), FastMem.HI(wp[win].colour));

            y = wp[win].y_pos;
            x = wp[win].x_pos;

            os_.scroll_area(y,
                    x,
                    y + wp[win].y_size - 1,
                    x + wp[win].x_size - 1,
                    (short)Process.zargs[1]);

            if (win != main.cwin && !amiga_screen_model())
                os_.set_colour(FastMem.LO(cwp.colour), FastMem.HI(cwp.colour));

        }/* z_scroll_window */

        /*
         * z_set_colour, set the foreground and background colours.
         *
         *	zargs[0] = foreground colour
         *	zargs[1] = background colour
         *	zargs[2] = window (-3 is the current one, optional)
         *
         */

        internal static void z_set_colour()
        {
            zword win = (zword)((main.h_version == ZMachine.V6) ? winarg2() : 0);

            zword fg = Process.zargs[0];
            zword bg = Process.zargs[1];

            Buffer.flush_buffer();

            if ((short)fg == -1)	/* colour -1 is the colour at the cursor */
                fg = os_.peek_colour();
            if ((short)bg == -1)
                bg = os_.peek_colour();

            if (fg == 0)		/* colour 0 means keep current colour */
                fg = FastMem.LO(wp[win].colour);
            if (bg == 0)
                bg = FastMem.HI(wp[win].colour);

            if (fg == 1)		/* colour 1 is the system default colour */
                fg = main.h_default_foreground;
            if (bg == 1)
                bg = main.h_default_background;

            if (fg == ZColor.TRANSPARENT_COLOUR)
                fg = FastMem.LO(wp[win].colour);
            if (bg == ZColor.TRANSPARENT_COLOUR && ((main.hx_flags & ZMachine.TRANSPARENT_FLAG) == 0))
                bg = FastMem.HI(wp[win].colour);

            if (main.h_version == ZMachine.V6 && amiga_screen_model())

                /* Changing colours of window 0 affects the entire screen */

                if (win == 0)
                {

                    int i;

                    for (i = 1; i < 8; i++)
                    {

                        zword bg2 = FastMem.HI(wp[i].colour);
                        zword fg2 = FastMem.LO(wp[i].colour);

                        if (bg2 < 16)
                            bg2 = (bg2 == FastMem.LO(wp[0].colour)) ? fg : bg;
                        if (fg2 < 16)
                            fg2 = (fg2 == FastMem.LO(wp[0].colour)) ? fg : bg;

                        wp[i].colour = (zword)((bg2 << 8) | fg2);

                    }

                }

            wp[win].colour = (zword)((bg << 8) | fg);

            if (win == main.cwin || main.h_version != ZMachine.V6)
                os_.set_colour(fg, bg);

        }/* z_set_colour */

        /*
         * z_set_true_colour, set the foreground and background colours
         * to specific RGB colour values.
         *
         *	zargs[0] = foreground colour
         *	zargs[1] = background colour
         *	zargs[2] = window (-3 is the current one, optional)
         *
         */

        internal static void z_set_true_colour()
        {
            zword win = (zword)((main.h_version == ZMachine.V6) ? winarg2() : 0);

            zword true_fg = Process.zargs[0];
            zword true_bg = Process.zargs[1];

            zword fg = 0;
            zword bg = 0;

            Buffer.flush_buffer();

            switch ((short)true_fg)
            {

                case -1:	/* colour -1 is the system default colour */
                    fg = main.h_default_foreground;
                    break;

                case -2:	/* colour -2 means keep current colour */
                    fg = FastMem.LO(wp[win].colour);
                    break;

                case -3:	/* colour -3 is the colour at the cursor */
                    fg = os_.peek_colour();
                    break;

                case -4:
                    fg = FastMem.LO(wp[win].colour);
                    break;

                default:
                    fg = os_.from_true_colour(true_fg);
                    break;
            }

            switch ((short)true_bg)
            {

                case -1:	/* colour -1 is the system default colour */
                    bg = main.h_default_background;
                    break;

                case -2:	/* colour -2 means keep current colour */
                    bg = FastMem.HI(wp[win].colour);
                    break;

                case -3:	/* colour -3 is the colour at the cursor */
                    bg = os_.peek_colour();
                    break;

                case -4:	/* colour -4 means transparent */
                    if ((main.hx_flags & ZMachine.TRANSPARENT_FLAG) > 0)
                        bg = ZColor.TRANSPARENT_COLOUR;
                    else
                        bg = FastMem.HI(wp[win].colour);
                    break;

                default:
                    bg = os_.from_true_colour(true_bg);
                    break;
            }

            wp[win].colour = (zword)((bg << 8) | fg);

            if (win == main.cwin || main.h_version != ZMachine.V6)
                os_.set_colour(fg, bg);

        }/* z_set_true_colour */

        /*
         * z_set_font, set the font for text output and store the previous font.
         *
         * 	zargs[0] = number of font or 0 to keep current font
         *	zargs[1] = window (-3 is the current one, optional)
         *
         */

        internal static void z_set_font()
        {
            zword font = Process.zargs[0];
            zword win = 0;

            if (main.h_version == ZMachine.V6)
            {

                if (Process.zargc < 2 || (short)Process.zargs[1] == -3)
                    win = main.cwin;
                else if (Process.zargs[1] >= 8)
                    Err.runtime_error(ErrorCodes.ERR_ILL_WIN);
                else
                    win = Process.zargs[1];

            }

            if (font != 0)
            {

                if (os_.font_data(font, ref font_height, ref font_width))
                {

                    Process.store(wp[win].font);

                    wp[win].font = font;
                    wp[win].font_size = (zword)((font_height << 8) | font_width);

                    if ((main.h_version != ZMachine.V6) || (win == main.cwin))
                    {

                        if (!main.ostream_memory && main.ostream_screen && main.enable_buffering)
                        {

                            Buffer.print_char(CharCodes.ZC_NEW_FONT);
                            Buffer.print_char(font);

                        }
                        else os_.set_font(font);

                    }

                }
                else Process.store(0);

            }
            else Process.store(wp[win].font);

        }/* z_set_font */

        /*
         * z_set_cursor, set the cursor position or turn the cursor on/off.
         *
         *	zargs[0] = y-coordinate or -2/-1 for cursor on/off
         *	zargs[1] = x-coordinate
         *	zargs[2] = window (-3 is the current one, optional)
         *
         */

        internal static void z_set_cursor()
        {
            zword win = (zword)((main.h_version == ZMachine.V6) ? winarg2() : 1);

            zword y = Process.zargs[0];
            zword x = Process.zargs[1];

            Buffer.flush_buffer();

            /* Supply default arguments */

            if (Process.zargc < 3)
                Process.zargs[2] = current_window;

            /* Handle cursor on/off */

            if ((short)y < 0)
            {

                if ((short)y == -2)
                    cursor = true;
                if ((short)y == -1)
                    cursor = false;

                return;

            }

            /* Convert grid positions to screen units if this is not V6 */

            if (main.h_version != ZMachine.V6)
            {

                if (main.cwin == 0)
                    return;

                y = (zword)((y - 1) * main.h_font_height + 1);
                x = (zword)((x - 1) * main.h_font_width + 1);

            }

            /* Protect the margins */

            if (y == 0)			/* use cursor line if y-coordinate is 0 */
                y = wp[win].y_cursor;
            if (x == 0)			/* use cursor column if x-coordinate is 0 */
                x = wp[win].x_cursor;
            if (x <= wp[win].left || x > wp[win].x_size - wp[win].right)
                x = (zword)(wp[win].left + 1);

            /* Move the cursor */

            wp[win].y_cursor = y;
            wp[win].x_cursor = x;

            if (win == main.cwin)
                update_cursor();

        }/* z_set_cursor */

        /*
         * z_set_margins, set the left and right margins of a window.
         *
         *	zargs[0] = left margin in pixels
         *	zargs[1] = right margin in pixels
         *	zargs[2] = window (-3 is the current one, optional)
         *
         */

        internal static void z_set_margins()
        {
            zword win = winarg2();

            Buffer.flush_buffer();

            wp[win].left = Process.zargs[0];
            wp[win].right = Process.zargs[1];

            /* Protect the margins */

            if (wp[win].x_cursor <= Process.zargs[0] || wp[win].x_cursor > wp[win].x_size - Process.zargs[1])
            {

                wp[win].x_cursor = (zword)(Process.zargs[0] + 1);

                if (win == main.cwin)
                    update_cursor();

            }

        }/* z_set_margins */

        /*
         * z_set_text_style, set the style for text output.
         *
         * 	zargs[0] = style flags to set or 0 to reset text style
         *
         */

        internal static void z_set_text_style()
        {
            zword win = (zword)((main.h_version == ZMachine.V6) ? main.cwin : 0);
            zword style = Process.zargs[0];

            wp[win].style |= style;

            if (style == 0)
                wp[win].style = 0;

            refresh_text_style();

        }/* z_set_text_style */

        /*
         * z_set_window, select the current window.
         *
         *	zargs[0] = window to be selected (-3 is the current one)
         *
         */

        internal static void z_set_window()
        {

            set_window(winarg0());

        }/* z_set_window */

        /*
         * pad_status_line
         *
         * Pad the status line with spaces up to the given position.
         *
         */

        static void pad_status_line(int column)
        {
            int spaces;

            Buffer.flush_buffer();

            spaces = units_left() / os_.char_width(' ') - column;

            /* while (spaces--) */
            /* Justin Wesley's fix for narrow displays (Agenda PDA) */
            while (spaces-- > 0)
                screen_char(' ');

        }/* pad_status_line */

        /*
         * z_show_status, display the status line for V1 to V3 games.
         *
         *	no zargs used
         *
         */

        internal static void z_show_status()
        {
            zword global0;
            zword global1;
            zword global2;
            zword addr;

            bool brief = false;

            /* One V5 game (Wishbringer Solid Gold) contains this opcode by
               accident, so just return if the version number does not fit */

            if (main.h_version >= ZMachine.V4)
                return;

            /* Read all relevant global variables from the memory of the
               Z-machine into local variables */

            addr = main.h_globals;
            FastMem.LOW_WORD(addr, out global0);
            addr += 2;
            FastMem.LOW_WORD(addr, out global1);
            addr += 2;
            FastMem.LOW_WORD(addr, out global2);

            /* Frotz uses window 7 for the status line. Don't forget to select
               reverse and fixed width text style */

            set_window(7);

            Buffer.print_char(CharCodes.ZC_NEW_STYLE);
            Buffer.print_char((zword)(ZStyles.REVERSE_STYLE | ZStyles.FIXED_WIDTH_STYLE));

            /* If the screen width is below 55 characters then we have to use
               the brief status line format */

            if (main.h_screen_cols < 55)
                brief = true;

            /* Print the object description for the global variable 0 */

            Buffer.print_char(' ');
            Text.print_object(global0);

            /* A header flag tells us whether we have to display the current
               time or the score/moves information */

            if ((main.h_config & ZMachine.CONFIG_TIME) > 0)
            {	/* print hours and minutes */

                zword hours = (zword)((global1 + 11) % 12 + 1);

                pad_status_line(brief ? 15 : 20);

                Text.print_string("Time: ");

                if (hours < 10)
                    Buffer.print_char(' ');
                Text.print_num(hours);

                Buffer.print_char(':');

                if (global2 < 10)
                    Buffer.print_char('0');
                Text.print_num(global2);

                Buffer.print_char(' ');

                Buffer.print_char((global1 >= 12) ? 'p' : 'a');
                Buffer.print_char('m');

            }
            else
            {				/* print score and moves */

                pad_status_line(brief ? 15 : 30);

                Text.print_string(brief ? "S: " : "Score: ");
                Text.print_num(global1);

                pad_status_line(brief ? 8 : 14);

                Text.print_string(brief ? "M: " : "Moves: ");
                Text.print_num(global2);

            }

            /* Pad the end of the status line with spaces */

            pad_status_line(0);

            /* Return to the lower window */

            set_window(0);

        }/* z_show_status */

        /*
         * z_split_window, split the screen into an upper (1) and lower (0) window.
         *
         *	zargs[0] = height of upper window in screen units (V6) or #lines
         *
         */

        internal static void z_split_window()
        {

            split_window(Process.zargs[0]);

        }/* z_split_window */

        /*
         * z_window_size, change the width and height of a window.
         *
         *	zargs[0] = window (-3 is the current one)
         *	zargs[1] = new height in screen units
         *	zargs[2] = new width in screen units
         *
         */

        internal static void z_window_size()
        {
            zword win = Screen.winarg0();

            Buffer.flush_buffer();

            wp[win].y_size = Process.zargs[1];
            wp[win].x_size = Process.zargs[2];

            /* Keep the cursor within the window */

            if (wp[win].y_cursor > Process.zargs[1] || wp[win].x_cursor > Process.zargs[2])
                reset_cursor(win);

            os_.set_window_size(win, wp[win]);

        }/* z_window_size */

        /*
         * z_window_style, set / clear / toggle window attributes.
         *
         *	zargs[0] = window (-3 is the current one)
         *	zargs[1] = window attribute flags
         *	zargs[2] = operation to perform (optional, defaults to 0)
         *
         */

        internal static void z_window_style()
        {
            zword win = winarg0();
            zword flags = Process.zargs[1];

            Buffer.flush_buffer();

            /* Supply default arguments */

            if (Process.zargc < 3)
                Process.zargs[2] = 0;

            /* Set window style */

            switch (Process.zargs[2])
            {
                case 0: wp[win].attribute = flags; break;
                case 1: wp[win].attribute |= flags; break;
                case 2: wp[win].attribute &= (zword)(~flags); break;
                case 3: wp[win].attribute ^= flags; break;
            }

            if (main.cwin == win)
                update_attributes();

        }/* z_window_style */

        ///*
        // * get_window_colours
        // *
        // * Get the colours for a given window.
        // *
        // */

        //void get_window_colours (zword win, zbyte* fore, zbyte* back)
        //{

        //    *fore = lo (wp[win].colour);
        //    *back = hi (wp[win].colour);

        //}/* get_window_colours */

        /*
         * get_window_font
         *
         * Get the font for a given window.
         *
         */

        internal static zword get_window_font(zword win)
        {
            zword font = wp[win].font;

            if (font == ZFont.TEXT_FONT)

                if (main.h_version != ZMachine.V6)
                {

                    if ((win != 0 || (main.h_flags & ZMachine.FIXED_FONT_FLAG) > 0))

                        font = ZFont.FIXED_WIDTH_FONT;

                }
                else
                {

                    if ((wp[win].style & ZStyles.FIXED_WIDTH_STYLE) > 0)

                        font = ZFont.FIXED_WIDTH_FONT;

                }

            return font;

        }/* get_window_font */

        /*
         * colour_in_use
         *
         * Check if a colour is set in any window.
         *
         */

        internal static int colour_in_use(zword colour)
        {
            int max = (main.h_version == ZMachine.V6) ? 8 : 2;
            int i;

            for (i = 0; i < max; i++)
            {

                zword bg = FastMem.HI(wp[i].colour);
                zword fg = FastMem.LO(wp[i].colour);

                if (colour == fg || colour == bg)
                    return 1;

            }

            return 0;

        }/* colour_in_use */

        ///*
        // * get_current_window
        // *
        // * Get the currently active window.
        // *
        // */

        //zword get_current_window (void)
        //{

        //    return cwp - wp;

        //}/* get_current_window */

        private static int cwp_index()
        {
            for (int i = 0; i < wp.Length; i++)
            {
                if (wp[i].index == cwp.index) return i;
            }
            return -1;
        }
    }
}