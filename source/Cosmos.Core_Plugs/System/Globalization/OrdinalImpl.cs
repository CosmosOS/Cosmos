using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Globalization
{
    [Plug(TargetName = "Interop+Globalization, System.Private.CoreLib")]
    class OrdinalImpl
    {
        [PlugMethod(Signature = "System_Int32__System_Globalization_Ordinal_CompareStringIgnoreCaseNonAscii__System_Char__System_Int32___System_Char__System_Int32_", PlugRequired = true, IsOptional =false)]
        public static unsafe int CompareStringIgnoreCaseNonAscii(char* aStrA, int aLengthA, char* aStrB, int aLengthB)
        {
            if (aLengthA < aLengthB)
            {
                return -1;
            }
            else if (aLengthA > aLengthB)
            {
                return 1;
            }
            for (int i = 0; i < aLengthA; i++)
            {
                if (aStrA[i] < aStrB[i])
                {
                    return -1;
                }
                else if (aStrA[i] < aStrB[i])
                {
                    return 1;
                }
            }
            return 0;
        }
    }
}
