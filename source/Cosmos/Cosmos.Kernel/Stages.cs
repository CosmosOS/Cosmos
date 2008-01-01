using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
        public class Stages {
                private static Staging.DefaultStageQueue queue = new Cosmos.Kernel.Staging.DefaultStageQueue ();

                public static void Initialize() {
                        queue.Run ();
                }

                public static void Teardown() {
                        queue.Teardown ();
                }
        }
}
