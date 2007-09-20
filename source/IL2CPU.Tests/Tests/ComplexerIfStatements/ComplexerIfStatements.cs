using System;

class Program
{
	static void Main()
	{
		bool theParam = true;
		int theValue = ConditionalFunction(theParam);
	}
	
	public static int ConditionalFunction(bool aValue)
	{
		if(aValue) {
			if(aValue) {
				return 4;
			} else {
				return 6;
			}
		} else {
			return 2;
		}
	}
}