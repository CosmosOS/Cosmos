using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Kernel;

namespace MatthijsTest
{
    public class MyATADevice: BlockDevice
    {
        #region Inherited members
        public override uint BlockSize
        {
            get
            {
                return 512;
            }
        }

        public override ulong BlockCount
        {
            get
            {
                return mSectorCount;
            }
        }

        public override void ReadBlock(ulong aBlock, byte[] aContents)
        {
            throw new NotImplementedException();
        }

        public override void WriteBlock(ulong aBlock, byte[] aContents)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get
            {
                return mName;
            }
        }

        private string mName;
        #endregion

        #region Initialization
        internal MyATADevice(MyATAController controller, string name)
        {
            mName = name;
            mController = controller;
            Initialize();
        }

        private void Initialize()
        {
            var xMemSpace = new ManagedMemorySpace(512);
            mController.RealReadBlock(xMemSpace);
            SupportsLBA48 = (xMemSpace.Read16(83 * 2) & 1 << 10) != 0; // byte 10 of word 83 specifies if LBA48 is supported
            SupportedUDMA = xMemSpace.Read8(88 * 2);
            mSectorCount = xMemSpace.Read32(60 * 2); // word 60 & 61 contain the number of LBA28 sectors
            SupportsLBA28 = mSectorCount != 0;
        }

        private static ushort TestFix(ushort value)
        {
            return (ushort)((value << 8) | (value >> 8));
        }

        public byte SupportedUDMA
        {
            get;
            private set;
        }

        public bool SupportsLBA28
        {
            get;
            private set;
        }

        public bool SupportsLBA48
        {
            get;
            private set;
        }

        private uint mSectorCount;
        
        private readonly MyATAController mController;
        #endregion
    }
}//http://wiki.osdev.org/opensearch_desc.php