using System;

class Test
{
	static void Main()
	{
		Console.WriteLine("-----Start Locals and Params test");
		LocalsAndParamsTest();
		Console.WriteLine("-----End Locals and Params test");
		Console.WriteLine("-----Start Empty Array test");
		EmptyArrayTest();
		Console.WriteLine("-----End Empty Array test");		
		Console.WriteLine("-----Start Array test");
		ArrayTest();
		Console.WriteLine("-----End Array test");		
	}
	static void LocalsAndParamsTest()
	{
		object MyObject = new Object();
		MyObject = null;
		MyObject = new Object();
		MyObject = new Object();
		Console.WriteLine("@Enter Sub");
		MyTestSubmethod(MyObject);
		Console.WriteLine("@Exit Sub");
	}
	
	public static void MyTestSubmethod(object aParam)
	{
		object TempArg = aParam;
		TempArg = null;
	}
	
	public static void EmptyArrayTest()
	{
		object[] x = new object[0];
	}
	
	public static void ArrayTest()
	{
		object[] MyArray = new object[2];
		MyArray[0] = new object();
		MyArray[1] = new object();
		Console.WriteLine("End of method");
	}
}