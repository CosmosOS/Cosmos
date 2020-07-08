using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

// In order to use the names at 'Cosmos.System.FileSystem' freely, we add the using statement.
// Without it, we must-have used the full name for every method and object type.
using Cosmos.System.FileSystem;

namespace VFS_example
{
    public class Kernel : Sys.Kernel
    {
        // First we create global variable of the type CosmosVFS with the name fs.
        // This will let us menage our file system.
        // We declere it right here, above BeforeRun(), so we can use it in all the methods and for the sake of code organization.
        CosmosVFS fs = new CosmosVFS();

        protected override void BeforeRun()
        {
            #region Initialize
            // We must initialize the VFS object, to do so we use VFSManager.RegisterVFS() method, which initialize and register the FS.
            // Without doing so, the VFS will not know about any FAT file systems and partitions on our drive.
            // This method will print some info about our file system(s) to the screen.
            Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);

            Console.WriteLine("This was printed by the Initialize() method.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
            #endregion


            #region GetAvailableFreeSpace
            long available_space = fs.GetAvailableFreeSpace("0:/");
            Console.WriteLine("Available Free Space: " + available_space + " Bytes");
            Console.WriteLine("Available Free Space: " + available_space / 1000000 + " MBs");
            Console.WriteLine("This is example of the useage of GetAvailableFreeSpace() method.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
            #endregion


            #region GetFileSystemType
            string fs_type = fs.GetFileSystemType("0:/");
            Console.WriteLine("File System Type: " + fs_type);
            Console.WriteLine("This is example of the useage of GetFileSystemType() method.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
            #endregion


            #region Get files list
            // GetDirectoryListing() will return us a list of all the entrys in the directory (folder).
            var directory_list_first_example = fs.GetDirectoryListing("0:/");

            // We go through all the entries and print their name.
            foreach (var directoryEntry in directory_list_first_example)
            {
                Console.WriteLine(directoryEntry.mName);
            }
            Console.WriteLine("This is example of listing the files in a folder.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
            #endregion


            #region Read all the files in a folder
            // GetDirectoryListing() will return us a list of all the entrys in the directory (folder).
            var directory_list_second_example = fs.GetDirectoryListing("0:/");
            try
            {
                foreach (var directoryEntry in directory_list_second_example)
                {
                    // We are getting the stream for each entry in the folder.
                    var file_stream = directoryEntry.GetFileStream();
                    var entry_type = directoryEntry.mEntryType;

                    // Now we should check the type of the entry, for this example we are interested in the files only.
                    if (entry_type == Sys.FileSystem.Listing.DirectoryEntryTypeEnum.File)
                    {
                        // We create a buffer, to which we will read the content of the file.
                        byte[] content = new byte[file_stream.Length];

                        // Next we read the content of the file to our buffer, starting at buffer[0] and reading (file_stream.Length) bytes.
                        file_stream.Read(content, 0, (int)file_stream.Length);
                        Console.WriteLine("File name: " + directoryEntry.mName);
                        Console.WriteLine("File size: " + directoryEntry.mSize);
                        Console.WriteLine("Content: ");
                        foreach (char ch in content)
                        {
                            Console.Write(ch.ToString());
                        }
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("This is example of reading the content of all the files in a folder.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
            #endregion


            #region Create new file
            try
            {
                if (fs == null)
                {
                    Console.WriteLine("fs is null!");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
                    Console.Clear();
                }
                fs.CreateFile(@"0:\hello_from_elia_example.txt");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("This is example of creating new file, no output should be printed by this method.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
            #endregion


            #region Write to an existing file
            try
            {
                // First we are getting the DirectoryEntry of the file.
                var hello_file = fs.GetFile(@"0:\hello_from_elia_example.txt");

                // Next we should get a stream to our file.
                var hello_file_stream = hello_file.GetFileStream();

                // We check if we can write to it.
                if (hello_file_stream.CanWrite)
                {
                    // This is the buffer we are going to write to our file, notice it's byte array.
                    byte[] text_to_write = Encoding.ASCII.GetBytes("Learning how to use VFS!");

                    // Lastly we write the buffer to our file, starting from buffer[0] and writing (text_to_write.Length) bytes.
                    hello_file_stream.Write(text_to_write, 0, text_to_write.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("This is example of writing to a file, no output should be printed by this method.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
            #endregion


            #region Read an existing file
            try
            {
                // First we are getting the DirectoryEntry of the file.
                var hello_file = fs.GetFile(@"0:\hello_from_elia_example.txt");

                // Next we should get a stream to our file.
                var hello_file_stream = hello_file.GetFileStream();

                // We check if we can read from it.
                if (hello_file_stream.CanRead)
                {
                    // This is the buffer that we are going to read our file to, notice it's byte array.
                    byte[] text_to_read = new byte[hello_file_stream.Length];

                    // Lastly we write our file to the buffer, starting from buffer[0] and writing all the bytes.
                    hello_file_stream.Read(text_to_read, 0, (int)hello_file_stream.Length);

                    // This is another way of writing byte array to the screen, instead of writing each char separately like in the 'Read all the files in a folder' example.
                    Console.WriteLine(Encoding.Default.GetString(text_to_read));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("This is example of reading an existing file.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
            #endregion
        }

        protected override void Run()
        {

        }
    }
}
