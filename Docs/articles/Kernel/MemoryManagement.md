# Memory Management in Cosmos

Cosmos provides dynamic memory allocation on a heap and a garbage collector to free unused objects.

The heap is initialised automatically when the kernel starts and the memory managed is split into pages of 4096 bytes each.

This article provides an overview of how both Memory Allocation and the Garbage Collector can be used (Usage Section) and how they work internally (Internals Section).

## Memory Allocation

### Usage

#### Allocation

Usually users should be allocating memory indirectly by using `new` or other standard methods provided by .Net to allocate new objects. In the cases, where you want to allocate a managed block of memory, which is not part of a certain .Net type, a `ManagedMemoryBlock` or `byte[]` should suffice. If this is not sufficient, one can use `uint GCImplementation.AllocNewObject(uint aSize)` to allocate a region of `aSize` bytes. The returned uint contains the memory address and can be converted to pointer if required. 

One can manually free an object using `Heap.Free(void* aPtr)` or `GCImplementation.Free(object aObj)`. It is recommended to not manually free .Net objects unless you know what you are doing since Cosmos does not always recognise when it is accessing already freed memory and this can lead to very weird bugs. 

#### Information

Cosmos provides a few methods to get information about the heap status:

 - `GCImplementation.GetAvailableRAM()` returns the size of the memory in MB available to the heap
 - `GCImplementation.GetUsedRAM()` provides a rough estimate of how many bytes are currently in use
 - `HeapSmall.GetAllocatedObjectCount()` returns the number of .Net objects currently allocated
 - `RAT.GetPageCount(byte aType)` returns how many pages of a certain type are allocated. The different type definitions are stored in `RAT.PageType`

### Internals

The Heap is managed using the RAT (RAM Allocation Table). The RAT consists of a byte array which for every page in the heap stores its status. The table is stored at the end of the heap starting at `mRAT` and does not grow during runtime. Pages can be of the following types, `Empty`, `HeapSmall`, `HeapMedium`, `HeapLarge`, `RAT`, `SMT` and `Extension`. If the value for a type is odd, it means that the page is managed by the GC and objects stored there will be scanned and if possible freed. 

The RAT initialisation is triggered using `GCImplementation.Init()` which is called in the boot sequence defined in CosmosAssembler.cs. If the MemoryMap is available, the largest continuous region of memory is used for the heap.

The RAT is managed through the `RAT` class. Pages are allocated via `void* RAT.AllocPages(byte aType, uint aPageCount = 1)`. If more than 1 page is allocated at once, the first page will be marked as type `aType`, while all later pages are marked as `Extension`. Pages can be freed (set to type `Empty`) using `void RAT.Free(uint aPageIdx)` which also frees any extension pages which follow the first page. To convert between a pointer and the page index, the method `uint RAT.GetFirstRATIndex(void* aPtr)` can be used. 

The Heap itself is managed by the `Heap` class. It contains the mechanism to allocate (`byte* Heap.Alloc(uint aSize)`), re-allocate ('byte* Heap.Realloc(byte* aPtr, uint newSize)') and free (`void Heap.Free(void* aPtr)`) objects of various sizes. Objects are seperated by size in bytes into Small (Smaller than 1/4 Page), Medium (Smaller than 1 Page) and Large (Larger than 1 Page). Currently Medium and Large objects are managed the same way using the methods in `HeapLarge` which do little more than allocating/freeing the necessary number of pages. Small objects are managed differently in `HeapSmall`. 

Small Objects are managed using the SMT (Size Map Table), which is initalised using `void HeapSmall.InitSMT(uint aMaxItemSize)`.
The basic idea of the SMT is to allocate objects of similar sizes on the same page. The SMT grows dynamically as required.
The SMT is made up of a series of pages, each of which contains a series of `RootSMTBlock` each of which link to a chain of `SMTBlock`.
The `RootSMTBlock` can be thought of as column headers and the `SMTBlock` as the elements stored in the column.
The `RootSMTBlock` are a linked list, each containing the maximum object size stored in its pages, the location of the first `SMTBlock` for this size, and the location of the next `RootSMTBlock`.
The list is in ascending order of size, so that the smallest large enough `RootSMTBlock` is found first.
A `SMTBlock` contains a pointer to the actual page where objects are stored, how much space is left on that page, and a pointer to the next `SMTBlock`.
If every `SMTBlock` for a certain size is full, a new `SMTBlock` is allocated.
The page linked to by the `SMTBlock` is split into an array of spaces, each large enough to allocate an object of maximum size with header, which can be iterated through via index and fixed size when allocating.
Each object allocated on the `HeapSmall` has a header of 2 `ushort`, the first one storing the actual size of the object and the second, the GC status of the object.

## Garbage Collection

### Interface

The garbage collector has to be manually triggerd using the call `int Heap.Collect()` which returns the number of objects freed. 

Note that the GC does not track objects only pointed to by pointers. To ensure that the GC nevertheless does not incorrectly free objects, you can use `void GCImplementation.IncRootCount(ushort* aPtr)` to manually increase the references of your object by 1. Once you no longer need the object you can use `void GCImplementation.DecRootCount(ushort* aPtr)` to remove the manual reference, which allows the next `Heap.Collect` call to free the object. 

`Heap.Collect` only cleans up the objects which are no longer used but will leave behind empty pages in the SMT.
These pages can be cleaned up using `HeapSmall.PruneSMT` which will return the number of pages it freed.
Note that if in future elements are reallocated, this will cause new pages in the SMT to be allocated again, so using this too often may not be useful.

## Automatically Trigger Garbage Collection

When `RAT.MinFreePages` is set to a positive value and the number of free pages (as tracked by `RAT.FreePageCount`) drops below this value, on page allocation `Heap.Collect` will automatically be called. Each time this happens the value in `RAT.GCTriggered` is incremented by one.

### Internals

The garbage collector uses the tracing approach, which means that during collection a graph of all reachable objects is created and all non-discovered objects are freed. The garbage collector will only check objects on pages which have a type where the `GCManaged` bit is set. The graph is created by starting from "root" objects which are either stored in static variables or part of the current stack. Each of these objects is "marked" and all objects referenced by this object are recursivly also "marked" and "swept". This is done using the methods `void Heap.MarkAndSweepObject(void* aPtr)` for objects and `void Heap.SweepTypedObject(uint* obj, uint type)` for structures. For this to work each allocated object holds a 1bit flag if the object was discovered during the marking phase and a 7bit value counter for the number of static references it has. The number of static references an object has is updated using `void GCImplementation.IncRootCount(ushort* aPtr)` and similar methods, which are called from the Stsfld opcode.
