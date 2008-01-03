using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel.Staging.Stages;

namespace Cosmos.Kernel.Staging {
	public class DefaultStageQueue : StageQueue {
		public DefaultStageQueue()
			: base () {
			Enqueue (new Cosmos.Kernel.Staging.Stages.TestStage ());
		}
	}
}
