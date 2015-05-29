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
  Adding an Block is easy, you simply find the first free block using compare/exchange(x86 opcode-CMPXCHG) and split it.

## Deallocation
  Freeing a block is easy set its flag(in the metadata) to free.
