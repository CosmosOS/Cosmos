using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    /// <summary>
    /// MMIOBase abstract class.
    /// </summary>
    public unsafe abstract class MMIOBase
    {
        /// <summary>
        /// Address.
        /// </summary>
        protected readonly uint Address;

        // all ctors are internal - Only Core ring can create it.. but hardware ring can use it.
        /// <summary>
        /// Create new instance of the <see cref="MMIOBase"/> class.
        /// </summary>
        /// <param name="address">An address.</param>
        protected MMIOBase(uint address)
        {
            Address = address;
        }

        /// <summary>
        /// Create new instance of the <see cref="MMIOBase"/> class.
        /// </summary>
        /// <param name="address">A base address.</param>
        /// <param name="aOffset">An offset from the base address.</param>
        protected MMIOBase(uint address, uint aOffset)
        {
            // C# math promotes things to integers, so we have this constructor
            // to relieve the use from having to do so many casts
            Address = address + aOffset;
        }

        /// <summary>
        /// Write byte to adress.
        /// </summary>
        /// <param name="address">An address to write to.</param>
        /// <param name="aData">A data.</param>
        public static void Write8(uint address, byte aData)
        {
            *(uint*)address = aData;
        }

        /// <summary>
        /// Write Word to address.
        /// </summary>
        /// <param name="address">A address to write to.</param>
        /// <param name="aData">A data.</param>
        public static void Write16(uint address, ushort aData)
        {
            *(uint*)address = aData;
        }

        /// <summary>
        /// Write DWord to address.
        /// </summary>
        /// <param name="address">An address to write to.</param>
        /// <param name="aData">A data.</param>
        public static void Write32(uint address, uint aData)
        {
            *(uint*)address = aData;
        }

        /// <summary>
        /// Read byte from address.
        /// </summary>
        /// <param name="address">An address to read from.</param>
        /// <returns>byte value.</returns>
        public static byte Read8(uint address)
        {
            return (byte)*(uint*)address;
        }

        /// <summary>
        /// Read Word from address.
        /// </summary>
        /// <param name="address">An address to read from.</param>
        /// <returns>ushort value.</returns>
        public static ushort Read16(uint address)
        {
            return (ushort)*(uint*)address;
        }

        /// <summary>
        /// Read DWord from address.
        /// </summary>
        /// <param name="address">An address to read from.</param>
        /// <returns>uint value.</returns>
        public static uint Read32(uint address)
        {
            return *(uint*)address;
        }
    }

    /// <summary>
    /// MMIO class. Used to read and write to address.
    /// </summary>
    public class MMIO : MMIOBase
    {
        /// <summary>
        /// Create new instance of the <see cref="MMIO"/> class.
        /// </summary>
        /// <param name="address">An address.</param>
        public MMIO(uint address)
            : base(address)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="MMIO"/> class.
        /// </summary>
        /// <param name="address">A base address.</param>
        /// <param name="aOffset">Offset from the base address.</param>
        public MMIO(uint address, uint aOffset)
            : base(address, aOffset)
        {
        }

        /// <summary>
        /// Get and set Byte value in address.
        /// </summary>
        public byte Byte
        {
            get => Read8(Address);
            set => Write8(Address, value);
        }

        /// <summary>
        /// Get and set Word value in address.
        /// </summary>
        public ushort Word
        {
            get => Read16(Address);
            set => Write16(Address, value);
        }

        /// <summary>
        /// Get and set DWord value in address.
        /// </summary>
        public uint DWord
        {
            get => Read32(Address);
            set => Write32(Address, value);
        }
    }
}
