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
		return DoTheTest(mTypes) == 1 ? 0 : 1;
	}
	
	public static int DoTheTest(VTable[] mTypes)
	{
		VTable xTable = mTypes[0];
		int xResult=0;
		for(int i = 0; i < xTable.MethodIndexes.Length; i++)
		{
			int xTheValue = xTable.MethodIndexes[i];
			if(xTheValue == 45)
			{
				return 1;
			}
		}
		return xResult;
	}
}