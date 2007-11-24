using System;
using System.Collections.Generic;

class test
{
	static int Main()
	{
		List<int> xItems = new List<int>();
		xItems.Add(1);
		xItems.Add(2);
		xItems.Add(3);
		int aReturn = 6;
		for(int i = 0; i < xItems.Count; i++)
		{
			aReturn -= xItems[i];			
		}
		xItems.Clear();
		return aReturn;
	}
}