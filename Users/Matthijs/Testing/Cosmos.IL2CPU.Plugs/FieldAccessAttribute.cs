using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPU.API
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class FieldAccessAttribute: Attribute {
		public string Name;
	}
}
