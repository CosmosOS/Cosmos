using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    /// <summary>
    /// IOPortBase abstract class.
    /// </summary>
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

        /// <summary>
        /// Port.
        /// </summary>
        protected readonly ushort Port;

        // all ctors are internal - Only Core ring can create it.. but hardware ring can use it.
        /// <summary>
        /// Create new instance of the <see cref="IOPortBase"/> class.
        /// </summary>
        /// <param name="aPort">A port.</param>
        protected IOPortBase(ushort aPort)
        {
            Port = aPort;
        }

        /// <summary>
        /// Create new instance of the <see cref="IOPortBase"/> class.
        /// </summary>
        /// <param name="aBase">A base port.</param>
        /// <param name="aOffset">A offset from the base port.</param>
        protected IOPortBase(ushort aBase, ushort aOffset)
        {
            // C# math promotes things to integers, so we have this constructor
            // to relieve the use from having to do so many casts
            Port = (ushort)(aBase + aOffset);
        }

        //TODO: Reads and writes can use this to get port instead of argument
        /// <summary>
        /// Write byte to port.
        /// Plugged.
        /// </summary>
        /// <param name="aPort">A port to write to.</param>
        /// <param name="aData">A data.</param>
        [PlugMethod(PlugRequired = true)]
        static protected void Write8(ushort aPort, byte aData) => throw null;

        /// <summary>
        /// Write Word to port.
        /// Plugged.
        /// </summary>
        /// <param name="aPort">A port to write to.</param>
        /// <param name="aData">A data.</param>
        [PlugMethod(PlugRequired = true)]
        static protected void Write16(ushort aPort, ushort aData) => throw null;

        /// <summary>
        /// Write DWord to port.
        /// Plugged.
        /// </summary>
        /// <param name="aPort">A port to write to.</param>
        /// <param name="aData">A data.</param>
        [PlugMethod(PlugRequired = true)]
        static protected void Write32(ushort aPort, uint aData) => throw null;

        /// <summary>
        /// Read byte from port.
        /// Plugged.
        /// </summary>
        /// <param name="aPort">A port to read from.</param>
        /// <returns>byte value.</returns>
        [PlugMethod(PlugRequired = true)]
        static protected byte Read8(ushort aPort) => throw null;

        /// <summary>
        /// Read Word from port.
        /// Plugged.
        /// </summary>
        /// <param name="aPort">A port to read from.</param>
        /// <returns>ushort value.</returns>
        [PlugMethod(PlugRequired = true)]
        static protected ushort Read16(ushort aPort) => throw null;

        /// <summary>
        /// Read DWord from port.
        /// Plugged.
        /// </summary>
        /// <param name="aPort">A port to read from.</param>
        /// <returns>uint value.</returns>
        [PlugMethod(PlugRequired = true)]
        static protected uint Read32(ushort aPort) => throw null;

        //TODO: Plug these Reads with asm to read directly to RAM
        // REP INSW
        /// <summary>
        /// Read byte from base port.
        /// </summary>
        /// <param name="aData">Output data array.</param>
        /// <exception cref="System.OverflowException">Thrown if aData lenght is greater than Int32.MaxValue.</exception>
        public void Read8(byte[] aData)
        {
            for (int i = 0; i < aData.Length / 2; i++)
            {
                var xValue = Read16(Port);
                aData[i * 2] = (byte)xValue;
                aData[i * 2 + 1] = (byte)(xValue >> 8);
            }
        }

        /// <summary>
        /// Read Word from base port.
        /// </summary>
        /// <param name="aData">Output data array.</param>
        /// <exception cref="System.OverflowException">Thrown if aData lenght is greater than Int32.MaxValue.</exception>
        public void Read16(ushort[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                aData[i] = Read16(Port);
            }
        }

        /// <summary>
        /// Read DWord from base port.
        /// </summary>
        /// <param name="aData">Output data array.</param>
        /// <exception cref="System.OverflowException">Thrown if aData lenght is greater than Int32.MaxValue.</exception>
        public void Read32(uint[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                aData[i] = Read32(Port);
            }
        }
    }

    /// <summary>
    /// IOPort class. Used to read and write to IO port.
    /// </summary>
    public class IOPort : IOPortBase
    {
        /// <summary>
        /// Create new instance of the <see cref="IOPort"/> class.
        /// </summary>
        /// <param name="aPort">A port.</param>
        public IOPort(ushort aPort)
            : base(aPort)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="IOPort"/> class.
        /// </summary>
        /// <param name="aBase">A base port.</param>
        /// <param name="aOffset">Offset from the base port.</param>
        public IOPort(ushort aBase, ushort aOffset)
            : base(aBase, aOffset)
        {
        }

        /// <summary>
        /// Wait for the previous IO read/write to complete.
        /// </summary>
        static public void Wait()
        {
            // Write to an unused port. This assures whatever we were waiting on for a previous
            // IO read/write has completed.
            // Port 0x80 is unused after BIOS POST.
            // 0x22 is just a random byte.
            // Since IO is slow - its just a dummy sleep to wait long enough for the previous operation
            // to have effect on the target.
            Write8(0x80, 0x22);
        }

        /// <summary>
        /// Get and set Byte value in IO port.
        /// </summary>
        public byte Byte
        {
            get => Read8(Port);
            set => Write8(Port, value);
        }

        /// <summary>
        /// Get and set Word value in IO port.
        /// </summary>
        public ushort Word
        {
            get => Read16(Port);
            set => Write16(Port, value);
        }

        /// <summary>
        /// Get and set DWord value in IO port.
        /// </summary>
        public uint DWord
        {
            get => Read32(Port);
            set => Write32(Port, value);
        }
    }

    // I split these instead of adding CanRead/CanWrite because this enforces
    // at build time, and its also faster at runtime. Finally it allows future optimizations better
    // than checking at runtime.
    /// <summary>
    /// IOPortRead class. Used to read to IO port. See also: <seealso cref="IOPortBase"/>.
    /// </summary>
    public class IOPortRead : IOPortBase
    {
        /// <summary>
        /// Create new instance of <see cref="IOPortRead"/> class.
        /// </summary>
        /// <param name="aPort">A port.</param>
        public IOPortRead(ushort aPort)
            : base(aPort)
        {
        }

        /// <summary>
        /// Create new instance of <see cref="IOPortRead"/> class.
        /// </summary>
        /// <param name="aBase">A base port address.</param>
        /// <param name="aOffset">Offset of the base port.</param>
        public IOPortRead(ushort aBase, ushort aOffset)
            : base(aBase, aOffset)
        {
        }

        /// <summary>
        /// Read byte to the port.
        /// </summary>
        public byte Byte => Read8(Port);

        /// <summary>
        /// Read Word to the port.
        /// </summary>
        public ushort Word => Read16(Port);

        /// <summary>
        /// Read DWord to the port.
        /// </summary>
        public uint DWord => Read32(Port);
    }

    /// <summary>
    /// IOPortWrite class. Used to write to IO port. See also: <seealso cref="IOPortBase"/>.
    /// </summary>
    public class IOPortWrite : IOPortBase
    {
        /// <summary>
        /// Create new instance of <see cref="IOPortWrite"/> class.
        /// </summary>
        /// <param name="aPort">A port.</param>
        public IOPortWrite(ushort aPort) : base(aPort)
        {
        }

        /// <summary>
        /// Create new instance of <see cref="IOPortWrite"/> class.
        /// </summary>
        /// <param name="aBase">A base port address.</param>
        /// <param name="aOffset">Offset of the base port.</param>
        public IOPortWrite(ushort aBase, ushort aOffset) : base(aBase, aOffset)
        {
        }

        /// <summary>
        /// Write byte to the port.
        /// </summary>
        public byte Byte
        {
            set => Write8(Port, value);
        }

        /// <summary>
        /// Write Word to the port.
        /// </summary>
        public ushort Word
        {
            set => Write16(Port, value);
        }

        /// <summary>
        /// Write DWord to the port.
        /// </summary>
        public uint DWord
        {
            set => Write32(Port, value);
        }
    }
}
