using System;

namespace Cosmos.Core
{
    [AttributeUsageAttribute(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DeviceIDAttribute : Attribute
    {
        public UInt16 VendorID;
        public UInt16 DeviceID;
    }
}