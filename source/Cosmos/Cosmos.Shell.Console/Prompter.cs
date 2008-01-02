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
            _commands = new List<Cosmos.Shell.Console.Commands.CommandBase>();
            _commands.Add(new Commands.TypeCommand());

            while (running)
            {
                System.Console.Write("/> ");
                string line = System.Console.ReadLine();
                int index = line.IndexOf(' ');
                string command;
                string param;
                if (index == -1)
                {
                    command = line;
                    param = "";
                }
                else
                {
                    command = line.Substring(0, index);
                    param = line.Substring(index + 1);
                }

                for (int i = 0; i < _commands.Count; i++)
                {

                    if (_commands[i].Name.CompareTo(command) == 0)
                    {
                        _commands[i].Execute(param);
                        break;
                    }
                }
                
            }
		}

        public override void Teardown()
        {
			
		}
	}
}
