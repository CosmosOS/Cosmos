using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Cosmos.Kernel.Staging.Stages {
	class MatthijsStage: StageBase {
		public override string Name {
			get {
				return "Matthijs";
			}
		}

		public override void Initialize() {
			//TestNewATA();

		}

		public static unsafe void TestNewATA() {
			Hardware.Storage.ATA.Initialize(CPU.Sleep);
			Hardware.Storage.ATA xDrive = new Cosmos.Hardware.Storage.ATA(0, 0);
			FileSystem.Ext2 xExt2 = new Cosmos.Kernel.FileSystem.Ext2(xDrive);
			if (xExt2.Initialize()) {
				Console.WriteLine("Ext2 Initialized");
			} else {
				Console.WriteLine("Ext2 Initialization failed!");
			}
			Stream xFileStream = xExt2.OpenFile(new string[] { "readme.txt" });
			if (xFileStream == null) {
				Console.WriteLine("Couldn't read file!");
				return;
			}
			Console.Write("File length = ");
			Hardware.Storage.ATAOld.WriteNumber((uint)xFileStream.Length, 32);
			Console.WriteLine(" bytes");
			byte[] xBytes = new byte[(int)xFileStream.Length];
			int xBytesRead = xFileStream.Read(xBytes, 0, (int)xFileStream.Length);
			DebugUtil.SendNumber("MatthijsStage", "Bytes Read", (uint)xBytesRead, 32);
			DebugUtil.SendByteStream("CPU", "Readme.txt contents", xBytes);
			char[] xChars = new char[xBytes.Length - 1];
			for (int i = 0; i < xChars.Length; i++) {
				xChars[i] = (char)xBytes[i];
			}
			String s = new String(xChars);
			Console.Write("Contents: '");
			Console.Write(s);
			Console.WriteLine("'");
			//Console.Write("Contents of root (");
			//string[] xItems = xExt2.GetDirectoryEntries(new string[0]);
			//Hardware.Storage.ATAOld.WriteNumber((uint)xItems.Length, 8);
			//Console.WriteLine(" items):");
			//DebugUtil.SendNumber("CPU", "Dir items count", (uint)xItems.Length, 32);
			//if (xItems == null) {
			//    Console.WriteLine("    Result array is null!");
			//}
			//for (int i = 0; i < xItems.Length; i++) {
			//    Console.Write("   - ");
			//    Console.Write(xItems[i]);
			//    Console.Write(" [");
			//    Hardware.Storage.ATAOld.WriteNumber((uint)xItems[i][0], 16);
			//    Console.Write("] (Length = ");
			//    Hardware.Storage.ATAOld.WriteNumber((uint)xItems[i].Length, 8);
			//    Console.WriteLine(")");
			//}
		}

		public static unsafe void TestATA() {
			Hardware.Storage.ATA.Initialize(CPU.Sleep);
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
			char[] xChars = new char[xItem.Length - 1];
			for (int i = 0; i < xChars.Length; i++) {
				xChars[i] = (char)xItem[i];
			}
			String s = new String(xChars);
			Console.Write("Contents: '");
			Console.Write(s);
			Console.WriteLine("'");
			Console.Write("Contents of root (");
			string[] xItems = xExt2.GetDirectoryEntries(new string[0]);
			Hardware.Storage.ATAOld.WriteNumber((uint)xItems.Length, 8);
			Console.WriteLine(" items):");
			DebugUtil.SendNumber("CPU", "Dir items count", (uint)xItems.Length, 32);
			if (xItems == null) {
				Console.WriteLine("    Result array is null!");
			}
			for (int i = 0; i < xItems.Length; i++) {
				Console.Write("   - ");
				Console.Write(xItems[i]);
				Console.Write(" [");
				Hardware.Storage.ATAOld.WriteNumber((uint)xItems[i][0], 16);
				Console.Write("] (Length = ");
				Hardware.Storage.ATAOld.WriteNumber((uint)xItems[i].Length, 8);
				Console.WriteLine(")");
			}
		}

		public override void Teardown() {
		}
	}
}
