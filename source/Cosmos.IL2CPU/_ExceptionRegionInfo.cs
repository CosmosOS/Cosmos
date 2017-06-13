using System.Reflection;
using System.Reflection.Metadata;

namespace Cosmos.IL2CPU
{
    public class _ExceptionRegionInfo
    {
        public readonly Module Module;
        public readonly ExceptionRegion ExceptionRegion;
        public readonly int HandlerOffset;
        public readonly int HandlerLength;
        public readonly int TryOffset;
        public readonly int TryLength;
        public readonly int FilterOffset;
        public readonly ExceptionRegionKind Kind;

        public _ExceptionRegionInfo(Module aModule, ExceptionRegion aExceptionRegion)
        {
            Module = aModule;
            ExceptionRegion = aExceptionRegion;
            HandlerOffset = aExceptionRegion.HandlerOffset;
            HandlerLength = aExceptionRegion.HandlerLength;
            TryOffset = aExceptionRegion.TryOffset;
            TryLength = aExceptionRegion.TryLength;
            FilterOffset = aExceptionRegion.FilterOffset;
            Kind = aExceptionRegion.Kind;
        }
    }
}
