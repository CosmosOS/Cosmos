using System;

class ConsoleDrv
{
	static int Main()
	{
		DataStruct ds;
		ds.Data1 = 1;
		ds.Data2 = 2;
		ds.Data3 = 3;
		ds.Data4 = 4;
		if (DoSum(ds) == 10) {
			return 0;
		} else {
			return 1;
		}
	}
	
	public static int DoSum(DataStruct ds)
	{
		return  ds.Data1 + 
						ds.Data2 + 
						ds.Data3 + 
						ds.Data4;
	}
	public struct DataStruct
	{
		public int Data1;
		public int Data2;
		public int Data3;
		public int Data4;
	}
}