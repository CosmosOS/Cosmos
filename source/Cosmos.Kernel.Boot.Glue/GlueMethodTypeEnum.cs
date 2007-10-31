using System;

namespace Cosmos.Kernel.Boot.Glue {
	public enum GlueMethodTypeEnum {
		SaveBootInfoStruct,
		IDT_SetHandler,
		IDT_InterruptHandler
	}
}
