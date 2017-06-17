using System;
using System.Text;

namespace Cosmos.Debug.DebugConnectors
{
    partial class DebugConnector
    {
        internal static string BytesToString(byte[] bytes, int index, int count)
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
