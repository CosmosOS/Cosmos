using System;
using System.Text;

namespace Cosmos.Debug.DebugConnectors
{
    partial class DebugConnector
    {
        protected static ushort GetUInt16(byte[] aBytes, int aOffset) => BitConverter.ToUInt16(aBytes, aOffset);
        protected static uint GetUInt32(byte[] aBytes, int aOffset) => BitConverter.ToUInt32(aBytes, aOffset);
        protected static ulong GetUInt64(byte[] aBytes, int aOffset) => BitConverter.ToUInt64(aBytes, aOffset);
        protected static float GetSingle(byte[] aBytes, int aOffset) => BitConverter.ToSingle(aBytes, aOffset);
        protected static double GetDouble(byte[] aBytes, int aOffset) => BitConverter.ToDouble(aBytes, aOffset);

        protected static string BytesToString(byte[] bytes, int index, int count)
        {
            if (count > 100 || count <= 0 || bytes.Length == 0)
            {
                return String.Empty;
            }
            var xSB = new StringBuilder(2 + (bytes.Length * 2));
            xSB.Append("0x");
            for (int i = index; i < index + count; i++)
            {
                xSB.Append(bytes[i].ToString("X2").ToString());
            }
            return xSB.ToString();
        }
    }
}
