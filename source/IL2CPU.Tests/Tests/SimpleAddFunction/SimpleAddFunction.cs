using System;

class Program
{
	static int Main()
	{
		int theValue = Add(1, 2);
		return theValue == 3 ? 0 : 1;
	}
	
	public static int Add(int a, int b)
	{
		return a + b;
	}
}