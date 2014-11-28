//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Globalization;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorMetadata;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.CorPublish;
using Microsoft.Samples.Debugging.Native;
using Microsoft.Samples.Debugging.CorDebug.Utility;

namespace Microsoft.Samples.Tools.Mdbg
{
    // no instances of this class 


    sealed internal class MdbgCommands : CommandBase
    {
        private static System.Resources.ResourceManager g_rm;
        private static string g_lastSavedRunCommand;
        private static ThreadNickNames g_threadNickNames;
        private static MDbgSymbolCache SymbolCache;
        private static ArrayList m_events;
        private static Dictionary<string, bool> m_loadedExtensions;

        // Initialize this extension. We can only initialize global (non-process specific) state right now
        // since we may not have an active process.
        // However, we can subscribe to the ProcessAdded event to register per-process state.
        public static void Initialize()
        {
            g_rm = new System.Resources.ResourceManager("mdbgCommands", System.Reflection.Assembly.GetExecutingAssembly());
            MDbgAttributeDefinedCommand.AddCommandsFromType(Shell.Commands, typeof(MdbgCommands));

            g_lastSavedRunCommand = null;
            g_threadNickNames = new ThreadNickNames();
            SymbolCache = new MDbgSymbolCache();
            m_events = new ArrayList();
            m_loadedExtensions = new Dictionary<string, bool>();

            // initialization of various properties
            InitModeShellProperty();
            InitStopOptionsProperty();

            Debug.Assert(Shell.Debugger != null);

            // Install our default breakpoint parser.
            if (Shell.BreakpointParser == null)
            {
                Shell.BreakpointParser = new DefaultBreakpointParser();
            }
            if (Shell.ExpressionParser == null)
            {
                Shell.ExpressionParser = new DefaultExpressionParser();
            }

            // We could subscribe to process-specific event handlers via the the Shell.Debugger.Processes.ProcessAdded event.
            Shell.Debugger.Processes.ProcessAdded += new ProcessCollectionChangedEventHandler(Processes_ProcessAdded);
        }

        static void Processes_ProcessAdded(object sender, ProcessCollectionChangedEventArgs e)
        {
            e.Process.PostDebugEvent += new PostCallbackEventHandler(PostDebugEventHandler);
        }

        static void PostDebugEventHandler(object sender, CustomPostCallbackEventArgs e)
        {
            MDbgStopOptions stopOptions = Shell.Properties[MDbgStopOptions.PropertyName]
                as MDbgStopOptions;

            stopOptions.ActOnCallback(sender as MDbgProcess, e);
        }

        public class DefaultExpressionParser : IExpressionParser
        {
            /// <summary>
            /// Creates a new parser
            /// </summary>
            public DefaultExpressionParser()
            {
                InitPrimitiveTypes();
            }

            public MDbgValue ParseExpression(string variableName, MDbgProcess process, MDbgFrame scope)
            {
                Debug.Assert(process != null);
                return process.ResolveVariable(variableName, scope);
            }

            public CorValue ParseExpression2(string value, MDbgProcess process, MDbgFrame scope)
            {
                if (value.Length == 0)
                {
                    return null;
                }
                CorGenericValue result;
                if (TryCreatePrimitiveValue(value, out result))
                {
                    //value is a primitive type
                    return result;
                }
                if (value[0] == '"' && value[value.Length - 1] == '"')
                {
                    //value is a string
                    return CreateString(value);
                }
                //value is some variable
                Debug.Assert(process != null);
                MDbgValue var = process.ResolveVariable(value, scope);
                return (var == null ? null : var.CorValue);
            }

            /// <summary>
            /// Creates a CorGenericValue object for primitive types, for use in function 
            /// evaluations and setting debugger variables.
            /// </summary>
            /// <param name="input">input has to be in the form "input" or "(type)input", where
            /// we use the ldasm naming convention (e.g. "int", "sbyte", "ushort", etc...) OR 
            /// full type names (e.g. System.Char, System.Int32) for type.
            /// Example inputs: 45, 'a', true, 556.3, (long)45, (sbyte)5, (System.Int64)65 </param>
            /// <param name="result">A CorGenericValue that has the value of input</param>
            /// <returns>True iff input was parsed succesfully</returns>
            public bool TryCreatePrimitiveValue(string input, out CorGenericValue result)
            {
                result = null;
                CorEval eval = Debugger.Processes.Active.Threads.Active.CorThread.CreateEval();
                CorValue val = null;
                CorGenericValue gv = null;
                PrimitiveType literalType;
                object value = null;
                PrimitiveType? castType = null;

                // check for a casting operation
                if (input[0] == '(')
                {
                    // type is specified in value
                    int index = input.IndexOf(')');
                    if (index == -1)
                    {
                        // input has no closing parenthesis
                        return false;
                    }

                    string typeName = input.Substring(1, index - 1).Trim();
                    if (m_primitiveTypes.ContainsKey(typeName))
                    {
                        castType = m_primitiveTypes[typeName];
                        input = input.Substring(index + 1);
                    }
                }

                if (!TryParsePrimitiveLiteral(input, out literalType, out value))
                {
                    return false;
                }
                Debug.Assert(value != null);

                // apply the cast if one was present earlier
                if (castType != null)
                {
                    try
                    {
                        value = Convert.ChangeType(value, castType.Value.type, CultureInfo.InvariantCulture);
                        literalType = castType.Value;
                    }
                    catch (InvalidCastException)
                    {
                        return false;
                    }
                }

                // create and set the Generic value
                val = eval.CreateValue(literalType.elementType, null);
                gv = val.CastToGenericValue();
                gv.SetValue(value);
                result = gv;
                return true;
            }

            /// <summary>
            /// Creates a CorReferenceValue for an input string.
            /// </summary>
            /// <param name="value">The input string. Must be surrounded by quotation marks,
            /// e.g. "mystring".</param>
            /// <returns>A CorReferenceValue representing the input string.</returns>
            public static CorReferenceValue CreateString(string value)
            {
                // ensure that input is in the correct format. Input must be surrounded by quotation marks.
                if (value.Length < 2 || !value.StartsWith("\"") || !value.EndsWith("\""))
                {
                    throw new MDbgShellException("Cannot create string; input is not in correct format. Input must be surrounded by quotation marks.");
                }
                // strip surrounding quotation marks
                string escapedLiteral = value.Substring(1, value.Length - 2);
                // the value after processing escape sequences
                string literal;
                if (!TryParseCharacterLiteral(escapedLiteral, out literal))
                {
                    throw new MDbgShellException("Invalid string literal");
                }
                CorEval eval = Debugger.Processes.Active.Threads.Active.CorThread.CreateEval();
                eval.NewString(literal);
                Debugger.Processes.Active.Go().WaitOne();
                Debug.Assert(Debugger.Processes.Active.StopReason != null);
                if (!(Debugger.Processes.Active.StopReason is EvalCompleteStopReason))
                {
                    throw new MDbgShellException("Wrong stop reason when creating string!");
                }
                CorValue corValue = (Debugger.Processes.Active.StopReason as EvalCompleteStopReason).Eval.Result;
                return corValue.CastToReferenceValue();
            }

