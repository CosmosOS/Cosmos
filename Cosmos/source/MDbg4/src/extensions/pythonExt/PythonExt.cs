//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.CorDebug;

using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using SS = System.Diagnostics.SymbolStore;

using IronPython.Hosting;


// extension class name must have [MDbgExtensionEntryPointClass] attribute on it and implement a LoadExtension()
[MDbgExtensionEntryPointClass(
    Url = "http://blogs.msdn.com/jmstall",
    ShortDescription = "Iron Python MDbg extension."
)]

public class PythonExt : CommandBase
{
    private static PythonEngine g_python; // The main python engine.
    private static Dictionary<string, DateTime> g_pythonScriptModificationTimes = new Dictionary<string, DateTime>();
    private static PythonCommandLine g_pythonInteractive;
    private static IMDbgCommand g_origLoadCmd;

    // This is called when the python extension is first loaded.
    public static void LoadExtension()
    {
        WriteOutput("IronPython-Mdbg Extension loaded");

        g_python = new IronPython.Hosting.PythonEngine();
        g_pythonInteractive = new PythonCommandLine(g_python);

        string ver = g_python.GetType().Assembly.GetName().Version.ToString();
        WriteOutput(MDbgOutputConstants.Ignore, "Binding against Iron Python version: " + ver);

        // Get original load command, for use in python load command.
        g_origLoadCmd = Shell.Commands.Lookup("load");

        // Add Python extension commands to Shell.Commands.
        MDbgAttributeDefinedCommand.AddCommandsFromType(Shell.Commands, typeof(PythonExt));

        // Add the current directory to the python engine search path.
        
        g_python.AddToPath(Environment.CurrentDirectory);

        // Tell Python about some key objects in Mdbg. Python can then reflect over these objects.
        // These variables live at some special "main" scope. Python Modules imported via python "import" command
        // can't access them. Use PythonEngine.ExecuteFile() to import files such that they can access these vars.
        g_python.Globals.Add("CommandBase", IronPython.Runtime.Types.ReflectedType.FromType(typeof(CommandBase)));

        // Hook input + output. This is redirecting pythons 'sys.stdout, sys.stdin, sys.stderr' 
        // This connects python's sys.stdout --> Stream --> Mdbg console.
        MDbgStream s = new MDbgStream();

        // New codepaths for IronPython 1.0
        g_python.SetStandardInput(s);
        g_python.SetStandardOutput(s);
        g_python.SetStandardError(s);
    }

    /// <summary>
    /// This extension's python engine, used for the execution of all python code in MDbg.
    /// </summary>
    public static PythonEngine Engine
    {
        get
        {
            return g_python;
        }
    }

    /// <summary>
    /// Helper function to print the result of a python evaluation.
    /// </summary>
    /// <param name="o">Result of a python evaluation.</param>
    public static void PrintPythonResult(object o)
    {
        if (o == null)
        {
            WriteOutput("Result: null");
        }
        else
        {
            WriteOutput("Result: " + o.ToString() + " (of type = " + o.GetType() + ")");
        }
    }

    #region MDbg Commands

    /// <summary>
    /// Command to import python modules into the same scope that we exposed Shell variable 
    /// to during initialization. This lets these modules access the key Mdbg vars
    /// and thus traverse the Mdbg tree.
    /// </summary>
    /// <param name="args">The filename to load. To load a python script, the filename must
    /// contain the ".py" extension.</param>
    [
        CommandDescription(
        CommandName = "load",
        MinimumAbbrev = 2,
        ShortHelp = "Loads mdbg extension or python script",
        LongHelp = @"Loads an mdbg extension or python script file."
        )
    ]
    public static void LoadCmd(string args)
    {
        if (args.EndsWith(".py", System.StringComparison.CurrentCultureIgnoreCase))
        {
            string pythonScript = Shell.FileLocator.GetFileLocation(args);

            if (pythonScript != null)
            {
                LoadPythonScript(pythonScript);
                ShellRescanPythonCommands();
            }
            else
            {
                WriteOutput(MDbgOutputConstants.StdError, "Python script not found");
            }
        }
        else
        {
            g_origLoadCmd.Execute(args);
        }
    }

