using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console.Commands {
	/// <summary>
	/// Represents a command.
	/// </summary>
	public abstract class CommandBase {
		/// <summary>
		/// Gets the name of the command (must be lowercase).
		/// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Gets the summary for the command.
        /// </summary>
        public abstract string Summary
        {
            get;
        }

		public abstract void Execute(string param);

        public abstract void Help();
        
        /*
        public virtual void Help()
        {
            System.Console.WriteLine(Name);
            System.Console.WriteLine(" " + Summary);
        }
        */
	}
}
