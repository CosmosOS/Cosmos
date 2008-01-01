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

		private List<Commands.ICommand> _commands;

		public override void Initialize() {


			string a = System.Console.ReadLine ();
			System.Console.WriteLine (a);
		}

		public override void Teardown() {
			
		}
	}
}
