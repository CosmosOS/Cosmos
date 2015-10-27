//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.CorDebug;

using IronPython.Hosting;
using IronPython.Compiler;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;


class ConsoleOptions : EngineOptions
{
    internal static int AutoIndentSize = 0;
}

/// <summary>
/// Class controlling the MDbg python interactive command line.
/// </summary>
public class PythonCommandLine
{
    private static StringBuilder b = new StringBuilder();
    private PythonEngine engine;
    private IConsole console;

    public PythonCommandLine(PythonEngine pEngine)
    {
        engine = pEngine;
        if (typeof(IMDbgIO3).IsAssignableFrom(CommandBase.Shell.IO.GetType()))
        {
            console = new MDbgSuperConsole(engine, false);
        }
        else
        {
            console = new MDbgBasicConsole();
        }
    }

    // Act on single line of python interactive input.
    public void Execute(string line)
    {
        if ((line == null) || (line == "exit"))
        {
            console.WriteLine("type pyout to exit python interactive mode", Style.Error);
            return;
        }
        b.Append(line);
        b.Append("\n");
        bool allowIncompleteStatement = !TreatAsBlankLine(line, 4);
        try
        {
            bool s = engine.ParseInteractiveInput(b.ToString(), allowIncompleteStatement);
            if (s)
            {
                engine.ExecuteToConsole(b.ToString());
                b = new StringBuilder();
            }
        }
        catch (Exception e)
        {
            console.WriteLine(e.Message, Style.Error);
            b = new StringBuilder();
        }
    }

    // Run python interactive input loop.
    public void RunInteractive()
    {
        string line = console.ReadLine(ConsoleOptions.AutoIndentSize);
        while (line != "pyout")
        {
            Execute(line);
            line = console.ReadLine(ConsoleOptions.AutoIndentSize);
        }
    }

    private static bool TreatAsBlankLine(string line, int autoIndentSize)
    {
        if (line.Length == 0) return true;
        if (autoIndentSize != 0 && line.Trim().Length == 0 && line.Length == autoIndentSize)
        {
            return true;
        }

        return false;
    }
}

public interface IConsole
{
    // Read a single line of interactive input
    // AutoIndentSize is the indentation level to be used for the current suite of a compound statement.
    // The console can ignore this argument if it does not want to support auto-indentation
    string ReadLine(int autoIndentSize);

    // Write text to console with no new line.
    void Write(string text, Style style);

    // Write text to console with new line.
    void WriteLine(string text, Style style);
}

public enum Style
{
    Prompt, Out, Error
}

/// <summary>
/// Basic console for interactive python input in MDbg. Does not support tab completion.
/// </summary>
public class MDbgBasicConsole : IConsole
{
    private static bool dotPrompt = false;

    // Read a single line of interactive input
    // AutoIndentSize is the indentation level to be used for the current suite of a compound statement.
    // The console can ignore this argument if it does not want to support auto-indentation
    public string ReadLine(int autoIndentSize)
    {
        string text;

        if (dotPrompt) Console.Write("... ");
        else Console.Write(">>> ");

        CommandBase.Shell.IO.ReadCommand(out text);
        text = text.TrimEnd();
        if (text.Length == 0)
        {
            dotPrompt = false;
        }
        else if (text[text.Length - 1] == ':')
        {
            dotPrompt = true;
        }

        return text;
    }

    public void Write(string text, Style style)
    {
        CommandBase.Write(GetMDbgWriteStyle(style), text);
    }

    public void WriteLine(string text, Style style)
    {
        // Call into MDbg
        CommandBase.WriteOutput(GetMDbgWriteStyle(style), text);
    }

    // Map from IronPython.Hosting.Style --> Mdbg console.
    // This determines how console output is formatted. 
    string GetMDbgWriteStyle(Style style)
    {
        if (style == Style.Error)
            return MDbgOutputConstants.StdError;
        else
            return MDbgOutputConstants.StdOutput;
    }
}

/// <summary>
/// Console for interactive Python input in MDbg. Supports tab completion.
/// </summary>
public class MDbgSuperConsole : IConsole
{

    public ConsoleColor PromptColor = Console.ForegroundColor;
    public ConsoleColor OutColor = Console.ForegroundColor;
    public ConsoleColor ErrorColor = Console.ForegroundColor;

    private static bool dotPrompt = false;

    public void SetupColors()
    {
        PromptColor = ConsoleColor.DarkGray;
        OutColor = ConsoleColor.DarkBlue;
        ErrorColor = ConsoleColor.DarkRed;
    }

    /// <summary>
    /// Class managing the command history.
    /// </summary>
    class History
    {
        protected ArrayList list = new ArrayList();
        protected int current = 0;

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public string Current
        {
            get
            {
                return current >= 0 && current < list.Count ? (string)list[current] : String.Empty;
            }
        }

