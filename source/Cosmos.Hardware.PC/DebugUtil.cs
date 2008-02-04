using System;
using System.Collections.Generic;
using System.Linq;

namespace Cosmos.Hardware.PC {
	public static class DebugUtil {
		public static unsafe void LogInterruptOccurred(Interrupts.InterruptContext* aContext) {
			uint aInterrupt = aContext->Interrupt;
			Cosmos.Hardware.DebugUtil.StartLogging();
			Cosmos.Hardware.DebugUtil.WriteSerialString("<InterruptOccurred Interrupt=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->Interrupt, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" SS=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->SS, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" GS=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->GS, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" FS=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->FS, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" ES=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->ES, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" DS=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->DS, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" CS=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->CS, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" ESI=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->ESI, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" EDI=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->EDI, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" EBP=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->EBP, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" ESP=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->ESP, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" EBX=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->EBX, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" EDX=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->EDX, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" ECX=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->ECX, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" EAX=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->EAX, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" Param=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->Param, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" EFlags=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->EFlags, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\" UserESP=\"");
			Cosmos.Hardware.DebugUtil.WriteNumber(aContext->UserESP, 32);
			Cosmos.Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
			Cosmos.Hardware.DebugUtil.EndLogging();
		}
	}
}
