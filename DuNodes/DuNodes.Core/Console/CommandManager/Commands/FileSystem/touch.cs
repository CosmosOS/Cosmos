using System;
using System.Collections.Generic;
using Cosmos.Common.Extensions;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class touch : CommandBase
    {
        public override void launch(string[] args)
        {
            if (args.Length > 0)
            {
                var currfile = args[0];
                var currPath = ENV.currentPath;
                if (!currPath.EndsWith("/") && !currfile.StartsWith("/"))
                    currPath += "/";

                var strB = "dummy";
                var str = strB.GetUtf8Bytes(0, (uint)strB.Length);

                HAL.FileSystem.Base.FileSystem.Root.saveFile(str, currPath + args[0], "DN");

                Console.WriteLine("File Created!");
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
