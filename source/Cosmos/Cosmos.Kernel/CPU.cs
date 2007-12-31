using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Cosmos.Kernel {
	public class CPU {
		private static void WriteBool(bool aValue) {
			if (aValue) {
				Console.WriteLine("true");
			} else {
				Console.WriteLine("false");
			}
		}
		public static unsafe void Init() {
			Heap.CheckInit();
			Console.Write("Creating GDT...");
			Hardware.CPU.CreateGDT();
			Console.WriteLine("Done");
			Console.Write("Initializing PIC...");
			Hardware.PIC.Init();
			Console.WriteLine("Done");
			Console.Write("Initializing Serial 0...");
			Hardware.Serial.InitSerial(0);
			Console.WriteLine("Done");
			Console.Write("Initializing Debug Utility...");
			Hardware.DebugUtil.Initialize();
			//System.Diagnostics.Debugger.Break();
			Hardware.DebugUtil.SendMessage("Logging", "Initialized!");
			//System.Diagnostics.Debugger.Break();
			Console.WriteLine("Done");
			Console.Write("Configuring PIT...");
			Hardware.PIT.Initialize(Tick);
			Console.WriteLine("Done");
			//System.Diagnostics.Debugger.Break();
			Console.Write("Creating IDT...");
			Hardware.CPU.CreateIDT();
			Console.WriteLine("Done");
			TestATA();
			//Console.WriteLine("Starting MM tests...");
			//Console.Write(" + Allocing 64 bytes, address = ");
			//uint* xPointer = (uint*)Heap.MemAlloc(64);
			//xPointer[1] = 0xFFFFFFFF;
			//Hardware.Storage.ATAOld.WriteNumber((uint)xPointer, 32);
			//Console.WriteLine("");
			//Console.WriteLine(" + Freeing pointer");
			//Heap.MemFree((uint)xPointer);
			//Console.Write(" + Allocing 64 bytes, address = ");
			//xPointer = (uint*)Heap.MemAlloc(64);
			//Hardware.Storage.ATAOld.WriteNumber((uint)xPointer, 32);
			//Console.WriteLine("");
			//Console.Write("Value = ");
			//Hardware.Storage.ATAOld.WriteNumber(xPointer[1], 32);
			//Console.WriteLine("");
			//xPointer = (uint*)Heap.MemAlloc(64);
			//Hardware.Storage.ATAOld.WriteNumber((uint)xPointer, 32);
			//Console.WriteLine("");
			//Console.Write("Initializing Keyboard...");
			//Keyboard.Initialize();
			//Console.WriteLine("Done");
			//Console.Write("Initializing ATA...");
			//Hardware.Storage.ATA.Initialize(Sleep);
			//Console.WriteLine("Done");
			//Console.Write("Getting Partition info...");
			//FileSystem.Ext2.PrintAllFilesAndDirectories(0, 0);
			//Console.WriteLine("Done");
			//Console.Write("Reading file...");
			//byte[] xContents = FileSystem.Ext2.ReadFile(0, 0, new string[] { "readme.txt" });
			//if (xContents == null) {
			//    Console.WriteLine("Failed or not fully implemented!");
			//} else {
			//    DebugUtil.SendByteStream("Kernel", "readme contents", xContents);
			//    Console.WriteLine("Done");
			//}

		}

		public static uint TickCount {
			get;
			private set;
		}
		private static void Tick(object aSender, EventArgs aEventArgs) {
			TickCount += 1;
		}

		public static void Sleep(uint aMSec) {
			uint xEnd = TickCount + aMSec;
			while (TickCount < xEnd)
				;
		}

		static unsafe void TestATA() {
			Hardware.Storage.ATA.Initialize(Sleep);
			Debugger.Break();
			Hardware.Storage.ATA xDrive = new Cosmos.Hardware.Storage.ATA(0, 0);
			byte* xBuffer = (byte*)Heap.MemAlloc(512);
			if (xDrive.ReadBlock(1, xBuffer)) {
				Console.WriteLine("Reading went fine");
			} else {
				Console.WriteLine("Error reading");
			}
			FileSystem.Ext2 xExt2 = new Cosmos.Kernel.FileSystem.Ext2(xDrive);
			if (xExt2.Initialize()) {
			    Console.WriteLine("Ext2 Initialized");
			} else {
				Console.WriteLine("Ext2 Initialization failed!");
			}
			byte[] xItem = xExt2.ReadFile(new string[] { "readme.txt" });
			if (xItem == null) {
				Console.WriteLine("Couldn't read file!");
				return;
			}
			Console.Write("File length = ");
			Hardware.Storage.ATAOld.WriteNumber((uint)xItem.Length, 32);
			Console.WriteLine(" bytes");
			DebugUtil.SendByteStream("CPU", "Readme.txt contents", xItem);
		}
	}
}