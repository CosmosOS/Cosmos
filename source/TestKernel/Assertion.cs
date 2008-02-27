using System;
using System.Collections.Generic;
using System.Linq;

namespace NUnit.Framework {
	public class Assertion {
		public class AssertException:Exception {
			public AssertException(string aMessage):base(aMessage) {
			}
		}
		public static void AssertNotNull(string aMessage, object aObject) {
			if (aObject == null) {
				throw new AssertException(aMessage);
			}
		}
		public static void Assert(string aMessage, bool aAssert) {
			if (!aAssert) {
				throw new AssertException(aMessage);
			}
		}

		public static void Fail(string aMessage) {
			throw new AssertException(aMessage);
		}
		public static void AssertEquals(string aMessage, int a, int b) {
			if (a != b) {
				Fail(aMessage);
			}
		}
		public static void AssertEquals(string aMessage, object a, object b) {
			if (!Object.Equals(a, b)) {
				Fail(aMessage);
			}
		}
	}
}