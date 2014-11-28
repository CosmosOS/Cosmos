using System;

namespace Cosmos.Core
{
    public abstract class IOPortBase
    {
        //TODO Make it that IO port classes are exclusive to each port. For example
        // only one IOPort class can be created per port number. This will prevent
        // two instances of an IOPort from using the same port.
        // A locking mechanism is not necessary as the creator can control access
        // to the instance.
        // We are not threaded yet anyways, but when we are will assume the caller
        // or owner handles any concurrency issues so as to minimize overhead in this
        // class. Or maybe some base support can be added to this class, but its functionality
        // is optional and only used by classes that need concurrency control like ATA.

        public readonly UInt16 Port;

        // all ctors are internal - Only Core ring can create it.. but hardware ring can use it.
        protected IOPortBase(UInt16 aPort)
        {
            Port = aPort;
        }
        protected IOPortBase(UInt16 aBase, UInt16 aOffset)
        {
            // C# math promotes things to integers, so we have this constructor
            // to relieve the use from having to do so many casts
            Port = (UInt16)(aBase + aOffset);
        }

        //TODO: Reads and writes can use this to get port instead of argument
        static protected void Write8(UInt16 aPort, byte aData) { } // Plugged
        static protected void Write16(UInt16 aPort, UInt16 aData) { } // Plugged
        static protected void Write32(UInt16 aPort, UInt32 aData) { } // Plugged

        static protected byte Read8(UInt16 aPort) { return 0; } // Plugged
        static protected UInt16 Read16(UInt16 aPort) { return 0; } // Plugged
        static protected UInt32 Read32(UInt16 aPort) { return 0; } // Plugged

        //TODO: Plug these Reads with asm to read directly to RAM
        // REP INSW
        public void Read8(byte[] aData)
        {
            UInt16 xValue;
            for (int i = 0; i < aData.Length / 2; i++)
            {
                xValue = Read16(Port);
                aData[i * 2] = (byte)xValue;
                aData[i * 2 + 1] = (byte)(xValue >> 8);
            }
        }
        public void Read16(UInt16[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                aData[i] = Read16(Port);
            }
        }
        public void Read32(UInt32[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                aData[i] = Read32(Port);
            }
        }
    }

    public class IOPort : IOPortBase
    {
        public IOPort(UInt16 aPort) : base(aPort) { }
        public IOPort(UInt16 aBase, UInt16 aOffset) : base(aBase, aOffset) { }

        static public void Wait()
        {
            // Write to an unused port. This assures whatever we were waiting on for a previous
            // IO read/write has completed.
            // Port 0x80 is unused after BIOS POST.
            // 0x22 is just a random byte.
            // Since IO is slow - its just a dummy sleep to wait long enough for the previous operation
            // to have effect on the target.
            IOPortBase.Write8(0x80, 0x22);
        }

        public byte Byte
        {
            get { return IOPortBase.Read8(Port); }
            set { IOPortBase.Write8(Port, value); }
        }

        public UInt16 Word
        {
            get { return IOPortBase.Read16(Port); }
            set { IOPortBase.Write16(Port, value); }
        }

        public UInt32 DWord
        {
            get { return IOPortBase.Read32(Port); }
            set { IOPortBase.Write32(Port, value); }
        }
    }

    // I split these instead of adding CanRead/CanWrite because this enforces
    // at build time, and its also faster at runtime. Finally it allows future optimizations better
    // than checking at runtime.
    public class IOPortRead : IOPortBase
    {
        public IOPortRead(UInt16 aPort) : base(aPort) { }
        public IOPortRead(UInt16 aBase, UInt16 aOffset) : base(aBase, aOffset) { }

        public byte Byte
        {
            get { return IOPortBase.Read8(Port); }
        }

        public UInt16 Word
        {
            get { return IOPortBase.Read16(Port); }
        }

        public UInt32 DWord
        {
            get { return IOPortBase.Read32(Port); }
        }
    }

    public class IOPortWrite : IOPortBase
    {
        public IOPortWrite(UInt16 aPort) : base(aPort) { }
        public IOPortWrite(UInt16 aBase, UInt16 aOffset) : base(aBase, aOffset) { }

        public byte Byte
        {
            set { IOPortBase.Write8(Port, value); }
        }

        public UInt16 Word
        {
            set { IOPortBase.Write16(Port, value); }
        }

        public UInt32 DWord
        {
            set { IOPortBase.Write32(Port, value); }
        }
    }

}
