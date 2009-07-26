using System;
using System.Reflection;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.X86;
using Indy.IL2CPU.Assembler.X86;
using S = Cosmos.Hardware.TextScreen;
using System.IO;

namespace HelloWorld {
	class Program {
		//#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args) {
      //Indy.IL2CPU.Engine.Execute()
      // which is called from Builder.RunEngine()

      using (var xOldAsmblr = new CosmosAssembler(0)) {
        //TODO: Move new build logic into "new sort".
        // Build stuff is all UI, launching QEMU, making ISO etc.
        // IL2CPU should only contain scanning and assembling of binary files
        var xAsmblr = new Cosmos.IL2CPU.X86.AssemblerNasm();
        var xScanner = new ILScanner(xAsmblr);
        var xEntryPoint = typeof(Program).GetMethod("Init", BindingFlags.Public | BindingFlags.Static);
        //InitializePlugs(aPlugs);
        xScanner.Execute(xEntryPoint);

				var xPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
				xPath = Path.Combine(xPath, @"..\..\Output.asm");
				using (var xOut = File.CreateText(xPath)) {
					xOldAsmblr.FlushText(xOut);
				}
      }
    }
		//#endregion

		// Main entry point of the kernel
		public static void Init() {
      var xBoot = new Cosmos.Sys.Boot();
      xBoot.Execute();
      Console.BackgroundColor = ConsoleColor.Green;
      //TODO: What is this next line for?
      S.ReallyClearScreen();
      Console.WriteLine("Congratulations! You just booted C# code.");
      Console.WriteLine("Edit Program.cs to create your own Operating System.");
      Console.WriteLine("Press a key to shutdown...");
      Console.Read();
      Cosmos.Sys.Deboot.ShutDown();
		}
	}
}