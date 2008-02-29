using System;
using System.Collections.Generic;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target=typeof(ValueType))]
	public class ValueTypeImpl {
		public static bool Equals(ValueType aThis, object aObject) {
			throw new NotImplementedException("ValueType.Equals Not Implemented!");
		}
	}
}
