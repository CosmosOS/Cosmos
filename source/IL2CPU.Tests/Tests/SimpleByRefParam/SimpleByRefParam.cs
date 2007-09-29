using System;

class ConsoleDrv
{
	static int Main()
	{
		int theValue = 2;
		DoCalc(ref theValue);
		int theValue2 = theValue;
		return (theValue == 4) ? 0 : 1;
	}
	
	public static void DoCalc(ref int aValue)
	{
		aValue += aValue;
	}
}