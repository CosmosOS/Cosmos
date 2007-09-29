using System;

class Program
{
	static int Main()
	{
		bool theParam = true;
		int theValue = ConditionalFunction(theParam);
		return (theValue == 4) ? 0 : 1;
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