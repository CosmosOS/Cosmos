using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Tests
{
    public static class Utilities
    {
        public static string PrettyPrint(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var xSB = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (i > 0
                    && (i % 8 == 0))
                {
                    xSB.AppendLine();
                }
                xSB.Append(data[i].ToString("X2"));
                xSB.Append(" ");
            }
            return xSB.ToString().Trim();
        }
    }
}
