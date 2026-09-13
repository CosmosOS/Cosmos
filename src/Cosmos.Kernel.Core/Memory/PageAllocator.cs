using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory.Heap;

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// a basic page allocator
/// </summary>
internal static unsafe class PageAllocator
{
    /// <summary>
    /// Native Intel page size.
    /// </summary>
    /// <remarks><list type="bullet">
    /// <item>x86 Page Size: 4k, 2m (PAE only), 4m.</item>
    /// <item>x64 Page Size: 4k, 2m</item>
    /// </list></remarks>
    public const ulong PageSize = 4096;

    /// <summary>Default Limine Higher Half Direct Map (HHDM) offset used when no HHDM response is available (virt = phys + offset). Public so drivers translating HHDM aliases for DMA share the same base.</summary>
    public const ulong DefaultHhdmOffset = AddressSpace.KernelSpaceStart;

    /// <summary>Bytes per kilobyte; applied once for KB and twice for MB conversions in diagnostics.</summary>
    private const ulong BytesPerKilobyte = 1024;

    /// <summary>Number of RAT entries between progress messages while initializing the RAT.</summary>
    private const ulong RatInitProgressInterval = 10000;

    /// <summary>Arbitrary marker byte written and read back to verify the RAT memory is writable.</summary>
    private const byte RatWriteTestValue = 123;

    /// <summary>
    /// Start of area usable for heap, and also start of heap.
    /// </summary>
    public static byte* RamStart;

    /// <summary>
    /// Number of pages in the heap.
    /// </summary>
    /// <remarks>Calculated from mSize.</remarks>
    public static ulong TotalPageCount;

    /// <summary>
    /// Number of pages which are currently not in use
    /// </summary>
    public static ulong FreePageCount { get; private set; }

    /// <summary>
    /// Pointer to the RAT.
    /// </summary>
    /// <remarks>
    /// Covers Data area only.
    /// stored at the end of RAM
    /// </remarks>
    // We need a pointer as the RAT can move around in future with dynamic RAM etc.
    private static byte* s_mRAT;

    /// <summary>
    /// Virtual address of the RAT base (one byte per page).
    /// </summary>
    public static ulong RatAddress => (ulong)s_mRAT;

    /// <summary>
    /// Pointer to end of the heap
    /// </summary>
    private static byte* s_heapEnd;

    /// <summary>
    /// Size of heap.
    /// </summary>
    public static ulong RamSize;

    /// <summary>
    /// Convert virtual address to physical address for Higher Half Kernel mapping.
    /// Subtracts the Limine HHDM offset when the address is an HHDM alias;
    /// passes through values that look already-physical. Kernel-image
    /// addresses (statics, stack, code — the top-2GiB window) are higher-half
    /// but NOT HHDM aliases: subtracting the offset from one fabricates a
    /// garbage physical address, and a device DMA aimed at it corrupts
    /// whatever lives there. Those are rejected loudly instead — callers
    /// that need DMA from static data must copy it to a heap buffer first.
    /// </summary>
    /// <param name="virtualAddress">Virtual address to convert</param>
    /// <returns>Physical address</returns>
    public static ulong VirtualToPhysical(ulong virtualAddress)
    {
        if (virtualAddress >= AddressSpace.KernelImageWindow)
        {
            Serial.WriteString("[PageAllocator] ERROR: VirtualToPhysical(0x");
            Serial.WriteHex(virtualAddress);
            Serial.WriteString(") is a kernel-image address, not an HHDM alias\n");
            throw new ArgumentOutOfRangeException(nameof(virtualAddress),
                "Kernel-image addresses have no HHDM alias; copy to a heap buffer for DMA.");
        }

        ulong hhdmOffset = Limine.HHDM.Response != null
            ? Limine.HHDM.Response->Offset
            : DefaultHhdmOffset;

        if (virtualAddress >= hhdmOffset)
        {
            return virtualAddress - hhdmOffset;
        }
        return virtualAddress;
    }

