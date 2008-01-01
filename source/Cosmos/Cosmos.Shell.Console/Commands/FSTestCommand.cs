using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console.Commands {
	public class FSTestCommand : CommandBase {
		public override string Name {
			get {
				return "fstest";
			}
		}

		public override void Execute() {
			System.Console.WriteLine ("Testing...");
		}
	}
}
