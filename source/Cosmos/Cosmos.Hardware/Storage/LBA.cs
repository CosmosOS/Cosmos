using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Storage {
	public class LBA:Hardware {
		private const ushort IDEController0Port = 0x1F0;
		private const ushort IDEController1Port = 0x170;
		public static void Initialize() {
			IOWriteByte(IDEController0Port + 3, 0x88);
			if (IOReadByte(IDEController0Port + 3) == 0x88) {
				Console.WriteLine("    Primary IDE Controller found.");
			} else {
				Console.WriteLine("    No Primary IDE Controller found!");
			}
			IOWriteByte(IDEController1Port + 3, 0x88);
			if (IOReadByte(IDEController1Port + 3) == 0x88) {
				Console.WriteLine("    Secondary IDE Controller found.");
			} else {
				Console.WriteLine("    No Secondary IDE Controller found!");
			}
		}
	}
}
