using System;

namespace Cosmos.IL2CPU.API.Attribs
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class AsmMarker : AsmLabel
    {
        private static string[] mAsmMarkers = new string[]
        {
            
        };

        public AsmMarker(AsmMarkerType aMarkerType) : base(mAsmMarkers[(int)aMarkerType])
        {
        }
    }

    public enum AsmMarkerType
    {
        
    }
}
