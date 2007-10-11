using System;

class ConsoleDrv
{
	public class BaseClass
	{
		public virtual int MyFunc()
		{
			return 1;
		}
	}
	
	public class SubClass: BaseClass
	{
		public override int MyFunc()
		{
			return 0;
		}
	}
	static void Main()
	{
		BaseClass item = new SubClass();
		int testValue = item.MyFunc();
	}
}