using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target=typeof(System.Type))]
	public static class Type {
		[PlugMethod(Signature="System_Void__System_Type__cctor__")]
		public static void CCtor() {
		}

        [PlugMethod(Signature = "System_Type__System_Type_GetTypeFromHandle_System_RuntimeTypeHandle_")]
        public static uint GetTypeFromHandle(uint aHandle) {
            return aHandle;
        }

	    //System.Type  System.Type.GetTypeFromHandle(System.RuntimeTypeHandle)
	}
}