using System;

class ConsoleDrv
{
	static int Main()
	{
		return TheTest(3) == 3 ? 0 : 1;
	}
	
	public static int TheTest(int aValue)
	{
		for(int i = 0; i < 5; i++)
		{
			if(i == aValue)
			{
				return i;
			}
		}
		return -1;
	}
}