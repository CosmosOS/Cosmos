using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Common.Extensions;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(TargetName = "System.ParseNumbers, System.Private.CoreLib")]
    class ParseNumbersImpl
    {
        public static string IntToString(int value, int radix, int width, char paddingChar, int flags)
        {
            if (flags != 0)
            {
                throw new NotImplementedException("IntToString with non-zero flags is not supported");
            }
            string valueString = "";

            if (radix == 2 || radix == 8 || radix == 16)
            {
                int shiftRightAmount = 1;
                if (radix == 8)
                {
                    shiftRightAmount = 3;
                }
                else if (radix == 16)
                {
                    shiftRightAmount = 4;
                }
                if (value < 0)
                {
                    throw new NotImplementedException();
                }
                while (value > 0)
                {
                    valueString = (value % radix).ToString("X") + valueString;
                    value >>= shiftRightAmount;
                }
            }
            else if (radix == 10)
            {
                valueString = value.ToString();
            }
            else
            {
                throw new ArgumentException(nameof(radix));
            }

            if (width == -1)
            {
                return valueString;
            }

            if (valueString.Length > width)
            {
                throw new NotImplementedException("IntToString Case not handled when value is longer than width");
            }

            int count = width - valueString.Length;
            for (int i = 0; i < count; i++)
            {
                valueString = paddingChar + valueString;
            }
            return valueString;
        }
    }
}
