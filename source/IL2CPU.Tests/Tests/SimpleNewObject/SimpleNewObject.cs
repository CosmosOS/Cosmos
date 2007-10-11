using System;

class ConsoleDrv
{
	public static int Main()
	{
		Object obj = new Object();
		if(obj == null) {
			return 1;
		} else {
			return 0;
		}
	}
}