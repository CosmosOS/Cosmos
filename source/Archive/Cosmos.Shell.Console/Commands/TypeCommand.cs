using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel;

namespace Cosmos.Shell.Console.Commands {
	public class TypeCommand : CommandBase {
		public override string Name {
			get {
				return "type";
			}
		}

		public override void Execute(string param) {

			//Hardware.Storage.ATA xDrive = new Cosmos.Hardware.Storage.ATA(0, 0);
			//Cosmos.Kernel.FileSystem.Ext2 xExt2 = new Cosmos.Kernel.FileSystem.Ext2(xDrive);
			//if (!xExt2.Initialize()){
			//    System.Console.WriteLine("Error: Ext2 Initialization failed!");
			//    return;
			//}
			//byte[] xItem = xExt2.ReadFile(new string[] { param });
			//if (xItem == null)
			//{
			//    System.Console.WriteLine("Error: Couldn't read file!");
			//    return;
			//}
			//char[] xChars = new char[xItem.Length - 1];
			//for (int i = 0; i < xChars.Length; i++)
			//{
			//    xChars[i] = (char)xItem[i];
			//}
			//String s = new String(xChars);
			//System.Console.WriteLine(s);
			throw new NotImplementedException();
		}

        public override void Help()
        {
            System.Console.WriteLine("type [filename]");
            System.Console.WriteLine("  Types the specified file out to the console window.");
            System.Console.WriteLine("  [filename]: The name of the file.");
        }

        public override string Summary
        {
            get { return "Types the specified file out to the console window."; }
        }
    }
}
