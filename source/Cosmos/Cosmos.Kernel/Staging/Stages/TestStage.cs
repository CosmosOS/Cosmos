using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.Staging.Stages {
        public class TestStage : IStage {
                #region IStage Members

                public string Name {
                        get {
                                return "Tests";
                        }
                }

                public void Initialize() {

                }

                public void Teardown() {
                }

                #endregion
        }
}
