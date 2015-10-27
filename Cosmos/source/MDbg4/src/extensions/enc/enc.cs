//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Reflection;
using System.Threading;
using System.Diagnostics.SymbolStore;
using System.Diagnostics;
using System.Text;
using System.Collections;
using System.IO;
using System.Security.Permissions;
using System.Globalization;

using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.CorMetadata;

//////////////////////////////////////////////////////////////////////////////////
//
// ENC Testing extension for MDbg
//
//////////////////////////////////////////////////////////////////////////////////

[assembly:System.Runtime.InteropServices.ComVisible(false)]
[assembly:SecurityPermission(SecurityAction.RequestMinimum, Unrestricted=true)]

namespace Microsoft.Samples.Tools.Mdbg.Extension
{
    public sealed class EncExtension : CommandBase
    {
        public static void LoadExtension()
        {
            MDbgAttributeDefinedCommand.AddCommandsFromType(Shell.Commands,typeof(EncExtension));
            WriteOutput("Extension EnC loaded");
        }

        public static void UnloadExtension()
        {
            MDbgAttributeDefinedCommand.RemoveCommandsFromType(Shell.Commands, typeof(EncExtension));
        }

        
        [
         CommandDescription(
                            CommandName="enc",
                            ShortHelp="Apply Edits to currently debugged process",
                            MinimumAbbrev=3,
                            LongHelp=@"
Usage: enc module [editSourceFile [deltaFiles]]
    edits have to be stored on disk in form of binary IL (.dil) 
    and binary metadata (.dmeta). Files should be in current
    directory or in directory specified by PathToEncFiles.
    if the module we are applying changes to is called a.exe,
    there has to be 2 files a.exe.dil, a.exe.dmeta
"
                            )
         ]
        public static void EncCmd(string args)
        {
            ArgParser ap = new ArgParser(args);
            if(ap.Count==0)
            {
                WriteOutput("Performed edits:");
                foreach(MDbgModule module in Debugger.Processes.Active.Modules)
                {
                    if(module.EditsCounter>0)
                    {
                        WriteOutput(module.CorModule.Name);
                        int edits = module.EditsCounter;
                        for(int j=1;j<=edits;j++)
                        {
                            string editFile = module.GetEditsSourceFile(j);
                            WriteOutput(String.Format(CultureInfo.InvariantCulture, "   {0}. - {1}",new Object[]{j,editFile==null?"N/A":editFile}));
                        }
                    }
                }
                return;
            }
            if(ap.Count<1 && ap.Count>3)
            {
                WriteError("Wrong amount of arguments!");
                return;
            }
            string encModule = ap.AsString(0);

            MDbgModule m = Debugger.Processes.Active.Modules.Lookup(encModule);
            string modName = System.IO.Path.DirectorySeparatorChar + 
                Path.GetFileName(m.CorModule.Name);

            string pathToEncFiles = Path.GetDirectoryName(m.CorModule.Name); // defaults to location of the module

            string editSourceFile;
            if(ap.Exists(1))
                editSourceFile = ap.AsString(1);
            else
                editSourceFile = null;

            string deltasBaseName;
            if(ap.Exists(2))
                deltasBaseName = ap.AsString(2);
            else
                deltasBaseName = pathToEncFiles+modName+".1";

            string deltaPdbFile = deltasBaseName+".pdb";
            if( !File.Exists(deltaPdbFile) )
            {
                WriteOutput("Delta PDB file not found; debug symbols won't be updated.");
                deltaPdbFile = null;
            }

            m.ApplyEdit(deltasBaseName+".dmeta",
                        deltasBaseName+".dil",
                        deltaPdbFile,
                        editSourceFile);

            WriteOutput("ENC successfully applied.");
        }

        [
         CommandDescription(
                            CommandName="gas",
                            ShortHelp="Get Active Statements",
                            MinimumAbbrev=3,
                            LongHelp=@"
Usage: gas
    prints active functions on current thread using EnC API,
    GetActiveFunctions
"
                            )
         ]
        public static void GetActiveStatementCmd(string args)
        {
            MDbgThread t = Debugger.Processes.Active.Threads.Active;
            CorActiveFunction[] afa = t.CorThread.GetActiveFunctions();
            WriteOutput("Active Statements Count: "+afa.Length);

            int frameNo = 0;
            foreach(CorActiveFunction caf in afa)
            {
                MDbgFunction f = Debugger.Processes.Active.Modules.LookupFunction(caf.Function);
                WriteOutput(String.Format(CultureInfo.InvariantCulture, "{0}. Method: {1}, IL offset: {2}",new Object[]{frameNo,f.FullName,caf.ILoffset}));
                frameNo++;
            }
        }

