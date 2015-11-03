using System;
using System.Collections.Generic;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class mkdir : CommandBase
    {
        public override void launch(string[] args)
        {
            if (args.Length > 0)
            {
                var currDir = args[0];
                var currPath = ENV.currentPath;
                if (!currPath.EndsWith("/") && !currDir.StartsWith("/"))
                    currPath += "/";

                HAL.FileSystem.Base.FileSystem.Root.makeDir(currPath+currDir, "DN");
            }
            else
            {
                Console.WriteLine("Please provide a directory name");
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
