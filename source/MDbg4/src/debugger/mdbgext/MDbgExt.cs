//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.MdbgEngine;

namespace Microsoft.Samples.Tools.Mdbg
{
    /// <summary>
    /// This class contains information about an executed command.
    /// </summary>
    public class CommandExecutedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <b>CommandExecutedEventArgs</b> class.
        /// </summary>
        /// <param name="sender">The sender for this event</param>
        /// <param name="command">The command that is getting executed</param>
        /// <param name="arguments">The arguments for the command</param>
        /// <param name="movementCommand">Is this a movement command.</param>
        public CommandExecutedEventArgs(IMDbgShell sender, IMDbgCommand command, string arguments,
                                    bool movementCommand)
        {
            Debug.Assert(sender != null);
            Debug.Assert((command == null) == (arguments == null));

            m_sender = sender;
            m_command = command;
            m_arguments = arguments;
            m_movementCommand = movementCommand;
        }

        /// <summary>
        /// The sender for this event
        /// </summary>
        /// <value>The sender for this event</value>
        public IMDbgShell Sender
        {
            get
            {
                return m_sender;
            }
        }

        /// <summary>
        /// The command that is getting executed.
        /// </summary>
        /// <value>The command that is getting executed.</value>
        public IMDbgCommand Command
        {
            get
            {
                return m_command;
            }
        }

        /// <summary>
        /// The arguments for the command
        /// </summary>
        /// <value>The arguments for the command</value>
        public string Arguments
        {
            get
            {
                return m_arguments;
            }
        }

        /// <summary>
        /// Is this a movement command.
        /// </summary>
        /// <value>Is this a movement command.</value>
        public bool MovementCommand
        {
            get
            {
                return m_movementCommand;
            }
        }

