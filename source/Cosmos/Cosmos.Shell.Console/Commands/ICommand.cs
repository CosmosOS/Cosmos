using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console.Commands {
	/// <summary>
	/// Represents a command.
	/// </summary>
	public abstract class ICommand {
		/// <summary>
		/// Gets the name of the command (must be lowercase).
		/// </summary>
		public abstract string Name {
			get;
		}

		public abstract void Execute();
	}
}
