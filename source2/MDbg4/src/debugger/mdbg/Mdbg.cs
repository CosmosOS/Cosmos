//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Text;
using System.Security.Permissions;
using System.Globalization;

using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;

// UNDONE: Fix the underlying cause of this warning

/*
1>mdbg\shell\mdbg.cs(29,24) : error CS3003: Warning as Error: Type of 'Microsoft.Samples.Tools.Mdbg.MDbgShell.IO' is not CLS-compliant
1>mdbg\shell\mdbg.cs(42,27) : error CS3003: Warning as Error: Type of 'Microsoft.Samples.Tools.Mdbg.MDbgShell.Debugger' is not CLS-compliant
1>mdbg\shell\mdbg.cs(58,39) : error CS3003: Warning as Error: Type of 'Microsoft.Samples.Tools.Mdbg.MDbgShell.Commands' is not CLS-compliant
1>mdbg\shell\mdbg.cs(73,33) : error CS3003: Warning as Error: Type of 'Microsoft.Samples.Tools.Mdbg.MDbgShell.FileLocator' is not CLS-compliant
1>mdbg\shell\mdbg.cs(81,36) : error CS3003: Warning as Error: Type of 'Microsoft.Samples.Tools.Mdbg.MDbgShell.SourceFileMgr' is not CLS-compliant
1>mdbg\shell\mdbg.cs(109,34) : error CS3003: Warning as Error: Type of 'Microsoft.Samples.Tools.Mdbg.MDbgShell.BreakpointParser' is not CLS-compliant
1>mdbg\shell\mdbg.cs(123,34) : error CS3003: Warning as Error: Type of 'Microsoft.Samples.Tools.Mdbg.MDbgShell.ExpressionParser' is not CLS-compliant
1>mdbg\shell\mdbg.cs(136,50) : error CS3003: Warning as Error: Type of 'Microsoft.Samples.Tools.Mdbg.MDbgShell.OnCommandExecuted' is not CLS-compliant
*/

namespace Microsoft.Samples.Tools.Mdbg
{

    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class MDbgShell : IMDbgShell
    {

        public IMDbgIO IO
        {
            get
            {
                return m_io;
            }
            set
            {
                Debug.Assert(value != null);
                m_io = value;
            }
        }

        public MDbgEngine Debugger
        {
            get
            {
                return m_debugger;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException();
                }
                m_debugger = value;
            }
        }

        public IMDbgCommandCollection Commands
        {
            get
            {
                return m_commands;
            }
        }


        // Quit w/ an exit code.
        public void QuitWithExitCode(int exitCode)
        {
            m_run = false;
            m_exitCode = exitCode;
        }

        public IMDbgFileLocator FileLocator
        {
            get
            {
                return m_fileLocator;
            }
        }

        public IMDbgSourceFileMgr SourceFileMgr
        {
            get
            {
                return m_sourceFileMgr;
            }
        }

        public IDictionary Properties
        {
            get
            {
                return m_properties;
            }
        }

        IBreakpointParser m_BreakpointParser;

        /// <summary>
        /// Get the default Breakpoint parser for this collection.
        /// </summary>
        /// <remarks> The breakpoint collection maintains a default breakpoint parser. 
        /// Extensions can get the parser so that they can use the share the parsing implementation to get the 
        /// same breakpoint syntax as the rest of the shell. This encourages a uniform breakpoint syntax.
        /// Extensions can also set the parser so that they can override and even extend the breakpoint syntax.
        /// This may be null (and is in fact null by default), though it is reasonable for extensions to 
        /// expect that the shell supplies a parser.
        /// </remarks>
        public IBreakpointParser BreakpointParser
        {
            get
            {
                return m_BreakpointParser;
            }
            set
            {
                m_BreakpointParser = value;
            }
        }

        IExpressionParser m_ExpressionParser;

        public IExpressionParser ExpressionParser
        {
            get
            {
                return m_ExpressionParser;
            }
            set
            {
                m_ExpressionParser = value;
            }
        }

        public event CommandExecutedEventHandler OnCommandExecuted;

        public int Start(string[] commandLineArguments)
        {
            this.Init(commandLineArguments);

            PrintStartupLogo();
            return this.RunInputLoop();
        }

        //////////////////////////////////////////////////////////////////////////////////
        //
        // Customization methods (to be overriden in aditional skins).
        //
        //////////////////////////////////////////////////////////////////////////////////

