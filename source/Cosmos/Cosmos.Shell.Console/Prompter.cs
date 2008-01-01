using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel.Staging;

namespace Cosmos.Shell.Console {
	/// <summary>
	/// The demonstration prompter.
	/// </summary>
	public class Prompter : StageBase {
		public override string Name {
			get {
				return "Console";
			}
		}

		private List<Commands.CommandBase> _commands;

		public override void Initialize() {

			while (true) {
				string a = System.Console.ReadLine ();
				System.Console.WriteLine (a);
			}
		}

		public override void Teardown() {
			
		}
	}
}