        public void Clear()
        {
            list.Clear();
            current = -1;
        }

        public void Add(string line)
        {
            if (line != null && line.Length > 0)
            {
                list.Add(line);
            }
        }

        public void AddLast(string line)
        {
            if (line != null && line.Length > 0)
            {
                current = list.Add(line) + 1;
            }
        }

        public string First()
        {
            current = 0;
            return Current;
        }

        public string Last()
        {
            current = list.Count - 1;
            return Current;
        }

        public string Previous()
        {
            if (list.Count > 0)
            {
                current = ((current - 1) + list.Count) % list.Count;
            }
            return Current;
        }

        public string Next()
        {
            if (list.Count > 0)
            {
                current = (current + 1) % list.Count;
            }
            return Current;
        }
    }

    /// <summary>
    /// List of available options
    /// </summary>
    class SuperConsoleOptions : History
    {
        private string root;

        public string Root
        {
            get
            {
                return root;
            }
            set
            {
                root = value;
            }
        }
    }

    /// <summary>
    /// Cursor position management
    /// </summary>
    struct Cursor
    {
        /// <summary>
        /// Beginning position of the cursor - top coordinate.
        /// </summary>
        private int anchorTop;
        /// <summary>
        /// Beginning position of the cursor - left coordinate.
        /// </summary>
        private int anchorLeft;

        public int Top
        {
            get
            {
                return anchorTop;
            }
        }
        public int Left
        {
            get
            {
                return anchorLeft;
            }
        }

        public void Anchor()
        {
            anchorTop = Console.CursorTop;
            anchorLeft = Console.CursorLeft;
        }

        public void Reset()
        {
            Console.CursorTop = anchorTop;
            Console.CursorLeft = anchorLeft;
        }

        public void Place(int index)
        {
            Console.CursorLeft = (anchorLeft + index) % Console.BufferWidth;
            int cursorTop = anchorTop + (anchorLeft + index) / Console.BufferWidth;
            if (cursorTop >= Console.BufferHeight)
            {
                anchorTop -= cursorTop - Console.BufferHeight + 1;
                cursorTop = Console.BufferHeight - 1;
            }
            Console.CursorTop = cursorTop;
        }

        public void Move(int delta)
        {
            int position = Console.CursorTop * Console.BufferWidth + Console.CursorLeft + delta;

            Console.CursorLeft = position % Console.BufferWidth;
            Console.CursorTop = position / Console.BufferWidth;
        }
    };

    /// <summary>
    /// The console input buffer.
    /// </summary>
    private StringBuilder input = new StringBuilder();
    /// <summary>
    /// Current position - index into the input buffer
    /// </summary>
    private int current = 0;
    /// <summary>
    /// The number of white-spaces displayed for the auto-indenation of the current line
    /// </summary>
    private int autoIndentSize = 0;
    /// <summary>
    /// Length of the output currently rendered on screen.
    /// </summary>
    private int rendered = 0;
    /// <summary>
    /// Input has changed.
    /// </summary>
    private bool changed = true;
    /// <summary>
    /// Command history
    /// </summary>
    private History history = new History();
    /// <summary>
    /// Tab options available in current context
    /// </summary>
    private SuperConsoleOptions options = new SuperConsoleOptions();
    /// <summary>
    /// Cursort anchor - position of cursor when the routine was called
    /// </summary>
    Cursor cursor;
    /// <summary>
    /// Python Engine reference. Used for the tab-completion lookup.
    /// </summary>
    private PythonEngine engine;

    private AutoResetEvent ctrlCEvent;
    private Thread MainEngineThread = Thread.CurrentThread;

    public MDbgSuperConsole(PythonEngine engine, bool colorfulConsole)
    {
        this.engine = engine;
        Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
        ctrlCEvent = new AutoResetEvent(false);
        if (colorfulConsole)
        {
            SetupColors();
        }
    }