    /// <summary>
    /// Check if an address is in a usable memory region according to Limine memory map
    /// </summary>
    /// <param name="address">Virtual address to check</param>
    /// <param name="size">Size of the region</param>
    /// <returns>True if the region is usable, false otherwise</returns>
    private static bool IsUsableMemoryRegion(byte* address, ulong size)
    {
        if (Limine.MemoryMap.Response == null)
        {
            Serial.WriteString("[PageAllocator] Warning: No memory map available, assuming usable\n");
            return true;
        }

        ulong virtualAddressStart = (ulong)address;
        ulong virtualAddressEnd = virtualAddressStart + size;

        // Convert virtual addresses to physical addresses for comparison with memory map
        ulong physicalAddressStart = VirtualToPhysical(virtualAddressStart);
        ulong physicalAddressEnd = VirtualToPhysical(virtualAddressEnd);

        Serial.WriteString("[PageAllocator] Checking virtual 0x");
        Serial.WriteHex(virtualAddressStart);
        Serial.WriteString(" -> physical 0x");
        Serial.WriteHex(physicalAddressStart);
        Serial.WriteString("\n");

        bool foundMatch = false;
        for (ulong i = 0; i < Limine.MemoryMap.Response->EntryCount; i++)
        {
            LimineMemmapEntry* entry = Limine.MemoryMap.Response->Entries[i];
            ulong entryStart = (ulong)entry->Base;
            ulong entryEnd = entryStart + entry->Length;

            // Check if our physical allocation overlaps with this memory region
            if (physicalAddressStart < entryEnd && physicalAddressEnd > entryStart)
            {
                foundMatch = true;
                Serial.WriteString("[PageAllocator] Physical region 0x");
                Serial.WriteHex(physicalAddressStart);
                Serial.WriteString("-0x");
                Serial.WriteHex(physicalAddressEnd);
                Serial.WriteString(" overlaps with entry ");
                Serial.WriteNumber(i);
                Serial.WriteString(" (0x");
                Serial.WriteHex(entryStart);
                Serial.WriteString("-0x");
                Serial.WriteHex(entryEnd);
                Serial.WriteString(") type: ");
                Serial.WriteNumber((uint)entry->Type);
                Serial.WriteString("\n");

                // Only allow allocation in usable memory
                if (entry->Type != LimineMemmapType.Usable)
                {
                    Serial.WriteString("[PageAllocator] ERROR: Attempting to allocate in non-usable memory region!\n");
                    return false;
                }
            }
        }

        if (!foundMatch)
        {
            Serial.WriteString("[PageAllocator] ERROR: Physical address 0x");
            Serial.WriteHex(physicalAddressStart);
            Serial.WriteString(" not found in any usable memory map entry!\n");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Init RAT based on Limine memory map usable regions only.
    /// This follows the Cosmos OS memory structure with RAT at the END of heap.
    /// </summary>
    /// <param name="aStartPtr">A pointer to the start of the heap (ignored, we use memory map).</param>
    /// <param name="aSize">A heap size hint (ignored, we use memory map).</param>
    /// <exception cref="Exception">Thrown if:
    /// <list type="bullet">
    /// <item>No usable memory found.</item>
    /// <item>RAM start or size is not page aligned.</item>
    /// </list>
    /// </exception>
    public static void InitializeHeap(byte* aStartPtr, ulong aSize)
    {
        Serial.WriteString("[PageAllocator] Initializing heap using Limine memory map...\n");

        // Find the largest usable memory region for our heap
        byte* usableStart = null;
        ulong usableSize = 0;

        if (Limine.MemoryMap.Response != null)
        {
            Serial.WriteString("[PageAllocator] Memory map detected with ");
            Serial.WriteNumber(Limine.MemoryMap.Response->EntryCount);
            Serial.WriteString(" entries:\n");

            // Display all memory map entries for debugging
            for (ulong i = 0; i < Limine.MemoryMap.Response->EntryCount; i++)
            {
                LimineMemmapEntry* entry = Limine.MemoryMap.Response->Entries[i];
                Serial.WriteString("[MemMap] Entry ");
                Serial.WriteNumber(i);
                Serial.WriteString(": 0x");
                Serial.WriteHex((ulong)entry->Base);
                Serial.WriteString(" - 0x");
                Serial.WriteHex((ulong)entry->Base + entry->Length);
                Serial.WriteString(" (");
                Serial.WriteNumber(entry->Length / BytesPerKilobyte / BytesPerKilobyte);
                Serial.WriteString(" MB) Type: ");
                Serial.WriteNumber((uint)entry->Type);

                // Add type name for clarity
                switch (entry->Type)
                {
                    case LimineMemmapType.Usable:
                        Serial.WriteString(" (Usable)");
                        break;
                    case LimineMemmapType.Reserved:
                        Serial.WriteString(" (Reserved)");
                        break;
                    case LimineMemmapType.AcpiReclaimable:
                        Serial.WriteString(" (ACPI Reclaimable)");
                        break;
                    case LimineMemmapType.AcpiNvs:
                        Serial.WriteString(" (ACPI NVS)");
                        break;
                    case LimineMemmapType.BadMemory:
                        Serial.WriteString(" (Bad Memory)");
                        break;
                    case LimineMemmapType.BootloaderReclaimable:
                        Serial.WriteString(" (Bootloader Reclaimable)");
                        break;
                    case LimineMemmapType.KernelAndModules:
                        Serial.WriteString(" (Kernel and Modules)");
                        break;
                    case LimineMemmapType.Framebuffer:
                        Serial.WriteString(" (Framebuffer)");
                        break;
                    default:
                        Serial.WriteString(" (Unknown)");
                        break;
                }
                Serial.WriteString("\n");
            }

            Serial.WriteString("[PageAllocator] Searching for largest safe usable memory region...\n");

            // First, collect all protected regions (kernel, framebuffer, etc.)
            ulong kernelStart = 0;
            ulong kernelEnd = 0;
            ulong framebufferStart = 0;
            ulong framebufferEnd = 0;

            for (ulong i = 0; i < Limine.MemoryMap.Response->EntryCount; i++)
            {
                LimineMemmapEntry* entry = Limine.MemoryMap.Response->Entries[i];

                if (entry->Type == LimineMemmapType.KernelAndModules)
                {
                    kernelStart = (ulong)entry->Base;
                    kernelEnd = (ulong)entry->Base + entry->Length;
                    Serial.WriteString("[PageAllocator] Protected: Kernel/Modules 0x");
                    Serial.WriteHex(kernelStart);
                    Serial.WriteString(" - 0x");
                    Serial.WriteHex(kernelEnd);
                    Serial.WriteString(" (");
                    Serial.WriteNumber((kernelEnd - kernelStart) / BytesPerKilobyte / BytesPerKilobyte);
                    Serial.WriteString(" MB)\n");
                }
                else if (entry->Type == LimineMemmapType.Framebuffer)
                {
                    framebufferStart = (ulong)entry->Base;
                    framebufferEnd = (ulong)entry->Base + entry->Length;
                    Serial.WriteString("[PageAllocator] Protected: Framebuffer 0x");
                    Serial.WriteHex(framebufferStart);
                    Serial.WriteString(" - 0x");
                    Serial.WriteHex(framebufferEnd);
                    Serial.WriteString(" (");
                    Serial.WriteNumber((framebufferEnd - framebufferStart) / BytesPerKilobyte / BytesPerKilobyte);
                    Serial.WriteString(" MB)\n");
                }
            }

            // Find the LARGEST usable memory region that doesn't conflict with protected areas
            // Following Cosmos convention: avoid kernel, framebuffer, and other protected regions
            for (ulong i = 0; i < Limine.MemoryMap.Response->EntryCount; i++)
            {
                LimineMemmapEntry* entry = Limine.MemoryMap.Response->Entries[i];
                if (entry->Type == LimineMemmapType.Usable)
                {
                    ulong physStart = (ulong)entry->Base;
                    ulong physEnd = physStart + entry->Length;

                    // ARM64 note: Check if higher-half mapping is enabled
                    // If Limine already provides virtual addresses, don't add offset
                    // x64 Limine uses higher-half, ARM64 might use direct mapping
                    ulong virtualStart;
#if ARCH_ARM64
                    // ARM64: Use physical addresses directly (identity mapping)
                    virtualStart = physStart;
                    Serial.WriteString("[PageAllocator] ARM64: Using identity mapping (phys == virt)\n");
#else
                    // x64: Use higher-half mapping
                    virtualStart = physStart + DefaultHhdmOffset;
#endif
                    ulong entrySize = entry->Length;

                    Serial.WriteString("[PageAllocator] Evaluating region: phys 0x");
                    Serial.WriteHex(physStart);
                    Serial.WriteString(" - 0x");
                    Serial.WriteHex(physEnd);
                    Serial.WriteString(" (");
                    Serial.WriteNumber(entrySize / BytesPerKilobyte / BytesPerKilobyte);
                    Serial.WriteString(" MB)");

                    // Check if this region overlaps with kernel/modules
                    bool overlapsKernel = false;
                    if (kernelStart > 0 && kernelEnd > 0)
                    {
                        // Regions overlap if one starts before the other ends
                        if (physStart < kernelEnd && physEnd > kernelStart)
                        {
                            overlapsKernel = true;
                            Serial.WriteString(" [OVERLAPS KERNEL]");
                        }
                    }

                    // Check if this region overlaps with framebuffer
                    bool overlapsFramebuffer = false;
                    if (framebufferStart > 0 && framebufferEnd > 0)
                    {
                        if (physStart < framebufferEnd && physEnd > framebufferStart)
                        {
                            overlapsFramebuffer = true;
                            Serial.WriteString(" [OVERLAPS FRAMEBUFFER]");
                        }
                    }

                    Serial.WriteString("\n");

                    // Skip regions that overlap with protected areas
                    if (overlapsKernel || overlapsFramebuffer)
                    {
                        Serial.WriteString("[PageAllocator] ⚠ Region overlaps protected memory, skipping\n");
                        continue;
                    }

                    // Use the largest safe usable region
                    if (entrySize > usableSize)
                    {
                        usableStart = (byte*)virtualStart;
                        usableSize = entrySize;

                        Serial.WriteString("[PageAllocator] ✓ Best candidate so far: ");
                        Serial.WriteNumber(usableSize / BytesPerKilobyte / BytesPerKilobyte);
                        Serial.WriteString(" MB\n");
                    }
                }
            }
        }

        if (usableStart == null || usableSize == 0)
        {
            Serial.WriteString("[PageAllocator] ERROR: No usable memory found in memory map!\n");
            throw new Exception("No usable memory found in Limine memory map");
        }

        Serial.WriteString("[PageAllocator] Selected largest usable region:\n");
        Serial.WriteString("  Start (virt): 0x");
        Serial.WriteHex((ulong)usableStart);
        Serial.WriteString("\n  Size: ");
        Serial.WriteNumber(usableSize / BytesPerKilobyte / BytesPerKilobyte);
        Serial.WriteString(" MB (");
        Serial.WriteNumber(usableSize);
        Serial.WriteString(" bytes)\n");

        // Check alignment
        if ((ulong)usableStart % PageSize != 0)
        {
            // Align start up to next page boundary
            ulong offset = PageSize - ((ulong)usableStart % PageSize);
            usableStart += offset;
            usableSize -= offset;
            Serial.WriteString("[PageAllocator] Aligned start to page boundary: 0x");
            Serial.WriteHex((ulong)usableStart);
            Serial.WriteString("\n");
        }

        if (usableSize % PageSize != 0)
        {
            // Align size down to page boundary
            usableSize = (usableSize / PageSize) * PageSize;
            Serial.WriteString("[PageAllocator] Aligned size to page boundary: ");
            Serial.WriteNumber(usableSize);
            Serial.WriteString(" bytes\n");
        }

        // Calculate total pages and RAT size
        // RAT needs 1 byte per page
        TotalPageCount = usableSize / PageSize;
        ulong xRatPageCount = (TotalPageCount - 1) / PageSize + 1;
        ulong xRatTotalSize = xRatPageCount * PageSize;

        Serial.WriteString("[PageAllocator] Total pages: ");
        Serial.WriteNumber(TotalPageCount);
        Serial.WriteString(", RAT pages: ");
        Serial.WriteNumber(xRatPageCount);
        Serial.WriteString(", RAT size: ");
        Serial.WriteNumber(xRatTotalSize / BytesPerKilobyte);
        Serial.WriteString(" KB\n");

        // IMPORTANT: Following Cosmos OS structure, place RAT at the END of usable memory
        // Heap: [RamStart ... s_heapEnd] [RAT]
        s_mRAT = usableStart + usableSize - xRatTotalSize;
        RamStart = usableStart;
        RamSize = usableSize - xRatTotalSize;
        s_heapEnd = s_mRAT;  // Heap ends where RAT begins

        Serial.WriteString("[PageAllocator] Memory layout (Cosmos-style):\n");
        Serial.WriteString("  RamStart (heap): 0x");
        Serial.WriteHex((ulong)RamStart);
        Serial.WriteString("\n  s_heapEnd: 0x");
        Serial.WriteHex((ulong)s_heapEnd);
        Serial.WriteString("\n  RAT location: 0x");
        Serial.WriteHex((ulong)s_mRAT);
        Serial.WriteString("\n  RAT end: 0x");
        Serial.WriteHex((ulong)(s_mRAT + xRatTotalSize));
        Serial.WriteString("\n  Heap size: ");
        Serial.WriteNumber(RamSize / BytesPerKilobyte / BytesPerKilobyte);
        Serial.WriteString(" MB\n");

        // Sanity checks
        if (s_mRAT < RamStart)
        {
            throw new Exception("RAT is before heap start - invalid memory layout!");
        }
        if ((ulong)s_mRAT % PageSize != 0)
        {
            throw new Exception("RAT is not page-aligned!");
        }

        // Initialize ALL RAT entries to Empty
        Serial.WriteString("[PageAllocator] Initializing ");
        Serial.WriteNumber(TotalPageCount);
        Serial.WriteString(" RAT entries...\n");

        // Test first write before loop
        Serial.WriteString("[PageAllocator] Testing first RAT write at 0x");
        Serial.WriteHex((ulong)s_mRAT);
        Serial.WriteString("...\n");
        s_mRAT[0] = (byte)PageType.Empty;
        Serial.WriteString("[PageAllocator] First write successful, initializing all entries...\n");

        for (ulong i = 0; i < TotalPageCount; i++)
        {
            s_mRAT[i] = (byte)PageType.Empty;

            // Progress indicator every 10000 pages to confirm loop is progressing
            if (i > 0 && i % RatInitProgressInterval == 0)
            {
                Serial.WriteString("[PageAllocator] Initialized ");
                Serial.WriteNumber(i);
                Serial.WriteString(" / ");
                Serial.WriteNumber(TotalPageCount);
                Serial.WriteString(" entries...\n");
            }
        }

        Serial.WriteString("[PageAllocator] All RAT entries initialized.\n");

        // Mark the RAT pages themselves as PageAllocator type
        // RAT pages are at the END, so we mark the LAST xRatPageCount pages
        ulong ratStartPage = TotalPageCount - xRatPageCount;
        Serial.WriteString("[PageAllocator] Marking RAT pages (");
        Serial.WriteNumber(xRatPageCount);
        Serial.WriteString(" pages starting at index ");
        Serial.WriteNumber(ratStartPage);
        Serial.WriteString(")...\n");

        for (ulong i = ratStartPage; i < TotalPageCount; i++)
        {
            s_mRAT[i] = (byte)PageType.PageAllocator;
        }

        // Free page count is total minus RAT pages
        FreePageCount = TotalPageCount - xRatPageCount;

        Serial.WriteString("[PageAllocator] Heap initialization complete!\n");
        Serial.WriteString("  Total pages: ");
        Serial.WriteNumber(TotalPageCount);
        Serial.WriteString("\n  Free pages: ");
        Serial.WriteNumber(FreePageCount);
        Serial.WriteString("\n  Usable heap: ");
        Serial.WriteNumber(FreePageCount * PageSize / BytesPerKilobyte / BytesPerKilobyte);
        Serial.WriteString(" MB\n");

        // Test RAT is writable
        Serial.WriteString("[PageAllocator] Testing RAT write access...\n");
        byte testValue = s_mRAT[0];
        s_mRAT[0] = RatWriteTestValue;
        if (s_mRAT[0] != RatWriteTestValue)
        {
            Serial.WriteString("[PageAllocator] ERROR: RAT is not writable!\n");
            throw new Exception("RAT memory is not writable!");
        }
        s_mRAT[0] = testValue; // Restore
        Serial.WriteString("[PageAllocator] RAT write test passed\n");

        // Initialize small heap
        Serial.WriteString("[PageAllocator] Initializing SmallHeap...\n");
        SmallHeap.Init();
    }

    /// <summary>
    /// Alloc a given number of pages, all of the same type.
    /// </summary>
    /// <param name="aType">A type of pages to alloc.</param>
    /// <param name="aPageCount">Number of pages to alloc. (default = 1)</param>
    /// <param name="zero"></param>
    /// <returns>A pointer to the first page on success, null on failure.</returns>
    public static void* AllocPages(PageType aType, ulong aPageCount = 1, bool zero = false)
    {
        Serial.WriteString("[PageAllocator] AllocPages - Type: ");
        Serial.WriteNumber((uint)aType);
        Serial.WriteString(", Count: ");
        Serial.WriteNumber(aPageCount);
        Serial.WriteString(", Free: ");
        Serial.WriteNumber(FreePageCount);
        Serial.WriteString("\n");

        byte* startPage = null;

        // Could combine with an external method or delegate, but will slow things down
        // unless we can force it to be inlined.
        // Alloc single blocks at bottom, larger blocks at top to help reduce fragmentation.
        uint xCount = 0;
        if (aPageCount == 1)
        {
            for (byte* ptr = s_mRAT; ptr < s_mRAT + TotalPageCount; ptr++)
            {
                if ((PageType)(*ptr) == PageType.Empty)
                {
                    startPage = ptr;
                    break;
                }
            }
        }
        else
        {
            // This loop will FAIL if s_mRAT is ever 0. This should be impossible though
            // so we don't bother to account for such a case. xPos would also have issues.
            for (byte* ptr = s_mRAT + TotalPageCount - 1; ptr >= s_mRAT; ptr--)
            {
                if (*ptr == (byte)PageType.Empty)
                {
                    if (++xCount == aPageCount)
                    {
                        startPage = ptr;
                        break;
                    }
                }
                else
                {
                    xCount = 0;
                }
            }
        }

        // If we found enough space, mark it as used.
        if (startPage != null)
        {
            long offset = startPage - s_mRAT;
            byte* pageAddress = RamStart + (ulong)offset * PageSize;

            if ((ulong)offset >= TotalPageCount)
            {
                return null;
            }

            s_mRAT[offset] = (byte)aType;

            for (ulong i = 1; i < aPageCount; i++)
            {
                s_mRAT[(ulong)offset + i] = (byte)PageType.Extension;
            }

            if (zero)
            {
                ulong* ptr = (ulong*)pageAddress;
                ulong count = (PageSize * aPageCount) / sizeof(ulong);
                for (ulong i = 0; i < count; i++)
                {
                    ptr[i] = 0;
                }
            }

            FreePageCount -= aPageCount;

            return pageAddress;
        }

        return null;
    }

    /// <summary>
    /// Get the first PageAllocator address.
    /// </summary>
    /// <param name="aPtr">A pointer to the block.</param>
    /// <returns>The index in RAT to which this pointer belongs</returns>
    /// <exception cref="Exception">Thrown if page type is not found.</exception>
    public static uint GetFirstPageAllocatorIndex(void* aPtr)
    {
        ulong xPos = (ulong)((byte*)aPtr - RamStart) / PageSize;
        // See note about when s_mRAT = 0 in Alloc.
        for (byte* p = s_mRAT + xPos; p >= s_mRAT; p--)
        {
            if (*p != (byte)PageType.Extension)
            {
                return (uint)(p - s_mRAT);
            }
        }

        throw new Exception("Page type not found. Likely RAT is rotten.");
    }

    /// <summary>
    /// Get the pointer to the start of the page containing the pointer's address
    /// </summary>
    /// <param name="aPtr"></param>
    /// <returns></returns>
    public static byte* GetPagePtr(void* aPtr) => (byte*)aPtr - (ulong)((byte*)aPtr - RamStart) % PageSize;

    /// <summary>
    /// Get the page type pointed by a pointer to the RAT entry.
    /// </summary>
    /// <param name="aPtr">A pointer to the page to get the type of.</param>
    /// <returns>byte value.</returns>
    /// <exception cref="Exception">Thrown if page type is not found.</exception>
    public static PageType GetPageType(void* aPtr)
    {
        if (aPtr < RamStart || aPtr > s_heapEnd)
        {
            return PageType.Empty;
        }

        return (PageType)s_mRAT[GetFirstPageAllocatorIndex(aPtr)];
    }

    /// <summary>
    /// Free page.
    /// </summary>
    /// <param name="aPageIdx">A index to the page to be freed.</param>
    public static void Free(uint aPageIdx)
    {
        byte* p = s_mRAT + aPageIdx;
        *p = (byte)PageType.Empty;
        FreePageCount++;
        for (; p < s_mRAT + TotalPageCount;)
        {
            if (*++p != (byte)PageType.Extension)
            {
                break;
            }

            *p = (byte)PageType.Empty;
            FreePageCount++;
        }
    }

    /// <summary>
    /// Free the page this pointer points to
    /// </summary>
    /// <param name="aPtr"></param>
    public static void Free(void* aPtr) => Free(GetFirstPageAllocatorIndex(aPtr));
    /// <summary>
    /// Fills out per-PageType counts by scanning the RAT. Returns zeros when
    /// the heap is not yet initialized. Used by the live-debug snapshot so
    /// the host VS Code extension can show RAT composition without pausing.
    /// </summary>
    public static void GetPageCountsByType(
        out ulong empty,
        out ulong gcHeap,
        out ulong heapSmall,
        out ulong heapMedium,
        out ulong heapLarge,
        out ulong unmanaged,
        out ulong pageDirectory,
        out ulong pageAllocator,
        out ulong smt,
        out ulong extension,
        out ulong unknown)
    {
        empty = gcHeap = heapSmall = heapMedium = heapLarge = unmanaged =
            pageDirectory = pageAllocator = smt = extension = unknown = 0;

        if (s_mRAT == null || TotalPageCount == 0)
        {
            return;
        }

        for (ulong i = 0; i < TotalPageCount; i++)
        {
            byte b = s_mRAT[i];
            switch ((PageType)b)
            {
                case PageType.Empty: empty++; break;
                case PageType.GCHeap: gcHeap++; break;
                case PageType.HeapSmall: heapSmall++; break;
                case PageType.HeapMedium: heapMedium++; break;
                case PageType.HeapLarge: heapLarge++; break;
                case PageType.Unmanaged: unmanaged++; break;
                case PageType.PageDirectory: pageDirectory++; break;
                case PageType.PageAllocator: pageAllocator++; break;
                case PageType.SMT: smt++; break;
                case PageType.Extension: extension++; break;
                default: unknown++; break;
            }
        }
    }

    internal static void DumpPageCounts()
    {
        GetPageCountsByType(
            out ulong empty,
            out ulong gcHeap,
            out ulong heapSmall,
            out ulong heapMedium,
            out ulong heapLarge,
            out ulong unmanaged,
            out ulong pageDirectory,
            out ulong pageAllocator,
            out ulong smt,
            out ulong extension,
            out ulong unknown);

        Serial.WriteString("[PageAllocator] Page counts by type:\n");
        Serial.WriteString("  Empty: "); Serial.WriteNumber(empty); Serial.WriteString("\n");
        Serial.WriteString("  GCHeap: "); Serial.WriteNumber(gcHeap); Serial.WriteString("\n");
        Serial.WriteString("  HeapSmall: "); Serial.WriteNumber(heapSmall); Serial.WriteString("\n");
        Serial.WriteString("  HeapMedium: "); Serial.WriteNumber(heapMedium); Serial.WriteString("\n");
        Serial.WriteString("  HeapLarge: "); Serial.WriteNumber(heapLarge); Serial.WriteString("\n");
        Serial.WriteString("  Unmanaged: "); Serial.WriteNumber(unmanaged); Serial.WriteString("\n");
        Serial.WriteString("  PageDirectory: "); Serial.WriteNumber(pageDirectory); Serial.WriteString("\n");
        Serial.WriteString("  PageAllocator: "); Serial.WriteNumber(pageAllocator); Serial.WriteString("\n");
        Serial.WriteString("  SMT: "); Serial.WriteNumber(smt); Serial.WriteString("\n");
        Serial.WriteString("  Extension: "); Serial.WriteNumber(extension); Serial.WriteString("\n");
        if (unknown > 0) { Serial.WriteString("  Unknown/Other: "); Serial.WriteNumber(unknown); Serial.WriteString("\n"); }
        Serial.WriteString("  Total: "); Serial.WriteNumber(TotalPageCount); Serial.WriteString("\n");
    }
}
