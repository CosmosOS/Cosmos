using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Globalization
{
    [Plug(Target = typeof(CharUnicodeInfo))]
    class CharUnicodeInfoImpl
    {
        //TODO: Implement these more correctly
        [PlugMethod(Signature = "System_Byte__System_Globalization_CharUnicodeInfo_InternalGetCategoryValue_System_Int32__System_Int32_")]
        public static byte InternalGetCategoryValue(int ch, int offset)
        {
            return InternalGetUnicodeCategory(ch);
        }

        private static byte InternalGetUnicodeCategory(int ch)
        {
            if (48 <= ch && ch <= 57)
            {
                return (byte)UnicodeCategory.DecimalDigitNumber;
            }

            if (65 <= ch && ch <= 90)
            {
                return (byte)UnicodeCategory.UppercaseLetter;
            }

            if (97 <= ch && ch <= 122)
            {
                return (byte)UnicodeCategory.LowercaseLetter;
            }

            return (byte)UnicodeCategory.OtherLetter;
        }
    }
}
