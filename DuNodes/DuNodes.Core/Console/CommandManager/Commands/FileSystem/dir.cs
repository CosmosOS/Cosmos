using System;
using System.Collections.Generic;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class dir : CommandBase
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

                    for (int i = 0; i < dir.Length; i++)
                    {

                        Console.WriteLine(args[0] + dir[i]);
                    }
                }
                else
                {
                    var dir = HAL.FileSystem.Base.FileSystem.Root.ListFiles(ENV.currentPath);
                    var currPath = ENV.currentPath;
                    if (!currPath.EndsWith("/"))
                        currPath += "/";
                    for (int i = 0; i < dir.Length; i++)
                    {
                        Console.WriteLine(currPath + dir[i]);
                    }
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
