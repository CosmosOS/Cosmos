using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Cosmos.IL2CPU {
    // todo: optimize this, probably using assembler
	public static class VTablesImpl {
		// this field seems to be always empty, but the VTablesImpl class is embedded in the final exe.
		public static VTable[] mTypes;
		public static bool IsInstance(int aObjectType, int aDesiredObjectType) {
			int xCurrentType = aObjectType;
			if (aObjectType == 0) {
				return true;
			}
			do {
				if (xCurrentType == aDesiredObjectType) {
					return true;
				}
				if (xCurrentType == mTypes[xCurrentType].BaseTypeIdentifier) {
					return false;
				}
				xCurrentType = mTypes[xCurrentType].BaseTypeIdentifier;
			} while (xCurrentType != 0);
			return false;
		}

		public static void LoadTypeTable(int aTypeCount) {
			mTypes = new VTable[aTypeCount];
            if (mTypes == null) {
                Console.WriteLine("No array exists!");
            }
		}

    public static void SetTypeInfo(int aType, int aBaseType, int[] aMethodIndexes, int[] aMethodAddresses, int aMethodCount) {
      //mTypes[aType] = new VTable();
      mTypes[aType].BaseTypeIdentifier = aBaseType;
      mTypes[aType].MethodIndexes = aMethodIndexes;
      mTypes[aType].MethodAddresses = aMethodAddresses;
      mTypes[aType].MethodCount = aMethodCount;
    }

		public static void SetMethodInfo(int aType, int aMethodIndex, int aMethodIdentifier, int aMethodAddress, char[] aName) {
			mTypes[aType].MethodIndexes[aMethodIndex] = aMethodIdentifier;
			mTypes[aType].MethodAddresses[aMethodIndex] = aMethodAddress;
            mTypes[aType].MethodCount = aMethodIndex + 1;
		}

		private static void WriteNumber(uint aValue, byte aBitCount) {
			uint xValue = aValue;
			byte xCurrentBits = aBitCount;
			Console.Write("0x");
			while (xCurrentBits >= 4) {
				xCurrentBits -= 4;
				byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
				string xDigitString = null;
				switch (xCurrentDigit) {
					case 0:
						xDigitString = "0";
						goto default;
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
						Console.Write(xDigitString);
						break;
				}
			}
		}

		public static int GetMethodAddressForType(int aType, int aMethodIndex) {
            	do {
            		if (mTypes[aType].MethodIndexes == null) {
							Console.Write("Type ");
					        WriteNumber((uint)aType, 32);
					        Console.WriteLine(", MethodIndexes is null!");
					        while(true) ;
					}
					for (int i = 0; i < mTypes[aType].MethodIndexes.Length; i++) {
						if (mTypes[aType].MethodAddresses == null) {
							Console.Write("Type ");
					        WriteNumber((uint)aType, 32);
					        Console.WriteLine(", MethodAddresses is null!");
					        while(true) ;
						}
						if (mTypes[aType].MethodIndexes[i] == aMethodIndex) {
							var xResult = mTypes[aType].MethodAddresses[i];
							if (xResult < 1048576) // if pointer is under 1MB, some issue exists!
							{
								Console.Write("Type ");
						        WriteNumber((uint)aType, 32);
						        Console.Write(", MethodIndex = ");
						        WriteNumber((uint)aMethodIndex, 32);
						        Console.WriteLine("");
						        Console.WriteLine("Method found, but address invalid!");
						        while(true) ;
							}
							return xResult;
						}
					}
                    if (aType == mTypes[aType].BaseTypeIdentifier)
                    {
                        break;
                    }
					aType = mTypes[aType].BaseTypeIdentifier;
				} while (true);
			//}
        Console.Write("Type ");
        WriteNumber((uint)aType, 32);
        Console.Write(", MethodIndex = ");
        WriteNumber((uint)aMethodIndex, 32);
        Console.WriteLine("");
        Console.WriteLine("Not FOUND!");
        while (true)
          ;
			throw new Exception("Cannot find virtual method!");
		}
	}
	[StructLayout(LayoutKind.Explicit, Size = 16)]
	public struct VTable {
		[FieldOffset(0)]
		public int BaseTypeIdentifier;
		[FieldOffset(4)]
		public int MethodCount;
		[FieldOffset(8)]
		public int[] MethodIndexes;
		[FieldOffset(12)]
		public int[] MethodAddresses;
	}
}
