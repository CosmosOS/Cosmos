namespace DuNodes.System.Console.CommandManager.Commands
{
    public class help : CommandBase
    {
        public help()
        {
            Console.NewLine();
            Console.WriteLine("Commands list : ");
            Console.WriteLine("bench - Performance tester ");
            Console.WriteLine("config - Configuration manager - Not implemented yet");
            Console.WriteLine("top - Ram / CPU / HDD monitor - Implementation in progress");
            Console.WriteLine("shutdown - Shutdown the computer - Not implemented yet");
            Console.WriteLine("ping - Ping ip address on network - Not implemented yet");
            Console.WriteLine("reboot - Reboot the computer");
            Console.WriteLine("rammanager - Temp command - Not implemented yet");
            Console.NewLine();
        }
    }
}
