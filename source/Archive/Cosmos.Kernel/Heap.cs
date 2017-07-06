//using System;

//namespace Cosmos.Kernel {
//  public static class Heap {
//    public static bool EnableDebug = true;
//    private static uint mStart;
//    private static uint mStartAddress;
//    private static uint mLength;
//    private static uint mEndOfRam;

//    private static void Initialize(uint aStartAddress, uint aEndOfRam) {
//      mStart = mStartAddress = aStartAddress + (4 - (aStartAddress % 4));
//      mLength = aEndOfRam - aStartAddress;
//      mLength = (mLength / 4) * 4;
//      mStartAddress += 1024;
//      mEndOfRam = aEndOfRam;
//      mStartAddress = (mStartAddress / 4) * 4;
//      mLength -= 1024;
//      ClearMemory(aStartAddress, mLength);
//      UpdateDebugDisplay();
//    }

//    private static void WriteNumber(uint aNumber, byte aBits) {
//      uint xValue = aNumber;
//      byte xCurrentBits = aBits;
//      Console.Write("0x");
//      while (xCurrentBits >= 4) {
//        xCurrentBits -= 4;
//        byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
//        string xDigitString = null;
//        switch (xCurrentDigit) {
//          case 0:
//            xDigitString = "0";
//            goto default;
//          case 1:
//            xDigitString = "1";
//            goto default;
//          case 2:
//            xDigitString = "2";
//            goto default;
//          case 3:
//            xDigitString = "3";
//            goto default;
//          case 4:
//            xDigitString = "4";
//            goto default;
//          case 5:
//            xDigitString = "5";
//            goto default;
//          case 6:
//            xDigitString = "6";
//            goto default;
//          case 7:
//            xDigitString = "7";
//            goto default;
//          case 8:
//            xDigitString = "8";
//            goto default;
//          case 9:
//            xDigitString = "9";
//            goto default;
//          case 10:
//            xDigitString = "A";
//            goto default;
//          case 11:
//            xDigitString = "B";
//            goto default;
//          case 12:
//            xDigitString = "C";
//            goto default;
//          case 13:
//            xDigitString = "D";
//            goto default;
//          case 14:
//            xDigitString = "E";
//            goto default;
//          case 15:
//            xDigitString = "F";
//            goto default;
//          default:
//            if (xDigitString == null) {
//              System.Diagnostics.Debugger.Break();
//            }
//            Console.Write(xDigitString);
//            break;
//        }
//      }
//    }

//    private static bool mDebugDisplayInitialized = false;

//    // this method displays the used/total memory of the heap on the first line of the text screen
//    private static void UpdateDebugDisplay() {
//      //if (EnableDebug)
//      //{
//      //    if (!mDebugDisplayInitialized)
//      //    {
//      //        mDebugDisplayInitialized = true;
//      //        int xOldPositionLeft = Console.CursorLeft;
//      //        int xOldPositionTop = Console.CursorTop;
//      //        Console.CursorLeft = 0;
//      //        Console.CursorTop = 0;
//      //        Console.Write("[Heap Usage: ");
//      //        WriteNumber(mStartAddress,
//      //                    32);
//      //        Console.Write("/");
//      //        WriteNumber(mEndOfRam,
//      //                    32);
//      //        Console.Write("] bytes");
//      //        while (Console.CursorLeft < (Console.WindowWidth-1))
//      //        {
//      //            Console.Write(" ");
//      //        }
//      //        Console.CursorLeft = xOldPositionLeft;
//      //        Console.CursorTop = xOldPositionTop;
//      //    }
//      //    else
//      //    {
//      //        int xOldPositionLeft = Console.CursorLeft;
//      //        int xOldPositionTop = Console.CursorTop;
//      //        Console.CursorLeft = 13;
//      //        Console.CursorTop = 0;
//      //        WriteNumber(mStartAddress,
//      //                    32);
//      //        Console.CursorLeft = xOldPositionLeft;
//      //        Console.CursorTop = xOldPositionTop;
//      //    }
//      //}
//    }

//    private static void ClearMemory(uint aStartAddress, uint aLength) {
//      CPU.ZeroFill(aStartAddress, aLength);
//    }

//    private static bool mInited;
//    public static uint MemAlloc(uint aLength) {
//      if (!mInited) {
//        mInited = true;
//        Initialize(CPU.EndOfKernel, (CPU.AmountOfMemory * 1024 * 1024) - 1024);
//      }