        protected virtual void Init(string[] commandLineArguments)
        {
            string[] initialCommands = null;

            // process startup commands
            if (commandLineArguments.Length != 0)
            {
                ArrayList startupCommands = new ArrayList();
                if (commandLineArguments[0].Length > 1 && commandLineArguments[0][0] == '!')
                {
                    // ! commands on command line
                    int i = 0;
                    while (i < commandLineArguments.Length)
                    {
                        StringBuilder sb = new StringBuilder();
                        Debug.Assert(commandLineArguments[i][0] == '!');
                        sb.Append(commandLineArguments[i].Substring(1));
                        ++i;
                        while (i < commandLineArguments.Length &&
                              !(commandLineArguments[i].Length > 1 && commandLineArguments[i][0] == '!'))
                        {
                            sb.Append(' ');
                            sb.Append(commandLineArguments[i]);
                            ++i;
                        }
                        startupCommands.Add(sb.ToString());
                    }
                }
                else
                {
                    // it is name of executable on the command line
                    StringBuilder sb = new StringBuilder("run");
                    for (int i = 0; i < commandLineArguments.Length; i++)
                    {
                        sb.Append(' ');
                        string arg = commandLineArguments[i];
                        if (arg.IndexOf(' ') != -1)
                        {
                            // argument contains spaces, need to quote it
                            sb.Append('\"').Append(arg).Append('\"');
                        }
                        else
                        {
                            sb.Append(arg);
                        }
                    }
                    startupCommands.Add(sb.ToString());
                }

                initialCommands = (string[])startupCommands.ToArray(typeof(string));
            }

            this.IO = new MDbgIO(this, initialCommands);

            MdbgCommands.Shell = this;

            m_debugger = new MDbgEngine();
            MdbgCommands.Initialize();

            OnCommandExecuted += new CommandExecutedEventHandler(MdbgCommands.WhenHandler);

            ProcessAutoExec();
        }

        protected virtual void PrintStartupLogo()
        {
            string myVersion = GetBinaryVersion();

            IO.WriteOutput(MDbgOutputConstants.StdOutput,
                           "MDbg (Managed debugger) v" + myVersion + " started.\n");
            IO.WriteOutput(MDbgOutputConstants.StdOutput,
                           "Copyright (C) Microsoft Corporation. All rights reserved.\n");
            IO.WriteOutput(MDbgOutputConstants.StdOutput,
                           "\nFor information about commands type \"help\";\nto exit program type \"quit\".\n\n");

            // Check for and output any debugging warnings
            try
            {
                (new CorDebugger(CorDebugger.GetDefaultDebuggerVersion())).CanLaunchOrAttach(0, false);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                IO.WriteOutput(MDbgOutputConstants.StdOutput, "WARNING: " + e.Message + "\n\n");
            }

        }

