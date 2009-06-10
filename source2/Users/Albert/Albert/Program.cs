using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Builder;
using Cosmos.Hardware;

namespace Albert
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            BuildUI.Run();
        }
        #endregion

        // Main entry point of the kernel
        //This is the playground for Albert
        public static void Init()
        {
            //var xBoot = new Cosmos.Sys.Boot();
            //xBoot.Execute();

            new Cosmos.Sys.Boot().Execute();

            Console.WriteLine("Cosmos -- Testing");
            try
            {
                Console.WriteLine("First test");
                System.Console.Write("Device Count: ");
                System.Console.WriteLine(Device.Devices.Count.ToString());

                for (int i = 0; i < Device.Devices.Count; i++)
                {
                    var xDevice = Device.Devices[i];
                    if (xDevice.Type == Cosmos.Hardware.Device.DeviceType.Storage)
                    {
                        Console.WriteLine("Device Type: " + xDevice.Type + " Device Name: " + xDevice.Name);
                        //var xBlockDevice = (BlockDevice)xDevice;
                    }
                }
                Console.Read();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("General error in Test: " + ex.Message);
            }

            //Done
            Console.WriteLine();
            Cosmos.Sys.Deboot.ShutDown();
        }


    }
}
