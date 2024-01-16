using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(typeof(StreamWriter))]
    public static class StreamWriterImpl
    {
        public static void CheckAsyncTaskInProgress(StreamWriter aThis) { }


        public static void WriteFormatHelper(StreamWriter aThis, string format, object?[] args, bool appendNewLine)
        {
            string formattedString = string.Format(format, args);
            if (appendNewLine)
            {
                aThis.WriteLine(formattedString);
            }
            else
            {
                aThis.Write(formattedString);
            }
        }
    }
}