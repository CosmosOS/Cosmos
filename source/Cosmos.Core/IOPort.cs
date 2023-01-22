using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    /// <summary>
    /// IOPort static class.
    /// </summary>
    public static class IOPort
    {
        //TODO: Reads and writes can use this to get port instead of argument
        /// <summary>
        /// Write byte to port.
        /// Plugged.
        /// </summary>
        /// <param name="aPort">A port to write to.</param>
        /// <param name="aData">A data.</param>
        [PlugMethod(PlugRequired = true)]
        public static void Write8(int aPort, byte aData) => throw null;

        /// <summary>
        /// Write Word to port.
        /// Plugged.
        /// </summary>
        /// <param name="aPort">A port to write to.</param>
        /// <param name="aData">A data.</param>
        [PlugMethod(PlugRequired = true)]
        public static void Write16(int aPort, ushort aData) => throw null;

        /// <summary>
        /// Write DWord to port.
        /// Plugged.
        /// </summary>
        /// <param name="aPort">A port to write to.</param>
        /// <param name="aData">A data.</param>
        [PlugMethod(PlugRequired = true)]
        public static void Write32(int aPort, uint aData) => throw null;

        /// <summary>
        /// Read byte from port.
        /// Plugged.
        /// </summary>
        /// <param name="aPort">A port to read from.</param>
        /// <returns>byte value.</returns>
        [PlugMethod(PlugRequired = true)]
        public static byte Read8(int aPort) => throw null;

        /// <summary>
        /// Read Word from port.
        /// Plugged.
        /// </summary>
        /// <param name="aPort">A port to read from.</param>
        /// <returns>ushort value.</returns>
        [PlugMethod(PlugRequired = true)]
        public static ushort Read16(int aPort) => throw null;

        /// <summary>
        /// Read DWord from port.
        /// Plugged.
        /// </summary>
        /// <param name="aPort">A port to read from.</param>
        /// <returns>uint value.</returns>
        [PlugMethod(PlugRequired = true)]
        public static uint Read32(int aPort) => throw null;

        //TODO: Plug these Reads with asm to read directly to RAM
        // REP INSW
        /// <summary>
        /// Read byte from base port.
        /// </summary>
        /// <param name="aData">Output data array.</param>
        /// <exception cref="System.OverflowException">Thrown if aData lenght is greater than Int32.MaxValue.</exception>
        public static void Read8(int aPort, byte[] aData)
        {
            for (int i = 0; i < aData.Length / 2; i++)
            {
                var xValue = Read16(aPort);
                aData[i * 2] = (byte)xValue;
                aData[i * 2 + 1] = (byte)(xValue >> 8);
            }
        }

        /// <summary>
        /// Read Word from base port.
        /// </summary>
        /// <param name="aData">Output data array.</param>
        /// <exception cref="System.OverflowException">Thrown if aData lenght is greater than Int32.MaxValue.</exception>
        public static void Read16(int aPort, ushort[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                aData[i] = Read16(aPort);
            }
        }

        /// <summary>
        /// Read DWord from base port.
        /// </summary>
        /// <param name="aData">Output data array.</param>
        /// <exception cref="System.OverflowException">Thrown if aData lenght is greater than Int32.MaxValue.</exception>
        public static void Read32(int aPort, uint[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                aData[i] = Read32(aPort);
            }
        }

        /// <summary>
        /// Wait for the previous IO read/write to complete.
        /// </summary>
        public static void Wait()
        {
            // Write to an unused port. This assures whatever we were waiting on for a previous
            // IO read/write has completed.
            // Port 0x80 is unused after BIOS POST.
            // 0x22 is just a random byte.
            // Since IO is slow - its just a dummy sleep to wait long enough for the previous operation
            // to have effect on the target.
            Write8(0x80, 0x22);
        }
    }
}
