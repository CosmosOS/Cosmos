class Program
{
	static int Main()
	{
		int result = GetValue(false, true);
		return result == 0xB0 ? 0 : 1;
	}
	
	public static int GetValue(bool a, bool b)
	{
		if (a)
		{
			return 0xA0;
		}
		if (b) 
		{
			return 0xB0;
		}
		return 0x10;
	}
}