using System;

class ConsoleDrv
{
	public struct VTable {
		public int BaseTypeIdentifier;
		public char[] Name;
		public int[] MethodIndexes;
		public int[] MethodAddresses;
	}

	private static VTable[] mTypes;
	
	public static int Main()
	{
		mTypes = new VTable[1];
		mTypes[0] = new VTable();
		mTypes[0].MethodIndexes = new int[2];
		mTypes[0].MethodAddresses = new int[2];
		mTypes[0].MethodIndexes[0] = 14;
		mTypes[0].MethodIndexes[1] = 45;
		mTypes[0].MethodAddresses[0] = 55;
		mTypes[0].MethodAddresses[1] = 2;
		return DoTheTest(0, 45) == 2 ? 0 : 1;
	}
	
	public static int DoTheTest(int aType, int aMethodIndex)
	{
		VTable xTable = mTypes[aType];
		for(int i = 0; i < xTable.MethodIndexes.Length; i++)
		{
			if(xTable.MethodIndexes[i] == aMethodIndex)
			{
				return xTable.MethodAddresses[i];
			}
		}
		return 0x00000000;
	}
}