using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Globalization
{
    [Plug(TargetName = "Interop+Globalization, System.Private.CoreLib")]
    class OrdinalImpl
    {
        [PlugMethod(IsOptional =false, PlugRequired =true)]
        public static unsafe int CompareStringIgnoreCaseNonAscii(char* aStrA, int aLengthA, char* aStrB, int aLengthB)
        {
            if(aLengthA < aLengthB)
            {
                return -1;
            } else if(aLengthA > aLengthB)
            {
                return 1;
            }
            for (int i = 0; i < aLengthA; i++)
            {
                if(aStrA[i] < aStrB[i])
                {
                    return -1;
                }
                else if(aStrA[i] < aStrB[i])
                {
                    return 1;
                }
            }
            return 0;
        }
    }
}
