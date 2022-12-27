using System;
using Cosmos.Core;
namespace Cosmos.HAL.Debug
{
    public class Serial
    {
        private static readonly ushort device = 0x3f8; //Com1 port for Qemu Serial

        public static void Enable()
        {
            IOPort.Write8((ushort)(device + 1), 0x00);
            IOPort.Write8((ushort)(device + 3), 0x80);
            IOPort.Write8(device, 0x03);
            IOPort.Write8((ushort)(device + 1), 0x00);
            IOPort.Write8((ushort)(device + 3), 0x03);
            IOPort.Write8((ushort)(device + 2), 0xC7);
            IOPort.Write8((ushort)(device + 4), 0x0B);
        }

        internal static int Received()
        {
            return IOPort.Read8((ushort)(device + 5)) & 1;
        }

        internal static byte Receive()
        {
            while (Received() == 0) ;
            return IOPort.Read8(device);
        }

        internal static byte ReceiveAsync()
        {
            return IOPort.Read8(device);
        }

        internal static int TransmitEmpty()
        {
            return IOPort.Read8((ushort)(device + 5)) & 0x20;
        }

        internal static void Send(char output)
        {
            while (TransmitEmpty() == 0) ;
            IOPort.Write8(device, (byte)output);
        }

        public static void SendString(string output)
        {
            for (int i = 0; i < output.Length; ++i)
            {
                Send(output[i]);
            }
        }
    }
}
