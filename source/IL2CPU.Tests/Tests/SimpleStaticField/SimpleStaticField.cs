using System;

class ConsoleDrv
{
	public static int SupposedLength = 5;
	public static string TheString = "Hello";
	static int Main()
	{
		if (TheString.Length == SupposedLength) {
			return 0;
		} else {
			return 1;
		}
	}
}