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
			DebugUtil.SendMessage("MultiBoot", "BootInfo retrieved");
			BootInformationStruct.MMapEntry* xMMap = (BootInformationStruct.MMapEntry*)MultiBootInfo.MMapAddr;
			while ((uint)xMMap < (MultiBootInfo.MMapAddr + MultiBootInfo.MMapLength)) {
				DebugUtil.SendMultiBoot_MMap(*xMMap);
				xMMap = (BootInformationStruct.MMapEntry*)((uint)xMMap + xMMap->Size + 4);
			}
			DebugUtil.SendMessage("MultiBoot", "Done Iterating MMaps");
			DetermineMemChunkInfo();
			MemoryManager.Initialize(MemChunkStartAddr, MemChunkLength);
		}

		[GluePlaceholderMethod(MethodType = GluePlaceholderMethodTypeEnum.GetKernelResource)]
		public static unsafe byte[] GetBinaryResource(int aIndex) {
			return null;
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
			WriteNumber(((MultiBootInfo.MemUpper + 1024) / 1024) + 1, true);
			Console.WriteLine("");
			Console.WriteLine("Initializing PIC");
			PIC.Initialize();
			Console.WriteLine("Slowing down PIT");
			PIT.PIT_SetSlowest();
			Console.WriteLine("Setting up GDT");
			GDT.Setup();
			Console.WriteLine("Setting up IDT");
			IDT.Setup();
			Console.WriteLine("Setting up Keyboard");
			Keyboard.Initialize();
			Console.WriteLine("Kernel booted!");
			Console.WriteLine("Bladibladidddfsdfsdfdfdfsdfbla");
			IDT.IDT_EnableInterrupts();
			DebugUtil.SendWarning("test", "inited");
		}

		private static uint MemChunkStartAddr;
		private static uint MemChunkLength;

		private static unsafe void DetermineMemChunkInfo() {
			MemChunkLength = 0;
			MemChunkStartAddr = 0;
			BootInformationStruct.MMapEntry* xMMap = (BootInformationStruct.MMapEntry*)MultiBootInfo.MMapAddr;
			while ((uint)xMMap < (MultiBootInfo.MMapAddr + MultiBootInfo.MMapLength)) {
				if (xMMap->@Type == 1) {
					if (xMMap->LengthLow > MemChunkLength) {
						MemChunkLength = xMMap->LengthLow;
						MemChunkStartAddr = xMMap->AddrLow;
					}
				}
				xMMap = (BootInformationStruct.MMapEntry*)((uint)xMMap + xMMap->Size + 4);
			}
			DebugUtil.SendMM_MemChunkFound(MemChunkStartAddr, MemChunkLength);
		}


		private static void WriteNumber(uint aValue, bool aZeroFill) {
			uint xValue = aValue;
			byte xCurrentBits = 32;
			byte xCharsWritten = 0;
			bool xSignificantDigitWritten = aZeroFill;
			Console.Write("0x");
			while (xCurrentBits >= 4) {
				xCurrentBits -= 4;
				byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
				string xDigitString = null;
				switch (xCurrentDigit) {
					case 0:
						if (xSignificantDigitWritten) {
							xDigitString = "0";
							goto default;
						}
						break;
					case 1:
						xDigitString = "1";
						goto default;
					case 2:
						xDigitString = "2";
						goto default;
					case 3:
						xDigitString = "3";
						goto default;
					case 4:
						xDigitString = "4";
						goto default;
					case 5:
						xDigitString = "5";
						goto default;
					case 6:
						xDigitString = "6";
						goto default;
					case 7:
						xDigitString = "7";
						goto default;
					case 8:
						xDigitString = "8";
						goto default;
					case 9:
						xDigitString = "9";
						goto default;
					case 10:
						xDigitString = "A";
						goto default;
					case 11:
						xDigitString = "B";
						goto default;
					case 12:
						xDigitString = "C";
						goto default;
					case 13:
						xDigitString = "D";
						goto default;
					case 14:
						xDigitString = "E";
						goto default;
					case 15:
						xDigitString = "F";
						goto default;
					default:
						if (xDigitString == null) {
							Console.Write("NoDigitSet");
						}
						xSignificantDigitWritten = true;
						xCharsWritten += 1;
						Console.Write(xDigitString);
						break;
				}
			}
		}
	}
}