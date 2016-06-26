using zword = System.UInt16;
using zbyte = System.Byte;

using System;
using System.Collections.Generic;
using System.IO;

using Frotz;
using Frotz.Generic;
using Frotz.Constants;
using Frotz.Other;

using System.Diagnostics;

using Frotz.Screen;

using FrotzNet.Frotz.Other;

namespace Frotz
{


    public static class os_
    {
        public static bool debug_mode = false;
        private static int _historyPos = 0;
        // TODO This really needs to get wired up when a new game is started
        private static List<String> _history = new List<string>();

        private static void debug(string text)
        {
            if (debug_mode)
            {
                List<string> debugtext = new List<string>();
                if (File.Exists("debug.log"))
                    debugtext = new List<string>(File.ReadAllLines("debug.log"));
                debugtext.Add(text);
                File.WriteAllLines("debug.log", debugtext.ToArray());
            }
        }

        // TODO Rename these
        private static long makeid(byte a, byte b, byte c, byte d)
        {
            return ((long)(((a) << 24) | ((b) << 16) | ((c) << 8) | (d)));
        }

        private static long makeid(byte[] buffer, int offset)
        {
            return makeid(buffer[offset], buffer[offset + 1], buffer[offset + 2], buffer[offset + 3]);
        }


        public static Blorb.Blorb _blorbFile = null; // TODO Make this static again, or something

        private static IZScreen _screen;

        public static void SetScreen(IZScreen Screen)
        {
            if (_screen != null)
            {
                _screen.KeyPressed = null;
            }

            _screen = Screen;
            _screen.KeyPressed = new EventHandler<ZKeyPressEventArgs>(_screen_KeyPressed);
        }

        private static void addTestKeys()
        {
            //entries.Enqueue(' ');
            //enqueue_word("ne");
            //enqueue_word("look at sundial");
        }

        static void _screen_KeyPressed(object sender, ZKeyPressEventArgs e)
        {
            entries.Enqueue(e.KeyPressed);
            debug($"Queued character \"{e.KeyPressed}\"");
        }
        private static void OnFatalError(String Message)
        {
            _screen.HandleFatalError(Message);
            debug($"[FATAL ERROR] {Message}");
        }

        private static void enqueue_word(String word)
        {
            foreach (Char c in word)
            {
                entries.Enqueue(c);
            }
            entries.Enqueue(CharCodes.ZC_RETURN);
        }

        public static Queue<zword> entries = new Queue<zword>();

        public static void fail(string message)
        {
            _screen.HandleFatalError(message);
        }

        /////////////////////////////////////////////////////////////////////////////
        // Interface to the Frotz core
        /////////////////////////////////////////////////////////////////////////////

        /*
         * os_beep
         *
         * Play a beep sound. Ideally, the sound should be high- (number == 1)
         * or low-pitched (number == 2).
         *
         */
        public static void beep(int number)
        {
#if !SILVERLIGHT
            if (number == 1)
            {
                Console.Beep(800, 200);
            }
            else
            {
                Console.Beep(392, 200);
            }
#endif
        }

        /*
         * os_display_char
         *
         * Display a character of the current font using the current colours and
         * text style. The cursor moves to the next position. Printable codes are
         * all ASCII values from 32 to 126, ISO Latin-1 characters from 160 to
         * 255, ZC_GAP (gap between two sentences) and ZC_INDENT (paragraph
         * indentation), and Unicode characters above 255. The screen should not
         * be scrolled after printing to the bottom right corner.
         *
         */
        public static void display_char(zword c)
        {
            if (c == CharCodes.ZC_INDENT)
            {
                display_char(' ');
                display_char(' ');
                display_char(' ');
            }
            else if (c == CharCodes.ZC_GAP)
            {
                display_char(' ');
                display_char(' ');
            }
            else if (IsValidChar(c))
            {
                _screen.DisplayChar((char)c);
            }
            debug($"Displaying char {(char)c}.");
        }

        /*
         * os_display_string
         *
         * Pass a string of characters to os_display_char.
         *
         */
        public static void display_string(zword[] s, int start)
        {
            zword c;

            for (int i = start; i < s.Length && s[i] != 0; i++)
            {
                c = s[i];
                if (c == CharCodes.ZC_NEW_FONT || c == CharCodes.ZC_NEW_STYLE)
                {
                    int arg = s[++i];
                    if (c == CharCodes.ZC_NEW_FONT)
                    {
                        set_font(arg);
                    }
                    else if (c == CharCodes.ZC_NEW_STYLE)
                    {
                        set_text_style(arg);
                    }
                }
                else
                {
                    display_char(c);
                }
            }
        }

        private static void display_string(String s)
        {
            zword[] word = new zword[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                word[i] = s[i];
            }
            display_string(word, 0);
        }

