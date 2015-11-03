using System;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class help : CommandBase
    {


        public override void launch(string[] args)
        {
            Console.NewLine();
            Console.WriteLine("Commands list : ");
            Console.WriteLine("bench - Performance tester ");
            Console.WriteLine("config - Configuration manager - Not implemented yet");
            Console.WriteLine("top - Ram / CPU / HDD monitor - Implementation in progress");
            Console.WriteLine("shutdown - Shutdown the computer - Not implemented yet");
            Console.WriteLine("ping - Ping ip address on network - Not implemented yet");
            Console.WriteLine("reboot - Reboot the computer");
            Console.WriteLine("clearcls - Clear console");
            Console.WriteLine("rammanager - Temp command - Not implemented yet");
            Console.WriteLine("cd 'path' - Navigate to path");
            Console.WriteLine("dir '[path]' - List files and directory of current path, path is optionnal");
            Console.WriteLine("mkdir 'name' - Create directory in the current path");
            Console.WriteLine("read 'filename' - Read file ");
            Console.WriteLine("touch 'filename' - Create the file");

            Console.NewLine();
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
