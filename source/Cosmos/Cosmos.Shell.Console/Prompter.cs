using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel.Staging;

namespace Cosmos.Shell.Console {
	/// <summary>
	/// The demonstration prompter.
	/// </summary>
	public class Prompter : IStage {
		public override string Name {
			get {
				return "Console";
			}
		}

		public override void Initialize() {
		}

		public override void Teardown() {
			
		}
	}
}
