using System;
using Sys = Cosmos.System;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;
using ZLibrary;

namespace ZKernel
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            //var fs = new Sys.FileSystem.CosmosVFS();
            //Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);

            Console.WriteLine("Welcome to the Cosmos port of the ZMachine emulator.");
        }

        [ManifestResourceStream(ResourceName = "ZKernel.ZORK1.DAT")]
        public static byte[] GameData;

        protected override void Run()
        {
            //Console.WriteLine("Specify game path:");
            //string fileName = string.Empty;
            try
            {
                //fileName = Console.ReadLine();
                //if (fileName == "ls")
                //{
                //    foreach (var dir in Directory.GetFiles("0:\\"))
                //    {
                //        Console.WriteLine($"0:\\{dir}");
                //    }
                //}
                //else
                //{
                    var machine = new ZMachine(GameData);
                    machine.Run();
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