        /*
         * os_erase_area
         *
         * Fill a rectangular area of the screen with the current background
         * colour. Top left coordinates are (1,1). The cursor does not move.
         *
         * The final argument gives the window being changed, -1 if only a
         * portion of a window is being erased, or -2 if the whole screen is
         * being erased.
         *
         */
        public static void erase_area(int top, int left, int bottom, int right, int win)
        {
            if (win == -2)
            {
                _screen.Clear();
            }
            else if (win == 1)
            {
                _screen.ClearArea(top, left, bottom, right);
            }
            else
            {
                _screen.ClearArea(top, left, bottom, right);
            }
        }

        /*
         * os_fatal
         *
         * Display error message and stop interpreter.
         *
         */
        public static void fatal(string s)
        {
            OnFatalError(s);
        }

        /*
         * os_font_data
         *
         * Return true if the given font is available. The font can be
         *
         *    TEXT_FONT
         *    PICTURE_FONT
         *    GRAPHICS_FONT
         *    FIXED_WIDTH_FONT
         *
         * The font size should be stored in "height" and "width". If
         * the given font is unavailable then these values must _not_
         * be changed.
         *
         */
        public static bool font_data(int font, ref zword height, ref zword width)
        {
            return _screen.GetFontData(font, ref height, ref width);
        }

        /*
         * os_read_file_name
         *
         * Return the name of a file. Flag can be one of:
         *
         *    FILE_SAVE     - Save game file
         *    FILE_RESTORE  - Restore game file
         *    FILE_SCRIPT   - Transcript file
         *    FILE_RECORD   - Command file for recording
         *    FILE_PLAYBACK - Command file for playback
         *    FILE_SAVE_AUX - Save auxiliary ("preferred settings") file
         *    FILE_LOAD_AUX - Load auxiliary ("preferred settings") file
         *
         * The length of the file name is limited by MAX_FILE_NAME. Ideally
         * an interpreter should open a file requester to ask for the file
         * name. If it is unable to do that then this function should call
         * print_string and read_string to ask for a file name.
         *
         */
        public static bool read_file_name(out string file_name, string default_name, FileTypes flag)
        {
            String fileName = "";
            switch (flag)
            {
                case FileTypes.FILE_SAVE:
                    fileName = _screen.OpenNewOrExistingFile(FastMem.save_name, "Choose save game file", "Save Files (*.sav)|*.sav", ".sav");
                    break;
                case FileTypes.FILE_RESTORE:
                    fileName = _screen.OpenExistingFile(FastMem.save_name, "Choose save game to restore", "Save Files (*.sav)|*.sav");
                    break;
                case FileTypes.FILE_SCRIPT:
                    fileName = _screen.OpenNewOrExistingFile(General.DEFAULT_SCRIPT_NAME, "Choose Script File", "Script File (*.scr)|*.scr", ".scr");
                    break;
                case FileTypes.FILE_RECORD:
                    fileName = _screen.OpenNewOrExistingFile(default_name, "Choose File to Record To", "Record File(*.rec)|*.rec", ".rec");
                    break;
                case FileTypes.FILE_PLAYBACK:
                    fileName = _screen.OpenExistingFile(default_name, "Choose File to playback from", "Record File(*.rec)|*.rec");
                    break;
                case FileTypes.FILE_SAVE_AUX:
                case FileTypes.FILE_LOAD_AUX:
                    fail("Need to implement other types of files");
                    break;
            }

            if (fileName != null)
            {
                file_name = fileName;
                return true;
            }
            else
            {
                file_name = null;
                return false;
            }
        }

