using System;
using System.Collections.Generic;
using System.Linq;

namespace Cosmos.Shell.Console.Commands {
	public class MountCommand: CommandBase {
		public override string Name {
			get {
				return "mount";
			}
		}

		public override string Summary {
			get {
				return "Tries to mount all blockdevices";
			}
		}

		public override void Execute(string param) {
			System.Console.WriteLine("Not implemented!");
			//Cosmos.Kernel.New.Partitioning.MBT.Initialize();
		}

		public override void Help() {
			System.Console.WriteLine("Tries to mount all blockdevices by probing MBT and FS's");
		}
	}
}