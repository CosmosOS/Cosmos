using System;

namespace Cosmos.Kernel.Boot.Glue {
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class KernelResourceAttribute: Attribute {
		public readonly string ResourceName;
		public readonly int Identifier;

		public KernelResourceAttribute(string aResourceName, int aIdentifier) {
			ResourceName = aResourceName;
			Identifier = aIdentifier;
		}
	}
}
