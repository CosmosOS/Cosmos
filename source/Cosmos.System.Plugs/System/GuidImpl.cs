using System;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(Guid))]
    public class GuidImpl
    {

        private static char[] HEX = new char[]{
        '0', '1', '2', '3', '4', '5', '6', '7',
        '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /**
         * Convert bytes to a base16 (HEX) string.
         */
        private static String encode(byte[] byteArray)
        {
            string hexBuffer = "";
            for (int i = 0; i < byteArray.Length; i++)
                for (int j = 1; j >= 0; j--)
                    hexBuffer += HEX[(byteArray[i] >> (j * 4)) & 0xF];
            return hexBuffer;
        }

        public static Guid Empty = new Guid(0,0,0,0,0,0,0,0,0,0,0);

        public static Guid NewGuid()
        {
            
            Random rnd = new Random();
            byte[] guid = new byte[16];
            rnd.NextBytes(guid);
            guid[6] = (byte) (0x40 | ((int) guid[6] & 0xf));
            guid[8] = (byte) (0x80 | ((int) guid[8] & 0x3f));

            return new Guid(encode(guid));
        }
    }
}