        /*
         * os_init_screen
         *
         * Initialise the IO interface. Prepare screen and other devices
         * (mouse, sound card). Set various OS depending story file header
         * entries:
         *
         *     h_config (aka flags 1)
         *     h_flags (aka flags 2)
         *     h_screen_cols (aka screen width in characters)
         *     h_screen_rows (aka screen height in lines)
         *     h_screen_width
         *     h_screen_height
         *     h_font_height (defaults to 1)
         *     h_font_width (defaults to 1)
         *     h_default_foreground
         *     h_default_background
         *     h_interpreter_number
         *     h_interpreter_version
         *     h_user_name (optional; not used by any game)
         *
         * Finally, set reserve_mem to the amount of memory (in bytes) that
         * should not be used for multiple undo and reserved for later use.
         *
         */
        public static void init_screen()
        {
            // TODO Really need to clean this up

            main.h_interpreter_number = 4;

            // Set the configuration
            if (main.h_version == ZMachine.V3)
            {
                main.h_config |= ZMachine.CONFIG_SPLITSCREEN;
                main.h_config |= ZMachine.CONFIG_PROPORTIONAL;
                // TODO Set Tandy bit here if appropriate
            }
            if (main.h_version >= ZMachine.V4)
            {
                main.h_config |= ZMachine.CONFIG_BOLDFACE;
                main.h_config |= ZMachine.CONFIG_EMPHASIS;
                main.h_config |= ZMachine.CONFIG_FIXED;
                main.h_config |= ZMachine.CONFIG_TIMEDINPUT;
            }
            if (main.h_version >= ZMachine.V5)
                main.h_config |= ZMachine.CONFIG_COLOUR;
            if (main.h_version == ZMachine.V6)
            {
                if (_blorbFile != null)
                {
                    main.h_config |= ZMachine.CONFIG_PICTURES;
                    main.h_config |= ZMachine.CONFIG_SOUND;
                }
            }
            //theApp.CopyUsername();

            main.h_interpreter_version = (byte)'F';
            if (main.h_version == ZMachine.V6)
            {
                main.h_default_background = ZColor.BLACK_COLOUR;
                main.h_default_foreground = ZColor.WHITE_COLOUR;
                // TODO Get the defaults from the application itself
            }
            else
            {
                main.h_default_foreground = 1;
                main.h_default_background = 1;
            }

            // Clear out the input queue incase a quit left characters
            entries.Clear();

            // TODO Set font to be default fixed width font

            _metrics = _screen.GetScreenMetrics();
            System.Diagnostics.Debug.WriteLine("Metrics:" + _metrics.WindowSize.Height + ":" + _metrics.WindowSize.Width);

            // TODO Make these numbers match the types (remove the casts)

            main.h_screen_width = (zword)_metrics.WindowSize.Width;
            main.h_screen_height = (zword)_metrics.WindowSize.Height;

            main.h_screen_cols = (zbyte)_metrics.Columns;
            main.h_screen_rows = (zbyte)_metrics.Rows;

            main.h_font_width = (zbyte)_metrics.FontSize.Width;
            main.h_font_height = (zbyte)_metrics.FontSize.Height;

            // Check for sound
            if ((main.h_version == ZMachine.V3) && ((main.h_flags & ZMachine.OLD_SOUND_FLAG) != 0))
            {
                // TODO Config sound here if appropriate
            }
            else if ((main.h_version >= ZMachine.V4) && ((main.h_flags & ZMachine.SOUND_FLAG) != 0))
            {
                // TODO Config sound here if appropriate
            }

            if (main.h_version >= ZMachine.V5)
            {
                ushort mask = 0;
                if (main.h_version == ZMachine.V6) mask |= ZMachine.TRANSPARENT_FLAG;

                // Mask out any unsupported bits in the extended flags
                main.hx_flags &= mask;

                // TODO Set fore & back color here if apporpriate
                //  hx_fore_colour =
                //  hx_back_colour =
            }


            String name = main.story_name;
            // Set default filenames

            FastMem.save_name = String.Format("{0}.sav", name);
            Files.script_name = String.Format("{0}.log", name);
            Files.command_name = String.Format("{0}.rec", name);
            FastMem.auxilary_name = String.Format("{0}.aux", name);

            addTestKeys();
        }

        /*
         * os_more_prompt
         *
         * Display a MORE prompt, wait for a keypress and remove the MORE
         * prompt from the screen.
         *
         */
        public static void more_prompt()
        {
            display_string("[MORE]");
            _screen.RefreshScreen();

            while (entries.Count == 0)
            {
                //System.Threading.Thread.Sleep(100);
            }
            entries.Dequeue();

            _screen.RemoveChars(6);
            _screen.RefreshScreen();
        }

