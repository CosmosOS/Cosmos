//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Cosmos.Hardware;
//using Cosmos.Kernel;

//namespace MatthijsTest
//{
//    public class MyATADevice: BlockDevice
//    {
//        #region Inherited members
//        public override uint BlockSize
//        {
//            get
//            {
//                return 512;
//            }
//        }

//        public override uint BlockCount
//        {
//            get
//            {
//                return mSectorCount;
//            }
//        }

//        public override void ReadBlock(uint aBlock, byte[] aContents)
//        {                                   
//            throw new NotImplementedException();
//        }

//        public override unsafe void WriteBlock(uint aBlock, byte[] aContents)
//        {
//            AddressSpace xAddrSpace;
//            fixed (byte* xContents = &aContents[0])
//            {
//                xAddrSpace = new ManagedMemorySpace(aContents);
//                Console.Write("Correct offset: ");
//                Interrupts.WriteNumber((uint)xContents, 32);
//                Console.WriteLine("");
//                Console.Write("Guessed offset: ");
//                Interrupts.WriteNumber(xAddrSpace.Offset, 32);
//                Console.WriteLine("");

//                if (xAddrSpace.Offset != ((uint)xContents))
//                {
//                    Console.WriteLine("Offset not correct");
//                }
//            }
//            //var xAddrSpace = new MemoryAddressSpace(xAddr, 512);
//            //for (uint i = 0; i < 512; i++)
//            //{
//            //    xAddrSpace.Write8(i, aContents[i]);
//            //}
//            mController.WriteSector_LBA28(mIsMaster, aBlock, xAddrSpace);
//        }

//        public override string Name
//        {
//            get
//            {
//                return mName;
//            }
//        }

//        private string mName;
//        #endregion

//        #region Initialization
//        internal MyATADevice(MyATAController controller, string name, bool isMaster)
//        {
//            mName = name;
//            mController = controller;
//            mIsMaster = isMaster;
//            Initialize();
//        }

//        private void Initialize()
//        {
//            var xMemSpace = new ManagedMemorySpace(512);
//            mController.RealReadBlock_PIO(xMemSpace);
//            SupportsLBA48 = (xMemSpace.Read16(83 * 2) & 1 << 10) != 0; // byte 10 of word 83 specifies if LBA48 is supported
//            SupportedUDMA = xMemSpace.Read8(88 * 2);
//            mSectorCount = xMemSpace.Read32(60 * 2); // word 60 & 61 contain the number of LBA28 sectors
//            SupportsLBA28 = mSectorCount != 0;
//        }

//        public byte SupportedUDMA
//        {
//            get;
//            private set;
//        }

//        public bool SupportsLBA28
//        {
//            get;
//            private set;
//        }

//        public bool SupportsLBA48
//        {
//            get;
//            private set;
//        }

//        private uint mSectorCount;
        
//        private readonly MyATAController mController;
//        private readonly bool mIsMaster;
//        #endregion
//    }
//}