using System;

public class ConsoleDrv
{
	static void Main()
	{
		int Value = Function2();
	}	
	
	public static int Function1()
	{
		int TestValue = 2;
		return TestValue;
	}
	
	public static int Function2()
	{
		int TestValue2 = Function1();
		int TestValue = 3;
		return TestValue + TestValue2;
	}
}