        /*
         * os_process_arguments
         *
         * Handle command line switches. Some variables may be set to activate
         * special features of Frotz:
         *
         *     option_attribute_assignment
         *     option_attribute_testing
         *     option_context_lines
         *     option_object_locating
         *     option_object_movement
         *     option_left_margin
         *     option_right_margin
         *     option_ignore_errors
         *     option_piracy
         *     option_undo_slots
         *     option_expand_abbreviations
         *     option_script_cols
         *
         * The global pointer "story_name" is set to the story file name.
         *
         */
        public static byte[] preloadedFileData;
        public static bool process_arguments(string[] args)
        {
            byte[] filedata;
            String fileName = null;
            if (preloadedFileData != null)
            {
                main.story_name = "game";
                main.story_data = preloadedFileData;
            }
            else if (args.Length == 0)
            {
                fileName = _screen.SelectGameFile(out filedata);
                if (fileName != null)
                {
                    main.story_name = fileName;
                    main.story_data = filedata;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                main.story_name = args[0];
                FileStream fs = new FileStream(args[0], FileMode.Open);
                main.story_data = new zbyte[fs.Length];
                fs.Read(main.story_data, 0, main.story_data.Length);
                fs.Close();
            }


            Err.err_report_mode = ErrorCodes.ERR_REPORT_NEVER;

            //'
            //// Set default filenames
            //String filename = main.story_name;
            //var fi = new System.IO.FileInfo(main.story_name);
            //if (fi.Exists)
            //{
            //    String name = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length);

            //    FastMem.save_name = String.Format("{0}.sav", name);
            //    Files.script_name = String.Format("{0}.log", name);
            //    Files.command_name = String.Format("{0}.rec", name);
            //    FastMem.auxilary_name = String.Format("{0}.aux", name);
            //}

            return true;
        }

        /*
         * os_read_line
         *
         * Read a line of input from the keyboard into a buffer. The buffer
         * may already be primed with some text. In this case, the "initial"
         * text is already displayed on the screen. After the input action
         * is complete, the function returns with the terminating key value.
         * The length of the input should not exceed "max" characters plus
         * an extra 0 terminator.
         *
         * Terminating keys are the return key (13) and all function keys
         * (see the Specification of the Z-machine) which are accepted by
         * the is_terminator function. Mouse clicks behave like function
         * keys except that the mouse position is stored in global variables
         * "mouse_x" and "mouse_y" (top left coordinates are (1,1)).
         *
         * Furthermore, Frotz introduces some special terminating keys:
         *
         *     ZC_HKEY_PLAYBACK (Alt-P)
         *     ZC_HKEY_RECORD (Alt-R)
         *     ZC_HKEY_SEED (Alt-S)
         *     ZC_HKEY_UNDO (Alt-U)
         *     ZC_HKEY_RESTART (Alt-N, "new game")
         *     ZC_HKEY_QUIT (Alt-X, "exit game")
         *     ZC_HKEY_DEBUG (Alt-D)
         *     ZC_HKEY_HELP (Alt-H)
         *
         * If the timeout argument is not zero, the input gets interrupted
         * after timeout/10 seconds (and the return value is 0).
         *
         * The complete input line including the cursor must fit in "width"
         * screen units.
         *
         * The function may be called once again to continue after timeouts,
         * misplaced mouse clicks or hot keys. In this case the "continued"
         * flag will be set. This information can be useful if the interface
         * implements input line history.
         *
         * The screen is not scrolled after the return key was pressed. The
         * cursor is at the end of the input line when the function returns.
         *
         * Since Frotz 2.2 the helper function "completion" can be called
         * to implement word completion (similar to tcsh under Unix).
         *
         */
        public static zword read_line(int max, zword[] buf, int timeout, int width, bool continued)
        {

            //        ZC_SINGLE_CLICK || ZC_DOUBLE_CLICK

            //        case VK_DELETE:
            //        case VK_HOME:
            //        case VK_END:
            //        case VK_TAB:

            List<BufferChars> _buffer = new List<BufferChars>();

            _screen.RefreshScreen();

            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] = 0;
            }

            int background;
            int foreground;

            _screen.GetColor(out foreground, out background);
            _screen.SetInputColor();

            _screen.SetInputMode(true, true);

            ZPoint p = _screen.GetCursorPosition();

            try
            {
                while (true)
                {
                    if (main.abort_game_loop == true)
                    {
                        return CharCodes.ZC_RETURN;
                    }

                    while (entries.Count == 0)
                    {
                        if (main.abort_game_loop == true)
                        {
                            return CharCodes.ZC_RETURN;
                        }

                        //System.Threading.Thread.Sleep(10);
                    }
                    zword c = entries.Dequeue();

                    switch (c)
                    {
                        case CharCodes.ZC_HKEY_HELP:
                        case CharCodes.ZC_HKEY_DEBUG:
                        case CharCodes.ZC_HKEY_PLAYBACK:
                        case CharCodes.ZC_HKEY_RECORD:

                        case CharCodes.ZC_HKEY_SEED:
                        case CharCodes.ZC_HKEY_UNDO:
                        case CharCodes.ZC_HKEY_RESTART:
                        case CharCodes.ZC_HKEY_QUIT:
                            return c;
                    }

                    if (c == CharCodes.ZC_SINGLE_CLICK || c == CharCodes.ZC_DOUBLE_CLICK)
                    {
                        // Just discard mouse clicks here
                        continue;
                    }
                    else  if (c == CharCodes.ZC_ARROW_UP)
                    {
                        clearInputAndShowHistory(1, _buffer);
                    }
                    else if (c == CharCodes.ZC_ARROW_DOWN)
                    {
                        clearInputAndShowHistory(-1, _buffer);
                    }
                    else if (c == CharCodes.ZC_ARROW_LEFT)
                    {
                    }
                    else if (c == CharCodes.ZC_ARROW_RIGHT)
                    {
                    }
                    else if (c == CharCodes.ZC_RETURN || c == '\n' || c == '\r')
                    {
                        var sb = new System.Text.StringBuilder();

                        foreach (BufferChars bc in _buffer)
                        {
                            sb.Append((char)bc.Char);
                        }
                        _history.Insert(0, sb.ToString());
                        _historyPos = 0;
                        return CharCodes.ZC_RETURN;
                    }
                    else if (c == CharCodes.ZC_BACKSPACE)
                    {
                        if (_buffer.Count > 0)
                        {
                            BufferChars bc = _buffer[_buffer.Count - 1];
                            _buffer.Remove(bc);

                            p.X -= bc.Width;
                            _screen.SetCursorPosition(p.X, p.Y);

                            _screen.RemoveChars(1);
                            _screen.RefreshScreen();
                        }
                    }
                    else if (c == '\t')
                    {
                        var sb = new System.Text.StringBuilder();

                        foreach (BufferChars bc in _buffer)
                        {
                            sb.Append((char)bc.Char);
                        }

                        String temp = sb.ToString();
                        String word;
                        int result = Text.completion(temp, out word);
                        if (result == 0)
                        {
                            foreach (char c1 in word)
                            {
                                entries.Enqueue(c1);
                            }
                            entries.Enqueue(' ');
                        }
                        else if (result == 1)
                        {
                            beep(0);
                        }
                        else
                        {
                            beep(1);
                        }
                    }
                    else
                    {
                        // buf[pos++] = c;

                        int w = _screen.GetStringWidth(((char)c).ToString(), new CharDisplayInfo(ZFont.TEXT_FONT, ZStyles.NORMAL_STYLE, -1, -1));
                        p.X += w;
                        _screen.SetCursorPosition(p.X, p.Y);

                        _buffer.Add(new BufferChars(c, w));

                        _screen.addInputChar((char)c);

                        _screen.RefreshScreen();
                    }
                }
            }
            finally
            {
                _screen.SetColor(foreground, background);
                _screen.SetInputMode(false, false);

                for (int i = 0; i < _buffer.Count; i++)
                {
                    buf[i] = _buffer[i].Char;
                }
            }
        }

