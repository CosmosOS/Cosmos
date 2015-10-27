using System;
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
                //var commandType = Type.GetType("DuNodes_Core.Terminal.CommandManager.Commands." + command);
               // Activator.CreateInstance(commandType); 
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
                        Console.WriteLine("Commands are : config - top - bench - rammanager");
                        break;

                    default:
                        Console.WriteLine("Unknown " + command + " command.");
                        break;
                }

               cmd = null;
               
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unknown " + command + " command.");
                Console.WriteLine("Ex : " + ex.Message, ConsoleColor.Red);
            }

            
        }
    }
}
