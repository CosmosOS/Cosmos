using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console.Commands {
	public class DirCommand: CommandBase {
		public override string Name {
			get {
				return "dir";
			}
		}

		public override string Summary {
			get {
				return "Lists the files in the current directory.";
			}
		}

		public override void Execute(string param) {
			for (int i = Hardware.Device.Devices.Count - 1; i >= 0; i--) {
				var xDevice = Hardware.Device.Devices[i];
				if (xDevice.Type == Cosmos.Hardware.Device.DeviceType.Storage) {
					var xBlockDevice = (Hardware.BlockDevice)xDevice;
					Cosmos.Kernel.FileSystem.Ext2 xExt2 = new Cosmos.Kernel.FileSystem.Ext2(xBlockDevice);

					if (!xExt2.Initialize()) {
						System.Console.WriteLine("Error while initializing Ext2 Filesystem!");
					} else {
						System.Console.WriteLine("ATA and Ext2 successfully initialized!");
						//    System.Diagnostics.Debugger.Break();
						string[] files = xExt2.GetDirectoryEntries(new string[0]);
						if (files == null) {
							System.Console.WriteLine("Error while getting DirectoryEntries");
						}
						for (int f = 0; f < files.Length; f++)
							System.Console.WriteLine(files[f]);
					}
					return;
				}
			}
			System.Console.WriteLine("No BlockDevices found!");
		}

		public override void Help() {
			System.Console.WriteLine("dir");
			System.Console.WriteLine("  Lists the files in the current directory.");
		}
	}
}
