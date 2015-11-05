using System;
using System.Collections.Generic;
using Cosmos.Common.Extensions;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class read : CommandBase
    {
        public override void launch(string[] args)
        {
            if (args.Length > 0)
            {
                var currfile = args[0];
                var currPath = ENV.currentPath;
                if (!currPath.EndsWith("/") && !currfile.StartsWith("/"))
                    currPath += "/";

               var strB =  HAL.FileSystem.Base.FileSystem.Root.readFile(currPath + currfile);
                var str = strB.GetUtf8String(0, (uint)strB.Length);
                Console.WriteLine(str);
                Console.NewLine();
            }
            else
            {
                Console.WriteLine("Please provide a file name");
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
