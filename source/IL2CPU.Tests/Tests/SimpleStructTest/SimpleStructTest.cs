using System;

class ConsoleDrv
{
	public struct TestData
	{
		public int Value;
	}
	public static void Main()
	{
		TestData data1;
		data1.Value = 2;
		TestData data2;
		data2.Value = 5;
		int theValue = data1.Value + data2.Value;
	}
}