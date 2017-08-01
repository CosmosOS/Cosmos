using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.IL2CPU.API
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class AsmMarkerAttribute : AsmLabelAttribute
    {
        private static string[] mAsmMarkers = new string[]
        {
            
        };

        public AsmMarkerAttribute(AsmMarkerType aMarkerType) : base(mAsmMarkers[(int)aMarkerType])
        {
        }
    }

    public enum AsmMarkerType
    {
        
    }
}
