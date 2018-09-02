using System;

using IL2CPU.Debug.Symbols;

namespace ElfMap2DebugDb
{
    /// <summary>
    /// Application which is populate debug database with symbols from the ELF map file.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Entry point for the application.
        /// </summary>
        /// <param name="args">Parameters passed from the command line.</param>
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintHelp();
                return;
            }

            var xMapFile = args[0];
            var xDebugDatabase = args[1];
            ObjDump.ExtractMapSymbolsForElfFile(xDebugDatabase, xMapFile);
        }

        /// <summary>
        /// Prints help page for the application.
        /// </summary>
        private static void PrintHelp()
        {
            Console.WriteLine("Usage: ElfMap2DebugDb.exe <mapFile> <debugDatabaseFile>");
        }
    }
}
