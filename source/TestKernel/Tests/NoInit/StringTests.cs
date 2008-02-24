using System;
using System.Collections.Generic;
using System.Linq;

namespace TestKernel.Tests.NoInit {
	public static class StringTests {
		[Test(TestGroup=TestGroups.NoInit, Description="System.String_EmbeddedStringContents", IsExperimental=false)]
		public static string TestEmbedded() {
			string MyString = "Hello";
			if (MyString == null)
				return "MyString is null";
			if (MyString.Length != 5)
				return "MyString Length is not 5";
			if (MyString[0] != 'H')
				return "MyString[0] != 'H'";
			if (MyString[1] != 'e')
				return "MyString[1] != 'e'";
			if (MyString[2] != 'l')
				return "MyString[2] != 'l'";
			if (MyString[3] != 'l')
				return "MyString[3] != 'l'";
			if (MyString[4] != 'o')
				return "MyString[4] != 'o'";
			return null;
		}
	}
}