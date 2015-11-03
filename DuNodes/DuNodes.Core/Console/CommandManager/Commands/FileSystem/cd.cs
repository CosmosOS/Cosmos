using System;
using System.Collections.Generic;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class cd : CommandBase
    {
        public override void launch(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    if (args[0] == "" || args[0] == " ")
                    {
                        args[0] = "/";
                    }

                    var dir = HAL.FileSystem.Base.FileSystem.Root.ListFiles(args[0]);
                    if (!args[0].EndsWith("/"))
                        args[0] += "/";


                    ENV.currentPath = args[0];
                }
                else
                {
                    Console.WriteLine("Please type any path");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured - path does not exist");
            }
        }

        public override void cancelled()
        {
            throw new NotImplementedException();
        }

        public override void pause()
        {
            throw new NotImplementedException();
        }

        public override void finished()
        {
            throw new NotImplementedException();
        }


    }
}
