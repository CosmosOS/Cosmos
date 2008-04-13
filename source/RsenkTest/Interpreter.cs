using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RsenkTest.Commands;

namespace RsenkTest
{
    class Interpreter
    {
        private List<CommandBase> _commands;

        public Interpreter(List<CommandBase> commands)
        {
            _commands = commands;
        }

        public CommandBase ParseCommand(string line)
        {
            string command = line.Substring(0, line.IndexOf(' '));

            CommandBase commandToRet = CheckCommand(command);

            return commandToRet;
        }

        private CommandBase CheckCommand(string comm)
        {
            CommandBase commandToRet = _commands.Find(delegate(CommandBase command)
            {
                return command.Name.Equals(comm);
            });

            return commandToRet;
        }
    }
}