            /// <summary>
            /// Parses an input string containing escaped literal characters. Most C escape sequences
            /// are supported, however trigraphs, /ooo (octal) and /x## (hex) are not.
            /// </summary>
            /// <param name="input">Expression representing the escaped literal string</param>
            /// <param name="value">The unescaped value of the literal string</param>
            /// <returns>True iff the string could be parsed correctly</returns>
            private static bool TryParseCharacterLiteral(string input, out string value)
            {
                value = null;
                if (input == null)
                {
                    return false;
                }
                StringBuilder result = new StringBuilder();

                // iterate over each character or escape sequence
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] != '\\') // not an escape sequence
                    {
                        result.Append(input[i]);
                    }
                    else
                    {
                        i++; // go past the slash character
                        if (input.Length <= i)
                        {
                            return false; // slash missing following character
                        }
                        if (input[i] == 'a')
                        {
                            result.Append('\a');
                        }
                        else if (input[i] == 'b')
                        {
                            result.Append('\b');
                        }
                        else if (input[i] == 'f')
                        {
                            result.Append('\f');
                        }
                        else if (input[i] == 'n')
                        {
                            result.Append('\n');
                        }
                        else if (input[i] == 'r')
                        {
                            result.Append('\r');
                        }
                        else if (input[i] == 't')
                        {
                            result.Append('\t');
                        }
                        else if (input[i] == 'v')
                        {
                            result.Append('\v');
                        }
                        else if (input[i] == '\'')
                        {
                            result.Append('\'');
                        }
                        else if (input[i] == '\"')
                        {
                            result.Append('\"');
                        }
                        else if (input[i] == '\\')
                        {
                            result.Append('\\');
                        }
                        else if (input[i] == '?')
                        {
                            result.Append('?');
                        }
                        else if (input[i] == 'x')
                        {
                            if (input.Length <= i + 4) // a 4 digit hex number should be here
                            {
                                return false;
                            }
                            int charCode;
                            if (!int.TryParse(input.Substring(i + 1, 4), NumberStyles.AllowHexSpecifier,
                                CultureInfo.InvariantCulture, out charCode))
                            {
                                return false;
                            }
                            result.Append((char)charCode);
                            i += 4;
                        }
                        else // C also supports trigraphs, /### (octal) and /x## (hex) but
                        // we don't parse them. Any other unrecognized escape sequence falls
                        // in here too
                        {
                            return false;
                        }
                    }
                }
                value = result.ToString();
                return true;
            }

            /// <summary>
            /// Parses an input string representing a literal primitive value.
            /// </summary>
            /// <param name="input">Expression representing a literal primitive value.</param>
            /// <param name="type">The type of the primitive value that the input expression represents.</param>
            /// <param name="value">A boxed primitive that the input expression represents.</param>
            /// <returns>True iff the input was parsed succesfully</returns>
            private bool TryParsePrimitiveLiteral(string input, out PrimitiveType type, out object value)
            {
                type = m_primitiveTypes["bool"];
                value = null;
                double doubleResult;
                int intResult;
                bool boolResult;
                if (input.Length == 0)
                {
                    return false;
                }

                // if the string starts and ends with ' and has something in the middle, assume it is a char
                else if (input[0] == '\'' && input[input.Length - 1] == '\'' && input.Length >= 3)
                {
                    string literal = input.Substring(1, input.Length - 2);
                    string literalResult;
                    if (TryParseCharacterLiteral(literal, out literalResult))
                    {
                        if (literalResult != null && literalResult.Length == 1)
                        {
                            type = m_primitiveTypes["char"];
                            value = (object)literalResult[0];
                            return true;
                        }
                    }
                    return false;

                }
                // if the value is parsable as an int, assume it represents one
                else if (int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out intResult))
                {
                    type = m_primitiveTypes["int"];
                    value = (object)intResult;
                    return true;
                }
                // if the value is parsable as a double, assume it represents one
                else if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out doubleResult))
                {
                    type = m_primitiveTypes["double"];
                    value = (object)doubleResult;
                    return true;
                }
                // if the value is parsable as a bool, assume it represents one
                else if (bool.TryParse(input, out boolResult))
                {
                    type = m_primitiveTypes["bool"];
                    value = (object)boolResult;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private void InitPrimitiveTypes()
            {
                m_primitiveTypes = new Dictionary<string, PrimitiveType>();

                PrimitiveType sbyteType = new PrimitiveType(typeof(SByte), CorElementType.ELEMENT_TYPE_I1);
                m_primitiveTypes.Add("sbyte", sbyteType);
                m_primitiveTypes.Add("System.SByte", sbyteType);

                PrimitiveType byteType = new PrimitiveType(typeof(Byte), CorElementType.ELEMENT_TYPE_U1);
                m_primitiveTypes.Add("byte", byteType);
                m_primitiveTypes.Add("System.Byte", byteType);

                PrimitiveType shortType = new PrimitiveType(typeof(Int16), CorElementType.ELEMENT_TYPE_I2);
                m_primitiveTypes.Add("short", shortType);
                m_primitiveTypes.Add("System.Int16", shortType);

                PrimitiveType intType = new PrimitiveType(typeof(Int32), CorElementType.ELEMENT_TYPE_I4);
                m_primitiveTypes.Add("int", intType);
                m_primitiveTypes.Add("System.Int32", intType);

                PrimitiveType longType = new PrimitiveType(typeof(Int64), CorElementType.ELEMENT_TYPE_I8);
                m_primitiveTypes.Add("long", longType);
                m_primitiveTypes.Add("System.Int64", longType);

                PrimitiveType ushortType = new PrimitiveType(typeof(UInt16), CorElementType.ELEMENT_TYPE_U2);
                m_primitiveTypes.Add("ushort", ushortType);
                m_primitiveTypes.Add("System.UInt16", ushortType);

                PrimitiveType uintType = new PrimitiveType(typeof(UInt32), CorElementType.ELEMENT_TYPE_U4);
                m_primitiveTypes.Add("uint", uintType);
                m_primitiveTypes.Add("System.UInt32", uintType);

                PrimitiveType ulongType = new PrimitiveType(typeof(UInt64), CorElementType.ELEMENT_TYPE_U8);
                m_primitiveTypes.Add("ulong", ulongType);
                m_primitiveTypes.Add("System.UInt64", ulongType);

                PrimitiveType floatType = new PrimitiveType(typeof(Single), CorElementType.ELEMENT_TYPE_R4);
                m_primitiveTypes.Add("float", floatType);
                m_primitiveTypes.Add("System.Single", floatType);

                PrimitiveType doubleType = new PrimitiveType(typeof(Double), CorElementType.ELEMENT_TYPE_R8);
                m_primitiveTypes.Add("double", doubleType);
                m_primitiveTypes.Add("System.Double", doubleType);

                PrimitiveType boolType = new PrimitiveType(typeof(Boolean), CorElementType.ELEMENT_TYPE_BOOLEAN);
                m_primitiveTypes.Add("bool", boolType);
                m_primitiveTypes.Add("System.Boolean", boolType);

                PrimitiveType charType = new PrimitiveType(typeof(Char), CorElementType.ELEMENT_TYPE_CHAR);
                m_primitiveTypes.Add("char", charType);
                m_primitiveTypes.Add("System.Char", charType);
            }

            private struct PrimitiveType
            {
                public Type type;
                public CorElementType elementType;

                public PrimitiveType(Type type, CorElementType elementType)
                {
                    this.type = type;
                    this.elementType = elementType;
                }
            }

            // Dictionary mapping primitive type names to PrimitiveValue objects
            private Dictionary<string, PrimitiveType> m_primitiveTypes;
        }

        [
         CommandDescription(
           CommandName = "exit",
           MinimumAbbrev = 2,
           IsRepeatable = false,
           ShortHelp = "Quits the program",
           LongHelp = "Usage: quit [exitcode]\n    Exits the mdbg shell, optionally specifying the process exit code.\n"
         ),
         CommandDescription(
           CommandName = "quit",
           MinimumAbbrev = 1,
           IsRepeatable = false,
           ShortHelp = "Quits the program",
           LongHelp = "Usage: quit [exitcode]\n    Exits the mdbg shell, optionally specifying the process exit code.\n"
         )
        ]
        public static void QuitCmd(string arguments)
        {
            // Look for optional exit code.
            ArgParser ap = new ArgParser(arguments);

            int exitCode;
            if (ap.Exists(0))
            {
                exitCode = ap.AsInt(0);
            }
            else
            {
                exitCode = 0;
            }

            // we cannot modify the collection during enumeration, so
            // we need to collect all processes to kill in advance.
            List<MDbgProcess> processesToKill = new List<MDbgProcess>();
            foreach (MDbgProcess p in Debugger.Processes)
            {
                processesToKill.Add(p);
            }

            foreach (MDbgProcess p in processesToKill)
            {
                if (p.IsAlive)
                {
                    Debugger.Processes.Active = p;
                    WriteOutput("Terminating current process...");
                    try
                    {
                        p.Kill();

                        // We can't wait for targets that never run (e.g. NoninvasiveStopGoController against a dump)
                        if (p.CanExecute())
                            p.StopEvent.WaitOne();
                    }
                    catch
                    {
                        // some processes cannot be killed (e.g. the one that have not loaded runtime)
                        try
                        {
                            Process np = Process.GetProcessById(p.CorProcess.Id);
                            np.Kill();
                        }
                        catch
                        {
                        }
                    }
                }
            }

            Shell.QuitWithExitCode(exitCode);
        }


        [
         CommandDescription(
           CommandName = "?",
           MinimumAbbrev = 1,
           IsRepeatable = false,
           ShortHelp = "Prints this help screen.",
           LongHelp = "Usage: help\n    Prints this help screen.\n"
         ),
         CommandDescription(
           CommandName = "help",
           MinimumAbbrev = 1,
           IsRepeatable = false,
           ShortHelp = "Prints this help screen.",
           LongHelp = "Usage: help\n    Prints this help screen.\n"
         )
        ]
        public static void HelpCmd(string arguments)
        {
            if (arguments.Length == 0)
            {
                StringBuilder sb = new StringBuilder(
                    "Following commands are available:\n"
                    );
                Assembly currentSection = null;
                foreach (IMDbgCommand c in Shell.Commands)
                {
                    if (c.LoadedFrom != currentSection)
                    {
                        bool bPrintExtensionName = currentSection != null;

                        currentSection = c.LoadedFrom;
                        if (bPrintExtensionName)
                        {
                            WriteOutput(MDbgOutputConstants.StdOutput, "\nExtension: " + currentSection.GetName().Name,
                                        0, Int32.MaxValue);
                        }
                    }

                    string name;
                    if (c.CommandName.Length != c.MinimumAbbrev)
                    {
                        name = c.CommandName.Substring(0, c.MinimumAbbrev) + "[" + c.CommandName.Substring(c.MinimumAbbrev) + "]";
                    }
                    else
                    {
                        name = c.CommandName;
                    }
                    const int indentSize = 14;
                    if (name.Length <= indentSize)
                    {
                        sb.Append(String.Format(CultureInfo.InvariantCulture, "{0,-" + indentSize + "}{1}", new Object[] { name, c.ShortHelp }));
                    }
                    else
                    {
                        sb.Append(String.Format(CultureInfo.InvariantCulture, "{0}\n{1,-" + indentSize + "}{2}", new Object[] { name, " ", c.ShortHelp }));
                    }
                    WriteOutput(sb.ToString());
                    sb.Length = 0;                            // clear the string builder
                }
            }
            else
            {
                IMDbgCommand c = Shell.Commands.Lookup(arguments);
                string name;
                if (c.CommandName.Length != c.MinimumAbbrev)
                {
                    name = c.CommandName.Substring(0, c.MinimumAbbrev) + "[" + c.CommandName.Substring(c.MinimumAbbrev) + "]";
                }
                else
                {
                    name = c.CommandName;
                }

                string longHelp = "<unavailable>";
                try
                {
                    // Extension may not define help strings.
                    longHelp = c.LongHelp;
                }
                catch (System.Resources.MissingManifestResourceException e)
                {
                    longHelp = "No resource for help. " + e.Message;
                }

                WriteOutput("abbrev: " + name + "\n" + longHelp);
            }
        }

        [
         CommandDescription(
           CommandName = "run",
           MinimumAbbrev = 1,
           IsRepeatable = false,
           ShortHelp = "Runs a program under the debugger",
           LongHelp = "Usage: run [debug_flag] [-ver version_string] [[path_to_exe] [args_to_exe]]\n    Kills the current process (if there is one) and starts a new one. If no\n    executable argument is passed, this command runs the program that was\n    previously executed with the run command. If the executable argument is\n    provided, the specified program is run using the optionally supplied args. \n    If class load, module load, and thread start events are being ignored (as\n    they are by default), then the program will stop on the first executable\n    instruction of the main thread.    \n    debug_flag parameter can be optionally passed in as a way to control how\n    the managed code executed will be optimized. It can be one of the following\n    values:\n      -d(ebug)\n          Causes all the code to be executed in debug mode so the debugging\n          experience is best. This is the default value if no debug_falg is\n          specified.\n      -o(ptimize)\n          Causes all the code to be executed in optimized mode. Debugger won't\n          necessary display all locals and function parameters. Debugger\n          stepping might be affected as well. \n      -enc\n          Causes all the code to be executed in \"Edit & Continue\" mode. Mode\n          is similar to -debug mode but also allows debugger to modify the\n          executed code at runtime.\n      -default\n          This flag causes the debugged program run in the way as if it were\n          run without debugger.\n    \n    The -ver flag allows to explicitly specify version string that used to\n    load correct debugger implementation. When the string is not entered it\n    is deducted from the debugged program.\nSee Also:\n    kill\n"
         )
        ]
        public static void RunCmd(string arguments)
        {
            ArrayList bpLocations = null;

            if (arguments.Length == 0 && g_lastSavedRunCommand != null)
            {
                // we have requested to restart the process.
                // We should kill the existing one.
                if (Debugger.Processes.HaveActive)
                {
                    // save locations of current breakpoints
                    foreach (MDbgBreakpoint b in Debugger.Processes.Active.Breakpoints)
                    {
                        if (bpLocations == null)
                        {
                            bpLocations = new ArrayList();
                        }
                        bpLocations.Add(b.Location);
                    }
                    ExecuteCommand("kill");
                }
                arguments = g_lastSavedRunCommand;
            }

            // Parse the arguments.
            RunOptions options = new RunOptions(arguments);

            if (options.Application == null)
            {
                throw new MDbgShellException("Must specify a program to run");
            }
            else if (!File.Exists(options.Application))
            {
                throw new MDbgShellException("The specified executable \'" + options.Application + "\' could not be found.");
            }

            String version = options.Version;
            // Only happens if no -ver was specified from commandline.
            if (version == null)
            {
                version = MdbgVersionPolicy.GetDefaultLaunchVersion(options.Application);

                if ((version != null) && (options.Verbosity >= RunVerbosity.High))
                {
                    CommandBase.WriteOutput("No version specified with run, defaulting to " + version);
                }
            }

            if ((version != null) && (options.Verbosity >= RunVerbosity.High))
            {
                WriteOutput("Debuggee runtime version will be: " + version);
            }

            // get MdbgProcess for that version
            MDbgProcess p = Debugger.Processes.CreateLocalProcess(new CorDebugger(version));

            if (p == null)
            {
                throw new MDbgShellException("Could not create debugging interface for runtime version " + version);
            }


            p.DebugMode = options.DebugMode;
            p.CreateProcess(options.Application, options.Arguments);

            // recreate debugger breakpoints
            if (bpLocations != null)
            {
                foreach (object location in bpLocations)
                {
                    p.Breakpoints.CreateBreakpoint(location);
                }
            }
            p.Go().WaitOne();
            g_lastSavedRunCommand = arguments;

            Shell.DisplayCurrentLocation();
        }

        [
         CommandDescription(
           CommandName = "go",
           MinimumAbbrev = 1,
           ShortHelp = "Continues program execution",
           LongHelp = "Usage: go\n    The program will continue until either a breakpoint is hit, the program\n    exits, or an event causes it to stop (for example an unhandled exception.)\n"
         )
        ]
        public static void GoCmd(string arguments)
        {
            Debugger.Processes.Active.Go().WaitOne();
            Shell.DisplayCurrentLocation();
        }

        [
         CommandDescription(
           CommandName = "kill",
           MinimumAbbrev = 1,
           IsRepeatable = false,
           ShortHelp = "Kills the active process",
           LongHelp = "Usage: kill\n    Kills the active process\nSee Also:\n    run"
         )
        ]
        public static void KillCmd(string arguments)
        {
            MDbgProcess p = Debugger.Processes.Active;
            try
            {
                p.Kill();

                // We can't wait for targets that never run (e.g. NoninvasiveStopGoController against a dump)
                if (p.CanExecute())
                    p.StopEvent.WaitOne();

            }
            catch
            {
                // some processes cannot be killed (e.g. the one that have not loaded runtime)
                try
                {
                    Process np = Process.GetProcessById(p.CorProcess.Id);
                    np.Kill();
                }
                catch
                {
                }
            }

            // Normally, the Jit will consider the end of the lifespan to be the last spot a var is used.
            // However, the debugger can extend variable lifespans for the entire function, so we null out p
            // for debug builds.
            p = null;
        }


        [
         CommandDescription(
           CommandName = "setip",
           ShortHelp = "Sets an ip into new position in the current function",
           LongHelp = "Usage: setip [-il] number\n    Sets current Instruction Pointer (ip) in the file to the position as\n    specified.  If -il optional switch is specified than the number represents\n    an IL (Intermediate Language) offset in the method. Otherwise the number\n    represents a source line number.\n"
         )
        ]
        public static void SetIPCmd(string arguments)
        {
            const string IlOpt = "il";
            ArgParser ap = new ArgParser(arguments, IlOpt);
            if (ap.OptionPassed(IlOpt))
            {
                Debugger.Processes.Active.Threads.Active.CurrentFrame.CorFrame.SetIP(ap.AsInt(0));
            }
            else
            {
                int ilOffset;
                if (!Debugger.Processes.Active.Threads.Active.CurrentFrame.Function.GetIPFromLine(ap.AsInt(0), out ilOffset))
                {
                    throw new MDbgShellException("cannot find correct function mapping");
                }

                int hresult;
                if (!Debugger.Processes.Active.Threads.Active.CurrentFrame.CorFrame.CanSetIP(ilOffset, out hresult))
                {
                    string reason;
                    switch ((HResult)hresult)
                    {
                        case HResult.CORDBG_S_BAD_START_SEQUENCE_POINT:
                            reason = "Attempt to SetIP from non-sequence point.";
                            break;
                        case HResult.CORDBG_S_BAD_END_SEQUENCE_POINT:
                            reason = "Attempt to SetIP from non-sequence point.";
                            break;
                        case HResult.CORDBG_S_INSUFFICIENT_INFO_FOR_SET_IP:
                            reason = "Insufficient information to fix program flow.";
                            break;
                        case HResult.CORDBG_E_CANT_SET_IP_INTO_FINALLY:
                            reason = "Attempt to SetIP into finally block.";
                            break;
                        case HResult.CORDBG_E_CANT_SET_IP_OUT_OF_FINALLY:
                        case HResult.CORDBG_E_CANT_SET_IP_OUT_OF_FINALLY_ON_WIN64:
                            reason = "Attempt to SetIP out of finally block.";
                            break;
                        case HResult.CORDBG_E_CANT_SET_IP_INTO_CATCH:
                            reason = "Attempt to SetIP into catch block.";
                            break;
                        case HResult.CORDBG_E_CANT_SET_IP_OUT_OF_CATCH_ON_WIN64:
                            reason = "Attempt to SetIP out of catch block.";
                            break;
                        case HResult.CORDBG_E_SET_IP_NOT_ALLOWED_ON_NONLEAF_FRAME:
                            reason = "Attempt to SetIP on non-leaf frame.";
                            break;
                        case HResult.CORDBG_E_SET_IP_IMPOSSIBLE:
                            reason = "The operation cannot be completed.";
                            break;
                        case HResult.CORDBG_E_CANT_SETIP_INTO_OR_OUT_OF_FILTER:
                            reason = "Attempt to SetIP into or out of filter.";
                            break;
                        case HResult.CORDBG_E_SET_IP_NOT_ALLOWED_ON_EXCEPTION:
                            reason = "SetIP is not allowed on exception.";
                            break;
                        default:
                            reason = "Reason unknown.";
                            break;
                    }
                    throw new MDbgShellException("Cannot set IP as requested. " + reason);
                }

                Debugger.Processes.Active.Threads.Active.CurrentFrame.CorFrame.SetIP(ilOffset);
            }
            Debugger.Processes.Active.Threads.InvalidateAllStacks();
            Shell.DisplayCurrentLocation();
        }

        [
         CommandDescription(
           CommandName = "where",
           MinimumAbbrev = 1,
           ShortHelp = "Prints a stack trace",
           LongHelp = "Usage: where [-v] [-c depth] [threadID]\n    The -v switch provides verbose information about each displayed\n    stack-frame.  If you specify a number for the depth, this limits how many\n    frames are displayed.  Use \"all\" to get all frames.  The default is 100. \n    If you specify the threadID, you can control which thread the stack is for.\n     Default is current frame only.  \"all\" will display for all threads.\n"
         )
        ]
        public static void WhereCmd(string arguments)
        {
            const int default_depth = 100; // default number of frames to print

            const string countOpt = "c";
            const string verboseOpt = "v";

            ArgParser ap = new ArgParser(arguments, countOpt + ":1;" + verboseOpt);
            int depth = default_depth;
            if (ap.OptionPassed(countOpt))
            {
                ArgToken countArg = ap.GetOption(countOpt);
                if (countArg.AsString == "all")
                {
                    depth = 0; // 0 means print entire stack
                }
                else
                {
                    depth = countArg.AsInt;
                    if (depth <= 0)
                    {
                        throw new MDbgShellException("Depth must be positive number or string \"all\"");
                    }
                }
            }
            if (ap.Count != 0 && ap.Count != 1)
            {
                throw new MDbgShellException("Wrong # of arguments.");
            }

            if (ap.Count == 0)
            {
                // print current thread only
                InternalWhereCommand(Debugger.Processes.Active.Threads.Active, depth, ap.OptionPassed(verboseOpt));
            }
            else if (ap.AsString(0).Equals("all"))
            {
                foreach (MDbgThread t in Debugger.Processes.Active.Threads)
                    InternalWhereCommand(t, depth, ap.OptionPassed(verboseOpt));
            }
            else
            {
                MDbgThread t = Debugger.Processes.Active.Threads[ap.AsInt(0)];
                if (t == null)
                {
                    throw new MDbgShellException("Wrong thread number");
                }
                else
                {
                    InternalWhereCommand(t, depth, ap.OptionPassed(verboseOpt));
                }
            }
        }

        private static void InternalWhereCommand(MDbgThread thread, int depth, bool verboseOutput)
        {
            Debug.Assert(thread != null);

            WriteOutput("Thread [#:" + g_threadNickNames.GetThreadName(thread) + "]");

            MDbgFrame af = thread.HaveCurrentFrame ? thread.CurrentFrame : null;
            MDbgFrame f = thread.BottomFrame;
            int i = 0;
            while (f != null && (depth == 0 || i < depth))
            {
                string line;
                if (f.IsInfoOnly)
                {
                    if (!ShowInternalFrames)
                    {
                        // in cases when we don't want to show internal frames, we'll skip them
                        f = f.NextUp;
                        continue;
                    }
                    line = string.Format(CultureInfo.InvariantCulture, "    {0}", f.ToString());
                }
                else
                {
                    string frameDescription = f.ToString(verboseOutput ? "v" : null);
                    line = string.Format(CultureInfo.InvariantCulture, "{0}{1}. {2}", f.Equals(af) ? "*" : " ", i, frameDescription);
                    ++i;
                }
                WriteOutput(line);
                f = f.NextUp;
            }
            if (f != null && depth != 0) // means we still have some frames to show....
            {
                WriteOutput(string.Format(CultureInfo.InvariantCulture, "displayed only first {0} frames. For more frames use -c switch", depth));
            }
        }

        [
         CommandDescription(
           CommandName = "next",
           MinimumAbbrev = 1,
           ShortHelp = "Step Over",
           LongHelp = "Usage: next\n    Debugger will execute whatever it needs to end up on the next line (even if\n    this includes many function calls).\nSee Also:\n    out\n    step"
         )
        ]
        public static void NextCmd(string arguments)
        {
            Debugger.Processes.Active.StepOver(false).WaitOne();
            Shell.DisplayCurrentLocation();
        }

        [
         CommandDescription(
           CommandName = "step",
           MinimumAbbrev = 1,
           ShortHelp = "Step Into",
           LongHelp = "Usage: step\n    Debugger will bring execution into the next function on the current line or\n    move to the next line if there is no function to step into.\nSee Also:\n    out\n    next"
         )
        ]
        public static void StepCmd(string arguments)
        {
            Debugger.Processes.Active.StepInto(false).WaitOne();
            Shell.DisplayCurrentLocation();
        }

        [
         CommandDescription(
           CommandName = "out",
           MinimumAbbrev = 1,
           ShortHelp = "Steps Out of function",
           LongHelp = "Usage: out\n    Debugger will bring execution to the end of the current function and leave\n    you in the calling function.\nSee Also:\n    next\n    step"
         )
        ]
        public static void StepOutCmd(string arguments)
        {
            Debugger.Processes.Active.StepOut().WaitOne();
            Shell.DisplayCurrentLocation();
        }

        [
         CommandDescription(
           CommandName = "show",
           MinimumAbbrev = 2,
           ShortHelp = "Show sources around the current location",
           LongHelp = "Usage: show [lines]\n    Optional \"lines\" specifies how far above and below the current line it will\n    show.\n"
         )
        ]
        public static void ShowCmd(string arguments)
        {
            MDbgSourcePosition pos = Debugger.Processes.Active.Threads.Active.CurrentSourcePosition;

            if (pos == null)
            {
                throw new MDbgShellException("No source location");
            }

            string fileLoc = Shell.FileLocator.GetFileLocation(pos.Path);
            if (fileLoc == null)
            {
                throw new MDbgShellException(string.Format(CultureInfo.InvariantCulture, "Source file '{0}' not available.", pos.Path));
            }

            IMDbgSourceFile file = Shell.SourceFileMgr.GetSourceFile(fileLoc);

            ArgParser ap = new ArgParser(arguments);
            if (ap.Count > 1)
            {
                throw new MDbgShellException("Wrong # of arguments.");
            }

            int around;
            if (ap.Exists(0))
            {
                around = ap.AsInt(0);
            }
            else
            {
                around = 3;
            }

            int lo, hi;
            lo = pos.Line - around;
            if (lo < 1)
            {
                lo = 1;
            }
            hi = pos.Line + around;
            if (hi > file.Count)
            {
                hi = file.Count;
            }

            for (int i = lo; i < hi; i++)
            {
                WriteOutput(String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", i, i == pos.Line ? ":*" : "  ", file[i]));
            }
        }


        // Default breakpoint parser for the MDbg shell. 
        // We don't expose this class publically, but extensions can grab the interface pointer from the Breakpoint Collection's parser property.
        class DefaultBreakpointParser : IBreakpointParser
        {
            // Parse a function breakpoint.
            ISequencePointResolver IBreakpointParser.ParseFunctionBreakpoint(string arguments)
            {
                Regex r;
                Match m;
                // maybe it's in the form:
                // "b ~number"
                r = new Regex(@"^~(\d+)$");
                m = r.Match(arguments);
                string symNum = m.Groups[1].Value;
                if (symNum.Length > 0)
                {
                    int intSymNum = Int32.Parse(symNum, CultureInfo.CurrentUICulture);
                    MdbgSymbol symbol = SymbolCache.Retrieve(intSymNum);

                    return new BreakpointFunctionLocation(
                        string.Format(CultureInfo.InvariantCulture, ":{0}", symbol.ModuleNumber),
                        symbol.ClassName,
                        symbol.Method,
                        symbol.Offset);
                }

                // maybe it's in the form:
                // "b Mdbg.cs:34"
                r = new Regex(@"^(\S+:)?(\d+)$");
                m = r.Match(arguments);
                string fname = m.Groups[1].Value;
                string lineNo = m.Groups[2].Value;
                int intLineNo = 0;

                if (lineNo.Length > 0)
                {
                    if (fname.Length > 0)
                    {
                        fname = fname.Substring(0, fname.Length - 1);
                    }
                    else
                    {
                        MDbgSourcePosition pos = null;

                        MDbgThread thr = Debugger.Processes.Active.Threads.Active;
                        if (thr != null)
                        {
                            pos = thr.CurrentSourcePosition;
                        }

                        if (pos == null)
                        {
                            throw new MDbgShellException("Cannot determine current file");
                        }

                        fname = pos.Path;
                    }
                    intLineNo = Int32.Parse(lineNo, CultureInfo.CurrentUICulture);

                    return new BreakpointLineNumberLocation(fname, intLineNo);
                }

                // now, to be valid, it must be in the form:
                //    "b mdbg!Mdbg.Main+3"
                //    
                // Note that this case must be checked after the source-file case above because 
                // we want to assume that a number by itself is a source line, not a method name.
                // This is the most general form, so check this case last. (Eg, "Mdbg.cs:34" could
                // match as Class='MDbg', Method = 'cs:34'. )
                //
                // The underlying metadata is extremely flexible and allows almost anything to be
                // in a method name, including spaces. Both C#, VB and MDbg's parsing are more restrictive.
                // Note we allow most characters in class and method names, except those we are using for separators
                // (+, ., :), <> since those are typically used to represent generics which we don't
                // support, and spaces since those are usually command syntax errors.  
                // We exclude '*' for sanity reasons. 
                // 
                // Other caveats:
                // - we must allow periods in the method name for methods like ".ctor". 
                // - be sure to allow $ character in the method and class names. Some compilers
                // like to use this in function names.
                // - Classes can't start with a number, but can include and end with numbers.
                // 
                // Ideally we'd have a quoting mechanism and a more flexible parsing system to 
                // handle generics, method overloads, etc. across all of MDbg.  At least we have the 'x' 
                // command and ~ shortcuts as a work-around.

                r = new Regex(@"^" +
                    @"([^\!]+\!)?" + // optional module
                    @"((?:[^.*+:<> ]+\.)*)" +  // optional class
                    @"([^*+:<>\d ][^*+:<> ]*)" +  // method
                    @"(\+\d+)?" +  // optional offset
                    @"$");

                m = r.Match(arguments);
                string module = m.Groups[1].Value;
                string className = m.Groups[2].Value;
                string method = m.Groups[3].Value;
                string offset = m.Groups[4].Value;
                int intOffset = 0;

                if (method.Length > 0)
                {
                    if (module.Length > 0)
                    {
                        module = module.Substring(0, module.Length - 1);
                    }

                    if (className.Length > 0)
                    {
                        // The class/module separator character is captured as part of className.
                        // Chop it off to get just the classname.
                        className = className.Substring(0, className.Length - 1);
                    }

                    if (offset.Length > 0)
                    {
                        intOffset = Int32.Parse(offset.Substring(1), CultureInfo.CurrentUICulture);
                    }

                    return new BreakpointFunctionLocation(module, className, method, intOffset);
                }

                // We don't recognize the syntax. Return null. If the parser is chained, it gives 
                // our parent a chance to handle it.
                return null;

            } // end function ParseFunctionBreakpoint
        } // end class DefaultBreakpointParser

        // Display current breakpoints
        public static void ListBreakpoints()
        {
            MDbgBreakpointCollection breakpoints = Debugger.Processes.Active.Breakpoints;
            WriteOutput("Current breakpoints:");
            bool haveBps = false;
            foreach (MDbgBreakpoint b in breakpoints)
            {
                WriteOutput(b.ToString());
                haveBps = true;
            }
            if (!haveBps)
            {
                WriteOutput("No breakpoints!");
            }
        }

        // Add IL-level breakpoints.
        // If no parameters specified, prints the current breakpoint list.
        [
         CommandDescription(
           CommandName = "break",
           MinimumAbbrev = 1,
           ShortHelp = "Sets or displays breakpoints",
           LongHelp = "Usage: break [ClassName.Method | FileName:LineNo]\n    Sets a breakpoint at the specified Method.  Modules are scanned\n    sequentially.  \"break FileName:LineNo\" sets a breakpoint at location in the\n    source.  \"break ~number\" sets a breakpoint on a symbol recently displayed\n    with 'x' command.  \"break module!ClassName.Method+IlOffset\" sets a\n    breakpoint on the fully qualified location.\nSee Also:\n    delete\n    x"
         )
        ]
        public static void BreakCmd(string arguments)
        {
            if (arguments.Length == 0)
            {
                ListBreakpoints();
                return;
            }

            // We're adding a breakpoint. Parse the argument string.
            MDbgBreakpointCollection breakpoints = Debugger.Processes.Active.Breakpoints;
            ISequencePointResolver bploc = Shell.BreakpointParser.ParseFunctionBreakpoint(arguments);
            if (bploc == null)
            {
                throw new MDbgShellException("Invalid breakpoint syntax.");
            }

            MDbgBreakpoint bpnew = Debugger.Processes.Active.Breakpoints.CreateBreakpoint(bploc);
            WriteOutput(bpnew.ToString());
        }

        [
         CommandDescription(
           CommandName = "delete",
           MinimumAbbrev = 3,
           ShortHelp = "Deletes a breakpoint",
           LongHelp = "Usage: delete [#]\n    Deletes a breakpoint\nSee Also:\n    break"
         )
        ]
        public static void DeleteCmd(string arguments)
        {
            ArgParser ap = new ArgParser(arguments);
            if (ap.Count != 1)
            {
                WriteOutput("Please choose some breakpoint to delete");
                BreakCmd("");
                return;
            }

            MDbgBreakpoint breakpoint = Debugger.Processes.Active.Breakpoints[ap.AsInt(0)];
            if (breakpoint == null)
            {
                throw new MDbgShellException("Could not find breakpint #:" + ap.AsInt(0));
            }
            else
            {
                breakpoint.Delete();
            }
        }

        private class ThreadNickNames
        {
            public MDbgThread GetThreadByNickName(string nickName)
            {
                Debug.Assert(nickName != null);
                if (nickName == null)
                {
                    throw new ArgumentException();
                }
                if (IsNumber(nickName))
                {
                    return Debugger.Processes.Active.Threads[Int32.Parse(nickName, CultureInfo.InvariantCulture)];
                }
                if (m_threadNickNames != null && m_threadNickNames.ContainsKey(nickName))
                {
                    return Debugger.Processes.Active.Threads[(int)NickNamesHash[nickName]];
                }
                else
                {
                    return null;
                }
            }

            public string GetThreadName(MDbgThread thread)
            {
                Debug.Assert(thread != null);
                if (thread == null)
                {
                    throw new ArgumentException();
                }
                string nick = GetNickNameFromThreadNumber(thread.Number);
                if (nick.Length == 0)
                {
                    // no nick name
                    return thread.Number.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    return nick;
                }
            }

            public void SetThreadNickName(string nickName, MDbgThread thread)
            {
                Debug.Assert(thread != null);
                if (thread == null) throw new ArgumentException();
                {
                    NickNamesHash.Remove(GetNickNameFromThreadNumber(thread.Number)); // remove old nick-name, if any
                }
                if (nickName == null || nickName.Length == 0)
                {
                    return; // we just want to remove nickname
                }

                if (IsNumber(nickName))
                {
                    throw new MDbgShellException("invalid nickname");
                }

                if (NickNamesHash.ContainsKey(nickName))
                {
                    throw new MDbgShellException("nickname already exists");
                }
                NickNamesHash.Add(nickName, Debugger.Processes.Active.Threads.Active.Number);
            }

            private string GetNickNameFromThreadNumber(int threadNumber)
            {
                if (m_threadNickNames == null)
                {
                    return "";
                }
                foreach (DictionaryEntry e in NickNamesHash)
                {
                    if (threadNumber == (int)e.Value)
                    {
                        return (string)e.Key;
                    }
                }
                return "";
            }

            private Hashtable NickNamesHash
            {
                get
                {
                    if (m_threadNickNames == null)
                    {
                        m_threadNickNames = new Hashtable();
                    }
                    return m_threadNickNames;
                }
            }

            private static bool IsNumber(string text)
            {
                int value;
                return Int32.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentUICulture, out value);
            }

            private Hashtable m_threadNickNames = null;
        }

        [
         CommandDescription(
           CommandName = "thread",
           MinimumAbbrev = 1,
           ShortHelp = "Displays active threads or switches to a specified thread",
           LongHelp = "Usage: thread -nick name\n    Assigns 'name' as Nickname to the currently active thread.  Nickname can be\n    used instead of thread name.  Nicknames cannot be numbers. If the current\n    thread already has some nickname assigned, old nickname is replaced with\n    new one. If the new nickname is \"\", the nickname for current thread is\n    deleted and no new nickname is assigned to the thread.  \"thread newThread\"\n    Sets the active thread to newThread. newThread can be either nickname for\n    the thread or thread number.  \"thread\" Displays all managed threads in the\n    current process.  Threads are identified by their thread number; if the\n    thread has assigned nick name, the nickname is displayed instead.\nSee Also:\n    suspend\n    resume"
         )
        ]
        public static void ThreadCmd(string arguments)
        {
            const string nickNameStr = "nick";
            ArgParser ap = new ArgParser(arguments, nickNameStr + ":1");
            if (ap.Count == 0)
            {
                if (ap.OptionPassed(nickNameStr))
                {
                    g_threadNickNames.SetThreadNickName(ap.GetOption(nickNameStr).AsString, Debugger.Processes.Active.Threads.Active);
                }
                else
                {
                    // we want to display active threads
                    MDbgProcess p = Debugger.Processes.Active;

                    WriteOutput("Active threads:");
                    foreach (MDbgThread t in p.Threads)
                    {
                        string stateDescription = GetThreadStateDescriptionString(t);
                        WriteOutput(string.Format("th #:{0} (ID:{1}){2}",
                                                  g_threadNickNames.GetThreadName(t),
                                                  t.Id,
                                                  stateDescription.Length == 0 ? String.Empty : " " + stateDescription));
                    }
                }
            }
            else
            {
                MDbgThread t = g_threadNickNames.GetThreadByNickName(ap.AsString(0));

                if (t == null)
                {
                    throw new MDbgShellException("No such thread");
                }
                Debugger.Processes.Active.Threads.Active = t;

                string currentThreadStateString = GetThreadStateDescriptionString(t);
                if (currentThreadStateString.Length > 0)
                    currentThreadStateString = currentThreadStateString.Insert(0, " ");

                WriteOutput(string.Format(CultureInfo.InvariantCulture, "Current thread is #{0}{1}.",
                                          t.Number, currentThreadStateString));
                Shell.DisplayCurrentLocation();
            }
        }

        private static string GetThreadStateDescriptionString(MDbgThread thread)
        {
            CorThread t = thread.CorThread;
            StringBuilder debuggerState = new StringBuilder();
            StringBuilder clientState = new StringBuilder();

            if (t.DebugState == CorDebugThreadState.THREAD_SUSPEND)
                debuggerState.Append("(SUSPENDED)");

            try
            {
                CorDebugUserState userState = t.UserState;

                if ((userState & CorDebugUserState.USER_SUSPENDED) != 0)
                    clientState.Append("user suspended");

                if ((userState & CorDebugUserState.USER_WAIT_SLEEP_JOIN) != 0)
                {
                    if (clientState.Length > 0)
                        clientState.Append(", ");
                    clientState.Append("waiting");
                }
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                if (e.ErrorCode != (int)HResult.CORDBG_E_BAD_THREAD_STATE)
                    throw;

                clientState.Append("in bad thread state");
            }

            StringBuilder result = debuggerState;
            if (clientState.Length > 0)
            {
                if (result.Length > 0)
                    result.Append(" ");
                result.Append("[").Append(clientState.ToString()).Append("]");
            }
            return result.ToString();
        }

        [
         CommandDescription(
           CommandName = "suspend",
           MinimumAbbrev = 2,
           ShortHelp = "Prevents thread from running",
           LongHelp = "Usage: suspend [-q] [*|[~]threadNumber]\n    Suspends current thread or thread specified by threadNumber.  If\n    threadNumber is specified as \"*\" then command applies to all threads.  If\n    thread number starts with \"~\", then the command applies to all threads\n    except one specified by number.  Suspended threads are excluded from\n    running when the process is let run by either \"go\", or \"step\" command.  If\n    there is no non-suspended thread in the process and the user issues \"go\"\n    command, the process doesn't continue.  In that case user has to issue\n    Ctrl-C to break into process and resume some threads by resume command.\n\n    If the -q option is passed, the command will not warn when a dangerous use of \n    the command is detected.\n\nSee Also:\n    resume\n    thread"
         )
        ]
        public static void SuspendCmd(string arguments)
        {
            string quietOpt = "q";
            ArgParser ap = new ArgParser(arguments, quietOpt);
            ThreadResumeSuspendHelper(ap, CorDebugThreadState.THREAD_SUSPEND, !ap.OptionPassed(quietOpt));
        }

        [
         CommandDescription(
           CommandName = "resume",
           MinimumAbbrev = 2,
           ShortHelp = "Resumes suspended thread",
           LongHelp = "Usage: resume [*|[~]threadNumber]\n    Resumes current thread or thread specified by threadNumber.  If\n    threadNumber is specified as \"*\" then command applies to all threads.  If\n    thread number starts with \"~\", then the command applies to all threads\n    except one specified by number.  Resumed thread is let run freely when the\n    user calls \"go\".  Current suspension status of the threads can be seen by\n    \"thread\" command.  Resuming non-suspended thread has no effect.\nSee Also:\n    suspend\n    thread"
         )
        ]
        public static void ResumeCmd(string arguments)
        {
            ArgParser ap = new ArgParser(arguments);
            ThreadResumeSuspendHelper(ap, CorDebugThreadState.THREAD_RUN, true);
        }

        // Helper to set the debug state and ignore certain errors. 
        // Throws on error.
        static void SetDebugStateWrapper(MDbgThread t, CorDebugThreadState newState, bool showWarnings)
        {
            if ((newState == CorDebugThreadState.THREAD_SUSPEND) &&
                ((t.CorThread.UserState & CorDebugUserState.USER_UNSAFE_POINT) != 0))
            {
                // Hard-suspending a thread while the thread is not at GC-safe point is bad.
                // If the users resumes a process and the GC triggers, the GC suspension logic
                // will not be able to suspend because it will wait forever on the thread that
                // is not at the GC safe point.  However, in this scenario, a debugger can always 
                // async-break and resume the suspended thread to avoid a live lock.  So we just
                // issue a warning here.
                if (showWarnings)
                {
                    WriteOutput("Warning: You are suspending Thread " + t.Number + " at an unsafe spot.");
                    WriteOutput("Until you resume this thread, the process may block if a garbage collection occurs.");
                    WriteOutput("To unblock the process, you need to asynchronously break the process (e.g. Ctrl-C) and resume the thread.");
                }
            }

            try
            {
                t.CorThread.DebugState = newState;
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                if (e.ErrorCode == (int)HResult.CORDBG_E_BAD_THREAD_STATE) // CORDBG_E_BAD_THREAD_STATE
                {
                    if (showWarnings)
                    {
                        WriteOutput(MDbgOutputConstants.Ignore, "Warning: Thread " + t.Number + " can't be set to " + newState);
                    }
                    return;   // thread is unavailable, ignore it
                }
                throw; // let error propogate up.
            }
        }
        private static void ThreadResumeSuspendHelper(ArgParser ap, CorDebugThreadState newState, bool showWarnings)
        {
            int threadNumber;

            if (ap == null)
            {
                throw new ArgumentException();
            }

            if (ap.Exists(0))
            {
                if (ap.AsString(0) == "*")
                {
                    // do an action on all threads
                    foreach (MDbgThread t in Debugger.Processes.Active.Threads)
                    {
                        SetDebugStateWrapper(t, newState, showWarnings);
                    }
                }
                else if (ap.AsString(0).StartsWith("~"))
                {
                    threadNumber = Int32.Parse(ap.AsString(0).Substring(1), CultureInfo.CurrentUICulture);
                    // it's ~number syntax -- do on all threads except this one.
                    foreach (MDbgThread t in Debugger.Processes.Active.Threads)
                    {
                        if (t.Number != threadNumber)
                        {
                            SetDebugStateWrapper(t, newState, showWarnings);
                        }
                    }
                }
                else
                {
                    MDbgThread t = g_threadNickNames.GetThreadByNickName(ap.AsString(0));
                    if (t == null)
                    {
                        throw new ArgumentException();
                    }
                    SetDebugStateWrapper(t, newState, showWarnings);
                }
            }
            else
            {
                SetDebugStateWrapper(Debugger.Processes.Active.Threads.Active, newState, showWarnings);
            }

        }

        [
         CommandDescription(
           CommandName = "intercept",
           MinimumAbbrev = 3,
           ShortHelp = "Intercepts the current exception at the given frame on the stack",
           LongHelp = "Usage: intercept FrameNumber\n    If the debugger has stopped because of an exception, you may use this\n    command to roll things back to the given frame number, potentially change\n    something using \"set\" for example, and continue with \"go\" hoping to avoid\n    the exception the second time around.\n"
         )
        ]
        public static void InterceptCmd(string arguments)
        {
            ArgParser ap = new ArgParser(arguments);

            if (ap.Count > 1)
            {
                throw new MDbgShellException("Wrong number of arguments.");
            }

            if (!ap.Exists(0))
            {
                throw new MDbgShellException("You must supply a frame number.");
            }
            int frameID = ap.AsInt(0);

            MDbgFrame f = Debugger.Processes.Active.Threads.Active.BottomFrame;

            while (--frameID >= 0)
            {
                f = f.NextUp;
                if (f == null)
                {
                    break;
                }
            }
            if (f == null)
            {
                throw new MDbgShellException("Invalid frame number.");
            }

            CorThread t = Debugger.Processes.Active.Threads.Active.CorThread;

            t.InterceptCurrentException(f.CorFrame);
            WriteOutput("Interception point is set.");
        }

        [
         CommandDescription(
           CommandName = "catch",
           MinimumAbbrev = 2,
           ShortHelp = "Set or display what events will be stopped on",
           LongHelp = "Usage: catch ex [FullyQualifiedExceptionType]\n    Example: catch ex System.ArgumentException    \n    This causes the debugger to break on all exceptions (Not just the unhandled\n    ones)\n    Use log command to log an event without stopping.\nSee Also:\n    ignore, log"
         )
        ]
        public static void CatchCmd(string arguments)
        {
            ModifyStopOptions(MDbgStopOptionPolicy.DebuggerBehavior.Stop, arguments);
        }

        [
         CommandDescription(
           CommandName = "ignore",
           MinimumAbbrev = 2,
           ShortHelp = "Set or display what events will be ignored",
           LongHelp = "Usage: ignore [event]\n    Opposite of catch. (\"help catch\" for more info)\nSee Also:\n    catch"
         )
        ]
        public static void IgnoreCmd(string arguments)
        {
            ModifyStopOptions(MDbgStopOptionPolicy.DebuggerBehavior.Ignore, arguments);
        }

        [
         CommandDescription(
           CommandName = "log",
           MinimumAbbrev = 3,
           ShortHelp = "Set or display what events will be logged",
           LongHelp = "Debugger will log the given event type but continue to run.\nThis uses the same event types as the catch and ignore commands.\nUse ignore command to clear log status.\n    \nUsage: log [event type]\n    Example: log ml    \n    This causes the debugger to log all module loads\n\nSee Also:\n    catch, ignore"
         )
        ]
        public static void LogCmd(string arguments)
        {
            ModifyStopOptions(MDbgStopOptionPolicy.DebuggerBehavior.Log, arguments);
        }

        [
         CommandDescription(
           CommandName = "enableNotification",
           MinimumAbbrev = 11,
           ShortHelp = "Enables or disables custom notifications for a given type",
           LongHelp = "Enables or disables custom debugger notifications for a given type (which must implement ICustomDebuggerNotification).       \nExample: enableNotification MyNamespace.myNotificationType 1\n"
         )
        ]
        ///<summary>
        /// enables or disables notifications for a particular type 
        /// </summary>
        /// <param name="argString">string including the fully qualified type name and a "1" or "0" to 
        /// indicate whether to enable or disable
        /// </param>
        public static void EnableCustomNotificationCmd(string argString)
        {
            ArgParser args = new ArgParser(argString);
            if (args.Count != 2)
            {
                throw new MDbgShellException("Expected 2 arguments");
            }

            string name = args.AsString(0);
            bool fEnable = args.AsBool(1);

            CorClass c = Debugger.Processes.Active.ResolveClass(name);
            Debugger.Processes.Active.CorProcess.SetEnableCustomNotification(c, fEnable);
            if (fEnable)
            {
                ModifyStopOptions(MDbgStopOptionPolicy.DebuggerBehavior.Notify, "cn");
            }

        }



        private static void DisplayStopOptions()
        {
            MDbgStopOptions stopOptions = Shell.Properties[MDbgStopOptions.PropertyName]
             as MDbgStopOptions;
            stopOptions.PrintOptions();
        }

        private static void ModifyStopOptions(MDbgStopOptionPolicy.DebuggerBehavior command, string arguments)
        {
            MDbgStopOptions stopOptions = Shell.Properties[MDbgStopOptions.PropertyName]
                as MDbgStopOptions;

            if (arguments.Length < 2)
            {
                DisplayStopOptions();
            }
            else
            {
                // Break up arguments string into the event type acronym and the arguments to 
                // send to the actual stop option policy. For example, if the arguments string
                // is "ex System.Exception System.ArgumentException", this will be split into:
                // eventType = "ex"
                // args = "System.Exception System.ArgumentException"
                // If there are no arguments to send to the stop option policy, args is set to null.
                string eventType = arguments.Split()[0].Trim();
                string args = null;
                if (arguments.Length > eventType.Length)
                {
                    args = arguments.Substring(eventType.Length).Trim();
                }
                stopOptions.ModifyOptions(eventType, command, args);
            }
        }

        private static void InitStopOptionsProperty()
        {
            MDbgStopOptions stopOptions = new MDbgStopOptions();
            Shell.Properties.Add(MDbgStopOptions.PropertyName, stopOptions);

            stopOptions.Add(new SimpleStopOptionPolicy("ml", "ModuleLoad"), ManagedCallbackType.OnModuleLoad);
            stopOptions.Add(new SimpleStopOptionPolicy("cl", "ClassLoad"), ManagedCallbackType.OnClassLoad);
            stopOptions.Add(new SimpleStopOptionPolicy("al", "AssemblyLoad"), ManagedCallbackType.OnAssemblyLoad);
            stopOptions.Add(new SimpleStopOptionPolicy("au", "AssemblyUnload"), ManagedCallbackType.OnAssemblyUnload);
            stopOptions.Add(new SimpleStopOptionPolicy("nt", "NewThread"), ManagedCallbackType.OnCreateThread);
            stopOptions.Add(new SimpleStopOptionPolicy("cn", "CustomNotification"), ManagedCallbackType.OnCustomNotification);
            stopOptions.Add(new SimpleStopOptionPolicy("lm", "LogMessage & MDAs"),
                new ManagedCallbackType[] { ManagedCallbackType.OnLogMessage, ManagedCallbackType.OnMDANotification });
            ExceptionStopOptionPolicy e = new ExceptionStopOptionPolicy();
            stopOptions.Add(e, ManagedCallbackType.OnException2);
            stopOptions.Add(e, ManagedCallbackType.OnExceptionUnwind2);

            ExceptionEnhancedStopOptionPolicy stopPolicy = new ExceptionEnhancedStopOptionPolicy(e);
            stopOptions.Add(stopPolicy, ManagedCallbackType.OnException2);
            stopOptions.Add(stopPolicy, ManagedCallbackType.OnExceptionUnwind2);
        }


        private static void InitModeShellProperty()
        {
            MDbgModeSettings modeSettings = new MDbgModeSettings();
            Shell.Properties.Add(MDbgModeSettings.PropertyName, modeSettings);

            GetModeValueEvent modeValueCallback = delegate(MDbgModeItem item)
            {
                switch (item.ShortCut)
                {
                    case "nc": return Debugger.Options.CreateProcessWithNewConsole;
                    case "if": return ShowInternalFrames;
                    case "ma": return Debugger.Options.ShowAddresses;
                    case "fp": return Debugger.Options.ShowFullPaths;
                    case "fe": return FailOnError;
                    case "ei": return CommandBase.ShowFullExceptionInfo;
                    default:
                        Debug.Assert(false, "invalid Shortcut name");
                        return false;
                }
            };
            ModeChangedEvent modeChangedCallback = delegate(MDbgModeItem item, bool onOff)
            {
                switch (item.ShortCut)
                {
                    case "nc":
                        Debugger.Options.CreateProcessWithNewConsole = onOff;
                        break;

                    case "if":
                        ShowInternalFrames = onOff;
                        break;

                    case "ma":
                        Debugger.Options.ShowAddresses = onOff;
                        break;

                    case "fp":
                        Debugger.Options.ShowFullPaths = onOff;
                        break;

                    case "fe":
                        FailOnError = onOff;
                        break;

                    case "ei":
                        CommandBase.ShowFullExceptionInfo = onOff;
                        break;

                    default:
                        Debug.Assert(false, "invalid Shortcut name");
                        break;
                }
            };

            modeSettings.Items.Add(new MDbgModeItem("nc", "Create process with new console",
                                                    modeValueCallback, modeChangedCallback));
            modeSettings.Items.Add(new MDbgModeItem("if", "Internal frames in call-stacks",
                                                    modeValueCallback, modeChangedCallback));
            modeSettings.Items.Add(new MDbgModeItem("ma", "Show memory addresses",
                                                    modeValueCallback, modeChangedCallback));
            modeSettings.Items.Add(new MDbgModeItem("fp", "Display full paths",
                                                    modeValueCallback, modeChangedCallback));
            modeSettings.Items.Add(new MDbgModeItem("fe", "Fail On Error",
                                                    modeValueCallback, modeChangedCallback));
            modeSettings.Items.Add(new MDbgModeItem("ei", "Exception Info",
                                                    modeValueCallback, modeChangedCallback));

        }

        [
         CommandDescription(
           CommandName = "mode",
           MinimumAbbrev = 2,
           ShortHelp = "Set/Query different debugger options",
           LongHelp = "Usage: mode [option on/off]\n    Set/Query different debugger options.  \"option\" should be a two-letter pair\n    from inside the (parentheses).    \nExample: mode nc on\n"
         )
        ]
        public static void ModeCmd(string arguments)
        {
            // get mode settings first
            MDbgModeSettings modeSettings = Shell.Properties[MDbgModeSettings.PropertyName]
                as MDbgModeSettings;
            Debug.Assert(modeSettings != null);
            if (modeSettings == null)
                throw new MDbgShellException("corrupted internal state.");


            ArgParser ap = new ArgParser(arguments);
            if (!ap.Exists(0))
            {
                WriteOutput("Debugging modes:");
                foreach (MDbgModeItem item in modeSettings.Items)
                {
                    WriteOutput(string.Format(CultureInfo.InvariantCulture, "({0}) {1}: {2}", item.ShortCut, item.Description.PadRight(50), item.OnOff ? "On" : "Off"));
                }
            }
            else
            {

                bool on = ap.AsCommand(1, new CommandArgument("on", "off")) == "on";
                string shortcut = ap.AsString(0);

                // now find the correct modeItem
                MDbgModeItem item = null;
                foreach (MDbgModeItem i in modeSettings.Items)
                    if (i.ShortCut == shortcut)
                    {
                        item = i;
                        break;
                    }
                if (item == null)
                    throw new MDbgShellException("Invalid mode option.  Modes are in (here).");

                item.OnOff = on;
            }
        }

        [
         CommandDescription(
           CommandName = "load",
           MinimumAbbrev = 2,
           ShortHelp = "Loads an extension from some assembly",
           LongHelp = "Usage: load assemblyName\n    Extension is loaded in this way; we load specified assembly and try to\n    execute the static method LoadExtension from type\n    Microsoft.Tools.Mdbg.Extension.Extension.    \nTry \"load gui\" for example.\n"
           ),

         SecurityPermissionAttribute(SecurityAction.Demand, Unrestricted = true) // we are loading custom assembly -- need high permissions
        ]
        public static void LoadCmd(string arguments)
        {
            ArgParser ap = new ArgParser(arguments);
            string extName = FindFilePath(ap.AsString(0)).ToLowerInvariant();

            if (extName == null || extName.Length < 1)
            {
                WriteOutput("Extension " + ap.AsString(0) + " was not found");
                return;
            }

            // For now, move this here since several test cases have a dependency on this line of text
            WriteOutput(MDbgOutputConstants.Ignore, "trying to load: " + extName);

            Assembly asext = LoadExtensionAssembly(extName);
            if (asext == null)
            {
                throw new MDbgShellException("Extension could not be loaded");
            }

            if (m_loadedExtensions.ContainsKey(extName) && m_loadedExtensions[extName])
            {
                // If we have already loaded the extension and it marked as active, then we need to unload it 
                // before continuing to reload it.
                ExecuteExtensionMethod(asext, "UnloadExtension");
                m_loadedExtensions[extName] = false;    // Make sure that we mark the extension as unloaded
            }

            ExecuteExtensionMethod(asext, "LoadExtension");

            if (m_loadedExtensions.ContainsKey(extName))
            {
                // We have loaded this extension before, just update the status
                Debug.Assert(!m_loadedExtensions[extName], "Extension already marked active");
                m_loadedExtensions[extName] = true;
            }
            else
            {
                // This is the first time this extension has loaded, create a new entry
                m_loadedExtensions.Add(extName, true);
            }
        }

        // Try and find the full path of the extension.
        static string FindFilePath(string extName)
        {
            Debug.Assert(extName != null);

            // Ensure we have a '.dll' extension.
            if (!extName.ToLower().EndsWith(".dll"))
            {
                extName += ".dll";
            }

            if (Path.IsPathRooted(extName) && File.Exists(extName))
            {
                // Supplied full path (eg "c:\test\MyExtension.dll"), try to load it straight up.
                return extName;
            }

            // Non rooted path. May be relative, or no name at all. eg:
            //   MyExt.dll
            //   mydir\subdir\MyExt.dll
            //   ..\..\dir\MyExt.dll
            //   .\MyExt.dll
            // Try each directory in our extension path
            foreach (string path in ExtensionPath.Split(Path.PathSeparator))
            {
                if (path.Length == 0)                  // skip empty dirs
                    continue;

                string expandedPath = Environment.ExpandEnvironmentVariables(path);
                string extPath = Path.Combine(expandedPath, extName);

                if (File.Exists(extPath))
                {
                    return extPath;
                }
            }

            if (File.Exists(Path.Combine(Environment.ExpandEnvironmentVariables("%SDK_ROOT%\\bin"), extName)))
            {
                return (Path.Combine(Environment.ExpandEnvironmentVariables("%SDK_ROOT%\\bin"), extName));
            }

            return "";    // not found
        }

        // Attempt to load the extention at the given path.  If no such file exists, returns null.
        // Throws on error.
        static Assembly LoadExtensionAssembly(string extensionPath)
        {
            if (File.Exists(extensionPath))
            {
                return Assembly.LoadFrom(extensionPath);
            }
            return null; // not found
        }

        // Given the assembly for an extension, find the main type in that assembly.
        // This will throw on error. It will not return null.
        static Type FindMainTypeInExtension(Assembly asext)
        {                   // 1) Try to load old-style extensions. 
            // These have goofy requirements that don't make sense for non-samples.
            {
                const string ExtNameSpace = "Microsoft.Samples.Tools.Mdbg.Extension.";
                string AssemblyName = asext.GetName().Name;
                AssemblyName = AssemblyName.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture) + AssemblyName.Substring(1, AssemblyName.Length - 1).ToLower(CultureInfo.InvariantCulture);
                string ExtType = ExtNameSpace + AssemblyName + "Extension";

                Type ext = asext.GetType(ExtType);
                if (ext != null)
                {
                    return ext;
                }
            }

            // 2) Try to load it as a new-style extension.
            // This just requires having some type with the [MDbgExtensionEntryPointClass] attribute.
            {
                // we'll try to scan also all the types in the extension to see if any of the types
                // has a custom attribute [MDbgExtensionEntryPointClass].
                foreach (Type t in asext.GetTypes())
                {
                    foreach (object o in t.GetCustomAttributes(false))
                    {
                        if (o is MDbgExtensionEntryPointClassAttribute)
                        {
                            return t;
                        }
                    }
                }
            }

            throw new MDbgShellException("Assembly is not in mdbg extension format.");
        }

        // Given an Assembly for an extension command, execute it.
        // This will throw if the extension is an invalid format.
        static void ExecuteExtensionMethod(Assembly asext, string methodName)
        {
            Debug.Assert(asext != null);
            Type ext = FindMainTypeInExtension(asext);

            // Now that we've found the type, execute the method.
            MethodInfo mi = ext.GetMethod(methodName);
            if (mi == null)
            {
                throw new MDbgShellException("Assembly is not in mdbg extension format. (missing method " + methodName + ")");
            }
            mi.Invoke(null, null);
        }

        [
         CommandDescription(
           CommandName = "unload",
           ShortHelp = "Unloads an extension",
           LongHelp = "Usage: unload assemblyName"
           ),
        ]
        public static void UnloadCmd(string arguments)
        {
            ArgParser ap = new ArgParser(arguments);
            string extName = FindFilePath(ap.AsString(0)).ToLowerInvariant();

            if (extName == null || extName.Length < 1)
            {
                WriteOutput("Extension " + ap.AsString(0) + " was not found");
                return;
            }
            else if (!m_loadedExtensions.ContainsKey(extName) || !m_loadedExtensions[extName])
            {
                // The extension has not been loaded, write message and return
                WriteOutput("Extension " + ap.AsString(0) + " is not currently loaded");
                return;
            }

            Assembly unloadAssembly = LoadExtensionAssembly(extName);
            if (unloadAssembly == null)
            {
                throw new MDbgException("Assembly could not be found for " + ap.AsString(0) + " extension");
            }

            try
            {
                // Execute UnloadExtension() and mark extension as unloaded
                ExecuteExtensionMethod(unloadAssembly, "UnloadExtension");
                m_loadedExtensions[extName] = false;
            }
            catch (MDbgShellException)
            {
                WriteError("The extension cannot be unloaded");
            }
        }

        [
         CommandDescription(
           CommandName = "print",
           MinimumAbbrev = 1,
           ShortHelp = "prints local or debug variables",
           LongHelp = "Usage: print [var] | [-d]\n    Either prints all variables in scope \"print\", the specified one \"print\n    var\", or debugger variables \"print -d\".\nSee Also:\n    set"
         )
        ]
        public static void PrintCmd(string arguments)
        {
            const string debuggerVarsOpt = "d";
            const string noFuncevalOpt = "nf";
            const string expandDepthOpt = "r";

            ArgParser ap = new ArgParser(arguments, debuggerVarsOpt + ";" + noFuncevalOpt + ";" + expandDepthOpt + ":1");
            bool canDoFunceval = !ap.OptionPassed(noFuncevalOpt);

            int? expandDepth = null;			    // we use optional here because
            // different codes bellow has different
            // default values.
            if (ap.OptionPassed(expandDepthOpt))
            {
                expandDepth = ap.GetOption(expandDepthOpt).AsInt;
                if (expandDepth < 0)
                    throw new MDbgShellException("Depth cannot be negative.");
            }

            MDbgFrame frame = Debugger.Processes.Active.Threads.Active.CurrentFrame;
            if (ap.OptionPassed(debuggerVarsOpt))
            {
                // let's print all debugger variables
                MDbgProcess p = Debugger.Processes.Active;
                foreach (MDbgDebuggerVar dv in p.DebuggerVars)
                {
                    MDbgValue v = new MDbgValue(p, dv.CorValue);
                    WriteOutput(dv.Name + "=" + v.GetStringValue(expandDepth == null ? 0 : (int)expandDepth,
                                                              canDoFunceval));
                }
            }
            else
            {
                if (ap.Count == 0)
                {
                    // get all active variables
                    MDbgFunction f = frame.Function;

                    ArrayList vars = new ArrayList();
                    MDbgValue[] vals = f.GetActiveLocalVars(frame);
                    if (vals != null)
                    {
                        vars.AddRange(vals);
                    }

                    vals = f.GetArguments(frame);
                    if (vals != null)
                    {
                        vars.AddRange(vals);
                    }
                    foreach (MDbgValue v in vars)
                    {
                        WriteOutput(v.Name + "=" + v.GetStringValue(expandDepth == null ? 0 : (int)expandDepth,
                                                                 canDoFunceval));
                    }
                }
                else
                {
                    // user requested printing of specific variables
                    for (int j = 0; j < ap.Count; ++j)
                    {
                        MDbgValue var = Debugger.Processes.Active.ResolveVariable(ap.AsString(j), frame);
                        if (var != null)
                        {
                            WriteOutput(ap.AsString(j) + "=" + var.GetStringValue(expandDepth == null ? 1
                                : (int)expandDepth, canDoFunceval));
                        }
                        else
                        {
                            throw new MDbgShellException("Variable not found");
                        }
                    }
                }
            }
        }

        [
         CommandDescription(
           CommandName = "funceval",
           MinimumAbbrev = 1,
           ShortHelp = "Evaluates a given function outside normal program flow",
           LongHelp = "Usage: funceval [-ad Num] functionName [args ... ]\n    Performs a function evaluation on the current active thread.  The function\n    to evaluate is functionName.  Function needs to be	fully qualified\n    including namespaces.  Optional -ad parameter can specify what appdomain\n    space should be used for resolution of the function.  If the -ad switch is\n    not specified it is assumed that the appdomain for resolution is same as\n    where the thread that is used for function evaluation is located.  If the\n    function that is being evaluated is not static, the first parameter passed\n    in should be \"this\".  Arguments to the function evaluation are looked-up\n    through all appdomains.  If you request some value from a certain\n    appdomain, the variable should be prefixed with module and appdomain\n    name.    \n    Example: funceval -ad 0 System.Object.ToString\n    hello.exe#0!MyClass.g_rootRef    \n    The command above will evaluate function System.Object.ToString in the\n    application domain 0. Since ToString is instance function, the first\n    parameter needs to be a this pointer -- in the example above we used\n    MyClass.g_rootRef. In more user friendly debugger this funceval command\n    could be executed by typing:	MyClass.g_rootRef.ToString();\n"
         )
        ]
        public static void FuncEvalCmd(string arguments)
        {
            const string appDomainOption = "ad";
            ArgParser ap = new ArgParser(arguments, appDomainOption + ":1");
            if (!(ap.Count >= 1))
            {
                throw new MDbgShellException("Not Enough arguments");
            }

            // Currently debugger picks first function -- we have not implementing resolving overloaded functions.
            // Good example is Console.WriteLine -- there is 18 different types:
            // 1) [06000575] Void WriteLine()
            // 2) [06000576] Void WriteLine(Boolean)
            // 3) [06000577] Void WriteLine(Char)
            // 4) [06000578] Void WriteLine(Char[])
            // 5) [06000579] Void WriteLine(Char[], Int32, Int32)
            // 6) [0600057a] Void WriteLine(Decimal)
            // 7) [0600057b] Void WriteLine(Double)
            // 8) [0600057c] Void WriteLine(Single)
            // 9) [0600057d] Void WriteLine(Int32)
            // 10) [0600057e] Void WriteLine(UInt32)
            // 11) [0600057f] Void WriteLine(Int64)
            // 12) [06000580] Void WriteLine(UInt64)
            // 13) [06000581] Void WriteLine(Object)
            // 14) [06000582] Void WriteLine(String)
            // 15) [06000583] Void WriteLine(String, Object)
            // 16) [06000584] Void WriteLine(String, Object, Object)
            // 17) [06000585] Void WriteLine(String, Object, Object, Object)
            // 18) [06000586] Void WriteLine(String, Object, Object, Object, Object, ...)
            // 19) [06000587] Void WriteLine(String, Object[])
            //
            CorAppDomain appDomain;
            if (ap.OptionPassed(appDomainOption))
            {
                MDbgAppDomain ad = Debugger.Processes.Active.AppDomains[ap.GetOption(appDomainOption).AsInt];
                if (ad == null)
                {
                    throw new ArgumentException("Invalid Appdomain Number");
                }
                appDomain = ad.CorAppDomain;
            }
            else
            {
                appDomain = Debugger.Processes.Active.Threads.Active.CorThread.AppDomain;
            }

            MDbgFunction func = Debugger.Processes.Active.ResolveFunctionNameFromScope(ap.AsString(0), appDomain);
            if (null == func)
            {
                throw new MDbgShellException(String.Format(CultureInfo.InvariantCulture, "Could not resolve {0}", new Object[] { ap.AsString(0) }));
            }

            CorEval eval = Debugger.Processes.Active.Threads.Active.CorThread.CreateEval();

            // Get Variables
            ArrayList vars = new ArrayList();
            String arg;
            for (int i = 1; i < ap.Count; i++)
            {
                arg = ap.AsString(i);

                CorValue v = Shell.ExpressionParser.ParseExpression2(arg, Debugger.Processes.Active,
                    Debugger.Processes.Active.Threads.Active.CurrentFrame);

                if (v == null)
                {
                    throw new MDbgShellException("Cannot resolve expression or variable " + ap.AsString(i));
                }

                if (v is CorGenericValue)
                {
                    vars.Add(v as CorValue);
                }

                else
                {
                    CorHeapValue hv = v.CastToHeapValue();
                    if (hv != null)
                    {
                        // we cannot pass directly heap values, we need to pass reference to heap valus
                        CorReferenceValue myref = eval.CreateValue(CorElementType.ELEMENT_TYPE_CLASS, null).CastToReferenceValue();
                        myref.Value = hv.Address;
                        vars.Add(myref);
                    }
                    else
                    {
                        vars.Add(v);
                    }
                }

            }

            eval.CallFunction(func.CorFunction, (CorValue[])vars.ToArray(typeof(CorValue)));
            Debugger.Processes.Active.Go().WaitOne();

            // now display result of the funceval
            if (!(Debugger.Processes.Active.StopReason is EvalCompleteStopReason))
            {
                // we could have received also EvalExceptionStopReason but it's derived from EvalCompleteStopReason
                WriteOutput("Func-eval not fully completed and debuggee has stopped");
                WriteOutput("Result of funceval won't be printed when finished.");
            }
            else
            {
                eval = (Debugger.Processes.Active.StopReason as EvalCompleteStopReason).Eval;
                Debug.Assert(eval != null);

                CorValue cv = eval.Result;
                if (cv != null)
                {
                    MDbgValue mv = new MDbgValue(Debugger.Processes.Active, cv);
                    WriteOutput("result = " + mv.GetStringValue(1));
                    if (cv.CastToReferenceValue() != null)
                        if (Debugger.Processes.Active.DebuggerVars.SetEvalResult(cv))
                            WriteOutput("results saved to $result");
                }
            }
            Shell.DisplayCurrentLocation();
        }

        [
         CommandDescription(
           CommandName = "newobj",
           MinimumAbbrev = 4,
           ShortHelp = "Creates new object of type typeName",
           LongHelp = "Usage: newobj typeName [arguments...]\n    Creates a new object of type typeName.\nSee Also:\n    funceval"
         )
        ]
        public static void NewObjCmd(string arguments)
        {
            ArgParser ap = new ArgParser(arguments);

            string className = ap.AsString(0);
            MDbgFunction func = Debugger.Processes.Active.ResolveFunctionName(null, className, ".ctor", Debugger.Processes.Active.Threads.Active.CorThread.AppDomain);
            if (null == func)
                throw new MDbgShellException(String.Format(CultureInfo.InvariantCulture, "Could not resolve {0}", ap.AsString(0)));

            CorEval eval = Debugger.Processes.Active.Threads.Active.CorThread.CreateEval();

            ArrayList callArguments = new ArrayList();
            // parse the arguments to newobj
            int i = 1;
            while (ap.Exists(i))
            {
                string arg = ap.AsString(i);
                // this is a normal argument
                MDbgValue rsMVar = Debugger.Processes.Active.ResolveVariable(arg,
                                                                             Debugger.Processes.Active.Threads.Active.CurrentFrame);
                if (rsMVar == null)
                {
                    // cordbg supports also limited literals -- currently only NULL & I4.
                    if (string.Compare(arg, "null", true) == 0)
                    {
                        callArguments.Add(eval.CreateValue(CorElementType.ELEMENT_TYPE_CLASS, null));
                    }
                    else
                    {
                        int v;
                        if (!Int32.TryParse(arg, out v))
                            throw new MDbgShellException(string.Format(CultureInfo.InvariantCulture, "Argument '{0}' could not be resolved to variable or number",
                                                                       arg));

                        CorGenericValue gv = eval.CreateValue(CorElementType.ELEMENT_TYPE_I4, null).CastToGenericValue();
                        Debug.Assert(gv != null);
                        gv.SetValue(v);
                        callArguments.Add(gv);
                    }
                }
                else
                {
                    callArguments.Add(rsMVar.CorValue);
                }
                ++i;
            }

            eval.NewParameterizedObject(func.CorFunction, null, (CorValue[])callArguments.ToArray(typeof(CorValue)));
            Debugger.Processes.Active.Go().WaitOne();

            // now display result of the funceval
            if (!(Debugger.Processes.Active.StopReason is EvalCompleteStopReason))
            {
                // we could have received also EvalExceptionStopReason but it's derived from EvalCompleteStopReason
                WriteOutput("Newobj command not fully completed and debuggee has stopped");
                WriteOutput("Result of Newobj won't be printed when finished.");
            }
            else
            {
                eval = (Debugger.Processes.Active.StopReason as EvalCompleteStopReason).Eval;
                Debug.Assert(eval != null);

                CorValue cv = eval.Result;
                if (cv != null)
                {
                    MDbgValue mv = new MDbgValue(Debugger.Processes.Active, cv);
                    WriteOutput("result = " + mv.GetStringValue(1));
                    if (Debugger.Processes.Active.DebuggerVars.SetEvalResult(cv))
                        WriteOutput("results saved to $result");
                }
            }
            Shell.DisplayCurrentLocation();
        }

        [
         CommandDescription(
           CommandName = "set",
           MinimumAbbrev = 3,
           ShortHelp = "Sets a variable to a new value",
           LongHelp = "Usage: set variable=value\n    You may alter the value of any in scope variable.  You may also create your\n    own \"debugger\" variables and assign to them reference values from within\n    your application.  These values will act as handles to the original value,\n    even when the real one is out of scope.  All such \"debugger\" variables must\n    begin with \"$\", for example \"$var\".  Clear these handles by setting them to\n    nothing, like this: \"set $var=\"\nSee Also:\n    print"
         )
        ]
        public static void SetCmd(string arguments)
        {
            // Arguments has to be in the form of variable=varName, variable=value or variable=(<type>)value, 
            // where we use the ldasm naming convention (e.g. "int", "sbyte", "ushort", etc...) for <type>.
            // Example inputs: var=myInt, var=45, var=(long)45
            int idx = arguments.IndexOf('=');
            if (idx == -1)
            {
                throw new MDbgShellException("Wrong arguments.");
            }

            string varName = arguments.Substring(0, idx).Trim();
            string value = arguments.Substring(idx + 1).Trim();

            MDbgValue lsMVar = null;
            MDbgDebuggerVar lsDVar = null;
            if (varName.StartsWith("$"))
            {
                //variable is a debugger variable
                lsDVar = Debugger.Processes.Active.DebuggerVars[varName];
            }
            else
            {
                //variable is a program variable
                lsMVar = Debugger.Processes.Active.ResolveVariable(varName,
                                           Debugger.Processes.Active.Threads.Active.CurrentFrame);
                if (lsMVar == null)
                {
                    throw new MDbgShellException("Cannot resolve variable " + varName);
                }
            }

            if (value.Length == 0)
            {
                if (varName.StartsWith("$"))
                {
                    Debugger.Processes.Active.DebuggerVars.DeleteVariable(varName);
                    return;
                }
                else
                {
                    throw new MDbgShellException("Missing value");
                }
            }


            CorValue val = Shell.ExpressionParser.ParseExpression2(value, Debugger.Processes.Active,
                    Debugger.Processes.Active.Threads.Active.CurrentFrame);

            if (val == null)
            {
                throw new MDbgShellException("cannot resolve value or expression " + value);
            }
            CorGenericValue valGeneric = val as CorGenericValue;
            bool bIsReferenceValue = val is CorReferenceValue;

            if (lsDVar != null)
            {
                //variable is a debugger variable
                if ((valGeneric != null) || bIsReferenceValue)
                {
                    lsDVar.Value = val;
                }

                else
                {
                    CorHeapValue rsHeapVal = val.CastToHeapValue();
                    if (rsHeapVal != null)
                    {
                        lsDVar.Value = rsHeapVal;
                    }
                    else
                    {
                        lsDVar.Value = val.CastToReferenceValue();
                    }
                }
            }

            else if (lsMVar != null)
            {
                //variable is a program variable
                if (valGeneric != null)
                {
                    CorValue lsVar = lsMVar.CorValue;
                    if (lsVar == null)
                    {
                        throw new MDbgShellException("cannot set constant values to unavailable variables");
                    }

                    // val is a primitive value                    
                    CorGenericValue lsGenVal = lsVar.CastToGenericValue();
                    if (lsGenVal == null)
                    {
                        throw new MDbgShellException("cannot set constant values to non-primitive values");
                    }
                    try
                    {
                        // We want to allow some type coercion. Eg, casting between integer precisions.
                        lsMVar.Value = val; // This may do type coercion
                    }
                    catch (MDbgValueWrongTypeException)
                    {
                        throw new MDbgShellException(String.Format("Type mismatch. Can't convert from {0} to {1}", val.Type, lsGenVal.Type));
                    }
                }
                else if (bIsReferenceValue)
                {
                    //reget variable
                    lsMVar = Debugger.Processes.Active.ResolveVariable(varName,
                                               Debugger.Processes.Active.Threads.Active.CurrentFrame);
                    lsMVar.Value = val;
                }
                else
                {
                    if (val.CastToHeapValue() != null)
                    {
                        throw new MDbgShellException("Heap values should be assigned only to debugger variables");
                    }
                    if (val.CastToGenericValue() != null)
                    {
                        lsMVar.Value = val.CastToGenericValue();
                    }
                    else
                    {
                        lsMVar.Value = val.CastToReferenceValue();
                    }
                }
            }


            // as a last thing we do is to print new value of the variable
            lsMVar = Debugger.Processes.Active.ResolveVariable(varName,
                                       Debugger.Processes.Active.Threads.Active.CurrentFrame);
            WriteOutput(varName + "=" + lsMVar.GetStringValue(1));
        }

        [
         CommandDescription(
           CommandName = "list",
           MinimumAbbrev = 1,
           ShortHelp = "Displays loaded modules appdomains or assemblies",
           LongHelp = "Usage: list [modules|appdomains|assemblies]\n    Displays loaded modules appdomains or assemblies\n"
         )
        ]
        public static void ListCmd(string arguments)
        {
            const string verboseOpt = "v";
            bool bVerbose;
            ArgParser ap = new ArgParser(arguments, verboseOpt);
            string listWhat = ap.AsCommand(0, new CommandArgument("modules", "appdomains", "assemblies"));
            switch (listWhat)
            {
                case "modules":
                    bVerbose = ap.OptionPassed(verboseOpt);
                    if (ap.Exists(1))
                    {
                        // user specified module to display info for
                        MDbgModule m = Debugger.Processes.Active.Modules.Lookup(ap.AsString(1));
                        if (m == null)
                        {
                            throw new MDbgShellException("No such module.");
                        }
                        ListModuleInternal(m, true);
                    }
                    else
                    {
                        // we list all modules
                        WriteOutput("Loaded Modules:");
                        foreach (MDbgModule m in Debugger.Processes.Active.Modules)
                        {
                            ListModuleInternal(m, bVerbose);
                        }
                    }
                    break;
                case "appdomains":
                    WriteOutput("Current appDomains:");
                    foreach (MDbgAppDomain ad in Debugger.Processes.Active.AppDomains)
                    {
                        WriteOutput(ad.Number + ". - " + ad.CorAppDomain.Name);
                    }
                    break;

                case "assemblies":
                    WriteOutput("Current assemblies:");
                    foreach (MDbgAppDomain ad in Debugger.Processes.Active.AppDomains)
                    {
                        foreach (CorAssembly assem in ad.CorAppDomain.Assemblies)
                        {
                            WriteOutput("\t" + assem.Name);
                        }
                    }

                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private static void ListModuleInternal(MDbgModule module, bool verbose)
        {
            string symbolsStatus = " (no symbols loaded)";

            // There's no guarantee that we can read syms on a dump.
            try
            {
                if (module.SymReader != null)
                {
                    symbolsStatus = String.Empty;
                }
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                // Nothing to be done for it when the target can't find memory; just swallow the exception.
                if (e.ErrorCode != (int)HResult.E_PARTIAL_COPY)
                {
                    throw;
                }
            }

            CorAppDomain ad = module.CorModule.Assembly.AppDomain;
            int adNumber = Debugger.Processes.Active.AppDomains.Lookup(ad).Number;
            if (verbose)
            {
                WriteOutput(string.Format(CultureInfo.InvariantCulture, ":{0}\t{1}#{2} {3}", module.Number, module.CorModule.Name, adNumber, symbolsStatus));
            }
            else
            {
                string moduleBaseName;
                try
                {
                    moduleBaseName = System.IO.Path.GetFileName(module.CorModule.Name);
                }
                catch
                {
                    moduleBaseName = module.CorModule.Name;
                }

                WriteOutput(string.Format(CultureInfo.InvariantCulture, ":{0}\t{1}#{2} {3}", module.Number, moduleBaseName, adNumber, symbolsStatus));
            }
        }

        [
         CommandDescription(
           CommandName = "symbol",
           MinimumAbbrev = 2,
           ShortHelp = "Sets/Displays path or Reloads/Lists symbols",
           LongHelp = "Usage: symbol commandName [commandParameters]\n    symbol path [path_value]\n        Displays or sets current symbol path to path_value.\n\n    symbol addpath path_value \n        Adds an extra path to the current symbol path.\n\n    symbol reload [moduleName] \n        Reloads symbols for all modules (if none is specified) or\n        just for the module moduleName.\n\n    symbol list [moduleName] \n        Shows currently loaded symbols for either all modules\n        or the module moduleName if specified.\n\nSee Also:\n    path\n"
         )
        ]
        /*
         * We want to have following commnads:
         *
         * symbol path "value"    -- sets symbol paths
         * symbol addpath "value" -- adds symbol path
         * symbol reload [module] -- reloads symbol for a module
         * symbol list [module]   -- shows currently loaded symbols
         */
        public static void SymbolCmd(string arguments)
        {
            ArgParser ap = new ArgParser(arguments);
            if (!ap.Exists(0))
            {
                ExecuteCommand("help symbol");
                return;
            }
            switch (ap.AsCommand(0, new CommandArgument("path", "addpath", "reload", "list")))
            {
                case "path":
                    if (!ap.Exists(1))
                    {
                        // we want to print current path
                        string p = Debugger.Options.SymbolPath;
                        WriteOutput("Current symbol path: " + p);
                    }
                    else
                    {
                        // we are setting path
                        Debugger.Options.SymbolPath = ap.AsString(1);
                        WriteOutput("Current symbol path: " + Debugger.Options.SymbolPath);
                    }
                    break;

                case "addpath":
                    Debugger.Options.SymbolPath = Debugger.Options.SymbolPath + Path.PathSeparator + ap.AsString(1);
                    WriteOutput("Current symbol path: " + Debugger.Options.SymbolPath);
                    break;

                case "reload":
                    {
                        IEnumerable modules;
                        if (ap.Exists(1))
                        {
                            // we want to reload only one module

                            MDbgModule m = Debugger.Processes.Active.Modules.Lookup(ap.AsString(1));
                            if (m == null)
                            {
                                throw new MDbgShellException("No such module.");
                            }
                            modules = new MDbgModule[] { m };
                        }
                        else
                        {
                            modules = Debugger.Processes.Active.Modules;
                        }

                        foreach (MDbgModule m in modules)
                        {
                            WriteOutput("Reloading symbols for module " + m.CorModule.Name);
                            m.ReloadSymbols(true);
                            WriteModuleStatus(m, true);
                        }
                    }
                    break;
                case "list":
                    {
                        IEnumerable modules;
                        if (ap.Exists(1))
                        {
                            // we want to list only one module
                            MDbgModule m = Debugger.Processes.Active.Modules.Lookup(ap.AsString(1));
                            if (m == null)
                            {
                                throw new MDbgShellException("No such module.");
                            }
                            modules = new MDbgModule[] { m };
                        }
                        else
                        {
                            modules = Debugger.Processes.Active.Modules;
                        }

                        foreach (MDbgModule m in modules)
                        {
                            WriteModuleStatus(m, false);
                        }
                    }
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private static void WriteModuleStatus(MDbgModule module, bool reloadMode)
        {
            string symbolLocation = null;

            bool bHaveSyms = module.SymReader != null;
            if (bHaveSyms)
            {
                symbolLocation = module.SymbolFilename;
                if (symbolLocation == null || symbolLocation.Length == 0)
                    symbolLocation = "<loaded from unknown location>";
            }
            string outputString;
            if (reloadMode)
            {
                if (bHaveSyms)
                    outputString = string.Format(CultureInfo.InvariantCulture, "Symbols loaded from: {0}\n",
                                                 symbolLocation);
                else
                    outputString = "No symbols could be loaded.\n";
            }
            else
            {
                outputString = string.Format(CultureInfo.InvariantCulture, "Module: {0}\nSymbols: {1}\n",
                                             module.CorModule.Name,
                                             bHaveSyms ? symbolLocation : "<no available>");
            }

            WriteOutput(outputString);
        }


        [
         CommandDescription(
           CommandName = "processenum",
           MinimumAbbrev = 3,
           ShortHelp = "Displays active processes",
           LongHelp = "Usage: processenum\n    Displays active processes\nSee Also:\n    attach"
         )
        ]
        public static void ProcessEnumCmd(string arguments)
        {

            WriteOutput("Active processes on current machine:");
            foreach (Process p in Process.GetProcesses())
            {

                if (Process.GetCurrentProcess().Id == p.Id)  // let's hide our process
                {
                    continue;
                }

                //list the loaded runtimes in each process, if the ClrMetaHost APIs are available
                CLRMetaHost mh = null;
                try
                {
                    mh = new CLRMetaHost();
                }
                catch (System.EntryPointNotFoundException)
                {
                    // Intentionally ignore failure to find GetCLRMetaHost().
                    // Downlevel we don't have one.
                    continue;
                }

                IEnumerable<CLRRuntimeInfo> runtimes = null;
                try
                {
                    runtimes = mh.EnumerateLoadedRuntimes(p.Id);
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    if ((e.NativeErrorCode != 0x0) &&           // The operation completed successfully.
                        (e.NativeErrorCode != 0x3f0) &&         // An attempt was made to reference a token that does not exist.
                        (e.NativeErrorCode != 0x5) &&           // Access is denied.
                        (e.NativeErrorCode != 0x57) &&          // The parameter is incorrect.
                        (e.NativeErrorCode != 0x514) &&         // Not all privileges or groups referenced are assigned to the caller.
                        (e.NativeErrorCode != 0x12))            // There are no more files.
                    {
                        // Unknown/unexpected failures should be reported to the user for diagnosis.
                        WriteOutput("Error retrieving loaded runtime information for PID " + p.Id
                            + ", error " + e.ErrorCode + " (" + e.NativeErrorCode + ") '" + e.Message + "'");
                    }

                    // If we failed, don't try to print out any info.
                    if ((e.NativeErrorCode != 0x0) || (runtimes == null))
                    {
                        continue;
                    }
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    if (e.ErrorCode != (int)HResult.E_PARTIAL_COPY)  // Only part of a ReadProcessMemory or WriteProcessMemory request was completed.
                    {
                        // Unknown/unexpected failures should be reported to the user for diagnosis.
                        WriteOutput("Error retrieving loaded runtime information for PID " + p.Id
                            + ", error " + e.ErrorCode + "\n" + e.ToString());
                    }

                    continue;
                }

                //if there are no runtimes in the target process, don't print it out
                if (!runtimes.GetEnumerator().MoveNext())
                {
                    continue;
                }

                WriteOutput("(PID: " + p.Id + ") " + p.MainModule.FileName);
                foreach (CLRRuntimeInfo rti in runtimes)
                {
                    WriteOutput("\t" + rti.GetVersionString());
                }
            }
        }

        [
         CommandDescription(
            CommandName = "clearException",
            MinimumAbbrev = 2,
            ShortHelp = "Clears the current exception",
            LongHelp = "Usage: clearException"
         )
        ]
        public static void ClearExceptionCmd(string arguments)
        {
            Debugger.Processes.Active.Threads.Active.CorThread.ClearCurrentException();
        }

        [
         CommandDescription(
           CommandName = "opendump",
           MinimumAbbrev = 8,
           IsRepeatable = false,
           ShortHelp = "Opens the specified dump file for debugging.",
           LongHelp = "Usage: OpenDump <path>"
         )
        ]
        public static void OpendumpCmd(string arguments)
        {
            const string pathArg = "path";
            ArgParser ap = new ArgParser(arguments, pathArg + ":1");

            if (ap.Count > 1)
            {
                throw new MDbgShellException("Wrong # of arguments.");
            }

            if (!ap.Exists(0) && !ap.OptionPassed(pathArg))
            {
                throw new MDbgShellException("Specify a dump file to open");
            }

            string path;
            if (ap.Exists(0))
                path = ap.AsString(0);
            else
                path = ap.GetOption(pathArg).AsString;

            DumpReader dump = new DumpReader(path);
            MDbgProcess process = Debugger.Processes.CreateProcess();

            // An exception partway through attaching can leave the debugger in an odd in-between state.
            // We need to attempt to detach.
            bool success = false;
            try
            {
                process.AttachToDump(dump, null);
                success = true;
            }
            finally
            {
                // Fault handler (emulate with finally & success bool since C# doesn't support fault blocks)
                // Detach on failure so we're not left with a partially attached process
                if (!success)
                    process.Detach();
            }

            WriteOutput("DBI path: " + ((LibraryProvider)process.LibraryProvider).LastLoadedDbi);
            // If there's an exception of interest stored in the dump, use it.
            // Otherwise, fall back on going to the first thread with managed code.
            if (dump.IsExceptionStream())
            {
                uint TID = dump.ExceptionStreamThreadId();
                WriteOutput("OS TID from last exception in dump was 0n" + TID);
                MDbgThread thread = process.Threads.GetThreadFromThreadId((int)TID);

                if (null == thread)
                {
                    WriteOutput("Could not find a managed thread corresponding to native TID!\n"
                        + "This should indicate that the last event was a native event on an unmanaged thread.\n"
                        );
                }
                else
                {
                    process.Threads.Active = thread;
                    WriteOutput("Active thread set to " + process.Threads.Active.Id);
                }
            }
            else
            {
                WriteOutput("No exception in dump, current thread will be chosen randomly.");
                // Set the currently active thread to the first thread we find with managed code on it.
                bool foundThread = false;
                for (int i = 0; i < process.Threads.Count && !foundThread; i++)
                {
                    foreach (MDbgFrame frame in process.Threads[i].Frames)
                    {
                        if (frame != null && frame.IsManaged)
                        {
                            process.Threads.Active = process.Threads[i];
                            foundThread = true;
                        }
                    }
                }
                if (!foundThread)
                {
                    WriteOutput("Warning: couldn't find thread with managed frame at base in dump");
                }
            }

            // This can fail silently if we can't walk the first frame of the stack.
            process.AsyncStop();
            process.Threads.Active.InvalidateStackWalker();
            WriteOutput("Dump loaded successfully.");
        }

        [
         CommandDescription(
           CommandName = "attach",
           MinimumAbbrev = 1,
           IsRepeatable = false,
           ShortHelp = "Attaches to a process or prints available processes",
           LongHelp = "Usage: attach [-ver version_string] [pid]\n    Attaches to a process or prints available processes\n\nThe -ver flag allows you to explicitly specify a version string for the\n    runtime hosted in the target process that you wish to debug.\n\nSee Also:\n    detach\n    processenum"
         )
        ]
        public static void AttachCmd(string arguments)
        {
            const string versionArg = "ver";
            const string continuationEventArg = "attachEvent";
            const string pidArg = "pid";
            ArgParser ap = new ArgParser(arguments, versionArg + ":1;" + continuationEventArg + ":1;" + pidArg + ":1");

            if (ap.Count > 1)
            {
                throw new MDbgShellException("Wrong # of arguments.");
            }

            if (!ap.Exists(0) && !ap.OptionPassed(pidArg))
            {
                WriteOutput("Please choose some process to attach");
                ProcessEnumCmd("");
                return;
            }

            int pid;
            if (ap.Exists(0))
            {
                pid = ap.AsInt(0);
                if (ap.OptionPassed(pidArg))
                {
                    WriteOutput("Do not specify pid option when also passing pid as last argument");
                    return;
                }
            }
            else
            {
                Debug.Assert(ap.OptionPassed(pidArg)); // verified above
                pid = ap.GetOption(pidArg).AsInt;
            }


            //
            // Do some sanity checks to give useful end-user errors.
            // 


            // Can't attach to ourselves!
            if (Process.GetCurrentProcess().Id == pid)
            {
                throw new MDbgShellException("Cannot attach to myself!");
            }

            // Can't attach to a process that we're already debugging.
            // ICorDebug may enforce this, but the error may not be very descriptive for an end-user.
            // For example, ICD may propogate an error from the OS, and the OS may return
            // something like AccessDenied if another debugger is already attached.
            // This only checks for cases where this same instance of MDbg is already debugging
            // the process of interest. 
            foreach (MDbgProcess procOther in Debugger.Processes)
            {
                if (pid == procOther.CorProcess.Id)
                {
                    throw new MDbgShellException("Can't attach to process " + pid + " because it's already being debugged");
                }
            }

            // Get the OS handle if there was one
            SafeWin32Handle osEventHandle = null;
            if (ap.OptionPassed(continuationEventArg))
            {
                osEventHandle = new SafeWin32Handle(new IntPtr(ap.GetOption(continuationEventArg).AsHexOrDecInt));
            }

            // determine the version to attach to
            string version = null;
            if (ap.OptionPassed(versionArg))
            {
                version = ap.GetOption(versionArg).AsString;
            }
            else
            {
                version = MdbgVersionPolicy.GetDefaultAttachVersion(pid);
            }
            if (version == null)
            {
                throw new MDbgShellException("Can't determine what version of the CLR to attach to in process " +
                    pid + ". Use -ver to specify a version");
            }

            // attach
            MDbgProcess p;
            p = Debugger.Attach(pid, osEventHandle, version);

            p.Go().WaitOne();

            if (osEventHandle != null)
            {
                osEventHandle.Dispose();
            }
        }

        [
         CommandDescription(
           CommandName = "detach",
           MinimumAbbrev = 2,
           ShortHelp = "Detaches from debugged process",
           LongHelp = "Usage: detach\n    Detaches from debugged process\nSee Also:\n    attach"
         )
        ]
        public static void DetachCmd(string arguments)
        {
            MDbgProcess active = Debugger.Processes.Active;
            active.Breakpoints.DeleteAll();

            active.Detach();

            // We can't wait for targets that never run (e.g. NoninvasiveStopGoController against a dump)
            if (active.CanExecute())
                active.StopEvent.WaitOne();
        }

        [
         CommandDescription(
           CommandName = "up",
           MinimumAbbrev = 1,
           ShortHelp = "Moves the active stack frame up",
           LongHelp = "Usage: up [frames]\n    Moves the active stack frame up\nSee Also:\n    down"
         )
        ]
        public static void UpCmd(string arguments)
        {
            string frameNum = "f";
            ArgParser ap = new ArgParser(arguments, frameNum);

            if (ap.OptionPassed(frameNum))
            {
                SwitchToFrame(ap.AsInt(0));
            }
            else
            {
                int count = 1;
                if (ap.Exists(0))
                {
                    count = ap.AsInt(0);
                }
                while (--count >= 0)
                {
                    Debugger.Processes.Active.Threads.Active.MoveCurrentFrame(false);
                }
            }
            if (Debugger.Processes.Active.Threads.Active.CurrentFrame.IsManaged)
            {
                WriteOutput("Current Frame:" +
                            Debugger.Processes.Active.Threads.Active.CurrentFrame.Function.FullName
                            );
            }
            Shell.DisplayCurrentLocation();
        }


        [
         CommandDescription(
           CommandName = "down",
           MinimumAbbrev = 1,
           ShortHelp = "Moves the active stack frame down",
           LongHelp = "Usage: down [frames]\n    Moves the active stack frame down\nSee Also:\n    up"
         )
        ]
        public static void DownCmd(string arguments)
        {
            string frameNum = "f";
            ArgParser ap = new ArgParser(arguments, frameNum);
            if (ap.OptionPassed(frameNum))
            {
                SwitchToFrame(ap.AsInt(0));
            }
            else
            {
                int count = 1;
                if (ap.Exists(0))
                {
                    count = ap.AsInt(0);
                }
                while (--count >= 0)
                {
                    Debugger.Processes.Active.Threads.Active.MoveCurrentFrame(true);
                }
            }

            if (Debugger.Processes.Active.Threads.Active.CurrentFrame.IsManaged)
            {
                WriteOutput("Current Frame:" +
                            Debugger.Processes.Active.Threads.Active.CurrentFrame.Function.FullName
                            );
            }
            Shell.DisplayCurrentLocation();
        }

        private static void SwitchToFrame(int frameNum)
        {
            if (frameNum < 0)
                throw new ArgumentException("Invalid frame number.");

            Debug.Assert(frameNum >= 0);
            int idx = 0;
            foreach (MDbgFrame f in Debugger.Processes.Active.Threads.Active.Frames)
            {
                if (f.IsInfoOnly)
                    continue;

                if (frameNum == idx)
                {
                    // we want to switch to this frame
                    Debugger.Processes.Active.Threads.Active.CurrentFrame = f;
                    return;
                }
                ++idx;
            }
            throw new MDbgShellException("No such frame");
        }

        [
         CommandDescription(
           CommandName = "path",
           MinimumAbbrev = 2,
           ShortHelp = "Sets or displays current source path",
           LongHelp = "Usage: path [pathName]\n    This path will be searched for the source files if the location in the\n    binaries is not available.\nSee Also:\n    symbol"
         )
        ]
        public static void PathCmd(string arguments)
        {
            ArgParser ap = new ArgParser(arguments);
            if (!ap.Exists(0))
            {
                WriteOutput("path: " + Shell.FileLocator.Path);
            }
            else
            {
                Shell.FileLocator.Path = Environment.ExpandEnvironmentVariables(arguments);
                WriteOutput("Path set to: " + arguments);

                if (Debugger.Processes.HaveActive)
                {
                    ShowCmd("");
                }
            }
        }


        private class MdbgSymbol
        {
            public MdbgSymbol(int moduleNumber, string className, string method, int offset)
            {
                Debug.Assert(className != null & method != null);

                ModuleNumber = moduleNumber;
                ClassName = className;
                Method = method;
                Offset = offset;
            }

            public int ModuleNumber;
            public string ClassName;
            public string Method;
            public int Offset;
        }

        private class MDbgSymbolCache
        {
            public MdbgSymbol Retrieve(int symbolNumber)
            {
                if (symbolNumber < 0 || symbolNumber >= m_list.Count)
                {
                    throw new ArgumentException();
                }
                return (MdbgSymbol)m_list[symbolNumber];
            }

            public void Clear()
            {
                m_list.Clear();
            }

            public int Add(MdbgSymbol symbol)               // will return symbol id.
            {
                m_list.Add(symbol);
                return m_list.Count - 1;
            }

            ArrayList m_list = new ArrayList();
        }

        [
         CommandDescription(
           CommandName = "x",
           ShortHelp = "Displays functions in a module",
           LongHelp = "Usage: x [-c numSymbols] [module[!pattern]]\n    Displays functions matching [pattern] for a module.  If numSymbols is\n    provided, the output is limited to the given number.  If !regex is not\n    provided, all functions are displayed.  If module is not provided either,\n    all loaded modules are displayed.  Symbols (~#) may be used to set\n    breakpoints using the \"break\" command.    \nExample: x mscorlib!*String*\nSee Also:\n    break"
         )
        ]
        public static void XCmd(string arguments)
        {
            if (arguments.Length == 0)
            {
                WriteOutput("Please specify module.");
                ListCmd("mo");
            }
            else
            {
                const int default_count = 100; // default number of frames to print

                const string countOpt = "c";

                ArgParser ap = new ArgParser(arguments, countOpt + ":1");
                int count = default_count;
                if (ap.OptionPassed(countOpt))
                {
                    ArgToken countArg = ap.GetOption(countOpt);
                    if (countArg.AsString == "all")
                    {
                        count = 0; // 0 means print all symbols
                    }
                    else
                    {
                        count = countArg.AsInt;
                        if (count <= 0)
                        {
                            throw new MDbgShellException("Count must be positive number or string \"all\"");
                        }
                    }
                }

                string moduleName, substrPart;

                string expr = ap.AsString(0);
                int i = expr.IndexOf('!');
                if (i == -1)
                {
                    moduleName = expr;
                    substrPart = null;
                }
                else
                {
                    moduleName = expr.Substring(0, i);
                    substrPart = expr.Substring(i + 1);
                }

                SymbolCache.Clear();
                // enum functions from the module
                MDbgModule m = Debugger.Processes.Active.Modules.Lookup(moduleName);
                if (m == null)
                {
                    throw new MDbgShellException("module not found!");
                }

                bool shouldPrint = substrPart == null;
                Regex r = null;
                if (substrPart != null)
                {
                    r = new Regex(ConvertSimpleExpToRegExp(substrPart));
                }

                foreach (Type t in m.Importer.DefinedTypes)
                {
                    foreach (MethodInfo mi in t.GetMethods())
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(t.Name).Append(".").Append(mi.Name).Append("(");
                        bool needComma = false;
                        foreach (ParameterInfo pi in mi.GetParameters())
                        {
                            if (needComma)
                            {
                                sb.Append(",");
                            }
                            sb.Append(pi.Name);
                            needComma = true;
                        }
                        sb.Append(")");
                        string fullFunctionName = sb.ToString();
                        if (r != null)
                        {
                            shouldPrint = r.IsMatch(fullFunctionName);
                        }
                        if (shouldPrint)
                        {
                            int idx = SymbolCache.Add(new MdbgSymbol(m.Number, t.Name, mi.Name, 0));
                            WriteOutput("~" + idx + ". " + fullFunctionName);
                            if (count != 0 && idx >= count)
                            {
                                WriteOutput(string.Format(CultureInfo.CurrentUICulture, "displayed only first {0} hits. For more symbols use -c switch", count));
                                return;
                            }
                        }
                    }
                }
            }
        }

        // converts a dos-like regexp to true regular expresion.
        // This enables simple filters for types as e.g.:
        // x mod!System.String*
        //
        // currently function supports just 2 special chars: * (match
        // 0-unlim chars) and ? (match 1 char).
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

        [
         CommandDescription(
           CommandName = "aprocess",
           MinimumAbbrev = 2,
           ShortHelp = "Switches to another debugged process or prints available ones",
           LongHelp = "Usage: aprocess [number]\n    Switches to another debugged process or prints available processes.  The\n    numbers are not real PIDs but a 0-indexed list.\n"
         )
        ]
        public static void ActiveProcess(string arguments)
        {
            ArgParser ap = new ArgParser(arguments);
            if (ap.Count > 1)
            {
                throw new MDbgShellException("Wrong # of arguments.");
            }

            if (ap.Exists(0))
            {
                int logicalPID = ap.AsInt(0);
                bool found = false;
                foreach (MDbgProcess ps in Debugger.Processes)
                    if (ps.Number == logicalPID)
                    {
                        Debugger.Processes.Active = ps;
                        found = true;
                        break;
                    }
                if (found)
                {
                    Shell.DisplayCurrentLocation();
                }
                else
                {
                    throw new MDbgShellException("Invalid process number");
                }
            }
            else
            {
                MDbgProcess ActiveProcess = Debugger.Processes.HaveActive ? Debugger.Processes.Active : null;

                WriteOutput("Active Process:");
                bool haveProcesses = false;

                CorPublish corPublish = null;
                foreach (MDbgProcess p in Debugger.Processes)
                {
                    haveProcesses = true;
                    string processName = p.Name;
                    string launchMode;
                    if (processName == null)
                    {
                        // in case we're attached (as opposed to launching),
                        // we don't know process name.
                        // Let's find it through CorPublishApi
                        try
                        {
                            if (corPublish == null)
                            {
                                corPublish = new CorPublish();
                            }
                            processName = corPublish.GetProcess(p.CorProcess.Id).DisplayName;
                        }
                        catch
                        {
                            processName = "N/A";
                        }
                        launchMode = "attached";
                    }
                    else
                    {
                        launchMode = "launched";
                    }
                    WriteOutput((ActiveProcess == p ? "*" : " ") + string.Format(CultureInfo.InvariantCulture, "{0}. [PID: {1}, {2}] {3}", p.Number, p.CorProcess.Id, launchMode, processName));
                }
                if (!haveProcesses)
                {
                    WriteOutput("No Active Process!");
                }
            }
        }

        [
         CommandDescription(
           CommandName = "foreach",
           MinimumAbbrev = 2,
           ShortHelp = "Executes other command on all threads",
           LongHelp = "Usage: foreach [OtherCommand]\n    Where \"OtherCommand\" is a valid command that operates on one thread,\n    \"foreach OtherCommand\" will do the same thing to all threads.\n"
         )
        ]
        public static void ForEachCmd(string arguments)
        {
            if (arguments.Length != 0)
            {
                MDbgProcess p = Debugger.Processes.Active;
                MDbgThreadCollection c = p.Threads;

                // Remember current thread so that we can restore it.
                MDbgThread tOriginal = c.HaveActive ? c.Active : null;
                foreach (MDbgThread t in c)
                {
                    try
                    {
                        p.Threads.Active = t;
                    }
                    catch (System.Runtime.InteropServices.COMException e)
                    {
                        // we'll ignore neutered threads -- they mean that the threads
                        // just got destroyed and we cannot make them active.
                        if (e.ErrorCode != (int)HResult.CORDBG_E_OBJECT_NEUTERED)
                        {
                            throw;
                        }
                    }
                    ExecuteCommand(arguments);
                }

                // Restore back to original thread.
                if (tOriginal != null)
                {
                    p.Threads.Active = tOriginal;
                }
            }
            else
            {
                ExecuteCommand("? foreach");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////
        //
        // When command implementation
        //
        //////////////////////////////////////////////////////////////////////////////////


        private class ExecuteCmdAction
        {
            public ExecuteCmdAction(IMDbgShell mdbg,
                                    string actionString,
                                    Type stopReasonType)
            {
                Debug.Assert(mdbg != null);
                Debug.Assert(actionString != null);
                Debug.Assert(stopReasonType != null);
                m_mdbg = mdbg;
                m_actionString = actionString;
                m_stopReasonType = stopReasonType;
                m_id = g_id++;
            }

            public int Id
            {
                get
                {
                    return m_id;
                }
            }

            public bool IsRunning;

            public string Name
            {
                get
                {
                    return m_name;
                }
                set
                {
                    Debug.Assert(value != null);
                    m_name = value;
                    bHaveCondition = true;
                }
            }
            public int Number
            {
                get
                {
                    return m_number;
                }
                set
                {
                    m_number = value;
                    bHaveCondition = true;
                }
            }

            public bool ShouldExecuteNow
            {
                get
                {
                    if (!m_mdbg.Debugger.Processes.HaveActive)
                    {
                        if (m_stopReasonType.Name == "ProcessExitedStopReason")
                        {
                            // special case when process exits
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    Object stopReason = m_mdbg.Debugger.Processes.Active.StopReason;
                    bool correctStopReason = m_stopReasonType == stopReason.GetType();
                    if (!correctStopReason)
                    {
                        return false;
                    }
                    if (!bHaveCondition)
                    {
                        return true;
                    }

                    if (m_stopReasonType == typeof(ThreadCreatedStopReason))
                    {
                        //execute on ThreadCreated [num] do
                        return Number == (stopReason as ThreadCreatedStopReason).Thread.Number;
                    }

                    else if (m_stopReasonType == typeof(BreakpointHitStopReason))
                    {
                        //execute on BreakpointHit [num] do
                        return Number == (stopReason as BreakpointHitStopReason).Breakpoint.Number;
                    }

                    else if (m_stopReasonType == typeof(ModuleLoadedStopReason))
                    {
                        //execute on ModuleLoaded [name] do
                        return IdentifierEquals(Name, Path.GetFileName((stopReason as ModuleLoadedStopReason).
                                                                      Module.CorModule.Name));
                    }
                    else if (m_stopReasonType == typeof(ClassLoadedStopReason))
                    {
                        //execute on ClassLoaded [name] do
                        throw new NotImplementedException();
                    }

                    else if (m_stopReasonType == typeof(AssemblyLoadedStopReason))
                    {
                        //execute on AssemblyLoaded [name] do 
                        return IdentifierEquals(Name, Path.GetFileName((stopReason as AssemblyLoadedStopReason).
                                                                      Assembly.Name));
                    }

                    else if (m_stopReasonType == typeof(AssemblyUnloadedStopReason))
                    {
                        //execute on AssemblyUnloaded [name] do
                        throw new NotImplementedException();
                    }

                    else if (m_stopReasonType == typeof(ExceptionThrownStopReason))
                    {
                        //execute on ExceptionThrown [name] do
                        MDbgValue ex = Debugger.Processes.Active.Threads.Active.CurrentException;
                        return IdentifierEquals(Name, ex.TypeName);
                    }
                    else if (m_stopReasonType == typeof(UnhandledExceptionThrownStopReason))
                    {
                        //execute on UnhandledExceptionThrown [name] do
                        MDbgValue ex = Debugger.Processes.Active.Threads.Active.CurrentException;
                        return IdentifierEquals(Name, ex.TypeName);
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                    return false;
                }
            }

            // returns wheather conditionString matches identifier.
            // currently legal operation for conditionString are:
            //     identifier     - matches identifier with the same name
            //     !identifier    - matches identifier that is of different name than identifier
            private bool IdentifierEquals(string conditionString, string identifier)
            {
                bool negation = conditionString.StartsWith("!");
                if (negation)
                {
                    conditionString = conditionString.Substring(1); // get rid of !
                }

                bool match = String.Compare(conditionString, identifier,
                                            true, //ignoreCase
                                            CultureInfo.InvariantCulture
                                            ) == 0;
                if (negation)
                {
                    match = !match;
                }

                return match;
            }

            public void Execute()
            {
                string[] cmds = m_actionString.Split(new char[] { '\n' });  // this will split string by "\n".
                int i;
                for (i = 0; i < cmds.Length; i++)
                {
                    CommandBase.ExecuteCommand(cmds[i]);
                }
            }


            public override string ToString()
            {
                string condition = "";
                if (bHaveCondition)
                {
                    switch (m_stopReasonType.Name)
                    {
                        case "BreakpointHitStopReason":
                        case "ThreadCreatedStopReason":
                            condition = "Number=" + Number;
                            break;

                        case "ModuleLoadedStopReason":
                        case "ClassLoadStopReason":
                        case "AssemblyLoadedStopReason":
                        case "AssemblyUnloadedStopReason":
                        case "ExceptionThrownStopReason":
                        case "UnhandledExceptionThrownStopReason":
                            condition = "Name=\"" + Name + "\"";
                            break;

                        default:
                            condition = "unknown";
                            Debug.Assert(false);
                            break;
                    }
                    condition = " (" + condition + ")";
                }

                string astring = m_actionString.Replace("\n", ";");
                return "when " + m_stopReasonType.Name.Replace("StopReason", "") + condition + " do: " + astring;
            }

            private Type m_stopReasonType;
            private IMDbgShell m_mdbg;
            private string m_actionString;

            private bool bHaveCondition;
            private string m_name;
            private int m_number;

            private static int g_id = 0;
            private int m_id;
        }

        [
         CommandDescription(
           CommandName = "when",
           MinimumAbbrev = 4,
           ShortHelp = "Execute commands based on debugger event",
           LongHelp = "Usage: when\n    when    \n        displays currently active when statements    \n        \n    when delete all | num [num [num ...]]    \n        deletes when statement specified by number (or all if \"all\"\n    specified)    \n        \n    when stopReason [specific_condition] do cmd [cmd [cmd ...] ]    \n        stopReason can be:    \n    StepComplete, ProcessExited, ThreadCreated, BreakpointHit, ModuleLoaded,\n    ClassLoaded, AssemblyLoaded, AssemblyUnloaded, ControlCTrapped,\n    ExceptionThrown, UnhandledExceptionThrown, AsyncStop, AttachComplete,\n    UserBreak, EvalComplete, EvalException, RemapOpportunityReached,\n    NativeStop    \n        \n        specific_condition can be:    \n    number - for ThreadCreated,BreakpointHit event triggers action only when\n    stopped by thread Id/breakpoint number with same value.    \n    [!]name for ModuleLoaded,ClassLoaded,AssemblyLoaded,\n    AssemblyUnloaded,ExceptionThrown, UnhandledExceptionThrown trigger action\n    only when name matches (not matches if prefix !) with the name of the\n    StopReason    \n        \n        specific_condition has to be empty for other stopReasons.\n        \n        \n    Examples:    \n        when ExceptionThrown !System.SystemException do go Continues on every\n    1st exception, unless that exception is SystemException.  (note you have to\n    have \"catch ex\" to enable exception notifications)    \n        \nwhen ProcessExited do quit    \n    Exits mdbg when debugged process quits\n"
         )
        ]
        public static void WhenCmd(string arguments)
        {
            ArgParser ap = new ArgParser(arguments);
            if (ap.Count == 0)
            {
                // we want to list all actions
                foreach (ExecuteCmdAction a in m_events)
                {
                    WriteOutput(a.Id + ".\t" + a.ToString());
                }
            }
            else if (ap.AsString(0) == "delete")
            {
                if (ap.AsString(1) == "all")
                {
                    // delete all actions
                    m_events.Clear();
                }
                else
                {
                    int idx = 1;
                    while (true)
                    {
                        int actionToRemove = ap.AsInt(idx);

                        foreach (ExecuteCmdAction a in m_events)
                        {
                            if (a.Id == actionToRemove)
                            {
                                m_events.Remove(a);
                                break; // once we remove an item, we cannot iterate further,
                                // this doesn't matter because id's are unique.
                            }
                        }
                        idx++;
                        if (!ap.Exists(idx))
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                //we want to create an when action
                ExecuteCmdAction action = null;
                string cmdString;
                int argCount;
                GetDoPart(ap, out cmdString, out argCount);
                switch (ap.AsString(0))
                {
                    case "StepComplete":
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(StepCompleteStopReason));
                        switch (argCount)
                        {
                            case 0:
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        break;
                    case "ProcessExited":
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(ProcessExitedStopReason));
                        switch (argCount)
                        {
                            case 0:
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        break;
                    case "ThreadCreated":
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(ThreadCreatedStopReason));
                        switch (argCount)
                        {
                            case 0:
                                break;
                            case 1:
                                action.Number = ap.AsInt(1);
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        break;
                    case "BreakpointHit":
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(BreakpointHitStopReason));
                        switch (argCount)
                        {
                            case 0:
                                break;
                            case 1:
                                action.Number = ap.AsInt(1);
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        break;
                    case "ModuleLoaded":
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(BreakpointHitStopReason));
                        switch (argCount)
                        {
                            case 0:
                                break;
                            case 1:
                                action.Name = ap.AsString(1);
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        break;
                    case "ClassLoaded":
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(ClassLoadedStopReason));
                        switch (argCount)
                        {
                            case 0:
                                break;
                            case 1:
                                action.Name = ap.AsString(1);
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        break;
                    case "AssemblyLoaded":
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(AssemblyLoadedStopReason));
                        switch (argCount)
                        {
                            case 0:
                                break;
                            case 1:
                                action.Name = ap.AsString(1);
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        break;
                    case "AssemblyUnloaded":
                        throw new NotImplementedException();

                    case "ControlCTrapped":
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(ControlCTrappedStopReason));
                        switch (argCount)
                        {
                            case 0:
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        break;
                    case "ExceptionThrown":
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(ExceptionThrownStopReason));
                        switch (argCount)
                        {
                            case 0:
                                break;
                            case 1:
                                action.Name = ap.AsString(1);
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        break;
                    case "UnhandledExceptionThrown":
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(UnhandledExceptionThrownStopReason));
                        switch (argCount)
                        {
                            case 0:
                                break;
                            case 1:
                                action.Name = ap.AsString(1);
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        break;
                    case "AsyncStop":
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(AsyncStopStopReason));
                        switch (argCount)
                        {
                            case 0:
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        break;
                    case "AttachComplete":
                        if (argCount != 0)
                        {
                            throw new ArgumentException();
                        }
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(AttachCompleteStopReason));
                        break;
                    case "UserBreak":
                        if (argCount != 0)
                        {
                            throw new ArgumentException();
                        }
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(UserBreakStopReason));
                        break;
                    case "EvalComplete":
                        if (argCount != 0)
                        {
                            throw new ArgumentException();
                        }
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(EvalCompleteStopReason));
                        break;
                    case "EvalException":
                        if (argCount != 0)
                        {
                            throw new ArgumentException();
                        }
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(EvalExceptionStopReason));
                        break;
                    case "RemapOpportunityReached":
                        if (argCount != 0)
                        {
                            throw new ArgumentException();
                        }
                        action = new ExecuteCmdAction(Shell, cmdString, typeof(RemapOpportunityReachedStopReason));
                        break;
                    default:
                        throw new ArgumentException("invalid event name");
                }
                m_events.Add(action);
            }
        }

        private static void GetDoPart(ArgParser ap, out string cmdString, out int argCount)
        {
            const int startPart = 1;
            Debug.Assert(ap != null);
            // all commands start with "when XXX [arguments...] do cmds
            int i;
            for (i = startPart; i < ap.Count; i++)
            {
                if (ap.AsString(i) == "do")
                {
                    break;
                }
            }
            if (i == ap.Count)
            {
                throw new ArgumentException("Invalid command syntax");
            }
            StringBuilder sb = new StringBuilder();
            int j = i + 1;
            sb.Append(ap.AsString(j));
            for (j++; j < ap.Count; j++)
            {
                sb.Append("\n").Append(ap.AsString(j));
            }
            cmdString = sb.ToString();
            argCount = i - startPart;
        }


        internal static void WhenHandler(object sender, CommandExecutedEventArgs e)
        {
            if (!e.MovementCommand)
            {
                return;
            }
            foreach (ExecuteCmdAction a in m_events)
            {
                if (!a.IsRunning && a.ShouldExecuteNow)
                {
                    try
                    {
                        a.IsRunning = true;
                        WriteOutput("exec> " + a.ToString());
                        a.Execute();
                    }
                    finally
                    {
                        a.IsRunning = false;
                    }
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////
        //
        // end of When command implementation
        //
        //////////////////////////////////////////////////////////////////////////////////

        [
         CommandDescription(
           CommandName = "echo",
           ShortHelp = "Echoes a message to the console",
           LongHelp = "Usage: echo message\n    Echoes a message to the console\n"
         )
        ]
        public static void EchoCmd(string arguments)
        {
            WriteOutput(arguments);
        }

        [
         CommandDescription(
           CommandName = "uwgchandle",
           MinimumAbbrev = 4,
           ShortHelp = "Prints the object tracked by a GC handle",
           LongHelp = "Usage: uwgchandle [var] | [address]\n    Prints the variable tracked by a handle. The handle can be specified by\n    name or address.\n"
         )
        ]
        public static void UnwrapGCHandleCmd(string arguments)
        {
            ArgParser ap = new ArgParser(arguments);
            if (ap.Count != 1)
            {
                WriteError("Wrong arguments, should be name or address of a \"System.Runtime.InteropServices.GCHandle\" object.");
                return;
            }

            long handleAdd = 0;

            // First try to resolve the argument as a variable in the current frame
            MDbgValue var = Debugger.Processes.Active.ResolveVariable(
                                                                      ap.AsString(0),
                                                                      Debugger.Processes.Active.Threads.Active.CurrentFrame);
            if (var != null)
            {
                if (var.TypeName != "System.Runtime.InteropServices.GCHandle")
                {
                    WriteError("Variable is not of type \"System.Runtime.InteropServices.GCHandle\".");
                    return;
                }

                foreach (MDbgValue field in var.GetFields())
                {
                    if (field.Name == "m_handle")
                    {
                        handleAdd = Int64.Parse(field.GetStringValue(0));
                        break;
                    }
                }
            }
            else
            {
                // Trying to resolve as a raw address now
                try
                {
                    handleAdd = ap.GetArgument(0).AsAddress;
                }
                catch (System.FormatException)
                {
                    WriteError("Couldn't recognize the argument as a variable name or address");
                    return;
                }
            }

            IntPtr add = new IntPtr(handleAdd);
            CorReferenceValue result;

            try
            {
                result = Debugger.Processes.Active.CorProcess.GetReferenceValueFromGCHandle(add);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                if (e.ErrorCode == (int)HResult.CORDBG_E_BAD_REFERENCE_VALUE)
                {
                    WriteError("Invalid handle address.");
                    return;
                }
                else
                {
                    throw;
                }
            }

            CorValue v = result.Dereference();
            MDbgValue mv = new MDbgValue(Debugger.Processes.Active, v);
            if (mv.IsComplexType)
            {
                WriteOutput(string.Format("GCHandle to <{0}>",
                                          InternalUtil.PrintCorType(Debugger.Processes.Active, v.ExactType)));

                // now print fields as well
                foreach (MDbgValue f in mv.GetFields())
                    CommandBase.WriteOutput(" " + f.Name + "=" + f.GetStringValue(0));
            }
            else
            {
                WriteOutput(string.Format("GCHandle to {0}", mv.GetStringValue(0)));
            }
        }

        [
         CommandDescription(
           CommandName = "config",
           MinimumAbbrev = 4,
           ShortHelp = "Sets or Displays debugger configurable options",
           LongHelp = "Usage: config [option value]\n    When invoked without arguments the command displays all configurable\n    options and how they are set. If the option is specified, it is set to\n    value.    \n    Currently available options are:\n    extpath:  set path where extensions are searched for when load command is\n    used.\n    extpath+: adds path to the existing paths where extensions can be loaded\n    from.\nSee Also:\n    symbol\n    path"
         )
        ]
        public static void ConfigCmd(string arguments)
        {
            const string extPathCmd = "extpath";
            const string extPathAddCmd = "extpath+";

            ArgParser ap = new ArgParser(arguments);
            if (!ap.Exists(0))
            {
                WriteOutput("Current configuration:");
                WriteOutput(string.Format("\tExtensionPath={0}", ExtensionPath));
                return;
            }

            switch (ap.AsCommand(0, new CommandArgument(extPathCmd, extPathAddCmd)))
            {
                case extPathCmd:
                    ExtensionPath = ap.AsString(1);
                PrintExt:
                    WriteOutput(string.Format("ExtensionPath={0}", ExtensionPath));
                    break;
                case extPathAddCmd:
                    ExtensionPath = ExtensionPath + Path.PathSeparator + ap.AsString(1);
                    goto PrintExt;
                default:
                    Debug.Assert(false);
                    break;
            }
        }


        [CommandDescription(
          CommandName = "printexception",
          MinimumAbbrev = 6,
          ShortHelp = "Prints the last exception on the current thread",
           LongHelp = "Usage: printexception [-r]\n    Prints the last exception on the current thread.  PrintException -r also prints all InnerExceptions."
          )
        ]
        public static void PrintExceptCmd(string args)
        {
            string recurse = args;

            MDbgThread currentThread = Debugger.Processes.Active.Threads.Active;
            if (currentThread == null)
            {
                WriteOutput("There is no active thread set!");
            }

            MDbgValue ex = currentThread.CurrentException;
            if (ex == null || ex.IsNull)
            {
                WriteOutput("There is no exception on the current thread.");
                return;
            }

            // Print the current exception
            WriteOutput("Exception thrown:\n" + ex.TypeName +
                "\n at function:\n  " +
                currentThread.CurrentFrame.Function.FullName +
                (currentThread.CurrentSourcePosition == null ? "" :
                "\n in source file:\n  " +
                currentThread.CurrentSourcePosition.Path + ":" + currentThread.CurrentSourcePosition.Line) +
                (ex.GetField("_message").IsNull ? "" :
                    "\n Message:\n " + ex.GetField("_message").GetStringValue(false)));

            // Print Inner Exceptions?
            if (args == "-r")
            {
                ex = ex.GetField("_innerException");
                for (uint i = 1; !ex.IsNull; ex = ex.GetField("_innerException"), i++)
                {
                    WriteOutput("\nInner Exception " + i + ":\n " +
                        ex.TypeName +
                        (ex.GetField("_exceptionMethodString").IsNull ? "" :
                            "\n  at function:\n   " + ex.GetField("_exceptionMethodString")) +
                        (ex.GetField("_source").IsNull ? "" :
                            "\n  in source file:\n   " + ex.GetField("_source")) +
                        (ex.GetField("_message").IsNull ? "" :
                            "\n Message:\n " + ex.GetField("_message").GetStringValue(false)));
                }
            }
            WriteOutput("");
        }


        [
         CommandDescription(
           CommandName = "monitorInfo",
           MinimumAbbrev = 3,
           ShortHelp = "Displays object monitor lock information",
           LongHelp = "Usage: monitorInfo [var]\n    Displays monitor lock information for the object referenced by variable var\n See Also:\n    blockingObjects"
         )
        ]
        public static void MonitorInfoCmd(string arguments)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                WriteOutput("Usage: monitorInfo [var]");
                return;
            }
            CorValue v = Shell.ExpressionParser.ParseExpression2(arguments, Debugger.Processes.Active,
                Debugger.Processes.Active.Threads.Active.CurrentFrame);

            CorReferenceValue refVal = v.CastToReferenceValue();
            if (refVal == null)
                throw new MDbgException(arguments + " is not a reference value");
            CorHeapValue heapVal = refVal.Dereference().CastToHeapValue();
            CorThread owner = heapVal.GetThreadOwningMonitorLock();
            int acquisitionCount = heapVal.GetMonitorAcquisitionCount();
            WriteOutput("Monitor Address: " + heapVal.Address);
            if (owner == null)
            {
                Debug.Assert(acquisitionCount == 0);
                WriteOutput("Unowned");
            }
            else
            {
                WriteOutput("Owner thread id:  " + owner.Id);
                WriteOutput("AcquisitionCount: " + acquisitionCount);
            }

            MDbgThread[] critSecWaitList = GetThreadsBlockedEnteringMonitor(heapVal);
            WriteOutput("Critical section wait list (unordered):");
            if (critSecWaitList.Length != 0)
            {
                for (int i = 0; i < critSecWaitList.Length; i++)
                {
                    WriteOutput("  " + critSecWaitList[i].CorThread.Id);
                }
            }
            else
            {
                WriteOutput("  <empty>");
            }

            CorThread[] waitList = heapVal.GetMonitorEventWaitList();
            WriteOutput("Event wait list:");
            if (waitList.Length != 0)
            {
                for (int i = 0; i < waitList.Length; i++)
                {
                    WriteOutput("  " + waitList[i].Id);
                }
            }
            else
            {
                WriteOutput("  <empty>");
            }
        }

        /// <summary>
        /// Scans all threads to generate the list of threads blocked on the monitor critical section
        /// for an object
        /// </summary>
        private static MDbgThread[] GetThreadsBlockedEnteringMonitor(CorHeapValue heapVal)
        {
            List<MDbgThread> blockedThreads = new List<MDbgThread>();
            foreach (MDbgThread t in Debugger.Processes.Active.Threads)
            {
                CorBlockingObject[] blockingObjects = t.CorThread.GetBlockingObjects();
                foreach (CorBlockingObject b in blockingObjects)
                {
                    if (b.BlockingReason == CorDebugBlockingReason.MonitorCriticalSection &&
                        b.BlockingObject.Address == heapVal.Address)
                    {
                        blockedThreads.Add(t);
                    }
                }
            }
            return blockedThreads.ToArray();
        }

        [
          CommandDescription(
            CommandName = "blockingObjects",
            MinimumAbbrev = 5,
            ShortHelp = "Displays any monitor locks blocking threads",
            LongHelp = "Usage: blockingObjects\n    Displays any monitor locks blocking threads.\nSee Also:\n    monitorInfo"
          )
        ]
        public static void BlockingObjectsCmd(string arguments)
        {
            foreach (MDbgThread t in Debugger.Processes.Active.Threads)
            {
                WriteOutput("Thread: " + t.CorThread.Id);
                CorBlockingObject[] blockingObjects = t.CorThread.GetBlockingObjects();
                if (blockingObjects.Length == 0)
                {
                    WriteOutput("  Not blocked");
                }
                else
                {
                    foreach (CorBlockingObject b in blockingObjects)
                    {
                        string timeout = b.Timeout.ToString();
                        if (b.Timeout == TimeSpan.MaxValue)
                            timeout = "Infinite";
                        if (b.BlockingReason == CorDebugBlockingReason.MonitorCriticalSection)
                        {
                            WriteOutput("Monitor critical section " + b.BlockingObject.Address + " " + timeout);
                        }
                        else if (b.BlockingReason == CorDebugBlockingReason.MonitorEvent)
                        {
                            WriteOutput("Monitor event            " + b.BlockingObject.Address + " " + timeout);
                        }
                    }
                }
            }
        }
    }
}
