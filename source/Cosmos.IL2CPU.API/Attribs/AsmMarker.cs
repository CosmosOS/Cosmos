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
    }

    public enum AsmMarkerType
    {
        INTs_LastKnownAddress,
        Processor_InterruptsEnabled
    }
}
