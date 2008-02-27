using System;
using System.Collections.Generic;
using System.Linq;

namespace TestKernel.Tests.NoInit {
	public static class StringTests {
		[Test(Name="Whoohoo", TestGroup="Test")]
		public static void OurTest() {
			// succeeds
			throw new Exception("Error");
		}
	}
}