using System;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(typeof(Convert))]
    public static class ConvertImpl
    {
        #region Digit Chars

        private static char[] DigitChars = new[]
        {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            'A',
            'B',
            'C',
            'D',
            'E',
            'F',
            'G',
            'H',
            'I',
            'J',
            'K',
            'L',
            'M',
            'N',
            'O',
            'P',
            'Q',
            'R',
            'S',
            'T',
            'U',
            'V',
            'W',
            'X',
            'Y',
            'Z'
        };

        #endregion

        public static string ToString(long value, int toBase)
        {
            if (toBase > 36)
            {
                throw new NotImplementedException();
            }

            if (value == 0)
            {
                return "0";
            }

            string str = null;

            while (value != 0)
            {
                str += DigitChars[value % toBase];
                value /= toBase;
            }

            return str;
        }
    }
}
