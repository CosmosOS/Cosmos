using System;

public class ConsoleDrv {
	static int Main() {
		int Value = Function2();
		return Value == 5 ? 0 : 1;
	}

	public static int Function1() {
		int TestValue = 2;
		return TestValue;
	}

	public static int Function2() {
		int TestValue2 = Function1();
		int TestValue = 3;
		return TestValue + TestValue2;
	}
}