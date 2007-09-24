using System;

class ConsoleDrv
{
	static void Main()
	{
		int theValue = 2;
		DoCalc(ref theValue);
		int theValue2 = theValue;
	}
	
	public static void DoCalc(ref int aValue)
	{
		aValue += aValue;
	}
}