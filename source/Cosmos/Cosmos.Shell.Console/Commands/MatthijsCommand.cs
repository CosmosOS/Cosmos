using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console.Commands {
	public class MatthijsCommand: CommandBase {
		public override string Name {
			get {
				return "matthijs";
			}
		}

		public override string Summary {
			get {
				return "Executes tests Matthijs is working on. DO NOT EXECUTE WITHOUT INVESTIGATING WHAT IT DOES!!";
			}
		}

		public override void Execute(string param) {
			//Kernel.FileSystem.TestsMatthijs.TestNewATA();
			//System.Diagnostics.Debugger.Break();
			throw new Exception("Hello, Error!");
		}

		public override void Help() {
			System.Console.WriteLine(Summary);
		}
	}
}
