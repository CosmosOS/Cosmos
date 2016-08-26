namespace Cosmos.Common
{
    public static class KernelPanicTypes
    {
        public static readonly uint VMT_MethodNotFound = 0x1;
        public static readonly uint VMT_MethodFoundButAddressInvalid = 0x2;
        public static readonly uint VMT_MethodAddressesNull = 0x3;
        public static readonly uint VMT_MethodIndexesNull = 0x4;
        public static readonly uint VMT_TypeIdInvalid = 0x5;
    }
}
