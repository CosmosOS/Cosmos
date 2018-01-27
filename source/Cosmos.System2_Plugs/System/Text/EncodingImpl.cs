//#define COSMOSDEBUG
using System.Text;
using Cosmos.Debug.Kernel;
using Cosmos.System2_Plugs.System.Text;
using IL2CPU.API.Attribs;

/* This plug is needed only because Cosmos does not support Hashtable :-( */
namespace Cosmos.System2_Plugs.System.Text
{
    [Plug(Target = typeof(global::System.Text.Encoding))]
    public static class EncodingImpl
    {
        private static Debugger mDebugger = new Debugger("System", "Encoding");

        enum cp {
            CodePageASCII = 20127,
            CodePageUTF7 = 65000,
            CodePageUTF8 = 65001,
            CodePageUnicode = 1200,
            CodePageBigEndian = 1201,
            CodePageUTF32 = 12000,
            CodePageUTF32BE = 12001
        };

        public static string get_BodyName(Encoding aThis)
        {
            mDebugger.SendInternal($"Get Body name for {aThis.CodePage}");

            cp cp = (cp) aThis.CodePage;
            switch (cp)
            {
                case cp.CodePageASCII:
                    return "us-ascii";

                case cp.CodePageUTF7:
                    return "UTF-7";

                case cp.CodePageUTF8:
                    return "UTF-8";

                case cp.CodePageUnicode:
                    return "utf-16";

                case cp.CodePageBigEndian:
                    return "utf-16BE";

                case cp.CodePageUTF32:
                    return "utf-32";

                case cp.CodePageUTF32BE:
                    return "utf-32BE";

                default:
                    return "null";
            }
        }
    }
}

