using System;

class Program
{
	static int Main()
	{
		int TheValue = Subtract(3, 2);
		return TheValue == 1 ? 0 : 1;
	}
	
	public static int Subtract(int a, int b)
	{
		return a - b;
	}
}