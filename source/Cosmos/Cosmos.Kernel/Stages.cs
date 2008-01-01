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
					Console.WriteLine("Do Stages.Initialize now");
                }

                public static void Teardown() {
                        queue.Teardown ();
                }
        }
}
