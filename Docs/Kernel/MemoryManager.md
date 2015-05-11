# The Memory Manager

The manager will init it self if there is no blocks, the manager is model after a double LinkedList and not a List.

# Memory Layout
The layout has not preset list or table but rather every item has meta data linking the next and previous item this allows for a robust system.

The data layout looks as follow :

```
Block|Block|Block etc.

Block =

Meta Data|Data

Meta data =
8 bytes (preveus block address start)|8 bytes (next block address start)|8 bytes (curent[this] block size)| 8 bytes (curent[this] block flag)
```
BlockFlags :

```
Allocated = 0,
Free = 1,
```

the final layout looks like this:

```
8 bytes|8 bytes|8 bytes|8 bytes | (size of Block) bytes  |  8 bytes|8 bytes|8 bytes | 8 bytes | (size of Block) bytes  | etc

```
Note:
this means the smallest size an Block can occupy is 33 bytes, 32 bytes for the header and 1 for the smallest data type a byte.

# How this system is used.

### Allocation
  Adding an Block is easy, you simply find the first free block using compare exchange and split it.

## Deallocation (free)
  Freeing a block is easy set its flag(in meta data) to Allocated