        public virtual void DisplayCurrentLocation()
        {
            if (!Debugger.Processes.HaveActive)
            {
                CommandBase.WriteOutput("STOP: Process Exited");
                return; // don't try to display current location
            }
            else
            {
                Debug.Assert(Debugger.Processes.HaveActive);
                Object stopReason = Debugger.Processes.Active.StopReason;
                Type stopReasonType = stopReason.GetType();
                if (stopReasonType == typeof(StepCompleteStopReason))
                {
                    // just ignore those
                }
                else if (stopReasonType == typeof(ThreadCreatedStopReason))
                {
                    CommandBase.WriteOutput("STOP: Thread Created");
                }
                else if (stopReasonType == typeof(BreakpointHitStopReason))
                {
                    MDbgBreakpoint b = (stopReason as BreakpointHitStopReason).Breakpoint;
                    if (b.Number == 0)                     // specal case to keep compatibility with our test scripts.
                    {
                        CommandBase.WriteOutput("STOP: Breakpoint Hit");
                    }
                    else
                    {
                        CommandBase.WriteOutput(String.Format(CultureInfo.InvariantCulture, "STOP: Breakpoint {0} Hit", new Object[] { b.Number }));
                    }
                }

                else if (stopReasonType == typeof(ExceptionThrownStopReason))
                {
                    ExceptionThrownStopReason ex = (ExceptionThrownStopReason)stopReason;
                    CommandBase.WriteOutput("STOP: Exception thrown");
                    PrintCurrentException();
                    if (Debugger.Options.StopOnExceptionEnhanced
                        || ex.ExceptionEnhancedOn)
                    {
                        // when we are in ExceptionEnhanced mode, we print more information
                        CommandBase.WriteOutput("\tOffset:    " + ex.Offset);
                        CommandBase.WriteOutput("\tEventType: " + ex.EventType);
                        CommandBase.WriteOutput("\tIntercept: " + (ex.Flags != 0));
                    }
                }

                else if (stopReasonType == typeof(UnhandledExceptionThrownStopReason))
                {
                    CommandBase.WriteOutput("STOP: Unhandled Exception thrown");
                    PrintCurrentException();
                    CommandBase.WriteOutput("");
                    CommandBase.WriteOutput("This is unhandled exception, continuing will end the process");
                }

                else if (stopReasonType == typeof(ExceptionUnwindStopReason))
                {
                    CommandBase.WriteOutput("STOP: Exception unwind");
                    CommandBase.WriteOutput("EventType: " + (stopReason as ExceptionUnwindStopReason).EventType);
                }

                else if (stopReasonType == typeof(ModuleLoadedStopReason))
                {
                    CommandBase.WriteOutput("STOP: Module loaded: " + (stopReason as ModuleLoadedStopReason).Module.CorModule.Name);
                }
                else if (stopReasonType == typeof(AssemblyLoadedStopReason))
                {
                    CommandBase.WriteOutput("STOP: Assembly loaded: " + (stopReason as AssemblyLoadedStopReason).Assembly.Name);
                }
                else if (stopReasonType == typeof(MDANotificationStopReason))
                {
                    CorMDA mda = (stopReason as MDANotificationStopReason).CorMDA;

                    CommandBase.WriteOutput("STOP: MDANotification");
                    CommandBase.WriteOutput("Name=" + mda.Name);
                    CommandBase.WriteOutput("XML=" + mda.XML);
                }
                else if (stopReasonType == typeof(MDbgErrorStopReason))
                {
                    Exception e = (stopReason as MDbgErrorStopReason).ExceptionThrown;
                    CommandBase.WriteOutput("STOP: MdbgError");
                    CommandBase.WriteOutput(FormatExceptionDiagnosticText(e));
                }
                else
                {
                    CommandBase.WriteOutput("STOP " + Debugger.Processes.Active.StopReason);
                }
            }

            if (!Debugger.Processes.Active.Threads.HaveActive)
            {
                return;                                     // we won't try to show current location
            }

            MDbgThread thr = Debugger.Processes.Active.Threads.Active;

            MDbgSourcePosition pos = thr.CurrentSourcePosition;
            if (pos == null)
            {
                MDbgFrame f = thr.CurrentFrame;
                if (f.IsManaged)
                {
                    CorDebugMappingResult mappingResult;
                    uint ip;
                    f.CorFrame.GetIP(out ip, out mappingResult);
                    string s = "IP: " + ip + " @ " + f.Function.FullName + " - " + mappingResult;
                    CommandBase.WriteOutput(s);
                }
                else
                {
                    CommandBase.WriteOutput("<Located in native code.>");
                }
            }
            else
            {
                string fileLoc = FileLocator.GetFileLocation(pos.Path);
                if (fileLoc == null)
                {
                    // Using the full path makes debugging output inconsistant during automated test runs.
                    // For testing purposes we'll get rid of them.
                    //CommandBase.WriteOutput("located at line "+pos.Line + " in "+ pos.Path);
                    CommandBase.WriteOutput("located at line " + pos.Line + " in " + System.IO.Path.GetFileName(pos.Path));
                }
                else
                {
                    IMDbgSourceFile file = SourceFileMgr.GetSourceFile(fileLoc);
                    string prefixStr = pos.Line.ToString(CultureInfo.InvariantCulture) + ":";

                    if (pos.Line < 1 || pos.Line > file.Count)
                    {
                        CommandBase.WriteOutput("located at line " + pos.Line + " in " + pos.Path);
                        throw new MDbgShellException(string.Format("Could not display current location; file {0} doesn't have line {1}.",
                                                                   file.Path, pos.Line));
                    }
                    Debug.Assert((pos.Line > 0) && (pos.Line <= file.Count));
                    string lineContent = file[pos.Line];

                    if (pos.StartColumn == 0 && pos.EndColumn == 0
                        || !(CommandBase.Shell.IO is IMDbgIO2)) // or we don't have support for IMDbgIO2
                    {
                        // we don't know location in the line
                        CommandBase.Shell.IO.WriteOutput(MDbgOutputConstants.StdOutput, prefixStr + lineContent + "\n");
                    }
                    else
                    {
                        int hiStart;
                        if (pos.StartColumn > 0)
                        {
                            hiStart = pos.StartColumn - 1;
                        }
                        else
                        {
                            hiStart = 0;
                        }

                        int hiLen;
                        if (pos.EndColumn == 0                   // we don't know ending position
                            || (pos.EndLine > pos.StartLine)) // multi-line statement, select whole 1st line
                        {
                            hiLen = lineContent.Length;
                        }
                        else
                        {
                            hiLen = pos.EndColumn - 1 - hiStart;
                        }
                        Debug.Assert(CommandBase.Shell.IO is IMDbgIO2); // see if condition above
                        (CommandBase.Shell.IO as IMDbgIO2).WriteOutput(MDbgOutputConstants.StdOutput, prefixStr + lineContent + "\n",
                                                         hiStart + prefixStr.Length, hiLen);
                    }
                }
            }
        }