//      uint xTemp = mStartAddress;

//      if ((xTemp + aLength) > (mStart + mLength)) {
//        Console.WriteLine("Too large memory block allocated!");
//        WriteNumber(aLength, 32);
//        while (true)
//          ;
//      }
//      mStartAddress += aLength;
//      UpdateDebugDisplay();
//      return xTemp;
//    }

//    public static void MemFree(uint aPointer) {
//    }

//  }

//  #region old
//  //	public unsafe static class Heap {
//  //		private enum MemoryBlockState: byte {
//  //			Free,
//  //			Used,
//  //			EndOfMemory
//  //		}
//  //		private unsafe struct MemoryBlock {
//  //			public MemoryBlockState State;
//  //			public MemoryBlock* Next;
//  //			public byte FirstByte;
//  //		}
//  //		private static uint mStart;
//  //		private static uint mStartAddress;
//  //		//		private static uint mCurrentAddress = mStartAddress;
//  //		private static uint mLength;
//  //		private static MemoryBlock* mFirstBlock;
//  //		//private const uint DefaultStartAddress = 4 * 1024 * 1024;
//  //		//private const uint DefaultMaxMemory = 32 * 1024 * 1024;
//  //
//  //		private static void ClearMemory(uint aStartAddress, uint aLength) {
//  //			//int xStart = (RTC.GetMinutes() * 60) + RTC.GetSeconds();
//  //			CPU.ZeroFill(aStartAddress, aLength);
//  //			//int xEnd = (RTC.GetMinutes() * 60) + RTC.GetSeconds();
//  //			//int xDiff = xEnd - xStart;
//  //			//Console.Write("Time to clear ");
//  //			//Hardware.Storage.ATAOld.WriteNumber((uint)xDiff, 32);
//  //			//Console.WriteLine("");
//  //		}
//  //
//  //		private static void Initialize(uint aStartAddress, uint aEndOfRam) {
//  //			mStart = mStartAddress = aStartAddress + (4 - (aStartAddress % 4));
//  //			mLength = aEndOfRam - aStartAddress;
//  //			mLength = (mLength / 4) * 4;
//  //			mStartAddress += 1024;
//  //			mStartAddress = (mStartAddress / 4) * 4;
//  //			mLength -= 1024;
//  //			//Console.Write("Clearing Memory at ");
//  //			int xCursorLeft = Console.CursorLeft;
//  //			// hack: try to get this working with the full chunk or chunks of 1MB
//  //			//const int xBlockSize = 1024 * 1024;
//  //			//for (uint i = 0; i < (mLength / xBlockSize); i++) {
//  //			//    Console.CursorLeft = xCursorLeft;
//  //			//    Hardware.Storage.ATAOld.WriteNumber(mStartAddress + (i * xBlockSize), 32);
//  //			//    ClearMemory(mStartAddress + (i * xBlockSize), xBlockSize);
//  //			//}
//  //			//Console.Write("Clearing Memory....");
//  //			ClearMemory(aStartAddress, mLength);
//  //			//Console.WriteLine("Done");
//  //			//mFirstBlock = (MemoryBlock*)aStartAddress;
//  //			//mFirstBlock->State = MemoryBlockState.Free;
//  //			//mFirstBlock->Next = (MemoryBlock*)(aStartAddress + mLength);
//  //			//mFirstBlock->Next->State = MemoryBlockState.EndOfMemory;
//  //			DebugUtil.SendMM_Init(mStartAddress, mLength);
//  //		}
//  //		private static bool mInited;
//  //		public static void Init() {
//  //			if (!mInited) {
//  //				mInited = true;
//  //				Initialize(CPU.EndOfKernel, (CPU.AmountOfMemory * 1024 * 1024) - 1024);
//  //			}
//  //		}
//  //
//  //		public static uint MemAlloc(uint aLength) {
//  //			Init();
//  //			uint xTemp = mStartAddress;
//  //			if ((xTemp + aLength) > (mStart + mLength)) {
//  //				Console.WriteLine("Too large memory block allocated!");
//  //				Console.Write("   BlockSize = ");
//  //                // dont use .ToString here, as it uses heap...
//  //                WriteNumber(aLength, 32);
//  //				Console.WriteLine("");
//  //				System.Diagnostics.Debugger.Break();
//  //			}
//  //			mStartAddress += aLength;
//  //			DebugUtil.SendMM_Alloc(xTemp, aLength);
//  //			return xTemp;
//  //			//CheckInit();
//  //			//MemoryBlock* xCurrentBlock = mFirstBlock;
//  //			//bool xFound = false;
//  //			//while (!xFound) {
//  //			//    if (xCurrentBlock->State == MemoryBlockState.EndOfMemory) {
//  //			//        DebugUtil.SendError("MM", "Reached maximum memory");
//  //			//        return 0;
//  //			//    }
//  //			//    if (xCurrentBlock->Next == null) {
//  //			//        DebugUtil.SendError("MM", "No next block found, but not yet at EOM", (uint)xCurrentBlock, 32);
//  //			//        return 0;
//  //			//    }
//  //			//    if (((((uint)xCurrentBlock->Next) - ((uint)xCurrentBlock)) >= (aLength + 5)) && (xCurrentBlock->State == MemoryBlockState.Free)) {
//  //			//        xFound = true;
//  //			//        break;
//  //			//    }
//  //			//    xCurrentBlock = xCurrentBlock->Next;
//  //			//}
//  //			//uint xFoundBlockSize = (((uint)xCurrentBlock->Next) - ((uint)xCurrentBlock));
//  //			//if (xFoundBlockSize > (aLength + 37)) {
//  //			//    MemoryBlock* xOldNextBlock = xCurrentBlock->Next;
//  //			//    xCurrentBlock->Next = (MemoryBlock*)(((uint)xCurrentBlock) + aLength + 5);
//  //			//    xCurrentBlock->Next->Next = xOldNextBlock;
//  //			//    xCurrentBlock->Next->State = MemoryBlockState.Free;
//  //			//}
//  //			//xCurrentBlock->State = MemoryBlockState.Used;
//  //			//DebugUtil.SendMM_Alloc((uint)xCurrentBlock, aLength);
//  //			//return ((uint)xCurrentBlock) + 5;
//  //		}
//  //
//  //		public static void MemFree(uint aPointer) {
//  //			//MemoryBlock* xBlock = (MemoryBlock*)(aPointer - 5);
//  //			//DebugUtil.SendMM_Free(aPointer - 5, (((uint)xBlock->Next) - ((uint)xBlock)));
//  //			//xBlock->State = MemoryBlockState.Free;
//  //			//uint xLength = ((uint)xBlock->Next) - aPointer;
//  //			//ClearMemory(aPointer, xLength);
//  //		}
//  //
//  //        public static void WriteNumber(uint aNumber, byte aBits)
//  //        {
//  //            uint xValue = aNumber;
//  //            byte xCurrentBits = aBits;
//  //            Console.Write("0x");
//  //            while (xCurrentBits >= 4)
//  //            {
//  //                xCurrentBits -= 4;
//  //                byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
//  //                string xDigitString = null;
//  //                switch (xCurrentDigit)
//  //                {
//  //                    case 0:
//  //                        xDigitString = "0";
//  //                        goto default;
//  //                    case 1:
//  //                        xDigitString = "1";
//  //                        goto default;
//  //                    case 2:
//  //                        xDigitString = "2";
//  //                        goto default;
//  //                    case 3:
//  //                        xDigitString = "3";
//  //                        goto default;
//  //                    case 4:
//  //                        xDigitString = "4";
//  //                        goto default;
//  //                    case 5:
//  //                        xDigitString = "5";
//  //                        goto default;
//  //                    case 6:
//  //                        xDigitString = "6";
//  //                        goto default;
//  //                    case 7:
//  //                        xDigitString = "7";
//  //                        goto default;
//  //                    case 8:
//  //                        xDigitString = "8";
//  //                        goto default;
//  //                    case 9:
//  //                        xDigitString = "9";
//  //                        goto default;
//  //                    case 10:
//  //                        xDigitString = "A";
//  //                        goto default;
//  //                    case 11:
//  //                        xDigitString = "B";
//  //                        goto default;
//  //                    case 12:
//  //                        xDigitString = "C";
//  //                        goto default;
//  //                    case 13:
//  //                        xDigitString = "D";
//  //                        goto default;
//  //                    case 14:
//  //                        xDigitString = "E";
//  //                        goto default;
//  //                    case 15:
//  //                        xDigitString = "F";
//  //                        goto default;
//  //                    default:
//  //                        Console.Write(xDigitString);
//  //                        break;
//  //                }
//  //            }
//  //        }
//  //	}
//  #endregion old
//}
