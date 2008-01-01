using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.Staging.Stages {
	class MatthijsStage : IStage {
		public override string Name {
			get {
				return "Matthijs";
			}
		}

		public override void Initialize() {
			CPU.TestATA ();
		}

		public override void Teardown() {
		}
	}
}
