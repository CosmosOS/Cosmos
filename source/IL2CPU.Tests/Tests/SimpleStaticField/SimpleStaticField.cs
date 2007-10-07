using System;

class ConsoleDrv {
	public static int SupposedLength;
	static int Main() {
		string MyString = "Hello";
		SupposedLength = 5;
		if (MyString.Length != SupposedLength) {
			return 1;
		}
		SupposedLength = 2;
		return 0;
	}
}