using System;
using System.Collections.Generic;
using Cosmos.Kernel.Boot.Glue;

namespace Cosmos.Kernel.Boot {
	public static class BootDrv {
		private static BootInformationStruct MultiBootInfo;
		private static bool BootInfoSet = false;
		[GlueMethod(MethodType = GlueMethodTypeEnum.SaveBootInfoStruct)]
		public static void SetMultiBootInfo(ref BootInformationStruct aBootInfo) {
			MultiBootInfo = aBootInfo;
			BootInfoSet = true;
		}

		public static void Main() {
			Console.WriteLine("This is CosmOS Booting...");
			Console.WriteLine("This is CosmOS Booting...");
			//			if (!BootInfoSet) {
//				Console.WriteLine("No boot info available, terminating!");
//				return;
//			}
//			Console.WriteLine("Boot information available.");
//			Console.WRite((MultiBootInfo.MemUpper + 1000) / 1024) + 1
				//(((mbinfo^.mem_upper + 1000) div 1024) +1);

		}
	}
}