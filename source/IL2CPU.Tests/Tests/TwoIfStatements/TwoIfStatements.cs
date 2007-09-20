class Program
{
	static void Main()
	{
		int result = GetValue(false, true);
		
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