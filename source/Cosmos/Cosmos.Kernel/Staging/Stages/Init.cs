using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.Staging.Stages {
        public class Init : IStage {
                #region IStage Members

                public string Name {
                        get {
                                return "Init";
                        }
                }

                public void Initialize() {
                        CPU.Init ();
                }

                public void Teardown() {

                }

                #endregion
        }
}
