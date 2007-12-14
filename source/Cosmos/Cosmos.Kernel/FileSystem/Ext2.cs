using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.FileSystem {
	public partial class Ext2: FileSystem {
		public static unsafe byte[] ReadFileContents(byte aController, byte aDrive, string[] aPath) {
			ushort* xBuffer = (ushort*)Heap.MemAlloc(512);
			if (!Hardware.Storage.ATA.ReadDataNew(aController, aDrive, 3, xBuffer)) {
				Console.WriteLine("[Ext2] Error while reading SuperBlock data");
				return null;
			} else {
				Console.WriteLine("[Ext2] SuperBlock data read");
			}
			SuperBlock* xSuperBlock = (SuperBlock*)xBuffer;
			DebugUtil.SendExt2_SuperBlock("ReadFileContents", xSuperBlock);
			return null;
		}
	}
}