    /// <summary>
    /// Command to reload a previously loaded and since modified python script.
    /// </summary>
    /// <param name="args">An empty string, "all", or one or more integers separated by spaces.</param>
    [
        CommandDescription(
        CommandName = "pyrefresh",
        MinimumAbbrev = 3,
        ShortHelp = "Reloads previously loaded and since modified python scripts",
        LongHelp = @"Reloads previously loaded and since modified python scripts.
    pyr
        displays a numbered list of all previously loaded and since modified python scripts
    pyr all
        reloads all previously loaded and since modified python scripts
    pyr num [num [num ...]]
        reloads scripts corresponding to numbers (in the list displayed if no arguments are entered)
    example inputs: pyr ; pyr all ; pyr 2 ; pyr 1 3 5        
")
    ]
    public static void RefreshCmd(string args)
    {
        List<string> changedScripts = ChangedScripts();
        if (args.Length == 0)
        {
            // no arguments have been entered, display a list of all changed scripts
            WriteOutput("Modified Python Scripts:");
            foreach (string script in changedScripts)
            {
                WriteOutput(String.Format("{0}) {1}", new object[] { changedScripts.IndexOf(script) + 1, script }));
            }
        }
        else if (args == "all")
        {
            // reload all changed scripts
            foreach (string script in changedScripts)
            {
                LoadPythonScript(script);
                WriteOutput(MDbgOutputConstants.Ignore, "python script reloaded: " + script);
                ShellRescanPythonCommands();
            }
        }
        else
        {
            // user has specified which scripts to reload
            string[] indices = args.Split();
            string script;
            int idx;
            foreach (string indx in indices)
            {
                idx = Int32.Parse(indx.Trim());
                try
                {
                    script = changedScripts[idx - 1];
                }
                catch
                {
                    throw new MDbgShellException("Index is out of range.");
                }
                LoadPythonScript(script);
                WriteOutput(MDbgOutputConstants.Ignore, "python script reloaded: " + script);
            }
            ShellRescanPythonCommands();
        }
    }

    /// <summary>
    /// Enter Python-interactive mode.
    /// This drops us to a python prompt:
    /// - so we don't need to use the "python" mdbg command to execute python commands). 
    /// - It also lets us type multi-line python commands (like defining functions).   
    /// </summary>
    /// <param name="args">To drop to the python interactive prompt, enter no arguments.
    /// To simulate entering a line of python code in the interactive prompt without
    /// actually dropping to the interactive prompt, enter the line of python code
    /// surrounded by quotes as the argument.</param>
    [
        CommandDescription(
        CommandName = "pyinteractive",
        MinimumAbbrev = 4,
        IsRepeatable = false,
        ShortHelp = "Enters python interactive mode.",
        LongHelp = @"Enters python interactive mode, which drops user to a python prompt.
    To exit python interactive mode, type 'pyout'.
"
        )
    ]
    public static void PythonInteractiveCmd(string args)
    {
        if (args.Length == 0)
        {
            g_pythonInteractive.RunInteractive();
        }
        else
        {
            // Check that input is in the correct format
            if ((args[0] != '\"') || (args[args.Length - 1] != '\"'))
            {
                throw new MDbgShellException("Argument to pyin command must be surrounded by quotation marks");
            }

            // Remove the quotes surrounding the line of python code. These
            // are necessary to preserve whitespace that would otherwise
            // be trimmed off by Shell.Commands.ParseCommand().
            g_pythonInteractive.Execute(args.Substring(1, args.Length - 2));
        }
    }

    /// <summary>
    /// Execute a python command. Commands can include expressions as well as 
    /// function definitions. 
    /// </summary>
    /// <param name="args">The command to execute.</param>
    [
        CommandDescription(
        CommandName = "python",
        MinimumAbbrev = 2,
        IsRepeatable = false,
        ShortHelp = "Executes a single python command",
        LongHelp = @"Executes a single python command or expression.
    Examples: 
        py print 1+2
        py def MyAdd(a,b): return a + b
        py print MyAdd(1,2)
"
        )
    ]
    public static void PythonCommand(string args)
    {
        g_python.Execute(args);
    }

    /// <summary>
    /// Evaluate a python expression.
    /// </summary>
    /// <param name="args">The command to evaluate.</param>
    [
        CommandDescription(
        CommandName = "peval",
        MinimumAbbrev = 2,
        ShortHelp = "Evaluate a python expression",
        LongHelp = @"Evaluates a python expression and prints the result.
    Examples:
        pe 1+2
        pe MyAdd(1,2)
"
        )
    ]
    public static void PyEval(string args)
    {
        object o = g_python.Evaluate(args);
        PrintPythonResult(o);
    }

    #endregion MDbg Commands

    private static void LoadPythonScript(string scriptLocation)
    {
        g_python.ExecuteFile(scriptLocation);
        DateTime changedOn = File.GetLastWriteTimeUtc(scriptLocation);
        if (g_pythonScriptModificationTimes.ContainsKey(scriptLocation))
            g_pythonScriptModificationTimes[scriptLocation] = changedOn;
        else
            g_pythonScriptModificationTimes.Add(scriptLocation.ToLower(), changedOn);
    }

    // Rescans loaded scripts for new MDbg commands, and adds them to Shell.Commands.
    private static void ShellRescanPythonCommands()
    {
        List<IMDbgCommand> cl = RescanPythonCommands();
        if (cl == null)
        {
            WriteError("Scanning for mdbg commands failed");
        }
        else
        {
            // add all commands from python scripts
            foreach (IMDbgCommand mc in cl)
            {
                Shell.Commands.Add(mc);
            }
            // there is currently no API to remove commands
        }
    }

    // Returns a list of all MDbg commands defined in loaded python scripts or at the command line.
    private static List<IMDbgCommand> RescanPythonCommands()
    {
        object r = g_python.Evaluate("dir()");
        IronPython.Runtime.List namesList = r as IronPython.Runtime.List;
        if (namesList == null)
            return null;

        List<IMDbgCommand> scannedCommands = new List<IMDbgCommand>();
        foreach (string name in namesList)
        {
            // test if it is an mdbg command
            try
            {
                r = g_python.Evaluate(string.Format("{0}.__doc__", name));
            }
            catch
            {
                r = null;
            }
            string docString = r as string;
            IMDbgCommand mdbgCmd = ParseCommandDocString(name, docString);
            if (mdbgCmd != null)
                scannedCommands.Add(mdbgCmd);
        }
        return scannedCommands;
    }

    // Returns a list of all scripts that have been modified since the last time they were loaded.
    private static List<string> ChangedScripts()
    {
        List<string> loadedScripts = new List<string>(g_pythonScriptModificationTimes.Keys);
        List<string> changedScripts = new List<string>();
        foreach (string pythonScript in loadedScripts)
        {
            DateTime accessedOn = File.GetLastWriteTimeUtc(pythonScript);
            if (accessedOn > g_pythonScriptModificationTimes[pythonScript])
            {
                changedScripts.Add(pythonScript);
            }
        }
        return changedScripts;
    }

    // Command to parse an MDbg command that has been defined in a python script.
    private static IMDbgCommand ParseCommandDocString(string funcName, string docString)
    {
        if (docString == null)
            return null;

        string[] docLines = docString.Split('\n');
        if (docLines.Length > 0 && docLines[0] == "[MdbgCommand]")
        {
            int cmdMinimumAbbrev = 0;
            bool cmdIsRepeatable = false;
            string cmdName = null;
            string cmdShortHelp = null;
            string cmdLongHelp = null;

            // it is a mdbg command
            for (int i = 1; i < docLines.Length; ++i)
            {
                string line = docLines[i];
                int colonIdx = line.IndexOf(":");
                if (colonIdx == -1)
                    throw new MDbgShellException(string.Format("Python mdbg command '{0}' has invalid documentation part: {1}",
                        funcName, line));
                string keyword = line.Substring(0, colonIdx).Trim();
                string value = line.Substring(colonIdx + 1).Trim();
                switch (keyword)
                {
                    case "CommandName":
                        cmdName = value;
                        break;
                    case "MinimumAbbrev":
                        cmdMinimumAbbrev = Int32.Parse(value);
                        break;
                    case "ShortHelp":
                        cmdShortHelp = value;
                        break;
                    case "Repeatable":
                        cmdIsRepeatable = Boolean.Parse(value);
                        break;
                    case "LongHelp":
                        // this string is tricky; we want to gather the rest of the file
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(value);
                            for (++i; i < docLines.Length; ++i)
                            {
                                sb.Append("\n");
                                sb.Append(docLines[i]);
                            }
                            cmdLongHelp = sb.ToString();
                        }
                        break;
                    default:
                        throw new MDbgShellException(string.Format("Python doc string for mdbg command {0} has invalid keyword {1}",
                            cmdName, keyword));
                }
            }
            return new MDbgPythonCommand(funcName, cmdName, cmdIsRepeatable, cmdMinimumAbbrev, cmdShortHelp, cmdLongHelp);
        }
        else
            return null;
    }

