# The Memory Manager

The manager will init it self if there is no blocks, the manager is model after a double LinkedList and not a List.

# Memory Layout
The layout has not preset list or table but rather every item has meta data linking the next and previous item this allows for a robust system.

The data layout looks as follow :

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

# How this system is used.

### Allocation
  Adding an Block is easy, you simply find the first free block using compare exchange and split it.

## Deallocation (free)
  Freeing a block is easy set its flag(in meta data) to Free
