using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.ELF {
    public enum PlatformEnum {
        I386
    }
    public class CurrentPlatformInfo {
        public static PlatformEnum Platform {
            get;
            set;
        }
    }
}