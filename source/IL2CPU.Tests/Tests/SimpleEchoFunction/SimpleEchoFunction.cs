using System;

class Program
{
	public static int Main()
	{
		int xValue = 0;
		xValue = DoEcho(5);
		return (xValue == 5) ? 0 : 1;
	}
	
	public static int DoEcho(int aValue)
	{
		return aValue;
	}
}