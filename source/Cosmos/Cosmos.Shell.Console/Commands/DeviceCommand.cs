using System;
using System.Collections.Generic;
using System.Linq;
using Cosmos.Hardware;

namespace Cosmos.Shell.Console.Commands {
	public class DeviceCommand: CommandBase {
		public override string Name {
			get {
				return "device";
			}
		}

		public override string Summary {
			get {
				return "Returns device information";
			}
		}

		public override void Execute(string param) {
			string xCommand = param;
			int xIndex = param.IndexOf(' ');
			if (xIndex != -1) {
				xCommand = param.Substring(0, xIndex);
			}
			if (String.IsNullOrEmpty(xCommand)) {
				System.Console.Write("Number of Storage devices: ");
				System.Console.WriteLine(Device.Find(Device.DeviceType.Storage).Count.ToString());
				System.Console.Write("Number of Keyboard devices: ");
				System.Console.WriteLine(Device.Find(Device.DeviceType.Keyboard).Count.ToString());
				System.Console.Write("Number of Mouse devices: ");
				System.Console.WriteLine(Device.Find(Device.DeviceType.Mouse).Count.ToString());
				System.Console.Write("Number of Other devices: ");
				System.Console.WriteLine(Device.Find(Device.DeviceType.Other).Count.ToString());
				System.Console.Write("Number of Unknown devices: ");
				System.Console.WriteLine(Device.Find(Device.DeviceType.Unknown).Count.ToString());
				return;
			}
			if (xCommand.CompareTo("test") == 0) {
				DebugUtil.SendMessage("test", "Starting Test");
				var xDevice = Hardware.Device.FindFirst(Cosmos.Hardware.Device.DeviceType.Storage);
				if (xDevice == null) {
					System.Console.WriteLine("No StorageDevicefound!");
					DebugUtil.SendMessage("test", "No storage devices");
				} else {
					DebugUtil.SendMessage("test", "Storage devices found!");
					BlockDevice xBD = (BlockDevice)xDevice;
					if (xBD == null) {
						DebugUtil.SendError("test", "Storage device is null!");
						System.Console.WriteLine("Storage device is null!");
						return;
					}
					var xBytes = xBD.ReadBlock(2);
					System.Console.Write("Byte 1-1: ");
					System.Console.WriteLine(xBytes[6].ToString());
					System.Console.Write("Byte 1-2: ");
					System.Console.WriteLine(xBytes[7].ToString());
					xBytes = xBD.ReadBlock(3);
					System.Console.Write("Byte 2-1: ");
					System.Console.WriteLine(xBytes[6].ToString());
					System.Console.Write("Byte 2-2: ");
					System.Console.WriteLine(xBytes[7].ToString());
				}
			}
		}

		public override void Help() {
			System.Console.WriteLine("device [command]");
			System.Console.WriteLine("  Shows information regarding devices");
			System.Console.WriteLine("  [command]: The command to execute");
		}
	}
}
