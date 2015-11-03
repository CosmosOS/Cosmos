using System;
using Cosmos.IL2CPU;
using DuNodes.System.Console.CommandManager.Commands;

namespace DuNodes.System.Console.CommandManager
{
    public class Command
    {
        public Command()
        {
            //Init, thread system, stack,
        }

        public void Handle(string command)
        {
            try
            {
                command = command.ToLower();
                var cmd = new object();
                //TODO: Need a plug for Reflector. In order to remove the switch case
                //Type commandType  = ReflectionUtilities.GetType("DuNodes.System", "Console.CommandManager.Commands." + command);
                //var commandType = Type.GetType("DuNodes.System.Console.CommandManager.Commands." + command);
                //cmd = Activator.CreateInstance(commandType); 
                switch (command)
                {
                    case "config":
                        cmd = new config();
                        break;

                    case "top":
                        cmd = new top();
                        break;

                    case "bench":
                        cmd = new bench();
                        break;

                    case "help":
                        cmd = new help();
                        break;

                    case "reboot":
                        cmd = new reboot();
                        break;

                    case "shutdown":
                        cmd = new shutdown();
                        break;

                    case "clearcls":
                        cmd = new clearcls();
                        break;

                    default:
                        Console.Error.WriteLine("Unknown " + command + " command. Type help in order to know all available commands and option.");
                        break;
                }

                cmd = null;
               
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unknown " + command + " command.");
                Console.Error.WriteLine("Ex : " + ex.Message);
            }
        }
    }
}
