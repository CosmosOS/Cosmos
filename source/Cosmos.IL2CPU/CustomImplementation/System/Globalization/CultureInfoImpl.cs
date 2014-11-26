using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using System.Globalization;

namespace Cosmos.IL2CPU.CustomImplementation.System.Globalization {
	[Plug(Target = typeof(CultureInfo))]
	public static class CultureInfoImpl {
		public static CultureInfo get_CurrentCulture() {
			return null;
		}
        
        public static CultureInfo get_InvariantCulture()
        {
            return null;
        }

        //[PlugMethod(Signature = "System_Void__System_Globalization_CultureInfo__cctor__")]
        public static void CCtor()
        {
        }

        public static bool Equals(CultureInfo aThis, object aThat)
        {
            return false;
        }
	}
}