    /// <summary>
    /// An MDbg command that has been defined in a loaded Python script, or while in Python
    /// interactive mode in MDbg.
    /// </summary>
    class MDbgPythonCommand : IMDbgCommand
    {
        public MDbgPythonCommand(string funcName, string cmdName, bool isRepeatable, int minAbbrev,
                                 string shortHelp, string LongHelp)
        {
            m_funcName = funcName;
            m_cmdName = cmdName;
            m_isRepeatable = isRepeatable;
            m_minAbbrev = minAbbrev;
            m_shortHelp = shortHelp;
            m_longHelp = LongHelp;
            if (m_minAbbrev <= 0 || m_minAbbrev > funcName.Length)
                m_minAbbrev = funcName.Length;
        }

        private string m_funcName, m_cmdName, m_shortHelp, m_longHelp;
        private bool m_isRepeatable;
        private int m_minAbbrev;

        #region IMDbgCommand Members

        string IMDbgCommand.CommandName
        {
            get { return m_cmdName; }
        }

        void IMDbgCommand.Execute(string args)
        {
            g_python.Execute(string.Format("{0}('{1}')", m_funcName, args));
        }

        bool IMDbgCommand.IsRepeatable
        {
            get { return m_isRepeatable; }
        }

        Assembly IMDbgCommand.LoadedFrom
        {
            get { return Assembly.GetExecutingAssembly(); }
        }

