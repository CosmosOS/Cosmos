# Layout
```
Top of Ram (F..F in a full system)

Stack
  Currently only one stack as we don't have threads yet.
  Stack resides at top of RAM and grows down. 
  In future each process will have its own stack in DATA. 
  And Stack master section will be eliminated.
  Because of this we currently treat the stack as a fixed size so the heap has a fixed top.
  In the future everything will be movable and thus individual stacks could even be moved, grown, or shrunk.
....

Data
  ALL items in data *must be movable* to allow very long up times of the system.
  Can relocate by moving page with in virtmem and changing refs.
  -Heap: Global heap for all processes since compiler enforces references.
  -Stacks (future)
  -Code (Future)
  -Disk Cache (Future)
  -RAT
  -All single blocks go on bottom, multi page blocks on top. Will help reduce fragmentation greatly.

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

### Notes
```
TODO
-Get some heap statistics, average size etc

Heap Containers
All meta data is identical and inline with data. Exists before handle pointer. Keeping
inline increases block size, but makes it faster to find metadata and keeps it uniform.
Could have a single pointer to another record elsewhere, but increases complexity and does not provide
major benefit.
-Data (handle pointer points here)
-32/64 Ptr to first ref
using 32 below saves space on small/med objects. slightly increases access time, but access to these fields is not time critical
-32 (64 to align on large) Ref count
-32 (64 for large) Size (always need, cant interpolate even from slotted as may be smaller than slot)
  -Always allocate bigger to word align (or page align for large?). Wont bother .NET, it never needs size from heap.
  -If need align - could add 3 and mask lower 2 bits to round up.
-Optional
  -32/64 Size allocated - may be bigger and not needed for slotted etc.

-Tiny 
If needed can make tiny types too with only ref and ptr, no need for size. But check heap and
see if such small ones are common. Probably not, likely only for boxing etc.

-Small (Tables)
Fixed sizes, max size one page. 
Keeping to one page maximizes movability.
MetaData integrated into page.
Page allocated at bottom (as with all single page items)

-Medium (Stacked)
Contains items end to end in a linked list.
Metadata storage? Prob external at bottom
Could span mult pages, but better if kept in single ones for movability. 
Faster compacting as well. 
Or at least define a default size which is a group of pages.

-Large (Single)
For large items that are close to page size or larger
Metadata is grouped from mult containers into one table to avoid excessive rewriting of references
and easy scanning of all items in system.
Could group metadata by process in future, or sort table from time to time?
Use linked list of single page tables in bottom and each item has a link back to the table so compacting is easy
Item
-Handle points directly to data

Heap Index
-Sections containing entries which
  -Point to first ref
  -Ref count
  -Other metadata about item in heap
  -Table also contains specific info about the page, largest free, smallest free, etc.
  Info could be on a delayed update

Heap pages come in several types
-Slotted - predefined sizes. 64, 256 etc... anything that sizes or smaller goes in a table
-Stacked - ie end to end wtih markers. 
-Single - large ones.. items bigger than a page. Possibly same as stacked, but with only one item. and a handle
from a grouped table rather than one table for the page.

Guest OSes can grow or release RAM as needed to host.

Allocate small and medium containers on bottom, large on top for distribution and easier reuse.

Heap items can grow by adding pages, extending size, or expanding into the slot.

DATA less likely to fragment much becuase the various container sizes aggregate and collect smaller items
together.

Code can be relocated and even split into various blocks.

Is portable and simple. Can even be used without VirtMem, but increases time during page moves.
```

```
MM API
+Allocate new item
-Add/remove ref
-Lock/unlock an item
-Force a compact
-Resize a heap item - now way to integrate .NET as it doesnt resize ever (copies instead), but may be useful internally.
Could also plug stringbuilder resize, etc. Possibly even special strings when used internally?

Implicit
+Get pointer

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
