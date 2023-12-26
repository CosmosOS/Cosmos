using IL2CPU.API.Attribs;
using IL2CPU.API;
using Cosmos.System;

namespace Cosmos.System_Plugs.System.IO
{
    // TODO A lot of these methods should be implemented using StreamReader / StreamWriter
    [Plug(Target = typeof(File))]
    public static class FileImpl
    {
        /*
         * Plug needed for the usual issue that Array can not be converted in IEnumerable... it is starting
         * to become annoying :-(
         */
        public static void WriteAllLines(string path, string[] contents)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (contents == null)
            {
                throw new ArgumentNullException(nameof(contents));
            }
            if (path.Length == 0)
            {
                throw new ArgumentException("Empty", nameof(path));
            }

            Global.Debugger.SendInternal("Writing contents");

            StreamWriter Writer = new(path);

            foreach (var current in contents)
            {
                Writer.WriteLine(current);
            }

            Writer.Dispose();
        }

        public static void WriteAllBytes(string path, byte[] aData)
        {
            var writer = new FileStream(path, FileMode.OpenOrCreate);
            writer.Write(aData);
            writer.Dispose();
        }
    }
}