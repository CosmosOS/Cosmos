using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel;

namespace Cosmos.Shell.Console.Commands {
	public class FSTestCommand : CommandBase {
		public override string Name {
			get {
				return "type";
			}
		}

		public override void Execute(string param) {
            System.Console.Write("Emitting contents of ");
            System.Console.Write(param);
            System.Console.WriteLine(":");

            Hardware.Storage.ATA xDrive = new Cosmos.Hardware.Storage.ATA(0, 0);
            Cosmos.Kernel.FileSystem.Ext2 xExt2 = new Cosmos.Kernel.FileSystem.Ext2(xDrive);
            if (xExt2.Initialize())
            {
                System.Console.WriteLine("Ext2 Initialized");
            }
            else
            {
                System.Console.WriteLine("Ext2 Initialization failed!");
            }
            byte[] xItem = xExt2.ReadFile(new string[] { param });
            if (xItem == null)
            {
                System.Console.WriteLine("Couldn't read file!");
                return;
            }
            char[] xChars = new char[xItem.Length - 1];
            for (int i = 0; i < xChars.Length; i++)
            {
                xChars[i] = (char)xItem[i];
            }
            String s = new String(xChars);
            System.Console.WriteLine(s);
		}
	}
}