        [
         CommandDescription(
                            CommandName="remap",
                            MinimumAbbrev=2,
                            ShortHelp="remap [new il] - remaps old version of function to the new one."
                            )
        ]
        public static void RemapCmd(string args)
        {
            ArgParser ap = new ArgParser(args);
            if(!(Debugger.Processes.Active.StopReason is RemapOpportunityReachedStopReason))
            {
                WriteError("Not stopped at RemapOpportunity breakpoint");
                return;
            }

            RemapOpportunityReachedStopReason sr = (RemapOpportunityReachedStopReason)Debugger.Processes.Active.StopReason;
            if(!ap.Exists(0))
            {
                // no arguments -- we just print content of last remap callback
                // we want to do remap.
                WriteOutput("Last FunctionRemapOpportunity Callback:");
                WriteOutput("-----------------------------------------");
                WriteOutput("AppDomain:     "+sr.AppDomain.Name);
                WriteOutput("NewFunction:   "+sr.NewFunction.Token);
                WriteOutput("OldFunction:   "+sr.OldFunction.Token);
                WriteOutput("OldILOffset:   "+sr.OldILOffset);
                WriteOutput("Thread Number: "+Debugger.Processes.Active.Threads.Lookup(sr.Thread).Number);
            }
            else
            {
                int newILOffset = ap.AsInt(0);
                sr.Thread.ActiveFrame.RemapFunction(newILOffset);
                Debugger.Processes.Active.Threads.InvalidateAllStacks();
                WriteOutput("Remap successful.");
            }
        }

        [
         CommandDescription(
                            CommandName="funcVersion",
                            ShortHelp="prints current function version",
                            LongHelp=@"
Usage: funcversion
    Prints current version of the function.
"
                            )
         ]
        public static void FuncVersionCmd(string args)
        {
            ArgParser ap = new ArgParser(args);
            MDbgFunction function;
            if(ap.AsString(0)==".")
                function = Debugger.Processes.Active.Threads.Active.CurrentFrame.Function;
            else
                function = Debugger.Processes.Active.ResolveFunctionNameFromScope(ap.AsString(0));
            WriteOutput("Current version: "+function.CorFunction.Version);
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Misc commands
        /////////////////////////////////////////////////////////////////////////////////////////
        private class OutputReader
        {
            public OutputReader(StreamReader stream)
            {
                Debug.Assert(null!=stream);
                
                m_stream = stream;
            }
            
            public void StartReading()
            {
                while(true)
                {
                    string line=null;
                    try 
                    {
                        line = m_stream.ReadLine();
                    }
                    catch(IOException) // if there is a problem with reading, end pipe
                    { 
                    } 
                    if(null==line) break;
                    WriteOutput(MDbgOutputConstants.Ignore,line);
                }
            }

            private StreamReader m_stream;
        }
                                
        [
         CommandDescription(
                            CommandName="shell",
                            ShortHelp="executes shell command",
                            MinimumAbbrev=3,
                            LongHelp=@"
Usage: shell command
    executes a shell command from within a mdbg debugger. Output of the command
    will be printed to the console.
"
                            )
         ]
        public static void ShellCommand(string args)
        {
            string cmdShell = Environment.GetEnvironmentVariable("ComSpec");
            if (null==cmdShell)
            {
                WriteError("Missing command specifier (No COMSPEC env variable).");
                return;
            }

            bool FailOnError = true;
            if(args.StartsWith("/Q"))
            {
                FailOnError = false;
                args = args.Substring(2,args.Length-2);
                args = args.TrimStart(' ');
            }

            bool silentMode = false;
            if(args.StartsWith("/S"))
            {
                silentMode= true;
                args = args.Substring(2,args.Length-2);
                args = args.TrimStart(' ');
            }

            ProcessStartInfo psi = new ProcessStartInfo(cmdShell,"/c " + Environment.ExpandEnvironmentVariables(args));
                
            //WriteOutput(String.Format("Starting process: {0} /c {1}", cmdShell, Environment.ExpandEnvironmentVariables(args)));

            psi.ErrorDialog = true;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.RedirectStandardOutput = psi.RedirectStandardError = !silentMode;
            psi.UseShellExecute = false;
            
            
            Process shell = Process.Start(psi);
            if(silentMode)
            {
                shell.WaitForExit();
            }
            else
            {
                Thread t1 = new Thread(new ThreadStart(new OutputReader(shell.StandardOutput).StartReading));
                t1.Start();

                Thread t2 = new Thread(new ThreadStart(new OutputReader(shell.StandardError).StartReading));
                t2.Start();
            
                shell.WaitForExit();
                t1.Join();
                t2.Join();
            }
            
            if(shell.ExitCode!=0 && FailOnError)
            {
                WriteError("ExitCode="+shell.ExitCode);
            }
        }
    }
}


