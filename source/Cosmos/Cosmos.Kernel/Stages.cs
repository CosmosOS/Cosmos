using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
        public class Stages {
                private static Staging.DefaultStageQueue queue;

                public static void Initialize() {
                        CPU.Init ();

                        //queue = new Cosmos.Kernel.Staging.DefaultStageQueue ();
                        //queue.Run ();
                }

                public static void Teardown() {
                        queue.Teardown ();
                }
        }
}