        private static void clearInputAndShowHistory(int direction, List<BufferChars> _buffer)
        {
            if (direction > 0)
            {
                if (_historyPos + direction > _history.Count)
                {
                    beep(0);
                    return;
                }
                _historyPos++;
            }

            if (direction < 0)
            {
                if (_historyPos + direction < 0)
                {
                    beep(0);
                    return;
                }
                _historyPos--;
            }

            // TODO Check if it's in bounds, and show history. If it would be out of bounds, beep!
            for (int i = 0; i < _buffer.Count; i++)
            {
                entries.Enqueue(CharCodes.ZC_BACKSPACE);
            }

            if (_historyPos > 0)
            {
                String temp = _history[_historyPos - 1];
                foreach (char c1 in temp)
                {
                    entries.Enqueue(c1);
                }
            }
        }

        static bool _setCursorPositionCalled = false;
        static ZPoint _newCursorPosition = new ZPoint(0, 0);

        /*
         * os_read_key
         *
         * Read a single character from the keyboard (or a mouse click) and
         * return it. Input aborts after timeout/10 seconds.
         *
         */
        public static zword read_key(int timeout, bool cursor)
        {
            _screen.RefreshScreen();

            _screen.SetInputMode(true, true);

            if (_setCursorPositionCalled == false)
            {
                set_cursor(_newCursorPosition.Y, _newCursorPosition.X);
            }

            ZPoint p = _screen.GetCursorPosition();

            try
            {
//#if SILVERLIGHT

//                var sw = new WiredPrairie.Silverlight.Stopwatch();
//#else
//                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//#endif
//                sw.Start();
                while (true)
                {
                    do
                    {
                        if (main.abort_game_loop == true)
                        {
                            return CharCodes.ZC_RETURN;
                        }

                        lock (entries)
                        {

                            if (entries.Count > 0) break;
                            //if (sw.Elapsed.TotalSeconds > timeout / 10 && timeout > 0)
                            //    return CharCodes.ZC_TIME_OUT;
                        }
                        //System.Threading.Thread.Sleep(10);

                    } while (true);

                    lock (entries)
                    {
                        _setCursorPositionCalled = false;
                        zword c = entries.Dequeue();

                        int width = _screen.GetStringWidth(((char)c).ToString(),
                            new CharDisplayInfo(ZFont.FIXED_WIDTH_FONT, ZStyles.NORMAL_STYLE, 1, 1));
                        // _screen.SetCursorPosition(p.X + width, p.Y);

                        _newCursorPosition = new ZPoint(p.X + width, p.Y);

                        return c;
                    }
                }
            }
            finally
            {
                _screen.SetInputMode(false, false);
            }
        }



        /*
         * os_read_mouse
         *
         * Store the mouse position in the global variables "mouse_x" and
         * "mouse_y", the code of the last clicked menu in "menu_selected"
         * and return the mouse buttons currently pressed.
         *
         */
        internal static zword read_mouse()
        {
            // return 0;
            os_.fail("Need to implement mouse handling");

            return 0;
        }

