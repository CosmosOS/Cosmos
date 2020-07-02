# File System

In this articel we will discuss about using Cosmos VFS (virtual file system).
Cosmos VFS and the VFS manager classes, let you manage your file system.

First, we should create and initialize an instance of the VFS;
```
var fs = new Sys.FileSystem.CosmosVFS();
fs.Initialize();
```
What is done here is that the partitions and file systems list is being initialized, and the file system is being registered.
This is essential for using the VFS.

Right after this lines is done, an message looking like this will apper on your screen:

![Alt text](System/images/File%20System%20Initialize.PNG?raw=true "Initialize")

This message is printed by the initialize method and it provide info about the file system.

After our VFS has been initialized, we can use more interesting functions, lets go over some of them:

## Get available free space.

```
long available_space = fs.GetAvailableFreeSpace("0:/");
Console.WriteLine("Available Free Space: " + available_space);
```
We use this function to get the size of the available free space in our file system, in bytes.
You have probably noticed the "0:/" argument passed to this function, this is the id of the drive we want to get available free space of.
Cosmos using DOS drive naming system and this is why we use "0".

![Alt text](System/images/File%20System%20Free%20Space.PNG?raw=true "Free Space")

## Get file system type.

```
string fs_type = fs.GetFileSystemType("0:/");
Console.WriteLine("File System Type: " + fs_type);
```
This will let us know what the file system type.

![Alt text](System/images/File%20System%20Type.PNG?raw=true "Type")

## Get files list.

We start by getting the directory entrys list, using:
```
var directory_list = fs.GetDirectoryListing("0:/");
```
Once we have it, we can get the names of our files:
```
foreach (var directoryEntry in directory_list)
{
    Console.WriteLine(directoryEntry.mName);
}
```

![Alt text](System/images/File%20System%20Files%20List.PNG?raw=true "Files List")

## Read files.

This one is more tricky,
We need to get a directoryEntryList, find files in the list and print the content to the screen.

```
var directory_list = fs.GetDirectoryListing("0:/");
```

Now we can go through our list, and and print the txt file content.
```
try
{
foreach (var directoryEntry in directory_list)
{
    var file_stream = directoryEntry.GetFileStream();
    var entry_type = directoryEntry.mEntryType;
    if(entry_type == Sys.FileSystem.Listing.DirectoryEntryTypeEnum.File)
    {
        byte[] content = new byte[file_stream.Length];
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
catch(Exception e)
{
    Console.WriteLine(e.ToString());
}
```
You must have noticed the try-catch block have been added here, you can find out more about the exception types each function in the api docs.

![Alt text](System/images/File%20System%20Read%20File.PNG?raw=true "Files List")
