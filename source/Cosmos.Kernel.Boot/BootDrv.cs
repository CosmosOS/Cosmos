using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cosmos.Kernel.Boot.Glue;

namespace Cosmos.Kernel.Boot {
	public static class BootDrv {
		private static BootInformationStruct MultiBootInfo;
		private static bool BootInfoSet = false;
		[GlueMethod(MethodType = GlueMethodTypeEnum.SaveBootInfoStruct)]
		public unsafe static void SetMultiBootInfo(ref BootInformationStruct aBootInfo) {
			MultiBootInfo = aBootInfo;
			BootInfoSet = true;
			Debug.WriteLine("BootInfo retrieved");
			Debug.Write("\tMMap length = ");
			IO.WriteSerialHexNumber(0, MultiBootInfo.MMapLength);
			Debug.WriteLine("");
			Debug.WriteLine("MMap:");
			for (uint i = 0; i < 6; i++) {
				uint* thePtr = (uint*)MultiBootInfo.MMapAddr;
				thePtr += 24 * i;
				thePtr -= 4;

				Debug.Write("MMapEntry ");
				IO.WriteSerialHexNumber(0, i);
				Debug.WriteLine("");	

				Debug.Write("\tSize ");
				IO.WriteSerialHexNumber(0, *thePtr);
				thePtr += 1;
				Debug.WriteLine("");	
				Debug.Write("\tAddrLow ");
				IO.WriteSerialHexNumber(0, *thePtr);
				thePtr += 1;
				Debug.WriteLine("");	
				Debug.Write("\tAddrHigh ");
				IO.WriteSerialHexNumber(0, *thePtr);
				thePtr += 1;
				Debug.WriteLine("");	
				Debug.Write("\tLengthLow ");
				IO.WriteSerialHexNumber(0, *thePtr);
				thePtr += 1;
				Debug.WriteLine("");	
				Debug.Write("\tLengthHigh ");
				IO.WriteSerialHexNumber(0, *thePtr);
				thePtr += 1;
				Debug.WriteLine("");	
				Debug.Write("\tType ");
				IO.WriteSerialHexNumber(0, *thePtr);
				thePtr += 1;
				Debug.WriteLine("");	
			}
			DebugUtil.WriteLine("Done Iterating");
		}

		public static void Main() {
			Console.WriteLine("This is CosmOS Booting..."); // 25 chars
			if (!BootInfoSet) {
				Console.WriteLine("No boot info available, terminating!");
				return;
			}
			Console.WriteLine("Boot information available.");
			Console.WriteLine("");
			Console.Write("Available Memory: ");
			WriteIntHex(((MultiBootInfo.MemUpper + 1024) / 1024) + 1);
			Console.WriteLine("");
			IO.WriteSerialString(0, "Hello, World");
			Console.WriteLine("Skipping GDT for now");
			//Console.Write("Loading IDT...");
			//IDT.SetupInterruptDescriptorTable();
			//Console.WriteLine("Done.");
		}

		private static void WriteInt(uint aValue) {
			WriteNumber(aValue, 10);
			Console.Write(" (Reverse number)");
		}

		private static void WriteIntHex(uint aValue) {
			WriteNumber(aValue, 16);
			Console.Write("x0 (Reverse hex)");
		}

		private static void WriteNumber(uint aValue, byte aBase) {
			uint theValue = aValue;
			int xReturnedChars = 0;
			while (theValue > 0) {
				switch (theValue % aBase) {
					case 0: {
							Console.Write("0");
							xReturnedChars++;
							break;
						}
					case 1: {
							Console.Write("1");
							xReturnedChars++;
							break;
						}
					case 2: {
							Console.Write("2");
							xReturnedChars++;
							break;
						}
					case 3: {
							Console.Write("3");
							xReturnedChars++;
							break;
						}
					case 4: {
							Console.Write("4");
							xReturnedChars++;
							break;
						}
					case 5: {
							Console.Write("5");
							xReturnedChars++;
							break;
						}
					case 6: {
							Console.Write("6");
							xReturnedChars++;
							break;
						}
					case 7: {
							Console.Write("7");
							xReturnedChars++;
							break;
						}
					case 8: {
							Console.Write("8");
							xReturnedChars++;
							break;
						}
					case 9: {
							Console.Write("9");
							xReturnedChars++;
							break;
						}
					case 10: {
							Console.Write("A");
							xReturnedChars++;
							break;
						}
					case 11: {
							xReturnedChars++;
							Console.Write("B");
							break;
						}
					case 12: {
							Console.Write("C");
							xReturnedChars++;
							break;
						}
					case 13: {
							Console.Write("D");
							xReturnedChars++;
							break;
						}
					case 14: {
							Console.Write("E");
							xReturnedChars++;
							break;
						}
					case 15: {
							Console.Write("F");
							xReturnedChars++;
							break;
						}
				}
				theValue = theValue / aBase;
			}
			while (xReturnedChars < 8) {
				Console.Write("0");
				xReturnedChars++;
			}
		}
	}
}