        /*
         * os_menu
         *
         * Add to or remove a menu item. Action can be:
         *     MENU_NEW    - Add a new menu with the given title
         *     MENU_ADD    - Add a new menu item with the given text
         *     MENU_REMOVE - Remove the menu at the given index
         *
         */
        public static void menu(int action, int menu, zword[] text)
        {
            fail("os_menu not yet handled");
        }


        /*
         * os_reset_screen
         *
         * Reset the screen before the program ends.
         *
         */
        public static void reset_screen()
        {
            _screen.Clear();

            set_text_style(0);

            _screen.RefreshScreen();
        }

        /*
         * os_scroll_area
         *
         * Scroll a rectangular area of the screen up (units > 0) or down
         * (units < 0) and fill the empty space with the current background
         * colour. Top left coordinates are (1,1). The cursor stays put.
         *
         */
        public static void scroll_area(int top, int left, int bottom, int right, int units)
        {
            // TODO This version can scroll better

            if (left > 1 || right < _metrics.Rows)
            {
                _screen.ScrollArea(top, bottom, left, right, units);
            }
            else
            {
                System.Diagnostics.Debug.Assert(units > 0);
                _screen.ScrollLines(top, bottom - top + 1, units);
            }
        }

        /*
         * os_set_colour
         *
         * Set the foreground and background colours which can be:
         *
         *     1
         *     BLACK_COLOUR
         *     RED_COLOUR
         *     GREEN_COLOUR
         *     YELLOW_COLOUR
         *     BLUE_COLOUR
         *     MAGENTA_COLOUR
         *     CYAN_COLOUR
         *     WHITE_COLOUR
         *     TRANSPARENT_COLOUR
         *
         *     Amiga only:
         *
         *     LIGHTGREY_COLOUR
         *     MEDIUMGREY_COLOUR
         *     DARKGREY_COLOUR
         *
         * There may be more colours in the range from 16 to 255; see the
         * remarks about os_peek_colour.
         *
         */
        public static void set_colour(int new_foreground, int new_background)
        {
            _screen.SetColor(new_foreground, new_background);
        }

        /*
         * os_from_true_culour
         *
         * Given a true colour, return an appropriate colour index.
         *
         */
        public static zword from_true_colour(zword colour)
        {
            return TrueColorStuff.GetColourIndex(TrueColorStuff.RGB5ToTrue(colour));
        }

        /*
         * os_to_true_colour
         *
         * Given a colour index, return the appropriate true colour.
         *
         */
        public static zword to_true_colour(int index)
        {
            return TrueColorStuff.TrueToRGB5(TrueColorStuff.GetColour(index));
        }

        /*
         * os_set_cursor
         *
         * Place the text cursor at the given coordinates. Top left is (1,1).
         *
         */
        public static void set_cursor(int row, int col)
        {
            _setCursorPositionCalled = true;
            // TODO Need to migrate these variables to a better location
            _screen.SetCursorPosition(col, row);
        }

        /*
         * os_set_font
         *
         * Set the font for text output. The interpreter takes care not to
         * choose fonts which aren't supported by the interface.
         *
         */
        public static void set_font(int new_font)
        {
            _screen.SetFont(new_font);
        }

        /*
         * os_set_text_style
         *
         * Set the current text style. Following flags can be set:
         *
         *     REVERSE_STYLE
         *     BOLDFACE_STYLE
         *     EMPHASIS_STYLE (aka underline aka italics)
         *     FIXED_WIDTH_STYLE
         *
         */
        public static void set_text_style(int new_style)
        {
            _screen.SetTextStyle(new_style);
        }

        /*
         * os_string_width
         *
         * Calculate the length of a word in screen units. Apart from letters,
         * the word may contain special codes:
         *
         *    ZC_NEW_STYLE - next character is a new text style
         *    ZC_NEW_FONT  - next character is a new font
         *
         */
        public static int string_width(zword[] s)
        {
            return string_width(s, 0);
        }

        public static int string_width(zword[] s, int pos)
        {
            var sb = new System.Text.StringBuilder();
            int font = -1;
            int style = -1;

            bool lateChange = false; // TODO This is testing code to determine if there are ever font changes mid word
            zword c;
            int width = 0;
            for (int i = pos; i < s.Length && s[i] != 0; i++)
            {
                c = s[i];
                if (c == CharCodes.ZC_NEW_FONT || c == CharCodes.ZC_NEW_STYLE)
                {
                    i++;
                    if (width == 0)
                    {
                        if (c == CharCodes.ZC_NEW_FONT) font = s[i];
                        if (c == CharCodes.ZC_NEW_STYLE) style = s[i];
                    }
                    else
                    {
                        lateChange = true;
                    }
                }
                else
                {
                    sb.Append((char)c);
                    width++;
                    if (lateChange == true)
                    {
                        // _screen.DisplayMessage("Characters after a late change!", "Message");
                    }
                }
            }

            int temp = _screen.GetStringWidth(sb.ToString(), new CharDisplayInfo(font, style, 0, 0));

            return temp;
        }


