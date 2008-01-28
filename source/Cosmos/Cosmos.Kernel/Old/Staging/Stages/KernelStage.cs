using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.Staging.Stages {
        public class KernelStage : StageBase {
                #region IStage Members

                public override string Name {
                        get {
                                return "Kernel";
                        }
                }

				public override void Initialize() {
                    System.Console.Clear();
                    System.Console.BackgroundColor = ConsoleColor.Black;
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine("Cosmos Kernel. Copyright 2007-2008 The Cosmos Project.");
                    System.Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                    System.Console.ForegroundColor = ConsoleColor.White;
                }

                public override void Teardown() {
                }

                #endregion
        }
}
