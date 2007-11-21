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
		Console.WriteLine("-----Start Static field test");
		StaticFieldTest();
		Console.WriteLine("-----End Static field test");
		Console.WriteLine("-----Start Static field array");
		StaticArrayTest();
		Console.WriteLine("-----End Static field array");
		Console.WriteLine("-----Start Instance field test");
		SimpleInstanceTest();
		Console.WriteLine("-----End Instance field test");
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
		MyArray = new object[2];
		MyArray[0] = new object();
		MyArray[1] = new object();
		Console.WriteLine("End of method");
	}
	
	private static object mTheObject;
	public static void StaticFieldTest()
	{
		mTheObject = null;
		mTheObject = new Object();
		mTheObject = null;
		mTheObject = new Object();
		mTheObject = new Object();
		mTheObject = null;
	}
	
	private static object[] mTheArray;
	public static void StaticArrayTest()
	{
		mTheArray = new object[2];
		mTheArray[0] = new object();
		mTheArray[1] = new object();
		mTheArray = new object[2];
		mTheArray[0] = new object();
		mTheArray[1] = new object();
		mTheArray = null;
	}
	
	private class TheData
	{
		public object InternalData;
	}
	
	public static void SimpleInstanceTest()
	{
		TheData x = new TheData();
		x = null;
		x = new TheData();
		x.InternalData = new Object();
		x = null;
	}
}