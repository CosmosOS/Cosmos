using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    [AttributeUsageAttribute(AttributeTargets.Class, AllowMultiple=true)]
    public sealed class DeviceIDAttribute : Attribute {
        public UInt16 VendorID;
        public UInt16 DeviceID;
    }
}