        /*
         * os_char_width
         *
         * Return the length of the character in screen units.
         *
         */
        public static int char_width(zword c)
        {
            return _metrics.FontSize.Width;
        }

        /*
         * os_check_unicode
         *
         * Return with bit 0 set if the Unicode character can be
         * displayed, and bit 1 if it can be input.
         *
         *
         */
        public static zword check_unicode(int font, zword c)
        {
            // TODO Just return 1 since almost all modern fonts should be able to handle this
            return 1;
        }

        /*
         * os_peek_colour
         *
         * Return the colour of the screen unit below the cursor. (If the
         * interface uses a text mode, it may return the background colour
         * of the character at the cursor position instead.) This is used
         * when text is printed on top of pictures. Note that this coulor
         * need not be in the standard set of Z-machine colours. To handle
         * this situation, Frotz entends the colour scheme: Colours above
         * 15 (and below 256) may be used by the interface to refer to non
         * standard colours. Of course, os_set_colour must be able to deal
         * with these colours.
         *
         */
        public static zword peek_colour()
        {
            return _screen.PeekColor();
        }

        /*
         * os_picture_data
         *
         * Return true if the given picture is available. If so, store the
         * picture width and height in the appropriate variables. Picture
         * number 0 is a special case: Write the highest legal picture number
         * and the picture file release number into the height and width
         * variables respectively when this picture number is asked for.
         *
         */
        public static bool picture_data(int picture, out int height, out int width)
        {
            if (_blorbFile != null)
            {
                if (picture == 0)
                {
                    height = -1;
                    width = -_blorbFile.ReleaseNumber;
                    foreach (var p in _blorbFile.Pictures.Keys)
                    {
                        if (p > height)
                        {
                            height = p;
                            width = _blorbFile.ReleaseNumber;
                        }
                    }

                    return true;
                }
                else
                {
                    byte[] buffer = _blorbFile.Pictures[picture].Image;
                    if (buffer.Length == 8)
                    { // TODO This is a bit of a hack, it would be better to handle this upfront so there is no guess work
                        width = (int)makeid(buffer, 0) * _metrics.Scale;
                        height = (int)makeid(buffer, 4) * _metrics.Scale;
                    }
                    else
                    {
                        ZSize size = _screen.GetImageInfo(buffer);
                        height = size.Height;
                        width = size.Width;
                    }

                    return true;
                }
            }
            height = 0;
            width = 0;
            return false;
        }

        /*
         * os_draw_picture
         *
         * Display a picture at the given coordinates.
         *
         */
        public static void draw_picture(int picture, int y, int x)
        {
            if (_blorbFile != null && _blorbFile.Pictures.ContainsKey(picture))
            {
                _screen.DrawPicture(picture, _blorbFile.Pictures[picture].Image, y, x);
            }
        }

        /*
         * os_random_seed
         *
         * Return an appropriate random seed value in the range from 0 to
         * 32767, possibly by using the current system time.
         *
         */
        public static int random_seed()
        {
            if (DebugState.IsActive)
            {
                return DebugState.RandomSeed();
            }
            else
            {
                System.Random r = new System.Random();
                return r.Next() & 32767;
            }
        }

        /*
         * os_restart_game
         *
         * This routine allows the interface to interfere with the process of
         * restarting a game at various stages:
         *
         *     RESTART_BEGIN - restart has just begun
         *     RESTART_WPROP_SET - window properties have been initialised
         *     RESTART_END - restart is complete
         *
         */
        public static void restart_game(int stage)
        {
            // Show Beyond Zork's title screen
            if ((stage == ZMachine.RESTART_BEGIN) && (main.story_id == Story.BEYOND_ZORK))
            {
                int w, h;
                if (os_.picture_data(1, out h, out w))
                {

                    os_.draw_picture(1, 1, 1);
                    os_.read_key(0, false);
                }
            }
        }

