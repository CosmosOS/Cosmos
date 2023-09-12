# Object

## Memory Layout

Each object is allocated with a header of three uints. The first uint is the type of the object, the second uint is the instance type of object (normal, boxed, array etc) and the third is the size in memory. 

## Garbage Collector Information

The garbage collector itself also alloctes its own header of 1 uint (2 ushort). The first ushort is the allocated size of the object (this includes the size of both headers) while the second tracks the GC information. 
The GC information is made up of a 1bit flag (the lowest bit) used to track if the GC has hit the object during the sweep phase already and the upper 7 bits count how many static references there are to the object. 

Combined in memory we have the format | ushort: memory size | ushort: gc info | uint: object type | uint: instance type | uint: object size | variable: object data |