        private string FormatExceptionDiagnosticText(Exception e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Exception Type:       " + e.GetType().Name);
            sb.AppendLine("Exception Message:    " + e.Message);
            sb.AppendLine("Exception StackTrace: ");
            sb.AppendLine(e.StackTrace);
            if (e.InnerException != null)
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("Exception was caused by the following inner exception...");
                sb.Append(FormatExceptionDiagnosticText(e.InnerException));
            }

            return sb.ToString();
        }

        public virtual void ReportException(Exception e)
        {
            if (e == null)
            {
                // If we don't know anything about the exception, report a standard error
                // This should only be used for handling Non-CLS compliant exceptions.  The
                // FxCop rule CatchNonClsCompliantExceptionsInGeneralHandlers requires that
                // we catch non-CLS exceptions whenever we are catching all other exceptions.
                // Ideally, we should just be more selective in the errors we throw and catch
                IO.WriteOutput(MDbgOutputConstants.StdError, "An Unknown error has occurred.\n");
            }
            else if (e is ThreadInterruptedException)
            {
                IO.WriteOutput(MDbgOutputConstants.StdError, "Interrupted!\n");
            }
            else
            {
                if (!CommandBase.ShowFullExceptionInfo)
                {
                    IO.WriteOutput(MDbgOutputConstants.StdError, e.GetBaseException().Message + "\n");
                }
                else
                {
                    IO.WriteOutput(MDbgOutputConstants.StdError, FormatExceptionDiagnosticText(e) + "\n");
                }
                if (MdbgCommands.AssertOnErrors)
                {
                    IO.WriteOutput(MDbgOutputConstants.StdError, e.GetBaseException().ToString() + "\n");
                    Debug.Assert(false, "Failure. Assert on Error turned on (mode ae on)\n" + e.GetBaseException().ToString());
                }
                if (Mdbg.CommandBase.FailOnError)
                {
                    IO.WriteOutput(MDbgOutputConstants.StdError, e.GetBaseException().ToString() + "\n");
                    Environment.Exit(-1);
                }

            }
        }

        //////////////////////////////////////////////////////////////////////////////////
        //
        // Helper methods
        //
        //////////////////////////////////////////////////////////////////////////////////

        protected static string GetBinaryVersion()
        {
            string assemblyName = Assembly.GetAssembly(typeof(MDbgShell)).Location;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assemblyName);
            string myVersion = fvi.FileVersion;
            return myVersion;
        }

        //////////////////////////////////////////////////////////////////////////////////
        //
        // Private implementation
        //
        //////////////////////////////////////////////////////////////////////////////////

        private int RunInputLoop()
        {
            // Run the event loop
            string input;
            IMDbgCommand cmd = null;
            string cmdArgs = null;

            int stopCount = -1;
            while (m_run && IO.ReadCommand(out input))
            {
                try
                {
                    if (Debugger.Processes.HaveActive)
                    {
                        stopCount = Debugger.Processes.Active.StopCounter;
                    }

                    if (input.Length == 0)
                    {
                        if (cmd == null || !cmd.IsRepeatable)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        Commands.ParseCommand(input, out cmd, out cmdArgs);
                    }
                    cmd.Execute(cmdArgs);

                    int newStopCount = Debugger.Processes.HaveActive ? Debugger.Processes.Active.StopCounter : Int32.MaxValue;
                    bool movementCommand = newStopCount > stopCount;
                    stopCount = newStopCount;

                    if (OnCommandExecuted != null)
                    {
                        OnCommandExecuted(this, new CommandExecutedEventArgs(this, cmd, cmdArgs, movementCommand));
                    }

                    newStopCount = Debugger.Processes.HaveActive ? Debugger.Processes.Active.StopCounter : Int32.MaxValue;
                    movementCommand = newStopCount > stopCount;

                    while (newStopCount > stopCount)
                    {
                        stopCount = newStopCount;

                        if (OnCommandExecuted != null)
                        {
                            OnCommandExecuted(this, new CommandExecutedEventArgs(this, null, null, movementCommand));
                        }

                        newStopCount = Debugger.Processes.HaveActive ? Debugger.Processes.Active.StopCounter : Int32.MaxValue;
                        movementCommand = newStopCount > stopCount;
                    }
                    stopCount = newStopCount;
                }
                catch (Exception e)
                {
                    ReportException(e);
                }
            } // end while
            return m_exitCode;
        }

        private void PrintCurrentException()
        {
            MDbgValue ex = Debugger.Processes.Active.Threads.Active.CurrentException;
            if (ex.IsNull)
            {
                // No current exception is available.  Perhaps the user switched to a different
                // thread which was not throwing an exception.
                return;
            }

            CommandBase.WriteOutput("Exception=" + ex.GetStringValue(0));
            foreach (MDbgValue f in ex.GetFields())
            {
                string outputType;
                string outputValue;

                if (f.Name == "_xptrs" || f.Name == "_xcode" || f.Name == "_stackTrace" ||
                    f.Name == "_remoteStackTraceString" || f.Name == "_remoteStackIndex" ||
                    f.Name == "_exceptionMethodString")
                {
                    outputType = MDbgOutputConstants.Ignore;
                }
                else
                {
                    outputType = MDbgOutputConstants.StdOutput;
                }

                outputValue = f.GetStringValue(0);
                // remove new line characters in string
                if (outputValue != null && (f.Name == "_exceptionMethodString" || f.Name == "_remoteStackTraceString"))
                {
                    outputValue = outputValue.Replace('\n', '#');
                }

                CommandBase.WriteOutput(outputType, "\t" + f.Name + "=" + outputValue);
            }
        }


        private void ProcessAutoExec()
        {
            const string autoLoadListFileName = "mdbgAutoExec.txt";
            string mdbgDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Common case is if the file does not exist, so check that first to avoid an exception.
            string filename = mdbgDirectory + Path.DirectorySeparatorChar + autoLoadListFileName;
            if (!File.Exists(filename))
            {
                return;
            }

            StreamReader sr = null;

            try
            {
                sr = new StreamReader(filename);

                string s = sr.ReadLine();
                while (s != null)
                {
                    try
                    {
                        if ((s.Length != 0)
                            && !s.StartsWith("#"))
                            CommandBase.ExecuteCommand(s);
                    }
                    catch (Exception e)
                    {
                        ReportException(e);
                    }
                    s = sr.ReadLine();
                }
            }
            catch (System.IO.IOException ex)
            {
                // we'll ignore IO exceptions
                ReportException(ex);
            }
            finally
            {
                if (sr != null)
                    sr.Close(); // free resources in advance
            }

        }

        //////////////////////////////////////////////////////////////////////////////////
        //
        // Variables
        //
        //////////////////////////////////////////////////////////////////////////////////

        private bool m_run = true;
        private int m_exitCode = 0; // exit code when we quit.

        private MDbgEngine m_debugger = null;
        private IMDbgFileLocator m_fileLocator = new MDbgFileLocator();

        private IMDbgIO m_io;
        private IMDbgCommandCollection m_commands = new MDbgCommandSetCollection();
        private MDbgSourceFileMgr m_sourceFileMgr = new MDbgSourceFileMgr();
        private Hashtable m_properties = new Hashtable();
    }


    //////////////////////////////////////////////////////////////////////////////////
    //
    // MDbgOutputImpl
    //
    //////////////////////////////////////////////////////////////////////////////////

    public class MDbgIO : IMDbgIO, IMDbgIO2, IMDbgIO3
    {

        public MDbgIO(MDbgShell shell, string[] startupCommands)
        {
            Debug.Assert(shell != null);
            m_shell = shell;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(this.ConsoleBreakHandler);

            if (startupCommands != null && startupCommands.Length > 0)
            {
                m_startupCommands = new Queue(startupCommands);
            }
        }

        public virtual void WriteOutput(string outputType, string text)
        {
            WriteOutput(outputType, text, 0, 0);
        }

        public virtual void WriteOutput(string outputType, string text, int hilightStart, int hilightLen)
        {
            Debug.Assert(null != outputType && null != text);

            if (outputType.Equals(MDbgOutputConstants.StdError))
            {
                string s = TEXT_ERROR + text;
                Display(s, HighlighType.Error, 0, s.Length);
            }
            else
            {
                Debug.Assert(hilightStart >= 0);
                Debug.Assert(hilightLen >= 0);
                if (hilightStart < 0)
                {
                    hilightStart = 0;
                }
                if (hilightLen < 0)
                {
                    hilightLen = 0;
                }
                Display(text, HighlighType.StatementLocation, hilightStart, hilightLen);
            }
        }

        public virtual bool ReadCommand(out string cmd)
        {
            if (m_startupCommands != null)
            {
                string c = (string)m_startupCommands.Dequeue();
                if (m_startupCommands.Count == 0)
                {
                    m_startupCommands = null;
                }
                cmd = c;
                WriteOutput(MDbgOutputConstants.StdOutput, c + "\r\n");
                return true;
            }

        retry:
            m_isConsoleBreakHandlerExecuted = false;
            WriteMdbgPrompt(true);
            cmd = Console.ReadLine();
            if (cmd == null)
            {
                Thread.Sleep(100);

                if (m_isConsoleBreakHandlerExecuted)
                    // means we have not hit an EOF.
                    goto retry;
            }

            return (cmd == null ? false : true);
        }

        public virtual ConsoleKeyInfo ReadKey(bool intercept)
        {
            return Console.ReadKey(intercept);
        }

        public virtual string ReadLine()
        {
            return Console.ReadLine();
        }

        //////////////////////////////////////////////////////////////////////////////////
        //
        // Private Implementation part
        //
        //////////////////////////////////////////////////////////////////////////////////

        private enum HighlighType
        {
            StatementLocation,
            Error,
            None
        }

        private void Display(string text, HighlighType ht, int highlightStart, int highlighLen)
        {
            if (highlighLen == 0 || highlightStart >= text.Length)
            {
                Console.Write(text);
            }
            else
            {
                Console.Write(text.Substring(0, highlightStart));
                ConsoleColor fc = Console.ForegroundColor;
                ConsoleColor bc = Console.BackgroundColor;

                switch (ht)
                {
                    case HighlighType.StatementLocation:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case HighlighType.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    default:
                        Debug.Assert(false);
                        throw new InvalidOperationException();
                }
                int l = highlightStart + highlighLen;
                if (l > text.Length)
                {
                    highlighLen = text.Length - highlightStart;
                }
                Console.Write(text.Substring(highlightStart, highlighLen));
                Console.ForegroundColor = fc;
                Console.BackgroundColor = bc;
                if (highlightStart + highlighLen < text.Length)
                {
                    Console.Write(text.Substring(highlightStart + highlighLen));
                }
            }
        }

        protected bool m_isConsoleBreakHandlerExecuted = false;

        private void ConsoleBreakHandler(Object sender, ConsoleCancelEventArgs e)
        {
            // When Control+C is pressed, the Console.Readline() returns immediatelly with
            // null. There is no way how to distinguish between EOF and Ctrl+C. Therefore we set
            // a m_isConsoleBreakHandlerExecuted to true whenever the handler is executed.
            // The code around Console.ReadLine() check when null is returned if the
            // m_isConsoleBreakHandlerExecuted flag is set. If yes, we know that this is caused by
            // Ctrl+C, the code clrears the flag and repeats the read.

            m_isConsoleBreakHandlerExecuted = true;

            switch (e.SpecialKey)
            {
                case ConsoleSpecialKey.ControlBreak:
                    Console.WriteLine();
                    WriteOutput(MDbgOutputConstants.StdError, "Immediate debugger termination reqested through <Ctrl+Break>");
                    WriteOutput(MDbgOutputConstants.StdError, "To break into debugger use Ctrl+C instead.");
                    //
                    // When ControlBreak is pressed, we cannot set e.Cancel=true....
                    // 
                    break;
                case ConsoleSpecialKey.ControlC:
                    try
                    {
                        WriteOutput(MDbgOutputConstants.StdOutput, "\n<Ctrl+C>");
                        MDbgProcess p = m_shell.Debugger.Processes.Active;
                        if (p.IsRunning)
                        {
                            p.AsyncStop().WaitOne();
                        }
                    }
                    catch
                    {
                    }
                    e.Cancel = true;
                    break;
                default:
                    break;
            }
        }

        private void WriteMdbgPrompt(bool processStopped)
        {
            string s;
            if (processStopped)
            {
                MDbgProcess p = null;
                if (m_shell.Debugger.Processes.HaveActive)
                {
                    p = m_shell.Debugger.Processes.Active;
                    if (p.Threads.HaveActive)
                    {
                        s = "[p#:" + p.Number + ", t#:" + p.Threads.Active.Number + "] ";
                    }
                    else
                    {
                        s = "[p#:" + p.Number + ", t#:no active thread] ";
                    }
                }
                else
                {
                    s = "";
                }
            }
            else
            {
                s = "[process running] ";
            }
            Console.Write(s + TEXT_PROMPT_STRING);
        }


        private const string TEXT_PROMPT_STRING = "mdbg> ";
        private const string TEXT_ERROR = "Error: ";
        private MDbgShell m_shell;
        protected Queue m_startupCommands = null;
    }

    //////////////////////////////////////////////////////////////////////////////////
    //
    // MDbgCommandSetImpl
    //
    //////////////////////////////////////////////////////////////////////////////////

    public class MDbgCommandSetCollection : IMDbgCommandCollection
    {
        public MDbgCommandSetCollection()
        {
        }

        public void Add(IMDbgCommand command)
        {
            if (command == null)
            {
                Debug.Assert(false);
                throw new Exception();
            }

            if (command.MinimumAbbrev > command.CommandName.Length)
            {
                throw new MDbgShellException("Cannot add command '" + command.CommandName + "'. Abbreviation is " +
                    command.MinimumAbbrev + " characters. Can't be more than " + command.CommandName.Length);
            }

            List<IMDbgCommand> cmdList;
            if (!m_commands.TryGetValue(command.CommandName, out cmdList))
            {
                // The command doesn't have an entry in the command list, we must create a new one.
                cmdList = new List<IMDbgCommand>();
                cmdList.Add(command);   // create a list for this command (this list allows overriding commands)
                m_commands.Add(command.CommandName, cmdList);
            }
            else
            {
                // We are reloading an extension, so remove the previous command and add it to the end of
                // the list.
                Debug.Assert(!cmdList.Contains(command), "Previous command was not unloaded properly");
                cmdList.Add(command);
                m_commands[command.CommandName] = cmdList;  // update
            }
        }

        public void Remove(IMDbgCommand command)
        {
            if (command == null)
            {
                Debug.Assert(false);
                throw new Exception();
            }

            if (command.MinimumAbbrev > command.CommandName.Length)
            {
                throw new MDbgShellException("Cannot remove command '" + command.CommandName + "'. Abbreviation is " +
                    command.MinimumAbbrev + " characters. Can't be more than " + command.CommandName.Length);
            }

            // Extensions are allowed to override a command, but we want to make sure that we retain all original commands
            // We don't delete any previously defined commands
            List<IMDbgCommand> cmdList;
            if (!m_commands.TryGetValue(command.CommandName, out cmdList))
            {
                // We shouldn't ever try to remove a command that hasn't been added
                Debug.Assert(false, "Attempted to remove a command that does not exist");
            }
            else
            {
                // We are reloading an extension, so remove the previous command and add it to the end of
                // the list.
                cmdList.Remove(command);
            }
        }

        public IMDbgCommand Lookup(string commandName)
        {
            ArrayList al = new ArrayList();

            foreach (List<IMDbgCommand> cmdList in m_commands.Values)
            {
                if (cmdList.Count < 1)
                {
                    // We may have unloaded an extension which can create empty entries in m_commands.  Continue
                    // to list valid commands.
                    continue;
                }
                // The last command in the list points to the last command that was added and ALWAYS points
                // to the command that we should execute
                IMDbgCommand cmd = cmdList[cmdList.Count - 1];

                if (commandName.Length < cmd.MinimumAbbrev)
                {
                    continue;
                }

                if (String.Compare(cmd.CommandName, 0, commandName, 0, commandName.Length, true,
                                  CultureInfo.CurrentUICulture // command names could get localized in the future
                                  ) == 0)
                {
                    if (cmd.CommandName.Length == commandName.Length)
                    {
                        // if we have a perfect match for the command, then we return the command without
                        // taking into account any other partial command completitions.
                        return cmd;
                    }
                    al.Add(cmd);
                }
            }


            if (al.Count == 0)
            {
                throw new MDbgShellException("Command '" + commandName + "' not found.");
            }
            else if (al.Count == 1)
            {
                return (IMDbgCommand)al[0];
            }
            else
            {
                StringBuilder s = new StringBuilder("Command prefix too short. \nPossible completitions:");
                foreach (IMDbgCommand c in al)
                {
                    s.Append("\n").Append(c.CommandName);
                }
                throw new MDbgShellException(s.ToString());
            }
        }

        public IEnumerator GetEnumerator()
        {
            foreach (List<IMDbgCommand> cmdList in m_commands.Values)
            {
                if (cmdList.Count < 1)
                {
                    // If an extension is unloaded, it causes some command entries to contain no valid IMDbgCommands.
                    // This is expected, simply continue iterating the commands.
                    continue;
                }

                // The last command in the list points to the last command that was added and ALWAYS points
                // to the command that we should execute
                yield return cmdList[cmdList.Count - 1];
            }
        }

        public void ParseCommand(string commandLineText, out IMDbgCommand command, out string commandArguments)
        {
            commandLineText = commandLineText.Trim();
            int n = commandLineText.Length;
            int i = 0;
            while (i < n && !Char.IsWhiteSpace(commandLineText, i))
            {
                i++;
            }
            string cmdName = commandLineText.Substring(0, i);
            commandArguments = commandLineText.Substring(i).Trim();
            command = Lookup(cmdName);
        }

        private Dictionary<string, List<IMDbgCommand>> m_commands = new Dictionary<string, List<IMDbgCommand>>();
    }


    //////////////////////////////////////////////////////////////////////////////////
    //
    // MDbgFileLocatorImpl
    //
    //////////////////////////////////////////////////////////////////////////////////

    public class MDbgFileLocator : IMDbgFileLocator
    {
        internal MDbgFileLocator()
        {
        }

        public string Path
        {
            get
            {
                return m_srcPath;
            }
            set
            {
                foreach (string pathPart in GetPathComponents(value))
                {
                    if (!Directory.Exists(pathPart))
                    {
                        throw new MDbgShellException("path doesn't exist: '" + pathPart + "'");
                    }
                }
                m_srcPath = value;
                m_fileLocations.Clear();
            }
        }

        public string GetFileLocation(string file)
        {
            string idx = String.Intern(file);
            if (m_fileLocations.Contains(idx))
            {
                return (string)m_fileLocations[idx];
            }

            string realPath = null;

            if (File.Exists(file))
            {
                realPath = file;
            }
            else
            {
                foreach (string p in GetPathComponents(m_srcPath))
                {
                    string filePath = file;
                    do
                    {
                        realPath = System.IO.Path.Combine(p, filePath);
                        if (File.Exists(realPath))
                            goto Found;

                        int i = filePath.IndexOfAny(new char[]{
                            System.IO.Path.DirectorySeparatorChar,System.IO.Path.AltDirectorySeparatorChar});
                        if (i != -1)
                            filePath = filePath.Substring(i + 1);
                        else
                            break;
                    }
                    while (true);
                }
                realPath = null;
            }
        Found:
            m_fileLocations.Add(idx, realPath);
            return realPath;
        }

        public void Associate(string originalName, string newName)
        {
            Debug.Assert(originalName != null && originalName.Length > 0);
            Debug.Assert(newName != null && newName.Length > 0);

            string idx = String.Intern(originalName);
            if (m_fileLocations.Contains(idx))
                m_fileLocations.Remove(idx);
            m_fileLocations.Add(idx, newName);
        }

        protected string[] GetPathComponents(string path)
        {
            if (path == null)
            {
                return new string[0];
            }
            string[] strs = path.Split(new char[] { ';' });
            return strs;
        }

        private Hashtable m_fileLocations = new Hashtable();
        private string m_srcPath;
    }
}
