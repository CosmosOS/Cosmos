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
			Console.Write("    ");
			WriteInt(566933);
			//			uint theMem = MultiBootInfo.MemLower;
			//			System.Diagnostics.Debugger.Break();
			//			theMem = MultiBootInfo.MemUpper;
			//			System.Diagnostics.Debugger.Break();
			//Console.WriteLine(" MB");
			//				//(((mbinfo^.mem_upper + 1000) div 1024) +1);

		}

		private static void WriteInt(uint aValue) {
			uint xValue = aValue;
			while (xValue > 0) {
				switch (xValue % 16) {
					case 0: {
							Console.WriteLine("0");
							break;
						}
					case 1: {
							Console.WriteLine("1");
							break;
						}
					case 2: {
							Console.WriteLine("2");
							break;
						}
					case 3: {
							Console.WriteLine("3");
							break;
						}
					case 4: {
							Console.WriteLine("4");
							break;
						}
					case 5: {
							Console.WriteLine("5");
							break;
						}
					case 6: {
							Console.WriteLine("6");
							break;
						}
					case 7: {
							Console.WriteLine("7");
							break;
						}
					case 8: {
							Console.WriteLine("8");
							break;
						}
					case 9: {
							Console.WriteLine("9");
							break;
						}
					case 10: {
							Console.WriteLine("A");
							break;
						}
					case 11: {
							Console.WriteLine("B");
							break;
						}
					case 12: {
							Console.WriteLine("C");
							break;
						}
					case 13: {
							Console.WriteLine("D");
							break;
						}
					case 14: {
							Console.WriteLine("E");
							break;
						}
					case 15: {
							Console.WriteLine("F");
							break;
						}
					default:
						Console.WriteLine("T");
						break;
				}
				xValue = xValue >> 4;
			}
			Console.WriteLine("x0 (Reverse hex)");
		}
	}
}