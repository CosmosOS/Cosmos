//#define COSMOSDEBUG
using System;
using System.IO;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(TextWriter))]
    public static class TextWriterImpl
    {
        public static void Ctor(TextWriter aThis, [FieldAccess(Name = "System.Char[] System.IO.TextWriter.CoreNewLine")] ref char[] CoreNewLine)
        {
            CoreNewLine = Environment.NewLine.ToCharArray();
        }
    }
}
