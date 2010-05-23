using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.User.Kernel {
    public class Program {
	    public static void Init() {
		    Main();
	    }

        // If VSIP / Debugger has changed
        //  -Close VS
        //  -Run \Build\VSIP\install.bat - Builds all and installs
        // Debugging the debugger
        //  -Can use the hive
        //  -Set startup project to /source2/VSIP/Cosmos.VS.Package
        //  -Open a Cosmos project in the hive.
        //  -Run it
        // To use IDA
        //  -Build target project
        //  -Start VMWare or QEMU
        //    -\Build\Run QEMU Manually for IDA.bat
        //      -NOTE: This bat is hardcoded for Kudzu.Breakpoints to run
        //  -IDA Method 1 - No symbols
        //    -Debugger, Attach, Remote GDB, 127.0.0.1:1234, Select ID 0 
        //  -IDA Method 2 - Symbols
        //    -File, Open. \source2\Users\Kudzu\Breakpoints\bin\Debug\CosmosKernel.obj, Open
        //    -Select ELF, Click OK
        //    -Debugger, Select Debugger (F9)
        //    -Remote GDB Debugger (Can also check set as default debugger), OK
        //  -Run or step

        static void Main() {
            // boot the Cosmos kernel:
            Cosmos.Sys.Boot xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();
            
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back:");
            string xResult = Console.ReadLine();
            // when Qemu shows you the above text, put a breakpoint on the next line, then type a line 
            // of text in qemu. you'll see that Visual Studio breaks on the breakpoint.
            // Note, you cannot set the breakpoints before running the project, this is a current bug
            // in Cosmos.
            Console.Write("Text typed: ");
            Console.WriteLine(xResult);
        }
    }
}
