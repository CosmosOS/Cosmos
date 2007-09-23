using System;

class ConsoleDrv
{
	static void Main()
	{
		int[] xTestValues = new int[] {1, 2, 3, 4};
		int xSum = xTestValues[0] + xTestValues[1] + xTestValues[2] + xTestValues[3];
		xTestValues[0] = 10;
		xSum = xSum + xTestValues[0] + xTestValues[1] + xTestValues[2] + xTestValues[3];
	}
}