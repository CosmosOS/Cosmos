using System;
using System.Collections.Generic;
using Cosmos.Kernel.Boot.Glue;

namespace Cosmos.Kernel.Boot {
	public static class BootDrv {
		private static BootInformationStruct MultiBootInfo;
		private static bool BootInfoSet = false;
		[GlueMethod(MethodType = GlueMethodTypeEnum.SaveBootInfoStruct)]
		public static void SetMultiBootInfo(ref BootInformationStruct aBootInfo) {
			MultiBootInfo = aBootInfo;
			BootInfoSet = true;
		}

		public static void Main() {
			Console.WriteLine("This is CosmOS Booting...");
			if (!BootInfoSet) {
				Console.WriteLine("No boot info available, terminating!");
				return;
			}
			Console.WriteLine("Boot information available.");
			Console.WriteLine("");
			Console.Write("Flags:   ");
			WriteIntHex(MultiBootInfo.Flags);
			Console.WriteLine("");
			Console.Write("MemLow:  ");
			WriteIntHex(MultiBootInfo.MemLower);
			Console.WriteLine("");
			Console.Write("MemHigh: ");
			WriteIntHex(MultiBootInfo.MemUpper);
			Console.WriteLine("");
			Console.WriteLine("Maybe it's this:");
			Console.Write("Flags:   ");
			WriteIntHex(MultiBootInfo.VbeIntfOff_Len);
			Console.WriteLine("");
			Console.Write("MemLow:  ");
			WriteIntHex(MultiBootInfo.VbeMode_IntfSeg);
			Console.WriteLine("");
			Console.Write("MemHigh: ");
			WriteIntHex(MultiBootInfo.VbeModeInfo);
			Console.WriteLine("");
//			uint theMem = MultiBootInfo.MemLower;
			//			System.Diagnostics.Debugger.Break();
			//			theMem = MultiBootInfo.MemUpper;
			//			System.Diagnostics.Debugger.Break();
			//Console.WriteLine(" MB");
			//				//(((mbinfo^.mem_upper + 1000) div 1024) +1);

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
			while(xReturnedChars < 8) {
				Console.Write("0");
				xReturnedChars++;
			}
		}
	}
}