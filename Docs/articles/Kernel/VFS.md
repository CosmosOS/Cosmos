# File System

In this article we will discuss about using Cosmos VFS (virtual file system).
Cosmos VFS and the VFS manager classes, let you manage your file system.

**Attention**: **Always** format your drive with Cosmos and **only** Cosmos if you plan to use it with Cosmos. Using any other tool such as Parted, FDisk (or any other tool) might lead to weird things when using that drive with Cosmos' VFS. Those tools are much more advanced and might format and read/write to the disk differently than Cosmos.
**WARNING!**: Please do **not** try this on actual hardware! It may cause **IRREPARBLE DAMAGE** to your data. It is recommended to use a virtual machine like VMware, Hyper-V, just to name a few.

First, we should create and initialize an instance of the VFS, this will initialize the partition and files-system lists, as well as register the new VFS.
This is essential for using the VFS.

We start with creating a global CosmosVFS, this line should appear outside of any function, and before the BeforeRun() function.

```C#
Sys.FileSystem.CosmosVFS fs = new Cosmos.FileSystem.CosmosVFS();
```

Next, we register our VFS at the VFS manager, this will initiate the VFS and make it usable, add this to your kernel's BeforeRun() function:

```C#
Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);
```

**Note**: From now on, we'll be using some plugged functions from ``System.IO``, so be sure to use that reference to your code. Alright, now, let's get started over some useful functions:

## Format disks

**Note**: You don't have to format your drive if you're debugging your Cosmos project with VMWare. The build will automatically add an already formatted FAT32 VMDK file for your convenience.

You can get all available disks using `VFSManager.GetDisks()`. The methods to get information about the disk or format it can be found under the [Disk](https://cosmosos.github.io/api/Cosmos.System.FileSystem.Disk.html) class. To format a disk use the [`FormatDisk(int index, string format, bool quick = true)`](https://cosmosos.github.io/api/Cosmos.System.FileSystem.Disk.html#Cosmos_System_FileSystem_Disk_FormatPartition_System_Int32_System_String_System_Boolean_)
method.


## Get available free space

We use this function to get the size of the available free space in our file system, in bytes.

```C#
var available_space = fs.GetAvailableFreeSpace(@"0:\");
Console.WriteLine("Available Free Space: " + available_space);
```

![Free Space](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Free%20Space.PNG)

You have probably noticed the "0:\" argument passed to this function, this is the id of the drive that we want to get available free space of.
Cosmos using DOS drive naming system and this is why we use "0".

**Attention**: Typing "0:/" instead of "0:\\" might lead to errors, you've been warned.

## Get file system type

This will let us know what is the file system type that we are using.
You **should** be seeing "FAT32", if you see other types of FAT like "FAT16" or "FAT12", then the virtual disk has probably been formatted with one of those FAT types, but remember, the best supported one is FAT32.

![System Type](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Type.PNG)

```C#
var fs_type = fs.GetFileSystemType(@"0:\");
Console.WriteLine("File System Type: " + fs_type);
```

## Get files list

We start by getting a list of files, using:

```C#
var files_list = Directory.GetFiles(@"0:\");
```

Once we have it, we can get the names of our files:

```C#
foreach (var file in files_list)
{
    Console.WriteLine(file);
}
```

![Files List](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Files%20List.PNG)

## Get directory listing (Files and directories)

You can get files and directory listing by using this code: 

```C#
var files_list = Directory.GetFiles(@"0:\");
var directory_list = Directory.GetDirectories(@"0:\");

foreach (var file in files_list);
{
    Console.WriteLine(file)
}
foreach (var directory in directory_list)
{
    Console.WriteLine(directory);
}
```


## Read all the files in a directory

This one is more tricky,
We need to get a list of files and print all of their content to the screen.

Of course, we'll start with geting that files list:

```C#
var directory_list = Directory.GetFiles(@"0:\");
```

Now we can go through our list, and print the raw content of each file.

```C#
try
{
    foreach (var file in directory_list)
    {
        var content = File.ReadAllText(file);

        Console.WriteLine("File name: " + file);
        Console.WriteLine("File size: " + content.Length);
        Console.WriteLine("Content: " + content);
    }
}
catch(Exception e)
{
    Console.WriteLine(e.ToString());
}
```

## Create new file
Reading and writing is working on existing files, but it's much more useful to write to our own files.
Let's jump right into it:

```C#
try
{
    var file_stream = File.Create(@"0:\testing.txt");
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

## Create a new Directory
Here is a example of code of creating a new directory:

```C#
try
{
    Directory.Create(@"0:\testing\");
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

## Deleting a file or a directory

You can also delete files or directories using this code:

```C#
try
{
    File.Delete(@"0:\testing.txt");
    Directory.Delete(@"0:\testing\");
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

## Write to file

Now we will write to an existing file.
Writing to a file is almost the same as reading from a file.
Always remember that we should put our code in a try catch block.

```C#
try
{
    File.WriteAllText(@"0:\testing.txt", "Learning how to use VFS!");
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

## Move a file

Actually, File.Move() is not plugged in Cosmos, so you need to Copy the file and then delete the old file.
Here is an example Method:
```C#
public static void MoveFile(string file, string newpath)
{
    try
    {
        File.Copy(file, newpath);
        File.Delete(file);
    }
    catch(Exception e)
    {
        Console.WriteLine(ex);
    }
}
```


## Read all text from a specific file

Now we will read a specific file from a given path.  
As usual, we'll do it in a try catch block.

```C#
try
{
    Console.WriteLine(File.ReadAllText(@"0:\testing.txt"));
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

![Read Specific File](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Read%20Specified%20File.PNG)

# Read All bytes from a specific file

As like the ReadAllText Method, ReadAllBytes should return all bytes the bytes from a file.

```C#
try
{
    Console.WriteLine(File.ReadAllBytes(@"0:\testing.txt"));
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```
