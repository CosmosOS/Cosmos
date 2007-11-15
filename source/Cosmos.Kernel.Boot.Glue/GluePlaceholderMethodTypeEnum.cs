using System;

namespace Cosmos.Kernel.Boot.Glue {
	public enum GluePlaceholderMethodTypeEnum {
		/// <summary>
		/// calls lgdt
		/// </summary>
		GDT_Register,
		/// <summary>
		/// loads the gdt array stuff
		/// </summary>
		GDT_LoadArray,
		/// <summary>
		/// calls lidt
		/// </summary>
		IDT_Register,
		/// <summary>
		/// loads the idt array stuff
		/// </summary>
		IDT_LoadArray,
		IO_WriteByte,
		IO_ReadByte,
		IDT_EnableInterrupts,
		GetKernelResource
	}
}