        private IMDbgShell m_sender;
        private IMDbgCommand m_command;
        private string m_arguments;
        private bool m_movementCommand;
    }

    /// <summary>
    /// Allows you to register for the CommandExecuted Event.
    /// </summary>
    /// <param name="sender">Object sending the event.</param>
    /// <param name="e">CommandExecutedEventArgs for the event.</param>
    public delegate void CommandExecutedEventHandler(Object sender, CommandExecutedEventArgs e);


    /// <summary>
    /// Pluggable parser object to parse breakpoint location strings.
    /// </summary>
    /// <remarks>Breakpoint location object. Returns null if the parser doesn't recognize the format. 
    /// This allows parsers to be chained together. If a parser does recognize the format, then it can do more
    /// specific argument checking and throw an exception. </remarks>
    public interface IBreakpointParser
    {
        /// <summary>
        /// Parser string describing IL-level breakpoint.
        /// </summary>
        /// <param name="args">string argument representing breakpoint location syntax to parse</param>
        /// <returns>Breakpoint location object. Returns null if the parser doesn't recognize the format. 
        /// This allows parsers to be chained together. If a parser does recognize the format, then it can do more
        /// specific argument checking and throw an exception. 
        /// </returns>
        ISequencePointResolver ParseFunctionBreakpoint(string args);
    }

    /// <summary> Pluggable parser object to parse an expression.
    /// </summary>
    public interface IExpressionParser
    {
        /// <summary> Parse a string into an expression.
        /// </summary>
        /// <param name="variableName">A name of variable to look-up.</param>
        /// <param name="process">A debugged process.</param>
        /// <param name="scope">A resolution scope.</param>
        /// <returns>MDbgValue class representing a resulting value.</returns>
        MDbgValue ParseExpression(string variableName, MDbgProcess process, MDbgFrame scope);

        /// <summary>
        /// Parse a string into a CorValue.
        /// </summary>
        /// <param name="value">An expression to parse.</param>
        /// <param name="process">A debugged process.</param>
        /// <param name="scope">A resolution scope.</param>
        /// <returns>CorValue class representing the value passed in.</returns>
        CorValue ParseExpression2(string value, MDbgProcess process, MDbgFrame scope);
    }

    //////////////////////////////////////////////////////////////////////////////////
    //
    // IMDbgShell Interface for extensions
    //
    //////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Interface for the mdbg shell
    /// </summary>
    public interface IMDbgShell
    {
        /// <summary>
        /// Gets or sets with MDbgEngine should be used for all other actions.
        /// </summary>
        /// <value>The MDbgEngine.</value>
        MDbgEngine Debugger
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the available commands.
        /// </summary>
        /// <value>The commands.</value>
        IMDbgCommandCollection Commands
        {
            get;
        }

        /// <summary>
        /// Gets or sets the IO interface.
        /// </summary>
        /// <value>The implementer of IMDbgIO.</value>
        IMDbgIO IO
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the File Locator.
        /// </summary>
        /// <value>The File Locator.</value>
        IMDbgFileLocator FileLocator
        {
            get;
        }

        /// <summary>
        /// Gets the Source File Manager
        /// </summary>
        /// <value>The Source File Manager</value>
        IMDbgSourceFileMgr SourceFileMgr
        {
            get;
        }

        /// <summary>
        /// Gets properties for the shell.
        /// </summary>
        IDictionary Properties
        {
            get;
        }

        /// <summary>
        /// Quits with a given exit code.
        /// </summary>
        /// <param name="exitCode">The exitcode to exit with.</param>
        void QuitWithExitCode(int exitCode);

        /// <summary>
        /// Called when command is executed and prompt is going to be displayed.
        /// </summary>
        event CommandExecutedEventHandler OnCommandExecuted;

        /// <summary>
        /// Displays current location.
        /// </summary>
        void DisplayCurrentLocation();


        /// <summary>
        /// Shell displays to the user a reason for the failure.
        /// </summary>
        void ReportException(Exception e);


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
        IBreakpointParser BreakpointParser
        {
            get;
            set;
        }

        /// <summary>
        /// Get the default expression parser for this shell.
        /// </summary>
        /// <remarks>
        ///   The expressino parser is responsible for parsing variable names and
        ///   simple expressions into MDbgValue objects that represent a result of
        ///   parsing.
        /// </remarks>
        IExpressionParser ExpressionParser
        {
            get;
            set;
        }

    }

    //////////////////////////////////////////////////////////////////////////////////
    //
    // IO Output Subsystem 
    //
    //////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// This class contains special output constant names for output of different types.
    /// </summary>
    public sealed class MDbgOutputConstants
    {
        /// <summary>
        /// Output goes to Standard Out
        /// </summary>
        public const string StdOutput = "STDOUT";
        /// <summary>
        /// Output goes to Standard Error
        /// </summary>
        public const string StdError = "STDERR";
        /// <summary>
        /// This is special output type.  It will be written to StdOut, but its content should be ignored (non-important output).
        /// </summary>
        public const string Ignore = "IGNORE";
    }

    /// <summary>
    /// Interface for mdbg Input/Output routines
    /// </summary>
    public interface IMDbgIO
    {
        /// <summary>
        /// Writes the specified string value to the output stream for the given output type.
        /// </summary>
        /// <param name="outputType">Specifies which MDbgOutputConstants output type to write to</param>
        /// <param name="output">The value to write.</param>
        /// <remarks>This does not include a newline. That is a breaking change
        /// from previous releases of MDbg</remarks>
        void WriteOutput(string outputType, string output);

        /// <summary>
        /// Prints the "mdbg>" prompt and reads the next line of input.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        bool ReadCommand(out string command);
    }

    /// <summary>
    /// Extension of the basic IMDbgIO functionality
    /// Users who want to take advantage of IMDBgIO2 functionality needs to cast IMDbgIO to IMDbgIO2
    /// </summary>
    public interface IMDbgIO2
    {
        /// <summary>
        /// Much like IMDbgIO:WriteOutput but allows for highlighting of parts of the string
        /// </summary>
        /// <param name="outputType">Specifies which MDbgOutputConstants output type to write to</param>
        /// <param name="message">The value to write.</param>
        /// <param name="highlightStart">The index to begin highlighting.</param>
        /// <param name="highlightLen">How many characters to highlight.</param>
        /// <remarks>This does not include a newline. That is a breaking change
        /// from previous releases of MDbg</remarks>
        void WriteOutput(string outputType, string message, int highlightStart, int highlightLen);
    }

    /// <summary>
    /// Another extension of the basic IMDbgIO functionality.
    /// Users who want to take advantage of IMDBgIO3 functionality needs to cast IMDbgIO to IMDbgIO3
    /// </summary>
    public interface IMDbgIO3
    {
        /// <summary>
        /// Returns the next character or function key pressed by the user.
        /// </summary>
        /// <param name="intercept">If intercept is true, the pressed key is displayed in the console window.
        /// If false, the key is not displayed.</param>
        /// <returns>The next character or function key pressed by the user.</returns>
        ConsoleKeyInfo ReadKey(bool intercept);

        /// <summary>
        /// Returns the next line of input without printing the "mdbg>" prompt.
        /// </summary>
        /// <returns>The next line of input.</returns>
        string ReadLine();
    }

    //////////////////////////////////////////////////////////////////////////////////
    //
    // IMDbgCommand and MDbgCommandCollection 
    //
    //////////////////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Interface for mdbg commands.
    /// </summary>
    public interface IMDbgCommand : IComparable, IEquatable<IMDbgCommand>
    {
        /// <summary>
        /// Returns the command name.
        /// </summary>
        /// <value>Name of the command.</value>
        string CommandName
        {
            get;
        }
        /// <summary>
        /// Returns the minimum number of characters you must use to invoke this command.
        /// </summary>
        /// <value>The minimum number of characters.</value>
        int MinimumAbbrev
        {
            get;
        }
        /// <summary>
        /// Returns if the command is repeatable (hitting enter again will repeat these commands)
        /// </summary>
        /// <value>true if the command is repeatable</value>
        bool IsRepeatable
        {
            get;
        }
        /// <summary>
        /// Returns a brief help message for the command.
        /// </summary>
        /// <value>The help message.</value>
        string ShortHelp
        {
            get;
        }
        /// <summary>
        /// Returns a more detailed help message for the command.
        /// </summary>
        /// <value>The help message.</value>
        string LongHelp
        {
            get;
        }
        /// <summary>
        /// Assembly the command was loaded from
        /// </summary>
        /// <value>The Assembly.</value>
        Assembly LoadedFrom
        {
            get;
        }
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="args">Arguments to pass to the command.</param>
        void Execute(string args);
    }

    /// <summary>
    /// Interface for mdbg command collections.
    /// </summary>
    public interface IMDbgCommandCollection : IEnumerable
    {
        /// <summary>
        /// Adds a command to the collection.
        /// </summary>
        /// <param name="command">Command to add.</param>
        void Add(IMDbgCommand command);

        /// <summary>
        /// Removes a command from the collection.
        /// </summary>
        /// <param name="command">Command to remove.</param>
        void Remove(IMDbgCommand command);

        /// <summary>
        /// Looks up a command in the collection.
        /// </summary>
        /// <param name="cmd">The name of the command to look up.</param>
        /// <returns>The command corresponding to the given name.</returns>
        IMDbgCommand Lookup(string cmd);

        /// <summary>
        /// Parses a command.
        /// </summary>
        /// <param name="fullText">Raw text for the command and arguments all together.</param>
        /// <param name="command">Returns the command from the given text.</param>
        /// <param name="commandArguments">Returns the arguments from the given text.</param>
        void ParseCommand(string fullText, out IMDbgCommand command, out string commandArguments);
    }

    //////////////////////////////////////////////////////////////////////////////////
    //
    // IMDbgFileLocator
    //
    //////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Interface for File Location
    /// </summary>
    public interface IMDbgFileLocator
    {
        /// <summary>
        /// Gets or Sets the path. Setting a path will clear any associations.
        /// </summary>
        /// <value>the path</value>
        string Path
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a file location
        /// </summary>
        /// <param name="file">the file</param>
        /// <returns></returns>
        string GetFileLocation(string file);

        /// <summary>
        /// Forces debugger to use different source files for displaying of sources.
        /// </summary>
        /// <param name="originalName">An original name that is stored in .pdb of debugged program.</param>
        /// <param name="newName">A new name that should be used as source file name instead.</param>
        void Associate(string originalName, string newName);
    }

    //////////////////////////////////////////////////////////////////////////////////
    //
    // Exceptions
    //
    //////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This is an exception for when mdbg shell needs its own special type
    /// </summary>
    [Serializable()]
    public class MDbgShellException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the MDbgShellException class.
        /// </summary>
        public MDbgShellException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgShellException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MDbgShellException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgShellException class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The inner exception for the new exception</param>
        public MDbgShellException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgShellException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected MDbgShellException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }

    /// <summary>
    /// A base abstract class for commands.
    /// </summary>
    public abstract class CommandBase
    {
        /// <summary>
        /// Gets or sets the IMdbgShell.
        /// </summary>
        /// <value>The IMdbgShell.</value>
        public static IMDbgShell Shell
        {
            get
            {
                return m_shell;
            }
            set
            {
                m_shell = value;
            }
        }

        /// <summary>
        /// Gets the Debugger.
        /// </summary>
        /// <value>The Debugger.</value>
        public static MDbgEngine Debugger
        {
            get
            {
                return Shell.Debugger;
            }
        }

        /// <summary>
        /// Executes the given command.
        /// </summary>
        /// <param name="commandText">What command to execute.</param>
        public static void ExecuteCommand(string commandText)
        {
            IMDbgCommand cmd;
            string cmdArgs;
            Shell.Commands.ParseCommand(commandText, out cmd, out cmdArgs);
            cmd.Execute(cmdArgs);
        }

        /// <summary>
        /// Much like IMDbgIO:WriteOutput but allows for highlighting of parts of the string.
        /// Prints a new line after message.
        /// </summary>
        /// <param name="outputType">Specifies which MDbgOutputConstants output type to write to</param>
        /// <param name="message">The value to write.</param>
        /// <param name="highlightStart">The index to begin highlighting.</param>
        /// <param name="highlightLen">How many characters to highlight.</param>
        public static void WriteOutput(string outputType, string message, int highlightStart, int highlightLen)
        {
            Write(outputType, message + "\n", highlightStart, highlightLen);
        }

        /// <summary>
        /// Much like IMDbgIO:WriteOutput but allows for highlighting of parts of the string.
        /// Does not print a new line after message.
        /// </summary>
        /// <param name="outputType">Specifies which MDbgOutputConstants output type to write to</param>
        /// <param name="message">The value to write.</param>
        /// <param name="highlightStart">The index to begin highlighting.</param>
        /// <param name="highlightLen">How many characters to highlight.</param>
        public static void Write(string outputType, string message, int highlightStart, int highlightLen)
        {
            if (Shell.IO is IMDbgIO2)
            {
                (Shell.IO as IMDbgIO2).WriteOutput(outputType, message, highlightStart, highlightLen);
            }
            else
            {
                WriteOutput(outputType, message);                // we don't have support for hi-ligting
            }
        }

        /// <summary>
        /// Writes output of a given type, followed by a new line.
        /// </summary>
        /// <param name="outputType">What MDbgOutputConstants type the output is</param>
        /// <param name="message">What text to output</param>
        public static void WriteOutput(string outputType, string message)
        {
            Shell.IO.WriteOutput(outputType, message + "\n");
        }

        /// <summary>
        /// Writes output of a given type. Does not print a new line after output.
        /// </summary>
        /// <param name="outputType">What MDbgOutputConstants type the output is</param>
        /// <param name="message">What text to output</param>
        public static void Write(string outputType, string message)
        {
            Shell.IO.WriteOutput(outputType, message);
        }

        /// <summary>
        /// Writes output using STDOUT output type, followed by a new line.
        /// </summary>
        /// <param name="message">What text to output</param>
        public static void WriteOutput(string message)
        {
            Shell.IO.WriteOutput(MDbgOutputConstants.StdOutput, message + "\n");
        }

        /// <summary>
        /// Writes output using STDOUT output type. Does not print a new line after output.
        /// </summary>
        /// <param name="message">What text to output</param>
        public static void Write(string message)
        {
            Shell.IO.WriteOutput(MDbgOutputConstants.StdOutput, message);
        }

        /// <summary>
        /// Writes output using STDERR output type
        /// </summary>
        /// <param name="message">What text to output</param>
        public static void WriteError(string message)
        {
            Shell.IO.WriteOutput(MDbgOutputConstants.StdError, message + "\n");
        }

        // AssertOnErrors flag is used by mode ae command
        // to Enable/Disable stopping at the point of failure.
        //
        // The flag is defined here because it is used also by
        // recorder extension to stop at the point of failure during
        // replay of recorded scripts.
        /// <summary>
        /// Internal Only.  Do not use.
        /// </summary>
        public static bool AssertOnErrors = false;

        // ShowFullExceptionInfo flag is used to toggle the
        // reporting of detailed exception information for all
        // exceptions which escape the method implementing a given
        // command. Typically this is off because the message alone
        // conveys the necessary user information, however for diagnostic
        // purposes this can be very useful
        public static bool ShowFullExceptionInfo = false;

        /// <summary>
        /// ShowInternalFrames is used by mode if command to
        /// Enable/Disable showing of internal frames in where
        /// command. This flag is defined here and not in the Engine.cs
        /// (engine layer) because this flag only affects whether the
        /// frames are visible in "where" command. However they are
        /// always exposed by the engine.
        /// </summary>
        public static bool ShowInternalFrames = true;

        /// <summary>
        /// Gets or sets if Mdbg should fail on errors.
        /// </summary>
        /// <value>Default is false.</value>
        public static bool FailOnError = false;

        /// <summary>
        /// ExtensionPath variable contains a set of paths that are searched for extensions when
        /// "load" command is used to load an extension.
        /// </summary>
        /// <remarks>
        /// The paths are separated with the PathSeparator char.
        /// This defaults to the 1) current directory and 2) the directory that the debugger 
        /// was loaded from.
        /// </remarks>
        public static string ExtensionPath = "." + System.IO.Path.PathSeparator + AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Parses arguments for the run command, but does not provide direct access to the path to the
        /// program to run.
        /// </summary>
        /// <param name="arguments">Argument to the run and crun command</param>
        /// <param name="debugMode">Returns desired debugging mode. Returns internal
        /// default if not specified
        /// </param>
        /// <param name="debuggeeVersion">Returns the debugger interface version.
        ///    Is null if the version cannot be determined.
        /// </param>
        /// <param name="programAndArgsToRun">Returns the command line for the program
        /// being run.  Note that the program name may be quoted and may not contain
        /// a ".exe" extension.  Rather than try to parse this, if you need the path to the binary
        /// being run, then use the 5 argument overload below.</param>
        static public void PreParseRunArguments(string arguments, out DebugModeFlag debugMode,
                                                out string debuggeeVersion,
                                                out string programAndArgsToRun)
        {
            string programToRun;
            string programArguments;
            PreParseRunArguments(arguments, out debugMode, out debuggeeVersion, out programToRun, out programArguments);

            // the binary name is included in the program arguments (as the 0th argument), so just use that
            if (programArguments == null)
                programAndArgsToRun = String.Empty;
            else
                programAndArgsToRun = programArguments;
        }

        /// <summary>
        /// Helper function that parses inputs to run and crun commands.
        /// </summary>
        /// <param name="arguments">Argument to the run and crun command</param>
        /// <param name="debugMode">Returns desired debugging mode. Returns internal
        /// default if not specified
        /// </param>
        /// <param name="debuggeeVersion">Returns the debugger interface version.
        ///    Is null if the version cannot be determined.
        /// </param>
        /// <param name="programToRun">Returns path of the program to execute.</param>
        /// <param name="programArguments">Returns arguments to pass to the program being 
        /// run, including the binary name as the 0th argument</param>
        /// <seealso cref="RunOptions"/>
        static public void PreParseRunArguments(string arguments, out DebugModeFlag debugMode,
                                        out string debuggeeVersion,
                                        out string programToRun,
                                        out string programArguments)
        {
            // This is now just a wrapper around RunOptions.
            RunOptions options = new RunOptions(arguments);
            debugMode = options.DebugMode;
            debuggeeVersion = options.Version;
            programToRun = options.Application;
            programArguments = options.Arguments;
        }


        static internal bool FindToken(string text, ref int start, ref int end)
        {
            Debug.Assert(text != null);
            Debug.Assert(end <= text.Length && end >= 0);
            start = end;

            // move a start to a non-space token
            while (start < text.Length && char.IsWhiteSpace(text[start]))
                ++start;
            end = start;

            bool isEscape = false;
            bool isInQuote = false;
            while (end < text.Length && (isEscape || isInQuote || !char.IsWhiteSpace(text[end])))
            {
                if (!isEscape)
                {
                    if (text[end].Equals('"'))
                        isInQuote = !isInQuote;
                    if (text[end].Equals('\\'))
                        isEscape = true;
                }
                else
                    isEscape = false;
                ++end;
            }
            return start != end;
        }

        private static IMDbgShell m_shell;
    }

    /// <summary>
    /// Identify the CLR implementation model which will ultimately be used to get the factory to instantiate
    /// an ICorDebugProcess.
    /// </summary>
    public enum ClrImplementationModel
    {
        /// <summary>
        /// Use Whidbey creation model via mscoree!CreateDebuggingInterfaceFromVersion
        /// </summary>
        Whidbey,
    }

    /// <summary>
    /// Controls the level of informational output in the standard "run" command
    /// </summary>
    public enum RunVerbosity
    {
        Low = 0,
        Normal = 1,
        High = 2
    }

    /// <summary>
    /// Options for a standard "run" command. This supercedes the PreParseRunArguments functions.
    /// </summary>
    public class RunOptions
    {
        DebugModeFlag m_debugMode;

        /// <summary>
        /// Debug mode (optimized, enc, etc).  Pass this to MDbgProcess.DebugMode.
        /// </summary>
        public DebugModeFlag DebugMode
        {
            get { return m_debugMode; }
        }

        string m_application;

        /// <summary>
        /// Path to application executable to startup. Non-null.
        /// </summary>
        public string Application
        {
            get { return m_application; }
        }

        string m_arguments;

        /// <summary>
        /// Arguments to pass to debuggee. May be null. This may honor the convention of including the application name as argument 0.
        /// </summary>
        public string Arguments
        {
            get { return m_arguments; }
        }

        string m_debuggeeVersion;

        /// <summary>
        /// Debuggee version string. May be null. This may default to a version infered from the Application path.
        /// </summary>
        public string Version
        {
            get { return m_debuggeeVersion; }
        }

        RunVerbosity m_verbosity;

        public RunVerbosity Verbosity
        {
            get { return m_verbosity; }
        }
        ClrImplementationModel m_model = ClrImplementationModel.Whidbey;
        
        /// <summary>
        /// Model used to get the factory for creating an ICorDebugProcess.
        /// </summary>
        public ClrImplementationModel Model
        {
            get { return m_model; }
        }



        /// <summary>
        /// Parse from a command string 
        /// </summary>
        /// <param name="arguments">arguments string passed to a 'run' command. This gets parsed to fill out
        /// the run options. </param>
        public RunOptions(string arguments)
        {
            Debug.Assert(arguments != null);
            if (arguments == null)
                throw new ArgumentException("Parameter cannot be null.", "arguments");

            this.m_debugMode = DebugModeFlag.Debug;
            m_verbosity = RunVerbosity.Normal;
            // Is this the mode we want to always start in?

            int start, end;
            start = end = 0;

            // those flags are necessary so that people can run any program under debugger.
            // e.g to run program named '-debug.exe', they can write 'run -debug -debug'
            //
            // One obvious issue here is that if the binary name is same as the flag, then
            // user need to pass the flag to the run command so that the program name is not
            // interpreted as a flag.
            //

            bool debugModeFlagSet = false;
            bool versionFlagSet = false;
            bool verboseSet = false;

            while (CommandBase.FindToken(arguments, ref start, ref end))
            {
                Debug.Assert(start <= end);
                switch (arguments.Substring(start, end - start))
                {
                    case "-default":
                        if (debugModeFlagSet)
                            goto default;
                        this.m_debugMode = DebugModeFlag.Default;
                        debugModeFlagSet = true;
                        break;

                    case "-debug":
                    case "-d":
                        if (debugModeFlagSet)
                            goto default;
                        this.m_debugMode = DebugModeFlag.Debug;
                        debugModeFlagSet = true;
                        break;

                    case "-optimize":
                    case "-o":
                        if (debugModeFlagSet)
                            goto default;
                        this.m_debugMode = DebugModeFlag.Optimized;
                        debugModeFlagSet = true;
                        break;

                    case "-enc":
                        if (debugModeFlagSet)
                            goto default;
                        this.m_debugMode = DebugModeFlag.Enc;
                        debugModeFlagSet = true;
                        break;

                    case "-ver":
                        if (versionFlagSet)
                            goto default;

                        if (!CommandBase.FindToken(arguments, ref start, ref end))
                        {
                            throw new MDbgShellException("missing argument to -ver option");
                        }
                        this.m_debuggeeVersion = arguments.Substring(start, end - start);
                        versionFlagSet = true;
                        break;

                    case "-verbose":
                        if (verboseSet)
                            goto default;

                        if (!CommandBase.FindToken(arguments, ref start, ref end))
                        {
                            throw new MDbgShellException("missing argument to -verbose option");
                        }
                        try
                        {
                            m_verbosity = (RunVerbosity)Enum.Parse(typeof(RunVerbosity), arguments.Substring(start, end - start), true);
                        }
                        catch (ArgumentException)
                        {
                            throw new MDbgShellException("Invalid argument to -verbose option. Use Low, Normal or High.");
                        }
                        verboseSet = true;
                        break;

                    default:
                        // this means we have found some different token.
                        // To keep comaptibility with cordbg, we assume that this is a program binary name.

                        // Expand environment variables on the binary name so that we can use env vars in
                        // the program path. Note that environment variables are not expanded in program
                        // arguments so that we can pass in original strings. Programs can expand the environment
                        // themselves if they wish so. 
                        string prog = Environment.ExpandEnvironmentVariables(arguments.Substring(start, end - start));

                        // program may be quoted, remove surrounding quotes
                        // Note: ideally we'd have a general escapedPath format and escape/unescape methods to convert
                        // but this is good enough for our purposes here.
                        if (prog.Length >= 2 && prog.StartsWith("\"") && prog.EndsWith("\""))
                        {
                            prog = prog.Substring(1, prog.Length - 2);
                        }

                        // may have omitted .exe extension, add it if necessary
                        const string optExt = ".exe";
                        if (!prog.EndsWith(optExt, StringComparison.OrdinalIgnoreCase))
                        {
                            prog += optExt;
                        }

                        // We're left with the program binary path
                        this.m_application = prog;

                        // Use everything unmodified (including the possibly quoted binary path) as the run arguments
                        // This will include the application name as arg[0], which is the standard convention.
                        this.m_arguments = arguments.Substring(start);

                        // Rest of line processed, we're done.
                        return;
                }
            }

            // we didn't find any token -- no other binary specified than options            
            // Leaves properties as null.
            Debug.Assert(this.m_application == null);
            Debug.Assert(this.m_arguments == null);

            return;
        }
    } // end class RunOptions


    //////////////////////////////////////////////////////////////////////////////////
    //
    // MDbgSourceFile display support
    //
    //////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Interface for mdbg Source File Management
    /// </summary>
    public interface IMDbgSourceFileMgr
    {
        /// <summary>
        /// Clears the document cache.
        /// </summary>
        void ClearDocumentCache();
        /// <summary>
        /// Gets a source file.
        /// </summary>
        /// <param name="path">Where to get the source file from.</param>
        /// <returns>An IMDbgSourceFile from the given url.</returns>
        IMDbgSourceFile GetSourceFile(string path);
    }

    /// <summary>
    /// Defines generalized functions relating to source files that a class may implement to create situation-specific methods.
    /// </summary>
    public interface IMDbgSourceFile
    {
        /// <summary>
        /// Where is the file.
        /// </summary>
        /// <value>Where is the file.</value>
        string Path
        {
            get;
        }
        /// <summary>
        /// Allows for indexing into the file by line number. Index is 1-based. Highest valud index is Count property.
        /// </summary>
        /// <param name="lineNo">Which line number to get.</param>
        /// <returns>The text in the file at the requested line number.</returns>
        string this[int lineNo]
        {
            get;
        }
        /// <summary>
        /// How many lines are in the file.
        /// </summary>
        /// <value>How many lines are in the file.</value>
        int Count
        {
            get;
        }
    }

    //////////////////////////////////////////////////////////////////////////////////
    //
    // Attribute that marks entry point to mdbg extension.
    //
    //////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    ///   Defines generalized functions relating to source files that a class may
    ///   implement to create situation-specific methods.
    /// </summary>
    [
     AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false,
        Inherited = false
        )
    ]
    public sealed class MDbgExtensionEntryPointClassAttribute : Attribute
    {
        /// <summary> URL to provde more detail about this extension. May be null if not supplied.
        /// </summary>
        public string Url
        {
            get { return m_url; }
            set { m_url = value; }
        }

        /// <summary> Email address to contract for more detail about this extension. May be null if not supplied.
        /// </summary>
        public string EMailAddress
        {
            get { return m_email; }
            set { m_email = value; }
        }

        /// <summary> Short description of functionality in this extension. May be null.
        /// </summary>        
        public string ShortDescription
        {
            get { return m_Descr; }
            set { m_Descr = value; }
        }

        private string m_url;
        private string m_email;
        private string m_Descr;
    }

    //////////////////////////////////////////////////////////////////////////////////
    //
    // Shell specific classes for integrating / extending basic commands and shell
    //
    //////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Class for storing and managing stop option policies for debugger callback events.
    /// </summary>
    public sealed class MDbgStopOptions
    {
        /// <value>
        ///   Key name that is used for storing this object into Shell.Properties.
        /// </value>
        public const string PropertyName = "MdbgStopOptions";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MDbgStopOptions()
        {
            m_stopOptions = new List<MDbgStopOptionPolicy>[Enum.GetValues(typeof(ManagedCallbackType)).Length];
        }

        /// <summary>
        /// Adds an MDbgStopOptionPolicy object to the collection of stop option policies,
        /// and registers it for a type of callback
        /// </summary>
        /// <param name="sop">The stop option policy to add.</param>
        /// <param name="callbackType">The type of callback to register the stop option
        /// policy for.</param>
        public void Add(MDbgStopOptionPolicy sop, ManagedCallbackType callbackType)
        {
            if (m_stopOptions[(int)callbackType] == null)
            {
                m_stopOptions[(int)callbackType] = new List<MDbgStopOptionPolicy>();
            }
            m_stopOptions[(int)callbackType].Add(sop);
        }

        /// <summary>
        /// Adds an MDbgStopOptionPolicy object to the collection of stop option policies,
        /// and registers it for one or more types of callbacks.
        /// </summary>
        /// <param name="sop">The stop option policy to add.</param>
        /// <param name="callbackTypes">The types of callbacks to register the stop option
        /// policy for.</param>
        public void Add(MDbgStopOptionPolicy sop, ManagedCallbackType[] callbackTypes)
        {
            foreach (ManagedCallbackType callbackType in callbackTypes)
            {
                Add(sop, callbackType);
            }
        }

        /// <summary>
        /// Removes an MDbgStopOptionPolicy object from the collection of stop option policies.
        /// </summary>
        /// <param name="sop">The stop option policy to remove.</param>
        public void Remove(MDbgStopOptionPolicy sop)
        {
            foreach (List<MDbgStopOptionPolicy> policies in m_stopOptions)
            {
                if (policies != null)
                {
                    if (policies.Contains(sop))
                    {
                        policies.Remove(sop);
                    }
                }
            }
            // If m_stopOptions does not contain the given stop option policy, do nothing. 
        }

        /// <summary>
        /// Acts on debugger callback based on the contained stop option policies matching
        /// the callback type.
        /// </summary>
        /// <param name="currentProcess">Current MDbgProcess.</param>
        /// <param name="args">Debugger callback arguments.</param>
        public void ActOnCallback(MDbgProcess currentProcess, CustomPostCallbackEventArgs args)
        {
            if (m_stopOptions[(int)args.CallbackType] != null)
            {
                foreach (MDbgStopOptionPolicy sop in m_stopOptions[(int)args.CallbackType])
                {
                    sop.ActOnCallback(currentProcess, args);
                }
            }
        }

        /// <summary>
        /// Modifies all contained stop option policies with the given acronym.
        /// </summary>
        /// <param name="acronym">Stop option policy acronym.</param>
        /// <param name="option">Desired debugger behavior - stop, log, or ignore.</param>
        /// <param name="arguments">Arguments to pass to the stop option policy.</param>
        public void ModifyOptions(string acronym, MDbgStopOptionPolicy.DebuggerBehavior option, string arguments)
        {
            int matchingPolicies = 0;
            foreach (MDbgStopOptionPolicy sop in Policies())
            {
                if (sop.Acronym == acronym)
                {
                    sop.SetBehavior(option, arguments);
                    matchingPolicies++;
                }
            }
            if (matchingPolicies == 0)
            {
                //no stop option policies exist that match the given acronym
                throw new MDbgShellException("Unrecognized option");
            }
        }

        /// <summary>
        /// Print current stop option policies.
        /// </summary>
        public void PrintOptions()
        {
            foreach (MDbgStopOptionPolicy sop in Policies())
            {
                sop.Print();
            }
        }

        /// <summary>
        /// Returns an iterator over all contained stop option policies.
        /// </summary>
        /// <returns>An iterator over all contained stop option policies.</returns>
        public IEnumerable<MDbgStopOptionPolicy> Policies()
        {
            List<MDbgStopOptionPolicy> allPolicies = new List<MDbgStopOptionPolicy>();
            foreach (List<MDbgStopOptionPolicy> policies in m_stopOptions)
            {
                if (policies != null)
                {
                    foreach (MDbgStopOptionPolicy policy in policies)
                    {
                        if (!allPolicies.Contains(policy))
                        {
                            allPolicies.Add(policy);
                            yield return policy;
                        }
                    }
                }
            }
        }

        // collection of stop option policies, indexed by their callback type.
        private List<MDbgStopOptionPolicy>[] m_stopOptions;
    }

    /// <summary>
    ///  Collection of modes supported by the debugger.
    /// </summary>
    public sealed class MDbgModeSettings
    {
        /// <value>
        ///   Key name that is used for storing this object into Shell.Properties.
        /// </value>
        public const string PropertyName = "MdbgModeSettings";

        /// <value>
        ///     A list of different modes supported the shell.
        /// </value>
        public ArrayList Items
        {
            get
            {
                return m_items;
            }
        }
        private ArrayList m_items = new ArrayList();
    }

    /// <summary>
    /// Delegate for MDbgModeItem class. Called whenever the mode is changed
    /// by the user with mode command.
    /// </summary>
    /// <param name="item">Object that sent the event.</param>
    /// <param name="onOff">True if the mode is turned on, otherwise false.</param>
    public delegate void ModeChangedEvent(MDbgModeItem item, bool onOff);

    /// <summary>
    /// Delegate for MDbgModeItem class. Called whenever the mode's value is read.
    /// </summary>
    /// <param name="item">Object that sent the event.</param>
    /// <returns> true if the mode is on </returns>
    public delegate bool GetModeValueEvent(MDbgModeItem item);

    /// <summary>
    ///   An individual item from the MDbgModeSettings collection.
    /// </summary>
    public sealed class MDbgModeItem
    {
        /// <summary>
        /// Creates a new MDbgModeItem object.
        /// </summary>
        /// <param name="shortcut">A shortcut for the mode command.</param>
        /// <param name="description">A description of what this mode represents.</param>
        /// <param name="modeValueCallback">An event that is called whenever current value of the mode is queried.</param>
        /// <param name="modeChangedCallback">An event that is called whenever user changes mode with mode command.</param>
        public MDbgModeItem(string shortcut, string description,
                            GetModeValueEvent modeValueCallback, ModeChangedEvent modeChangedCallback)
        {
            Debug.Assert(!string.IsNullOrEmpty(shortcut));
            Debug.Assert(!string.IsNullOrEmpty(description));
            Debug.Assert(modeChangedCallback != null);
            Debug.Assert(modeValueCallback != null);

            if (string.IsNullOrEmpty(shortcut) ||
                string.IsNullOrEmpty(description) ||
                modeChangedCallback == null ||
                modeValueCallback == null)
                throw new ArgumentException();

            m_shortcut = shortcut;
            m_description = description;
            m_getModeValue = modeValueCallback;
            m_onModeChanged = modeChangedCallback;
        }

        /// <value>
        ///     Shortcut used by user to change the mode.
        /// </value>
        public string ShortCut
        { get { return m_shortcut; } }

        /// <value>
        ///     A description of the mode.
        /// </value>
        public string Description
        { get { return m_description; } }

        /// <value>
        ///     Gets or sets current setting of the mode.
        /// </value>
        public bool OnOff
        {
            get
            {
                return m_getModeValue(this);
            }
            set
            {
                m_onModeChanged(this, value);
            }
        }

        public override bool Equals(Object obj)
        {
            MDbgModeItem mode = obj as MDbgModeItem;
            if (this.ShortCut == mode.ShortCut && this.Description == mode.Description)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private ModeChangedEvent m_onModeChanged;
        private GetModeValueEvent m_getModeValue;
        private string m_description;
        private string m_shortcut;
    }

    /// <summary>
    /// A class of utility functions used in mdbg.
    /// </summary>
    static public class MDbgUtil
    {
        /// <summary>
        /// converts a dos-like regexp to true regular expresion.
        /// This enables simple filters for types as e.g.:
        /// x mod!System.String*
        ///
        /// currently function supports just 2 special chars: * (match
        /// 0-unlim chars) and ? (match 1 char).
        /// </summary>
        /// <param name="simpleExp">dos-like regular expression</param>
        /// <returns>A regexp pattern matching teh dos regular exrepssion</returns>
        public static string ConvertSimpleExpToRegExp(string simpleExp)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("^");
            foreach (char c in simpleExp)
            {
                switch (c)
                {
                    case '\\':
                    case '{':
                    case '|':
                    case '+':
                    case '[':
                    case '(':
                    case ')':
                    case '^':
                    case '$':
                    case '.':
                    case '#':
                    case ' ':
                        sb.Append('\\').Append(c);
                        break;
                    case '*':
                        sb.Append(".*");
                        break;
                    case '?':
                        sb.Append(".");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            sb.Append("$");
            return sb.ToString();
        }

        /// <summary>
        /// Checks if a string is a regular expression supported by MDbg. Currently, MDbg supports 
        /// just 2 special chars: * (match 0-unlim chars) and ? (match 1 char).
        /// </summary>
        /// <param name="s">input string</param>
        /// <returns>true if s is a regular expression supported by MDbg, false otherwise</returns>
        public static bool IsRegex(String s)
        {
            if (s.Contains("*") || s.Contains("?"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Given a CorEventArgs object, returns a corresponding StopReason.
        /// </summary>
        /// <param name="args">Callback Arguments.</param>
        /// <param name="currentProcess">Current MDbgProcess.</param>
        /// <returns>A stop reason corresponsing to the given callback arguments. This
        /// function currently creates StopReason objects for CorEventArgs with the following
        /// callback types: OnCreateThread, OnExceptionUnwind2, OnModuleLoad, OnMDANotification.
        /// For all other callback types, this function returns args.ToString().</returns>
        public static Object CreateStopReasonFromEventArgs(CorEventArgs args, MDbgProcess currentProcess)
        {
            if (args.CallbackType == ManagedCallbackType.OnCreateThread)
            {
                return new ThreadCreatedStopReason(currentProcess.Threads.GetThreadFromThreadId(args.Thread.Id));
            }
            if (args.CallbackType == ManagedCallbackType.OnExceptionUnwind2)
            {
                CorExceptionUnwind2EventArgs ea = args as CorExceptionUnwind2EventArgs;
                return new ExceptionUnwindStopReason(ea.AppDomain, ea.Thread, ea.EventType, ea.Flags);
            }
            if (args.CallbackType == ManagedCallbackType.OnModuleLoad)
            {
                CorModuleEventArgs ea = args as CorModuleEventArgs;
                return new ModuleLoadedStopReason(currentProcess.Modules.Lookup(ea.Module));
            }
            if (args.CallbackType == ManagedCallbackType.OnMDANotification)
            {
                CorMDAEventArgs ea = args as CorMDAEventArgs;
                return new MDANotificationStopReason(ea.MDA);
            }
            return args.ToString();
        }


    }

}
