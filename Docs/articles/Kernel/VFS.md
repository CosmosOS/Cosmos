# File System

In this article we will discuss about using Cosmos VFS (virtual file system).
Cosmos VFS and the VFS manager classes, let you manage your file system.

**Attention**: **Always** format your drive with Cosmos and **only** Cosmos if you plan to use it with Cosmos. Using any other tool such as Parted, FDisk (or any other tool) might lead to weird things when using that drive with Cosmos' VFS. Those tools are much more advanced and might format and read/write to the disk differently than Cosmos.

First, we should create and initialize an instance of the VFS, this will initialize the partition and files-system lists, as well as register the new VFS.
This is essential for using the VFS.

We start with creating a global CosmosVFS, this line should appear outside of any function, and before the BeforeRun() function.

```C#
var fs = new Sys.FileSystem.CosmosVFS();
```

Next, we register our VFS at the VFS manager, this will initiate the VFS and make it usable, add this to your kernel's BeforeRun() function:

```C#
Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);
```

After the initialization process is done, a message like this would appear on your screen:
![Initialize](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Initialize.PNG)

This message is printed by the RegisterVFS() method and it provides info about the partition.

After our VFS has been initialized, we can use more interesting functions, let's go over some of them:

## Format drive

**Note**: You don't have to format your drive if you're debugging your Cosmos project with VMWare. The build will automatically add an already formatted FAT32 VMDK file for your convenience.

**Note 2**: You can only format a drive that already has been formatted with FAT32.

You can format your drive with the Format() function, just like this:

```C#
fs.Format("0" /*drive id*/, "FAT32" /*fat type*/, true /*use quick format*/);
```

**Attention**: **Don't** add anything after the drive id, or formatting won't work. You just need to put the drive id, here we put 0 to format the main drive.

## Get available free space

We use this function to get the size of the available free space in our file system, in bytes.

```C#
var available_space = fs.GetAvailableFreeSpace(@"0:\");
Console.WriteLine("Available Free Space: " + available_space);
```

![Free Space](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Free%20Space.PNG)

You have probably noticed the "0:\" argument passed to this function, this is the id of the drive that we want to get available free space of.
Cosmos using DOS drive naming system and this is why we use "0".

**Attention**: Typing "0:/" instead of "0:\" might lead to errors, you've been warned.

## Get file system type

This will let us know what is the file system type that we are using.
You **should** be seeing "FAT32", if you see other types of FAT like "FAT16" or "FAT12", then the virtual disk has probably been formatted with one of those FAT types, but remember, the best supported one is FAT32.

![System Type](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Type.PNG)

```C#
var fs_type = fs.GetFileSystemType(@"0:\");
Console.WriteLine("File System Type: " + fs_type);
```

## Get files list

We start by getting a list of DirectoryEntry, using:

```C#
var directory_list = fs.GetDirectoryListing(@"0:\");
```

Once we have it, we can get the names of our files:

```C#
foreach (var directoryEntry in directory_list)
{
    Console.WriteLine(directoryEntry.mName);
}
```

![Files List](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Files%20List.PNG)

## Read all the files in a directory

This one is a bit more tricky. We'll need to use our list of DirectoryEntry, loop through all files and print their raw content to the standard output.

Of course, we'll start with geting the list of DirectoryEntry:

```C#
var directory_list = fs.GetDirectoryListing(@"0:\");
```

Now we can go through our list, and print the raw content of each file.

```C#
try
{
    foreach (var directoryEntry in directory_list)
    {
        var file_stream = directoryEntry.GetFileStream();   
        if (file_stream.CanRead)
        {
            var entry_type = directoryEntry.mEntryType;
            if(entry_type == Sys.FileSystem.Listing.DirectoryEntryTypeEnum.File)
            {
                var content = new byte[file_stream.Length];
                file_stream.Read(content, 0, content.Length);
                Console.WriteLine("File name: " + directoryEntry.mName);
                Console.WriteLine("File size: " + directoryEntry.mSize);
                Console.WriteLine("Content: ");
                foreach (var ch in content)
                {
                    Console.Write(ch.ToString());
                }
                Console.WriteLine();
            }
        }
    }
}
catch(Exception e)
{
    Console.WriteLine(e.ToString());
}
```
![Read File](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Read%20File.PNG)

## Create new file
Reading and writing is working on existing files, but it's much more useful to write to our own files.
let's jump right into it:

```C#
try
{
    var directory_entry = fs.CreateFile(@"0:\hello_from_elia.txt");
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

This would create new, empty file (its size is 0 bytes). We can directly write to the created DirectoryEntry like the section below except replacing the **file** variable which uses the GetFile() function by this **directory_entry** which uses the CreateFile() function.

We can also [check our files list](https://github.com/CosmosOS/Cosmos/wiki/FAT-FileSystem#get-files-list) and see our new file in it.

![Create File](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Create%20File.PNG)

## Write to file

Now we will write to an existing file.
Writing to a file is almost the same as reading from a file.
Always remember that we should put our code in a try catch block.

```C#
try
{
    var file = fs.GetFile(@"0:\testing.txt");
    var file_stream = file.GetFileStream();

    if (file_stream.CanWrite)
    {
        byte[] write_buffer = Encoding.ASCII.GetBytes("Learning how to use VFS!");
        file_stream.Write(write_buffer, 0, write_buffer.Length);
    }
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

Again we use the GetFile() method, which returns a file from a path.  
Next we open a stream to the file and check if we can write to it.  
The last thing we do is writing to the stream, we use Write() method, which write a byte array to the stream.  

## Read specific file

Now we will read a specific file from a given path.  
As usual, we'll do it in a try catch block.

```C#
try
{
    var file = fs.GetFile(@"0:\testing.txt");
    var file_stream = file.GetFileStream();

    if (file_stream.CanRead)
    {
        byte[] file_buffer = new byte[file_stream.Length];
        file_stream.Read(file_buffer, 0, file_buffer.Length);
        foreach (var ch in file_buffer)
        {
            Console.Write(ch.ToString());
        }
    }
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

We use the GetFile() method, which returns a DirectoryEntry from a path.  
Next we open its stream and check if we can read it.  
The last thing we do is reading from the stream, we use the Read() method, which writes the stream to a byte array.

![Read Specific File](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Read%20Specified%20File.PNG)

Notice that there is not much difference between this and [read all the files in a directory](https://github.com/CosmosOS/Cosmos/wiki/FAT-FileSystem#read-all-the-files-in-a-directory).  
Actually the difference is by using GetFile() instead of GetDirectoryListing(), as the first returns a single DirectoryEntry, and the second returns a list of DirectoryEntry.