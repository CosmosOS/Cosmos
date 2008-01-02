using System;

class Test
{
	static int Main()
	{
		long xLong = GetValue();
		if(xLong == 1) {
			return 0;
		} else {
			return 1;
		}
	}
	
	public static long GetValue()
	{
		return 1;
	}
}