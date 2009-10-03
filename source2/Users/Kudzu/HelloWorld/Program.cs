using System;
using System.Collections.Generic;
using System.Reflection;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.X86;
using Indy.IL2CPU.Assembler.X86;
using S = Cosmos.Hardware.TextScreen;
using System.IO;
using Indy.IL2CPU;
using Cosmos.Hardware;

namespace HelloWorld {
	class Program {
    private static void TestMethod(object aSender, EventArgs e) {
      Console.WriteLine("Callback?");
    }
		//#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args) {
      // enforce assembly linking:
      var xTheType = typeof(Indy.IL2CPU.X86.Plugs.CustomImplementations.System.Runtime.CompilerServices.RuntimeHelpersImpl);
      xTheType = typeof(Cosmos.Kernel.Plugs.ArrayListImpl);
      xTheType = typeof(Cosmos.Hardware.Plugs.FCL.System.Console);
      xTheType = typeof(Cosmos.Sys.Plugs.Deboot);
      
      // end enforce assembly linking
      //Indy.IL2CPU.Engine.Execute()
      // which is called from Builder.RunEngine()

      var xOutPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
      xOutPath = Path.Combine(xOutPath, @"..\..\");

      //TODO: Move new build logic into "new sort".
      // Build stuff is all UI, launching QEMU, making ISO etc.
      // IL2CPU should only contain scanning and assembling of binary files
      var xAsmblr = new Cosmos.IL2CPU.X86.AssemblerNasm();
      try {
        xAsmblr.Initialize();

        using (var xScanner = new ILScanner(xAsmblr)) {
          xScanner.EnableLogging(xOutPath + "Scanner Map.html");

          var xEntryPoint = typeof(Program).GetMethod("Init", BindingFlags.Public | BindingFlags.Static);
          xScanner.Execute(xEntryPoint);

          Console.WriteLine("Method Count: {0}", xScanner.MethodCount);
        }
      } finally {
        using (var xOut = File.CreateText(xOutPath + "Output.asm")) {
          xAsmblr.FlushText(xOut);
        }
      }
    }
		//#endregion


    private static void Test(byte aIRQ) {
      var xDict = new TempDictionary<EventHandler>();
      var xMethod = new EventHandler(TestMethod);
      xDict.Add(aIRQ, xMethod);
      if (xDict.ContainsKey(aIRQ)) {
        Console.WriteLine("Found! ie, inside event handler!");
      } else {
        Console.WriteLine("Not found!");
      }
    }

		// Main entry point of the kernel
		public static unsafe void Init() {
      var xTempBool = true;
      if (xTempBool) {
        var xBoot = new Cosmos.Sys.Boot();
        xBoot.Execute();
      }
      //Test(1);
      Console.WriteLine("Done");
      var xLine = Console.ReadLine();
      Console.Write("Given text was: ");
      Console.WriteLine(xLine);


      

      return;
      //Console.BackgroundColor = ConsoleColor.Green;
      //TODO: What is this next line for?
      //S.ReallyClearScreen();
      var xMessage = "Congratulations! You just booted C# code.";
      //var xChar = xMessage[0];
      //var xByte = (byte)xChar;
      
      //byte* xChars = (byte*)0xB8000;
      //if (xByte == 67) {
      //  xChars[0] = 74;
      //} else {
      //  xChars[0] = 78;
      //}
      Console.WriteLine(xMessage);
      Console.Write("Read: ");
      Console.WriteLine(Console.ReadLine());

//      Console.Write(xMessage);      
      //var xTempBool2 = false;
      //if (xTempBool2) {
      //  xChars[1] = 7;
      //  xChars[0] = 65;
      //  } else {
      //  xChars[0] = 66;
      //}
      

      //Console.WriteLine("Edit Program.cs to create your own Operating System.");
      //Console.WriteLine("Press a key to shutdown...");
      //while (true) {
      //}
      
      
      //Console.Read();
      //Cosmos.Sys.Deboot.ShutDown();
		}
	}

  public class Base {
    public virtual void Test() { Console.WriteLine("Base"); }
  }

  public class Derived: Base {
    public override void Test() {
      Console.WriteLine("Derived");
    }
  }
}