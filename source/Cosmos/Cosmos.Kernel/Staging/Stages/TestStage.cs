using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.Staging.Stages {
        public class TestStage : IStage {
                #region IStage Members

                public override string Name {
                        get {
                                return "Tests";
                        }
                }

				public override void Initialize() {
					//CPU.TestATA ();
                }

                public override void Teardown() {
                }

                #endregion
        }
}
