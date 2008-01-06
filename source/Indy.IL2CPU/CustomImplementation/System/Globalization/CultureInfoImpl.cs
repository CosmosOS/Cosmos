using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using System.Globalization;

namespace Indy.IL2CPU.CustomImplementation.System.Globalization {
	[Plug(Target = typeof(CultureInfo))]
	public static class CultureInfoImpl {
		public static CultureInfo get_CurrentCulture() {
			return null;
		}
	}
}