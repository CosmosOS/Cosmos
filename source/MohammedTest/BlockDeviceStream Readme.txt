
Ext2 is Linux fs what is its role in COSMOS

ext2 in cosmos is just a filesystem. we implemented it for cosmos because it's an easy one. 
there are IFS handlers for windows+ext2 too, but that's irrelevant. we have the Ext2FS class in cosmos. 
this one is right now only testable (and debuggable) on cosmos using vmware/qemu/whatever. 
with the StreamBlockDevice, we can test it on windows, and debug using vs
 
what does the block device do

a blockdevice provides raw access to physical blocks of data.
all filesystems and partition schemes (mbr, later on others), use blockdevices to do reading/writing
 
why do we need the streamblockdevice

we need to test writing on windows and the only restriction currently preventing us to testext2/mbr on windows, 
is not having a working blockdevice. so if we implement a StreamBlockDevice, which is working on windows, then
suddenly we can test ext2/mbr on windows. we need this because filesystems are relatively heavy on msilopcode usage/compiler capabilities. 
so whenever something goes wrong on cosmos,  we can now test it on windows too, that way we can tell whether the 
filesystem implementation is buggy, or the compiler is buggy..

what is the streamblockdevice

qemu uses (well, it's an option) flat files for harddisks. 
the streamblockdevice will fake a blockdevice, using that file & the .NET framework classes for I/O. 
with this, we can just test mbr, ext2 and later on fat12/16/32 and iso just on windows..

what does the streamblockdevice do

the blockdevice only writes physical blocks. the StreamBlockDevice will be backed by a stream, and read/write 512-byte blocks..

Is it like creating a file maybe have a ctor to take a filename and allow read write on it using .NET base classes
no, normally a filesystem/partitioning scheme is "backed" by a blockdevice 
(it uses this blockdevice class to read/write). because the blockdevices we have now are implementing ATA(PI), 
we cant use them on windows. we now implement a second blockdevice, which uses .NET Streaming stuff
to do actual reading/writing. then we can test the original filesystems/partitioning schemes on windows

