using Cosmos.Core;

namespace Cosmos.HAL.Debug;

public class Serial
{
    protected static int device = 0x3f8; //Com1 port for Qemu Serial

    public static void Enable()
    {
        new IOPort((ushort)(device + 1)).Byte = 0x00;
        new IOPort((ushort)(device + 3)).Byte = 0x80;
        new IOPort((ushort)(device + 0)).Byte = 0x03;
        new IOPort((ushort)(device + 1)).Byte = 0x00;
        new IOPort((ushort)(device + 3)).Byte = 0x03;
        new IOPort((ushort)(device + 2)).Byte = 0xC7;
        new IOPort((ushort)(device + 4)).Byte = 0x0B;
    }

    internal static int Received() => new IOPortRead((ushort)(device + 5)).Byte & 1;

    internal static byte Receive()
    {
        while (Received() == 0)
        {
            ;
        }

        return new IOPortRead((ushort)device).Byte;
    }

    internal static byte ReceiveAsync() => new IOPortRead((ushort)device).Byte;

    internal static int TransmitEmpty() => new IOPortRead((ushort)(device + 5)).Byte & 0x20;

    internal static void Send(char output)
    {
        while (TransmitEmpty() == 0)
        {
            ;
        }

        new IOPort((ushort)device).Byte = (byte)output;
    }

    public static void SendString(string output)
    {
        for (var i = 0; i < output.Length; ++i)
        {
            Send(output[i]);
        }
    }
}
