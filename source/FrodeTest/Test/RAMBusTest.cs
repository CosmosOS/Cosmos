using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Test
{
    public class RAMBusTest
    {
        public static void RunTest()
        {
            uint address = 0xc100;
            
            //ushort address = 49409;

            //byte data = 0x23;
            //UInt16 longdata = 0xFFFF;

            //Console.WriteLine("Writing " + data + " to memory");
            //Write8(address, data);
            Console.WriteLine("Reading back from memory: " + Read8(address));

            //Console.WriteLine("Writing " + longdata + " to memory");
            //Write16(address, longdata);
            Console.WriteLine("Reading back from memory: " + Read16(address));
            Console.WriteLine("Reading 4 bytes: " + Read64(address));
        }

        public static unsafe void Write8(uint address, byte data)
        {
            ushort* pointer = (ushort*)address;
            *pointer = data;
        }

        public static unsafe void Write16(uint address, UInt16 data)
        {
            UInt16* pointer = (UInt16*)address;
            *pointer = data;
        }

        public static unsafe byte Read8(uint address)
        {
            byte* pointer = (byte*)address;
            byte data = *pointer;
            return data;
        }

        public static unsafe UInt16 Read16(uint address)
        {
            UInt16* pointer = (UInt16*)address;
            UInt16 data = *pointer;
            return data;
        }

        public static unsafe UInt64 Read64(uint address)
        {
            UInt64* pointer = (UInt64*)address;
            UInt64 data = *pointer;
            return data;
        }



    }
}
