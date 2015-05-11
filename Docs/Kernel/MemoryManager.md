# The Memory Manager

The manager will init it self if there is no blocks, the manager is model after a LinkedList and not a List.

# Memory Layout
The layout has not preset list or table but rather every item has meta data linking the next and prevuis item this allows for a robust system.

The data layout looks as follow :

```
Object|Object|Object etc.

Object =

Meta Data|Data

Meta data =
4 bytes(preveus block address start)|4 bytes(curent[this] block address start)|4 bytes(next block address start)|4 bytes(curent[this] block size)
```
the final layout looks like this:

```
4 bytes|4 bytes|4 bytes|4 bytes | (size of object) bytes  |  4 bytes|4 bytes|4 bytes | 4 bytes | (size of object) bytes  | etc

```
Note:
this means the smallest size an object can ocupy is 17 bytes, 16 bytes for the header and 1 for the smallest data type a byte.

# How this system is used.

### Allocation
  Adding an object is easy, you simply find the first free block using compare exchange and split it.

## Deallocation (free)
  Freeing ablock is easy set its size(in meta data) to 0