    void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        if (e.SpecialKey == ConsoleSpecialKey.ControlC)
        {
            e.Cancel = true;
            ctrlCEvent.Set();
            MainEngineThread.Abort(new PythonKeyboardInterruptException(""));
        }
    }

    private bool GetOptions()
    {
        options.Clear();

        int len;
        for (len = input.Length; len > 0; len--)
        {
            char c = input[len - 1];
            if (Char.IsLetterOrDigit(c))
            {
                continue;
            }
            else if (c == '.' || c == '_')
            {
                continue;
            }
            else
            {
                break;
            }
        }

        string name = input.ToString(len, input.Length - len);
        if (name.Trim().Length > 0)
        {
            int lastDot = name.LastIndexOf('.');
            string attr, pref, root;
            if (lastDot < 0)
            {
                attr = String.Empty;
                pref = name;
                root = input.ToString(0, len);
            }
            else
            {
                attr = name.Substring(0, lastDot);
                pref = name.Substring(lastDot + 1);
                root = input.ToString(0, len + lastDot + 1);
            }

            try
            {
                IEnumerable result = engine.Evaluate(String.Format("dir({0})", attr)) as IEnumerable;
                options.Root = root;
                foreach (string option in result)
                {
                    if (option.StartsWith(pref, StringComparison.CurrentCultureIgnoreCase))
                    {
                        options.Add(option);
                    }
                }
            }
            catch
            {
                options.Clear();
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SetInput(string line)
    {
        input.Length = 0;
        input.Append(line);

        current = input.Length;

        Render();
    }

    private void Initialize()
    {
        cursor.Anchor();
        input.Length = 0;
        current = 0;
        rendered = 0;
        changed = false;
    }

    // Check if the user is backspacing the auto-indentation. In that case, we go back all the way to
    // the previous indentation level.
    // Return true if we did backspace the auto-indenation.
    private bool BackspaceAutoIndentation()
    {
        if (input.Length == 0 || input.Length > autoIndentSize) return false;

        // Is the auto-indenation all white space, or has the user since edited the auto-indentation?
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] != ' ') return false;
        }

        // Calculate the previous indentation level
        int newLength = ((input.Length - 1) / ConsoleOptions.AutoIndentSize) * ConsoleOptions.AutoIndentSize;

        int backspaceSize = input.Length - newLength;
        input.Remove(newLength, backspaceSize);
        current -= backspaceSize;
        Render();
        return true;
    }

    private void Backspace()
    {
        if (BackspaceAutoIndentation()) return;

        if (input.Length > 0 && current > 0)
        {
            input.Remove(current - 1, 1);
            current--;
            Render();
        }
    }

    private void Delete()
    {
        if (input.Length > 0 && current < input.Length)
        {
            input.Remove(current, 1);
            Render();
        }
    }

    private void Insert(ConsoleKeyInfo key)
    {
        char c;
        if (key.Key == ConsoleKey.F6)
        {
            Debug.Assert(FinalLineText.Length == 1);

            c = FinalLineText[0];
        }
        else
        {
            c = key.KeyChar;
        }
        Insert(c);
    }

    private void Insert(char c)
    {
        if (current == input.Length)
        {
            if (Char.IsControl(c))
            {
                string s = MapCharacter(c);
                current++;
                input.Append(c);
                CommandBase.Write(s);
                rendered += s.Length;
            }
            else
            {
                current++;
                input.Append(c);
                CommandBase.Write(new string(new char[] { c }));
                rendered++;
            }
        }
        else
        {
            input.Insert(current, c);
            current++;
            Render();
        }
    }

    private string MapCharacter(char c)
    {
        if (c == 13) return "\r\n";
        if (c <= 26) return "^" + ((char)(c + 'A' - 1)).ToString();

        return "^?";
    }

    private int GetCharacterSize(char c)
    {
        if (Char.IsControl(c))
        {
            return MapCharacter(c).Length;
        }
        else
        {
            return 1;
        }
    }

    private void Render()
    {
        cursor.Reset();
        StringBuilder output = new StringBuilder();
        int position = -1;
        for (int i = 0; i < input.Length; i++)
        {
            if (i == current)
            {
                position = output.Length;
            }
            char c = input[i];
            if (Char.IsControl(c))
            {
                output.Append(MapCharacter(c));
            }
            else
            {
                output.Append(c);
            }
        }

        if (current == input.Length)
        {
            position = output.Length;
        }

        string text = output.ToString();
        CommandBase.Write(text);

        if (text.Length < rendered)
        {
            CommandBase.Write(new String(' ', rendered - text.Length));
        }
        rendered = text.Length;
        cursor.Place(position);
    }

    private void MoveLeft(ConsoleModifiers keyModifiers)
    {
        if ((keyModifiers & ConsoleModifiers.Control) != 0)
        {
            // move back to the start of the previous word
            if (input.Length > 0 && current != 0)
            {
                bool nonLetter = IsSeperator(input[current - 1]);
                while (current > 0 && (current - 1 < input.Length))
                {
                    MoveLeft();

                    if (IsSeperator(input[current]) != nonLetter)
                    {
                        if (!nonLetter)
                        {
                            MoveRight();
                            break;
                        }

                        nonLetter = false;
                    }
                }
            }
        }
        else
        {
            MoveLeft();
        }
    }

    private bool IsSeperator(char ch)
    {
        return !Char.IsLetter(ch);
    }

    private void MoveRight(ConsoleModifiers keyModifiers)
    {
        if ((keyModifiers & ConsoleModifiers.Control) != 0)
        {
            // move to the next word
            if (input.Length != 0 && current < input.Length)
            {
                bool nonLetter = IsSeperator(input[current]);
                while (current < input.Length)
                {
                    MoveRight();

                    if (current == input.Length) break;
                    if (IsSeperator(input[current]) != nonLetter)
                    {
                        if (nonLetter)
                            break;

                        nonLetter = true;
                    }
                }
            }
        }
        else
        {
            MoveRight();
        }
    }

    private void MoveRight()
    {
        if (current < input.Length)
        {
            char c = input[current];
            current++;
            cursor.Move(GetCharacterSize(c));
        }
    }

    private void MoveLeft()
    {
        if (current > 0 && (current - 1 < input.Length))
        {
            current--;
            char c = input[current];
            cursor.Move(-GetCharacterSize(c));
        }
    }

    private const int TabSize = 4;
    private void InsertTab()
    {
        for (int i = TabSize - (current % TabSize); i > 0; i--)
        {
            Insert(' ');
        }
    }

    private void MoveHome()
    {
        current = 0;
        cursor.Reset();
    }

    private void MoveEnd()
    {
        current = input.Length;
        cursor.Place(rendered);
    }


    public string ReadLine(int autoIndentSizeInput)
    {
        if (dotPrompt)
        {
            CommandBase.Write("... ");
        }
        else
        {
            CommandBase.Write(">>> ");
        }

        Initialize();

        autoIndentSize = autoIndentSizeInput;
        for (int i = 0; i < autoIndentSize; i++)
            Insert(' ');

        int count = 0;
        for (; ; )
        {
            ConsoleKeyInfo key;
            key = (CommandBase.Shell.IO as IMDbgIO3).ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    Backspace();
                    break;
                case ConsoleKey.Delete:
                    Delete();
                    break;
                case ConsoleKey.Enter:
                    CommandBase.Write("\n");
                    string line = input.ToString();
                    if (line == FinalLineText) return null;
                    if (line.Length > 0)
                    {
                        history.AddLast(line);
                    }
                    if (count == 0)
                    {
                        dotPrompt = false;
                    }
                    options.Clear();
                    return line;
                case ConsoleKey.Tab:
                    {
                        bool prefix = false;
                        if (changed)
                        {
                            prefix = GetOptions();
                            changed = false;
                        }

                        if (options.Count > 0)
                        {
                            string part = (key.Modifiers & ConsoleModifiers.Shift) != 0 ? options.Previous() : options.Next();
                            SetInput(options.Root + part);
                        }
                        else
                        {
                            InsertTab();
                        }
                        continue;
                    }
                case ConsoleKey.UpArrow:
                    SetInput(history.Previous());
                    break;
                case ConsoleKey.DownArrow:
                    SetInput(history.Next());
                    break;
                case ConsoleKey.RightArrow:
                    MoveRight(key.Modifiers);
                    break;
                case ConsoleKey.LeftArrow:
                    MoveLeft(key.Modifiers);
                    break;
                case ConsoleKey.Escape:
                    SetInput(String.Empty);
                    break;
                case ConsoleKey.Home:
                    MoveHome();
                    break;
                case ConsoleKey.End:
                    MoveEnd();
                    break;
                case ConsoleKey.LeftWindows:
                case ConsoleKey.RightWindows:
                    // ignore these
                    break;
                default:
                    if (key.KeyChar == '\x0D') goto case ConsoleKey.Enter;      // Ctrl-M
                    if (key.KeyChar == '\x08') goto case ConsoleKey.Backspace;  // Ctrl-H
                    Insert(key);
                    break;
            }
            if (key.KeyChar == ':')
            {
                dotPrompt = true;
            }
            changed = true;
            count++;
        }
    }



    string FinalLineText
    {
        get
        {
            return Environment.OSVersion.Platform != PlatformID.Unix ? "\x1A" : "\x04";
        }
    }

    public void Write(string text, Style style)
    {
        switch (style)
        {
            case Style.Prompt: WriteColor(text, PromptColor); break;
            case Style.Out: WriteColor(text, OutColor); break;
            case Style.Error: WriteColor(text, ErrorColor); break;
        }
    }

    public void WriteLine(string text, Style style)
    {
        Write(text + Environment.NewLine, style);
    }

    private void WriteColor(string s, ConsoleColor c)
    {
        ConsoleColor origColor = Console.ForegroundColor;
        Console.ForegroundColor = c;
        CommandBase.Write(s);
        Console.ForegroundColor = origColor;
    }

}