        /*
         * os_path_open
         *
         * Open a file in the current directory.
         * -- Szurgot: Changed this to return a Memory stream, and also has Blorb Logic.. May need to refine
         * -- Changed this again to take a byte[] to allow the data to be loaded further up the chain
         */
        public static MemoryStream path_open(byte[] story_data)
        {
            // System.IO.FileInfo fi = new System.IO.FileInfo(FileName);
            if (story_data.Length < 4)
            {
                throw new ArgumentException("story_data isn't long enough");
            }
            else
            {
                if (story_data[0] == (char)'F' && story_data[1] == (byte)'O' &&
                    story_data[2] == (byte)'R' && story_data[3] == (byte)'M') {
                    _blorbFile = Blorb.BlorbReader.ReadBlorbFile(story_data);

                    return new System.IO.MemoryStream(_blorbFile.ZCode);
                }
                else
                {
                    //FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                    //byte[] buffer = new byte[fs.Length];
                    //fs.Read(buffer, 0, buffer.Length);
                    //fs.Close();

#if !SILVERLIGHT
                    String temp = Path.ChangeExtension(main.story_name, "blb");
                    _blorbFile = null;

                    if (File.Exists(temp))
                    {
                        var s = File.OpenRead(temp);
                        byte[] buffer = new zbyte[s.Length];
                        s.Read(buffer, 0, buffer.Length);
                        s.Close();
                        var tempB = Blorb.BlorbReader.ReadBlorbFile(buffer);
                        _blorbFile = tempB;
                    }
#endif
                    return new MemoryStream(story_data);
                }
            }
        }

        /*
         * os_finish_with_sample
         *
         * Remove the current sample from memory (if any).
         *
         */
        public static void finish_with_sample(int number)
        {
            _screen.FinishWithSample(number);
        }

        /*
         * os_prepare_sample
         *
         * Load the given sample from the disk.
         *
         */
        public static void prepare_sample(int number)
        {
            _screen.PrepareSample(number);
        }

        /*
         * os_start_sample
         *
         * Play the given sample at the given volume (ranging from 1 to 8 and
         * 255 meaning a default volume). The sound is played once or several
         * times in the background (255 meaning forever). The end_of_sound
         * function is called as soon as the sound finishes, passing in the
         * eos argument.
         *
         */
        public static void start_sample(int number, int volume, int repeats, zword eos)
        {
            // TODO Refine this a little better to wait for the end and then trigger
            _screen.StartSample(number, volume, repeats, eos);

            Sound.end_of_sound(eos);
        }

        /*
         * os_stop_sample
         *
         * Turn off the current sample.
         *
         */
        public static void stop_sample(int number)
        {
            _screen.StopSample(number);
        }

        /*
         * os_scrollback_char
         *
         * Write a character to the scrollback buffer.
         *
         */
        public static void scrollback_char(zword c)
        {
            // TODO Implement scrollback
        }

        /*
         * os_scrollback_erase
         *
         * Remove characters from the scrollback buffer.
         *
         */
        public static void scrollback_erase(int erase)
        {
            // TODO Implement scrollback
        }

        /*
         * os_tick
         *
         * Called after each opcode.
         *
         */
        static int os_tick_count = 0;
        public static void tick()
        {
            // Check for completed sounds
            if (++os_tick_count > 1000)
            {
                os_tick_count = 0;
                // TODO Implement sound at some point :)
            }
        }

        /*
         * os_buffer_screen
         *
         * Set the screen buffering mode, and return the previous mode.
         * Possible values for mode are:
         *
         *     0 - update the display to reflect changes when possible
         *     1 - do not update the display
         *    -1 - redraw the screen, do not change the mode
         *
         */
        public static int buffer_screen(int mode)
        {
            fail("os_buffer_screen is not yet implemented");
            return 0;
        }

        /*
         * os_wrap_window
         *
         * Return non-zero if the window should have text wrapped.
         *
         */
        public static int wrap_window(int win)
        {
            bool shouldWrap = _screen.ShouldWrap();
            if (shouldWrap == true)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /*
         * os_window_height
         *
         * Called when the height of a window is changed.
         *
         */
        public static void set_window_size(int win, Frotz.Other.ZWindow wp)
        {
            _screen.SetWindowSize(win, wp.y_pos, wp.x_pos, wp.y_size, wp.x_size);
        }

        /*
         * set_active_window
         * Called to set the output window (I hope)
         *
         */
        public static void set_active_window(int win)
        {
            System.Diagnostics.Debug.WriteLine("Setting Window:" + win);
            _screen.SetActiveWindow(win);
        }

        // New Variables go here //
        private static ScreenMetrics _metrics;

        private static bool IsValidChar(zword c)
        {
            if (c >= CharCodes.ZC_ASCII_MIN && c <= CharCodes.ZC_ASCII_MAX)
                return true;
            if (c >= CharCodes.ZC_LATIN1_MIN && c <= CharCodes.ZC_LATIN1_MAX)
                return true;
            if (c >= 0x100)
                return true;
            return false;
        }

        public static void game_started()
        {
            _screen.StoryStarted(main.story_name, _blorbFile);
        }

        public static void mouse_moved(int x, int y) {
            main.mouse_x = (ushort)x;
            main.mouse_y = (ushort)y;

        }

        public static byte[] GetStoryFile()
        {
            return FastMem.storyData;
        }
    }

    internal class BufferChars
    {
        internal ushort Char { get; private set; }
        internal int Width { get; private set; }

        internal BufferChars(ushort Char, int Width)
        {
            this.Char = Char;
            this.Width = Width;
        }
    }
}
