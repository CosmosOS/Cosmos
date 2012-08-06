//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Reflection;
using System.Security.Permissions;

using Microsoft.Samples.Tools.Mdbg;

// This is declared in the assemblyrefs file
//[assembly:System.Runtime.InteropServices.ComVisible(false)]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
#pragma warning restore 618

// Main entry point to the managed debugger.
public class Bootstap
{
    [MTAThread]
    public static int Main(string[] args)
    {
        if (args.Length > 0)
        {
            switch (args[0])
            {
                case "/?":
                case "-?":
                    Console.WriteLine(usageString);
                    return 0;
            }
        }

        MDbgShell shell = new MDbgShell();
        return shell.Start(args);
    }

    private const string usageString =
@"
Usage: mdbg [program [ arguments... ] ]
       mdbg !command1 [!command2 !command3 ... ]

  When program name is entered on the command line, the debugger
  automatically starts debugging such program.

  Arguments starting with ! are interpreted as debugger commands. 

Examples:
  mdbg myProgram.exe

  mdbg !run myProgram.exe !step !go !kill !quit
";
}
