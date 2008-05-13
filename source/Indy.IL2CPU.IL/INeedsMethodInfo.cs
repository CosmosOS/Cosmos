using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL {
    public interface INeedsMethodInfo {
        MethodInformation MethodInfo {
            set;
        }
    }
}