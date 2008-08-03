using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.FileSystem;

namespace FrodeTest.Test
{
    public class FilesystemEntryTest
    {
        public static void RunTest()
        {
            Console.WriteLine("-- Testing FilesystemEntry --");

            var xFSEntry = new FilesystemEntry();
            xFSEntry.Name = "1";

            Console.WriteLine(xFSEntry.Name);

            var two = new FilesystemEntry { Name = "2" };
            Console.WriteLine(two.Name);

            var test = new FilesystemEntryTest();
            Console.WriteLine(test.GetEntry().Name);

            foreach (FilesystemEntry xEntry in GetMultipleEntries())
            {
                Console.WriteLine(xEntry.Name);
            }
        }

        public FilesystemEntry GetEntry()
        {
            return new FilesystemEntry { Name = "3" };
        }

        public static FilesystemEntry[] GetMultipleEntries()
        {
            var xResult = new List<FilesystemEntry>(2);
            for (int i = 10; i < 20; i++)
            {
                var xEntry = new FilesystemEntry { Name = i.ToString() };
                xResult.Add(xEntry);
            }

            return xResult.ToArray();
        }
    }
}
