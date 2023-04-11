using GCImplementation = Cosmos.Core.GCImplementation;
using IL2CPU.API.Attribs;
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
            string fullPath = Path.GetFullPath(path);

            if (string.IsNullOrEmpty(fullPath))
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (contents.Length == 0)
            {
                File.Create(fullPath);
                return;
            }

            Global.FileSystemDebugger.SendInternal("Writing contents");

            StreamWriter Writer = new(fullPath);

            foreach (var current in contents)
            {
                Writer.WriteLine(current);
            }

            GCImplementation.Free(fullPath);

            Writer.Dispose();
        }

        public static void WriteAllBytes(string path, byte[] aData)
        {
            string fullPath = Path.GetFullPath(path);

            BinaryWriter Writer = new(new FileStream(fullPath, FileMode.OpenOrCreate));

            Writer.Write(aData);

            GCImplementation.Free(fullPath);
            Writer.Dispose();
        }
    }
}