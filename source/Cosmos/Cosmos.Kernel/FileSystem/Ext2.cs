using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.FileSystem {
	public partial class Ext2: FileSystem {
		public static unsafe byte[] ReadFileContents(byte aController, byte aDrive, string[] aPath) {
			ushort* xBuffer = (ushort*)Heap.MemAlloc(512);
			if (!Hardware.Storage.ATA.ReadDataNew(aController, aDrive, 2, xBuffer)) {
				Console.WriteLine("[Ext2|SuperBlock] Error while reading SuperBlock data");
				return null;
			} else {
				Console.WriteLine("[Ext2|SuperBlock] SuperBlock data read");
			}
			SuperBlock xSuperBlock = *(SuperBlock*)xBuffer;
			DebugUtil.SendExt2_SuperBlock("ReadFileContents", &xSuperBlock);
			int xBlockSize = (int)(1024 << (byte)(xSuperBlock.LogBlockSize));
			int xGroupDescriptorPerBlock = xBlockSize / 32;// groupdescriptor size
			GroupDescriptor[] xGroupDescriptors = ReadGroupDescriptorsOfBlock(aController, aDrive, xSuperBlock.FirstDataBlock + 1, xSuperBlock, xBuffer);
			return null;
		}

		private static unsafe GroupDescriptor[] ReadGroupDescriptorsOfBlock(byte aController, byte aDrive, uint aBlock, SuperBlock aSuperBlock, ushort* aBuffer) {
			//GroupDescriptor[] xResult = new GroupDescriptor[32];
			int xBlockSize = (int)(1024 << aSuperBlock.LogBlockSize);
			int xGroupDescriptorPerBlock = xBlockSize / 32;// groupdescriptor size
			Hardware.Storage.ATA.ReadDataNew(aController, aDrive, aBlock, aBuffer);
			if (!true/*Hardware.Storage.ATA.ReadDataNew(aController, aDrive, aBlock, aBuffer)*/) {
			    Console.WriteLine("[Ext2|GroupDescriptors] Error while reading GroupDescriptor data");
			    return null;
			} else {
			    Console.WriteLine("[Ext2|GroupDescriptors] Block read");
			}
			GroupDescriptor* xDescriptorPtr = (GroupDescriptor*)aBuffer;
			//for (uint i = 0; i < 32; i++) {
			uint i = 0;
			Console.Write("Start Iteration ");
			Hardware.Storage.ATA.WriteNumber(i, 8);
			Console.WriteLine("");
			//xResult[i] = xDescriptorPtr[i];
			Console.WriteLine("End Iteration");
			////}
			return null;
		}
	}
}
