using System;
using System.Reflection;

namespace Cosmos.IL2CPU.API.Attribs
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class AsmMarker : AsmLabel
    {
        public AsmMarker(AsmMarkerType aMarkerType)
          : base((string)typeof(AsmMarker).GetRuntimeField(aMarkerType.ToString()).GetRawConstantValue())
        {
        }

        public const string INTs_LastKnownAddress = "INTs_LastKnownAddress";
        /// <summary>
        /// Bool field. Be careful when changing the value, because it's only 1 byte.
        /// </summary>
        public const string Processor_InterruptsEnabled = "Processor_InterruptsEnabled";
        public const string DebugStub_CallerEIP = "DebugStub_CallerEIP";
        public const string DebugStub_Init = "DebugStub_Init";
        public const string DebugStub_SendNullReferenceOccurred = "DebugStub_SendNullReferenceOccurred";
        public const string DebugStub_SendSimpleNumber = "DebugStub_SendSimpleNumber";
        public const string DebugStub_SendStackCorruptionOccurred = "DebugStub_SendStackCorruptionOccurred";
        public const string DebugStub_SendStackOverflowOccurred = "DebugStub_SendStackOverflowOccurred";
        public const string DebugStub_Step = "DebugStub_Step";
        public const string DebugStub_TracerEntry = "DebugStub_TracerEntry";
    }

    public enum AsmMarkerType
    {
        INTs_LastKnownAddress,
        Processor_InterruptsEnabled,
        DebugStub_CallerEIP,
        DebugStub_Init,
        DebugStub_SendNullReferenceOccurred,
        DebugStub_SendSimpleNumber,
        DebugStub_SendStackCorruptionOccurred,
        DebugStub_SendStackOverflowOccurred,
        DebugStub_Step,
        DebugStub_TracerEntry
    }
}
