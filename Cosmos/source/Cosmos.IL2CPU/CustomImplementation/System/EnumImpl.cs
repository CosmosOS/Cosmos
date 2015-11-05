﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target = typeof(Enum))]
	public static class EnumImpl {
//		[PlugMethod(Signature = "System_Void__System_Enum__cctor__")]
		public static void Cctor() {
			//
		}

		public static bool Equals(Enum aThis, object aEquals) {
			throw new NotSupportedException("Enum.Equals not supported yet!");
		}

		//[PlugMethod(Signature = "System_String___System_Enum_ToString____")]
		public static string ToString(Enum aThis) {
      return "<Enum.ToString> not implemented";
//			return UInt32Impl.ToString(ref aThis);
		}
	}
}