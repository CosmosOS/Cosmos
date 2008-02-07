using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Plugs {
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class FieldAccessAttribute: Attribute {
		public string Name;
	}
}
