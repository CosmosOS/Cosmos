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

        private bool running = true;

        public void Stop()
        {
            running = false;
        }

		public override void Initialize() {
            while (running)
            {
                string line = System.Console.ReadLine();
                int index = IndexOf(line, ' ') + 1;
                string command = Substring(line, index);
                System.Console.WriteLine(command);
            }
		}

        private int IndexOf(string source, char look)
        {
            for (int i = 0; i < source.Length; i++)
                if (source[i] == look)
                    return i;
            return -1;
        }

        private string Substring(string source, int index)
        {
            List<Char> target = new List<char>();
            for (int i = index; i < source.Length; i++)
                target.Add(source[i]);

            // HACK: Should use .ToArray here.
            char[] final = new char[target.Count];
            for (int i = 0; i < final.Length; i++)
                final[i] = target[i];

            return new string(final);
        }

        public override void Teardown()
        {
			
		}
	}
}
