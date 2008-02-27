using System;
using System.Collections.Generic;
using System.Linq;
using TestKernel;

namespace System {
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class TestFixtureAttribute:Attribute {
		public string BaseName {
			get;
			set;
		}
	}
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class TestAttribute: Attribute {
		public TestAttribute() {
			TestGroup = TestGroups.NoInit;
		}

		public string TestGroup {
			get;
			set;
		}

		public string Name{
			get;
			set;
		}

		public bool IsExperimental {
			get;
			set;
		}
	}
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class ExpectedExceptionAttribute: Attribute {
		public ExpectedExceptionAttribute(Type aException) {
			Exception = aException;
		}
		public Type Exception {
			get;
			set;
		}
	}
}
