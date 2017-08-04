using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cosmos.IL2CPU.API.Attribs {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class AsmMarker : AsmLabel {
        public enum Type {
            INTs_LastKnownAddress,
            //
            Processor_InterruptsEnabled,
            //
            DebugStub_CallerEIP,
            DebugStub_Init,
            DebugStub_SendNullReferenceOccurred,
            DebugStub_SendSimpleNumber,
            DebugStub_SendStackCorruptionOccurred,
            DebugStub_SendStackOverflowOccurred,
            DebugStub_Step,
            DebugStub_TracerEntry
        }

        static public readonly Dictionary<Type, string> Labels = new Dictionary<Type, string>();

        static AsmMarker() {
            Labels.Add(Type.INTs_LastKnownAddress, "INTs_LastKnownAddress");
            //
            Labels.Add(Type.Processor_InterruptsEnabled, "Processor_InterruptsEnabled");
            //
            Labels.Add(Type.DebugStub_CallerEIP, "DebugStub_CallerEIP");
            Labels.Add(Type.DebugStub_Init, "DebugStub_Init");
            Labels.Add(Type.DebugStub_SendNullReferenceOccurred, "DebugStub_SendNullRefEvent");
            Labels.Add(Type.DebugStub_SendSimpleNumber, "DebugStub_SendSimpleNumber");
            Labels.Add(Type.DebugStub_SendStackCorruptionOccurred, "DebugStub_SendStackCorruptEvent");
            Labels.Add(Type.DebugStub_SendStackOverflowOccurred, "DebugStub_SendStackOverflowEvent");
            Labels.Add(Type.DebugStub_Step, "DebugStub_Step");
            Labels.Add(Type.DebugStub_TracerEntry, "DebugStub_TracerEntry");
        }

        public AsmMarker(Type aType)
          : base(Labels[aType]) {
        }
    }

}
