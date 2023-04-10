using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;
using System.Text;

/* This plug is needed only because Cosmos does not support Hashtable :-( */
namespace Cosmos.System_Plugs.System.Text
{
    [Plug(Target = typeof(Encoding))]
    public static class EncodingImpl
    {
        #region Methods

        public static string get_BodyName(Encoding aThis)
        {
            debugger.SendInternal($"Get Body name for {aThis.CodePage}");

            return (CodePage)aThis.CodePage switch
            {
                CodePage.CodePageASCII => "us-ascii",
                CodePage.CodePageUTF7 => "UTF-7",
                CodePage.CodePageUTF8 => "UTF-8",
                CodePage.CodePageUnicode => "utf-16",
                CodePage.CodePageBigEndian => "utf-16BE",
                CodePage.CodePageUTF32 => "utf-32",
                CodePage.CodePageUTF32BE => "utf-32BE",
                _ => "null",
            };
        }

        #endregion

        #region Fields

        private static readonly Debugger debugger = new("Encoding");

        #endregion
    }
}