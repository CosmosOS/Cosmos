using System;

namespace Cosmos.IL2CPU.API.Attribs 
 {
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class FieldAccess : Attribute {
		public string Name;
	}
}
