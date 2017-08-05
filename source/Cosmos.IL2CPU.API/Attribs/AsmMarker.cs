using System;
using System.Collections.Generic;

namespace Cosmos.IL2CPU.API.Attribs {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class AsmMarker : AsmLabel {
        public enum Type {
            Int_LastKnownAddress,
            //
            Processor_IntsEnabled,
            //
            DebugStub_CallerEIP,
            DebugStub_Init,
            DebugStub_SendNullRefEvent,
            DebugStub_SendSimpleNumber,
            DebugStub_SendStackCorruptedEvent,
            DebugStub_SendStackOverflowEvent,
            DebugStub_Step,
            DebugStub_TracerEntry
        }

        static public readonly Dictionary<Type, string> Labels = new Dictionary<Type, string>();

        static AsmMarker() {
            Labels.Add(Type.Int_LastKnownAddress, "INTs_LastKnownAddress");
            //
            Labels.Add(Type.Processor_IntsEnabled, "Processor_InterruptsEnabled");
            //
            Labels.Add(Type.DebugStub_CallerEIP, "DebugStub_CallerEIP");
            Labels.Add(Type.DebugStub_Init, "DebugStub_Init");
            Labels.Add(Type.DebugStub_SendNullRefEvent, "DebugStub_SendNullReferenceOccurred");
            Labels.Add(Type.DebugStub_SendSimpleNumber, "DebugStub_SendSimpleNumber");
            Labels.Add(Type.DebugStub_SendStackCorruptedEvent, "DebugStub_SendStackCorruptionOccurred");
            Labels.Add(Type.DebugStub_SendStackOverflowEvent, "DebugStub_SendStackOverflowOccurred");
            Labels.Add(Type.DebugStub_Step, "DebugStub_Step");
            Labels.Add(Type.DebugStub_TracerEntry, "DebugStub_TracerEntry");
        }

        public AsmMarker(Type aType)
          : base(Labels[aType]) {
        }
    }
}
