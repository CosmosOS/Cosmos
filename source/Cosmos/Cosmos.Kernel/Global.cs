using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
    public class Global {
        public static void Init() {
            //Init Heap first - Hardware loads devices and they need heap
			Heap.Init();
        }
    }
}
