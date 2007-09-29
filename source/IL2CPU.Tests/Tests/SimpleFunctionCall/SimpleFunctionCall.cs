using System;

public static class Program
{
	public static int Main()
	{
		int theValue = TheMethod();
		return (theValue == 5) ? 0 : 1;
	}
	public static int TheMethod()
	{
		return 5;
	}
}