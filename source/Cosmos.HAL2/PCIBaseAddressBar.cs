using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL
{
    public class PCIBaseAddressBar
    {
        private uint baseAddress = 0;
        private ushort prefetchable = 0;
        private byte type = 0;
        private bool isIO = false;

        internal PCIBaseAddressBar(uint raw)
        {
            isIO = (raw & 0x01) == 1;

            if (isIO)
            {
                baseAddress = raw & 0xFFFFFFFC;
            }
            else
            {
                type = (byte)((raw >> 1) & 0x03);
                prefetchable = (ushort)((raw >> 3) & 0x01);
                switch (type)
                {
                    case 0x00:
                        baseAddress = raw & 0xFFFFFFF0;
                        break;
                    case 0x01:
                        baseAddress = raw & 0xFFFFFFF0;
                        break;
                }
            }
        }
        private static int pci_size(int b, int mask) 
        {
            int size = mask & b;
            size = size & ~(size - 1);
            return size;
        }

        public uint BaseAddress()
        {
            return baseAddress;
        }

        public bool IsIO()
        {
            return isIO;
        }
    }
}
