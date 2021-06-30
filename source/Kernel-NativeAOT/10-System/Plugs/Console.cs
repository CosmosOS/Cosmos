using System;

namespace System
{
    public class Console
    {
        public static void WriteLine(string str)
        {
            Loader.Program.Write(str);
        }
    }
}
