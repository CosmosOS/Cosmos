using UInt32 = System.UInt32;
using NumberStyles = System.Globalization.NumberStyles;
using ArgumentException = System.ArgumentException;

namespace Cosmos.Debug.GDB {
    public class Global {
        static public GDB GDB;
        static public AsmFile AsmSource;
		static readonly public char[] SpaceSeparator = new[] { ' ' };
		static readonly public char[] TabSeparator = new[] { '\t' };

        static public UInt32 FromHexWithLeadingZeroX(string aValue) {
			if (false == aValue.StartsWith("0x"))
				throw new ArgumentException("aValue");
            return UInt32.Parse(aValue.Substring(2), NumberStyles.HexNumber);
        }
    }
}