        string IMDbgCommand.LongHelp
        {
            get { return m_longHelp; }
        }

        int IMDbgCommand.MinimumAbbrev
        {
            get { return m_minAbbrev; }
        }

        string IMDbgCommand.ShortHelp
        {
            get { return m_shortHelp; }
        }

        #endregion

        #region IComparable Members

        int IComparable.CompareTo(object obj)
        {
            MDbgPythonCommand pc = obj as MDbgPythonCommand;
            if (pc != null)
                return string.Compare(this.m_funcName, pc.m_funcName);
            else
            {
                IMDbgCommand mc = obj as IMDbgCommand;
                Debug.Assert(mc != null);
                if (mc == null)
                    return 1;
                return -mc.CompareTo(this);
            }
        }
        #endregion
    }

    // Stream to send Python Output to Mdbg console.
    // This can be used to redirect python's sys.stdout, sys.stdin, and sys.stderr.
    class MDbgStream : Stream
    {
        #region unsupported Read + Seek members
        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            //nop
        }

        public override long Length
        {
            get { throw new NotSupportedException("Seek not supported"); } // can't seek 
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException("Seek not supported");  // can't seek 
            }
            set
            {
                throw new NotSupportedException("Seek not supported");  // can't seek 
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Reed not supported"); // can't read
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seek not supported"); // can't seek
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("SetLength not supported"); // can't seek
        }
        #endregion

        public override void Write(byte[] buffer, int offset, int count)
        {
            StringBuilder sb = new StringBuilder();

            while (count > 0)
            {
                char ch = (char)buffer[offset];

                sb.Append(ch);

                offset++;
                count--;
            }
            // Dump remainder.
            CommandBase.Write(MDbgOutputConstants.StdOutput, sb.ToString());
        }
    }
}