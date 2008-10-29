using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU {
	[Plug(TargetName="System.RuntimeType", IsMicrosoftdotNETOnly = true)]
	public static class RuntimeType {
		[PlugMethod(Signature="System_RuntimeType_RuntimeTypeCache__System_RuntimeType_get_Cache__")]
		public static IntPtr Cache_Get(IntPtr aThis) {
			throw new NotSupportedException("Reflection not supported");
		}

		public static string get_Name(uint aThis) {
			return "**Reflection not supported yet**";
		}

		[PlugMethod(Signature="System_Void__System_RuntimeType__cctor__")]
		public static void CCtor() {
			//
		}
	}
}