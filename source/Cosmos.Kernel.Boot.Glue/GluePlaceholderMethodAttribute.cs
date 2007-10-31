using System;

namespace Cosmos.Kernel.Boot.Glue {
	[AttributeUsage(AttributeTargets.Method)]
	public class GluePlaceholderMethodAttribute: Attribute {
		public GluePlaceholderMethodTypeEnum MethodType {
			get;
			set;
		}
	}
}
