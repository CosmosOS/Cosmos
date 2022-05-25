# File System

In this article we will discuss about using Cosmos VFS (virtual file system).
Cosmos VFS and the VFS manager classes, let you manage your file system.

**Attention**: **Always** format your drive with Cosmos and **only** Cosmos if you plan to use it with Cosmos. Using any other tool such as Parted, FDisk (or any other tool) might lead to weird things when using that drive with Cosmos' VFS. Those tools are much more advanced and might format and read/write to the disk differently than Cosmos.

First, we should create and initialize an instance of the VFS, this will initialize the partition and files-system lists, as well as register the new VFS.
This is essential for using the VFS.

We start with creating a global CosmosVFS, this line should appear outside of any function, and before the BeforeRun() function.

```C#
Sys.FileSystem.CosmosVFS fs = new Sys.FileSystem.CosmosVFS();
```

Next, we register our VFS at the VFS manager, this will initiate the VFS and make it usable, add this to your kernel's BeforeRun() function:

```C#
Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);
```

**Note**: From now on, we'll be using some plugged functions from ``System.IO``, so be sure to use that reference to your code. Alright, now, let's get started over some useful functions:

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

We start by getting a list of files, using:

```C#
var directory_list = Directory.GetFiles(@"0:\");
```

Once we have it, we can get the names of our files:

```C#
foreach (var file in directory_list)
{
    Console.WriteLine(file);
}
```

![Files List](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Files%20List.PNG)

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

![Read File](https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/articles/Kernel/images/File%20System%20Read%20File.PNG)

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

We can also [check our files list](https://github.com/CosmosOS/Cosmos/wiki/FAT-FileSystem#get-files-list) and see our new file in it.

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

## Read specific file

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