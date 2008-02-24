using System;
using System.Collections.Generic;
using System.Linq;

namespace TestKernel {
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class TestAttribute: Attribute {
		public TestAttribute() {
		}

		public string TestGroup {
			get;
			set;
		}

		public string Description {
			get;
			set;
		}

		public bool IsExperimental {
			get;
			set;
		}
	}
}
