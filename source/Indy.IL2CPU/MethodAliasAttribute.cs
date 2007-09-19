using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class MethodAliasAttribute: Attribute {
		public string Name;
	}
}
