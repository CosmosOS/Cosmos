# Layout

```
F..F

Stack
  Currently only one stack as we don't have threads yet. Stack resides at top of RAM and grows down. 
  In future each process will have its own stack in DATA. And Stack master section will be eliminated.
....

Data
  -Heap
  Global heap for all processes since compiler enforces references.

Text
  All sections are fixed in size and are stacked.
  -Syslinux Boot Code
  -Cosmos Boot Code
  -Kernel
  -Apps (Monolithic currently, will move to DATA later)
  -Legacy GDT
  -IDT
  -Page Tables

0..0
```

```
MM API
-Allocate new item
-Add/remove ref
-Lock/unlock an item
-Force a compact

Implicit
-Get pointer

Internal
-Compact
-Relocate items

Properties
-Ref count
-Lock status
-Size

Handles
-Use indirect pointers via a lookup table. Handle is ptr to table. Global table to save space.
  -No way to compact tables? 
  -use linked list of tables to allow some compaction? 
  -Allocate tables to processes so they will go away 100% when process goes way since its not fully shrinkable.
  -Keep in data space in future?
  -Small tables increase compaction opportunities
-Points to actual data
-Properties are before pointer
-In atomic ops (IL emit groups) - pointer can be grabbed and stored
```

# OLD BELOW THIS POINT

On initialization of the kernel, a GlobalInformationTable is setup. This contains the address of the first DataLookupEntry

# The Memory Manager

The manager will init itself if there are no blocks. The manager is modeled after a double LinkedList and is not a List.

# Memory Layout
The layout does  not have a preset list or table but rather every item has meta data linking the next and previous item. This allows for a robust system.

The data layout is as follows:

````
Block|Block|Block etc.

Block =

Meta Data|Data

Meta data =
4 bytes (preveus block address start)|4 bytes (next block address start)|4 bytes (curent[this] block size)| 4 bytes (curent[this] block flag)
```
BlockFlags :

```
Allocated = 0,
Free = 1,
```

the final layout looks like this:

```
4 bytes|4 bytes|4 bytes|4 bytes | (size of Block) bytes  |  4 bytes|4 bytes|4 bytes | 4 bytes | (size of Block) bytes  | etc

```
Note:
this means the smallest size an Block can occupy is 17 bytes, 16 bytes for the header and 1 for the smallest data type a byte.

# Usage

### Allocation
  Allocating a block happens by finding the first free block and split it (if necessary).

## Deallocation
  Freeing a block is easy set its flag (in the metadata) to free.
