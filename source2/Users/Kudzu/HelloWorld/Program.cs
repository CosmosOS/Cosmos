using System;
using System.Reflection;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.X86;
using Indy.IL2CPU.Assembler.X86;
using S = Cosmos.Hardware.TextScreen;

namespace HelloWorld {
	class Program {
		#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args) {
      //Indy.IL2CPU.Engine.Execute()
      // which is called from Builder.RunEngine()

      //TODO: Move new build logic into new sort.
      // Build stuff is all UI, launching QEMU, making ISO etc.
      // IL2CPU should only contain scanning and assembling of binary files
      var xAsmblr = new Cosmos.IL2CPU.Assembler(typeof(Cosmos.IL2CPU.X86.ILOp));
      var xScanner = new ILScanner(xAsmblr);
      var xEntryPoint = typeof(Program).GetMethod("Init", BindingFlags.Public | BindingFlags.Static);

      using (var xOldAsmblr = new CosmosAssembler(0)) {
        //InitializePlugs(aPlugs);

        xScanner.Execute(xEntryPoint);
      }
    }
		#endregion

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