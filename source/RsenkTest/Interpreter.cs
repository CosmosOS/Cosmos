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

        /// <summary>
        /// Parses the command off. Reads everything up to the first space.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>Returns the command if it exists, otherwise null.</returns>
        public CommandBase ParseCommand(string line)
        {
            string command = line.Substring(0, line.IndexOf(' '));

            CommandBase commandToRet = CheckCommand(command);

            return commandToRet;
        }

        /// <summary>
        /// Checks to see if the command is valid.
        /// </summary>
        /// <param name="comm"></param>
        /// <returns>Returns the command in the form of CommandBase if found, otherwise returns null.</returns>
        private CommandBase CheckCommand(string comm)
        {
            CommandBase commandToRet = _commands.Find(delegate(CommandBase command)
            {
                return command.Name.Equals(comm);
            });

            return commandToRet;
        }

        public ParameterBase[] ParseParameters(CommandBase command, string line)
        {
            ParameterBase[] parameters = new ParameterBase[0];
            line = line.Substring(command.Name.Length + 1).Trim();
            bool invalidArg = false;

            if ((command != null) && (String.IsNullOrEmpty(line)))
            {
                List<ParameterBase> paramsTemp = new List<ParameterBase>();

                while (line.Length > 0)
                {
                    string param = line.Substring(0, line.IndexOf(' '));
                    Console.WriteLine(param);
                    line = line.Substring(line.IndexOf(' '));
                    Console.WriteLine(line);

                    if (param.Equals(line))
                    {
                        break;
                    }

                    ParameterBase temp = ValidParam(command, param);

                    if (temp != null)
                        paramsTemp.Add(temp);
                    else
                    {
                        invalidArg = true;
                        break;
                    }
                }

                if (line.Trim().Length > 0)
                {
                    ParameterBase temp = ValidParam(command, line);

                    if (temp != null)
                        paramsTemp.Add(temp);
                    else
                        invalidArg = true;
                }

                if (!invalidArg)
                {
                    parameters = paramsTemp.ToArray();
                }
            }

            return parameters;
        }

        private ParameterBase ValidParam(CommandBase command, string paramToCheck)
        {
            ParameterBase param = command.Parameters.Find(delegate(CommandBase parameter)
            {
                return parameter.Name.Equals(paramToCheck);
            }) as ParameterBase;

            return param;
        }
    }
}
