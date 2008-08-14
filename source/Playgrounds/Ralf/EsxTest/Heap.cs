//#define NOCOSMOS

using System;
using System.IO;
using System.Text;
using Cosmos.Kernel;

namespace EsxTest
{
    //RalfHeap

    public struct FragmentHeader
    {
        internal const UInt32 HeaderSize = 4;    //to avoid the usage of sizeof(FragmentHeader)
        internal const UInt32 MaxSizeIndex = 31; //Max value for SizeIndex (31 on 32Bit Systems) (255 on 256Bit Systems ;-)

        private const UInt32 HasChild1Bit = 0x100;
        private const UInt32 HasChild2Bit = 0x200;
        private const UInt32 HasDataBit = 0x400;
        private const UInt32 IsChild2OfParentBit = 0x800;

        private UInt32 _Header;      
        //00000000 00000000 00000000 00000000
        //|||||||| |||||||| |||||||| ||||||||
        //|||||||| |||||||| |||||||| -------- SizeIndex
        //|||||||| |||||||| ||||||||
        //|||||||| |||||||| |||||||+--------- HasChild1
        //|||||||| |||||||| ||||||+---------- HasChild2
        //|||||||| |||||||| |||||+----------- HasData           -> 1 if Fragment contains Data (in this case HasChild1 and HasChild2 must be 0)
        //|||||||| |||||||| ||||+------------ IsChild2OfParent  -> 1 if Child2 of the parent, 0 if Child1 of parent
        //|||||||| |||||||| |||+------------- Not used
        //|||||||| |||||||| ||+-------------- Not used
        //|||||||| |||||||| |+--------------- Not used
        //|||||||| |||||||| +---------------- Not used
        //|||||||| |||||||| 
        //|||||||| -------------------------- Not used 
        //|||||||| 
        //----------------------------------- Not used

        internal void Initialize(int sizeIndex)
        {
            _Header = 0;
            SizeIndex = sizeIndex;
#if NOCOSMOS
            if (SizeIndex!=sizeIndex)
            {
                Heap.CallException("Initialize failed");
            }
#endif
        }

        internal int SizeIndex
        {
            get
            {
                return (int)(_Header &0xFF);
            }
            private set
            {
                #region validation
                //general heap logic problem, if one these exceptions occurs 
                if (value > MaxSizeIndex)
                {
                    Heap.CallException("FragmentHeader.SizeIndex overflow");
                }
                #endregion
                _Header = _Header & 0xFF;
                _Header = _Header | (UInt32)(value);
            }
        }

        internal bool HasChild1
        {
            get { return (_Header & HasChild1Bit) != 0; }
            set
            {
                if (value)
                {
                    #region validation
                    //general heap logic problem, if one these exceptions occurs 
                    if (HasData)
                    {
                        Heap.CallException("Set HasChild1 true failed! Data exists");
                    }
                    #endregion
                    _Header = (_Header | HasChild1Bit);
                }
                else
                {
                    _Header = _Header & ~HasChild1Bit;
                }
            }
        }

        internal bool HasChild2
        {
            get { return (_Header & HasChild2Bit) != 0; }
            set
            {
                if (value)
                {
                    #region validation
                    //general heap logic problem, if one these exceptions occurs 
                    if (HasData)
                    {
                        Heap.CallException("Set HasChild2 true failed! Data exits");
                    }
                    #endregion
                    _Header = (_Header | HasChild2Bit);
                }
                else
                {
                    _Header = _Header & ~HasChild2Bit;
                }
            }
        }

        internal bool HasChild { get { return HasChild1 || HasChild2; } }

        internal bool HasData
        {
            get { return (_Header & HasDataBit) != 0; }
            set
            {
                if (value)
                {
                    #region validation
                    //general heap logic problem, if one these exceptions occurs 
                    if (HasChild)
                    {
                        Heap.CallException("Set HasData true failed! Child exits");
                    }
                    #endregion
                    _Header = _Header | HasDataBit;
                }
                else
                {
                    _Header = _Header & ~HasDataBit;
                }
            }
        }

        internal bool IsEmpty { get { return !(HasData || HasChild); } }

        internal bool IsChild2OfParent
        {
            get { return (_Header & IsChild2OfParentBit) != 0; }
            set
            {
                if (value)
                {
                    _Header = (_Header | IsChild2OfParentBit);
                }
                else
                {
                    _Header = (_Header & ~IsChild2OfParentBit);
                }
            }
        }

        internal unsafe UInt32 FragmentAddress
        {
            get
            {
                fixed (UInt32* address = &_Header)
                {
                    return (UInt32) address;
                }
            }
        }

        internal UInt32 Child1Address
        {
            get
            {
                return FragmentAddress + HeaderSize;
            }
        }

        internal UInt32 Child2Address
        {
            get
            {
                return FragmentAddress + Heap.GetFragmentSize(SizeIndex - 1) + HeaderSize;
            }
        }

        internal UInt32 DataAddress
        {
            get
            {
                return FragmentAddress + HeaderSize;
            }
        }

        internal bool HasParent
        {
            get
            {
                if (FragmentAddress > Heap.StartAddress)
                {
                    return true;
                }
                return false;
            }
        }

        internal UInt32 ParentAddress
        {
            get
            {
                #region validation
                //general heap logic problem, if one these exceptions occurs 
                if (FragmentAddress == Heap.StartAddress)
                {
                    Heap.CallException("No Parent available");
                }
                #endregion
                if (IsChild2OfParent)
                {
                    return FragmentAddress - Heap.GetFragmentSize(SizeIndex) - HeaderSize;
                }
                return FragmentAddress - HeaderSize;
            }
        }

        internal unsafe void AllocateInParent()
        {
            var parentPtr = (FragmentHeader*) ParentAddress;
            #region validation
            //general heap logic problem, if one these exceptions occurs 
            if (parentPtr->HasData)
            {
                Heap.CallException("AllocateInParent failed: Data already exits", FragmentAddress);
            }
            if (IsChild2OfParent)
            {
                if (parentPtr->HasChild2)
                {
                    Heap.CallException("AllocateInParent failed: Child2 already exits", FragmentAddress);
                }
            }
            else
            {
                if (parentPtr->HasChild1)
                {
                    Heap.CallException("AllocateInParent failed: Child1 already exits", FragmentAddress);
                }
            }
            #endregion
            if (IsChild2OfParent)
            {
                parentPtr->HasChild2 = true;
            }
            else
            {
                parentPtr->HasChild1 = true;
            }

        }

        internal unsafe void FreeInParent()
        {
            var parentPtr = (FragmentHeader*) ParentAddress;
            #region validation
            //general heap logic problem, if one these exceptions occurs 
            if (parentPtr->HasData)
            {
                Heap.CallException("FreeInParent failed: Parent HasData ", FragmentAddress);
            }
            if (IsChild2OfParent)
            {
                if (!parentPtr->HasChild2)
                {
                    Heap.CallException("FreeInParent failed: Child2 doesnt exits", FragmentAddress);
                }
            }
            else
            {
                if (!parentPtr->HasChild1)
                {
                    Heap.CallException("FreeInParent failed: Child1 doesnt exits", FragmentAddress);
                }
            }
            #endregion
            if (IsChild2OfParent)
            {
                parentPtr->HasChild2 = false;
            }
            else
            {
                parentPtr->HasChild1 = false;
            }
        }

        internal static unsafe void Debug(UInt32 address)
        {
#if NOCOSMOS
            if (address == 0)
            {
                Heap.Debug("[]");
                return;
            }
            var header = (FragmentHeader*)address;
            Heap.Debug("[" + Convert.ToString(address - Heap.StartAddress, 10) + "]   si=" + (*header).SizeIndex + " header=" +
                       Convert.ToString((*header)._Header, 16));

//            StringBuilder sb = new StringBuilder();
//            
//            for (int i = 0; i < Heap.GetFragmentDataSize((*header).SizeIndex); i++)
//            {
//                var b = (byte*) ((*header).DataAddress+i);
//                sb.Append(Convert.ToString(*b,16));
//                sb.Append(" ");
//            }
//            Heap.Debug(sb.ToString());
#else
            Heap.WriteNumber(address);
//            Heap.Debug("");
//            Heap.DebugAppendAddress(address);
//            Heap.DebugAppend("si=");
//            Heap.DebugAppend((UInt32)(*header).SizeIndex);
//            if ((*header).IsChild2OfParent)
//            {
//                Heap.DebugAppend(" PC2");
//            }
//            else
//            {
//                Heap.DebugAppend(" PC1");
//            }
//            if ((*header).HasData)
//            {
//                Heap.DebugAppend(" D=1");
//            }
//            else
//            {
//                Heap.DebugAppend(" D=0");
//            }
//            if ((*header).HasChild1)
//            {
//                Heap.DebugAppend(" C1=1");
//            }
//            else
//            {
//                Heap.DebugAppend(" C1=0");
//            }
//            if ((*header).HasChild2)
//            {
//                Heap.DebugAppend(" C2=1");
//            }
//            else
//            {
//                Heap.DebugAppend(" C2=0");
//            }
//
#endif
                                       
        }
    }

    public static class HeapCounter
    {
        public static UInt32 Count { get; internal set; }
        public static UInt32 DataSize { get; internal set; }        //should be UInt64 
        public static UInt32 MemAlloc { get; internal set; }        //should be UInt64 
        public static UInt32 MemFree { get; internal set; }         //should be UInt64 
        public static UInt32 CachePush { get; internal set; }       //should be UInt64 
        public static UInt32 CachePop { get; internal set; }        //should be UInt64 
        public static UInt32 Create { get; internal set; }          //should be UInt64 
        public static UInt32 Search { get; internal set; }          //should be UInt64 
        public static UInt32 SearchSuccess { get; internal set; }   //should be UInt64 
        public static UInt32 SearchTotal { get; internal set; }     //should be UInt64 includes recursive calls

        public static void Print()
        {
            Console.Write("Count........ =");
            Heap.WriteNumber(Count);
            Console.WriteLine();
            Console.Write("DataSize..... =");
            Heap.WriteNumber(DataSize);
            Console.WriteLine();
            Console.Write("MemAlloc..... =");
            Heap.WriteNumber(MemAlloc);
            Console.WriteLine();
            Console.Write("MemFree...... =");
            Heap.WriteNumber(MemFree);
            Console.WriteLine();
            Console.Write("CachePush.... =");
            Heap.WriteNumber(CachePush);
            Console.WriteLine();
            Console.Write("CachePop..... =");
            Heap.WriteNumber(CachePop);
            Console.WriteLine();
            Console.Write("Create....... =");
            Heap.WriteNumber(Create);
            Console.WriteLine();
            Console.Write("Search....... =");
            Heap.WriteNumber(Search);
            Console.WriteLine();
            Console.Write("SearchSuccess =");
            Heap.WriteNumber(SearchSuccess);
            Console.WriteLine();
            Console.Write("SearchTotal.. =");
            Heap.WriteNumber(SearchTotal);
            Console.WriteLine();
        }
    }

    public static class Heap
    {       
        private const byte PointerSize = 4; //4 Byte on 32Bit Systems
        internal static UInt32 StartAddress { get; private set; }
        private static UInt32 EndAddress { get; set; }
        private static UInt32 Size { get; set; }
        internal static UInt32 MaxFragmentDataSize { get; private set; }
        internal static int MaxFragmentSizeIndex { get; private set; }

        private static bool Initiated { get; set; }

        public static unsafe void Init(UInt32 startAddress,UInt32 size, UInt32 cacheSize)
        {
            Debug("Begin Init");
            if (Initiated)
                return;
            DebugActive = false;
#if NOCOSMOS
            DebugActive = true;
#endif
            if (cacheSize>=size)
            {
                CallException("Init: cachesize>=size");
            }
            CacheSize = cacheSize;

            MaxStackValue = CacheSize / CacheStackPointerMemorySize;
            if (MaxStackValue == 0)
            {
                CallException("Init: cacheSize too small");
            }
            if (CacheSize % CacheStackPointerMemorySize!=0)
            {
                CallException("Init: wrong cacheSize");                
            }
            CacheStackPointerAddress = startAddress;
            ZeroFill(CacheStackPointerAddress, CacheStackPointerMemorySize);

            CacheAddress = CacheStackPointerAddress + CacheStackPointerMemorySize;
            ZeroFill(CacheAddress, CacheSize);

            StartAddress = CacheAddress + CacheSize;
            EndAddress = StartAddress 
                + (size - CacheSize - CacheStackPointerMemorySize) 
                - (PointerSize - ((size - CacheSize - CacheStackPointerMemorySize) % PointerSize)) - PointerSize;
            Size = EndAddress - StartAddress;
            MaxFragmentDataSize = Size - FragmentHeader.HeaderSize;
            CalculateFragmentSizes(Size);
            var rootPtr = (FragmentHeader*)StartAddress;
            rootPtr->Initialize(MaxFragmentSizeIndex);
            //ClearFragment(StartAddress, GetFragmentDataSize((*RootHeader).SizeIndex)); //this is bad for ESX Server
#if NOCOSMOS
            Debug("CacheStackPointerAddress=" + CacheStackPointerAddress);
            Debug("CacheAddress=" + CacheAddress);
            Debug("StartAddress=" + StartAddress);
            Debug("EndAddress=" + EndAddress);
            Debug("");
#endif
            PushFreeFragmentAddress(MaxFragmentSizeIndex, StartAddress);
            if (MaxFragmentDataSize == 0)
            {
                CallException("Heap Init failed!");
            }
            Initiated = true;
            Debug("End Init");
        }



        public static unsafe UInt32 MemAlloc(UInt32 size)
        {
#if NOCOSMOS
            Debug("Begin Malloc HeaderSize=" + size);
#else
            Debug("Begin Malloc");
#endif
            if (!Initiated)
            {
                CallException("MemAlloc: not Initiated"); //Todo: eliminate this behaviour, we need a centralized point for Heap.Init
            }
            if (size == 0)
            {
                CallException("MemAlloc: size==0");
            }
            int sizeIndex = GetSizeIndex(size);
            if (GetFragmentDataSize(sizeIndex) > MaxFragmentDataSize)
            {
                CallException("MemAlloc: Too large memory block allocated!");
            }
            var fragmentAddress = GetFreeFragmentAddress(sizeIndex);
            if (fragmentAddress == 0)
            {
                fragmentAddress = CreateFragment(sizeIndex);
            }
            if (fragmentAddress==0)
            {
                CallException("MemAlloc: Out of memory");
            }
            var fragmentPtr = (FragmentHeader*)fragmentAddress;
            fragmentPtr->HasData = true;
            #region validation
            //general heap logic problem, if one these exceptions occurs 
            if (fragmentPtr->SizeIndex != sizeIndex)
            {
                CallException("MemAlloc: SizeIndex wrong", fragmentAddress);
            }
            if (GetFragmentDataSize(fragmentPtr->SizeIndex) < size)
            {
                CallException("MemAlloc: HeaderSize mismatch", fragmentAddress);
            }
            #endregion
            ClearFragment(fragmentAddress,size);            
            ++HeapCounter.MemAlloc;
            ++HeapCounter.Count;
            HeapCounter.DataSize += GetFragmentSize(fragmentPtr->SizeIndex);
            Debug(fragmentAddress);
#if NOCOSMOS
            Debug("End Malloc HeaderSize=" + size);
            Debug("");
            Debug(StartAddress);
            Debug("");
#else
            Debug("End Malloc");
#endif
            return fragmentAddress + FragmentHeader.HeaderSize;
        }

        public static unsafe void MemFree(UInt32 pointer)
        {
            Debug("Begin MemFree");
            if (pointer == 0)
            {
                CallException("MemFree: pointer==0");
            }
            var fragmentAddress = pointer - FragmentHeader.HeaderSize;
            var fragmentPtr = (FragmentHeader*)fragmentAddress;
            Debug(fragmentAddress);
            fragmentPtr->HasData = false;
            fragmentPtr->FreeInParent();
            PushFreeFragmentAddress(fragmentPtr->SizeIndex, fragmentAddress);
            ++HeapCounter.MemFree;
            --HeapCounter.Count;
            HeapCounter.DataSize -= GetFragmentSize(fragmentPtr->SizeIndex);
            Debug("End MemFree");
        }

        private static unsafe void ClearFragment(UInt32 fragmentAddress, UInt32 size)
        {
#if NOCOSMOS
            Debug("Begin ClearFragment HeaderSize=" + size);
#else
            Debug("Begin ClearFragment");
#endif

            Debug(fragmentAddress);
            #region validation
            //general heap logic problem, if one these exceptions occurs 
            var fragmentPtr = (FragmentHeader*) fragmentAddress;
            if (size>GetFragmentDataSize(fragmentPtr->SizeIndex))
            {
                CallException("ClearFragment: size mismatch", fragmentAddress);
            }
            #endregion
            ZeroFill(fragmentAddress + FragmentHeader.HeaderSize, size);
            Debug("After Clear:");
            Debug(fragmentAddress);
#if NOCOSMOS
            Debug("End ClearFragment HeaderSize=" + size);
#else
            Debug("End ClearFragment");
#endif
        }

        private static unsafe void ZeroFill(UInt32 address, UInt32 size)
        {            
#if NOCOSMOS
            int* ptr = (int*) address;
            for (UInt32 i = 0; i < size /4 ; i++)
            {
                *ptr = 0;
                ptr++;
            }
#else
            CPU.ZeroFill(address, size);
#endif
        }

        private static int GetSizeIndex(UInt32 size)
        {
            for (int i = 0; i < 32; i++)
            {
                if (GetFragmentDataSize(i) >= size)
                {
#if NOCOSMOS
                    Debug("GetSizeIndex for size=" + size + " result=" + i);
#endif
                    return i;
                }
            }
            CallException("GetSizeIndex failed");
            return -1;
        }

        private static unsafe UInt32 GetFreeFragmentAddress(int sizeIndex)
        {
#if NOCOSMOS
            Debug("Begin GetFreeFragmentAddress si=" + sizeIndex);
#else
            Debug("Begin GetFreeFragmentAddress");
#endif
            var fragmentAddress = PopFreeFragmentAddress(sizeIndex);
            if (fragmentAddress != 0)
            {
                if (fragmentAddress != StartAddress)
                {
                    var fragmentPtr = (FragmentHeader*) fragmentAddress;
                    fragmentPtr->AllocateInParent();
                }
            }
            else
            {
                //Starting at the root of the heap
                ++HeapCounter.Search;
                fragmentAddress = SearchFreeFragmentAddress(sizeIndex, StartAddress);
                if (fragmentAddress != 0)
                {
                    Debug("Search success");
                    Debug(fragmentAddress);
                }
            }

            Debug(fragmentAddress);
#if NOCOSMOS
            Debug("End GetFreeFragmentAddress si=" + sizeIndex);
#else
            Debug("End GetFreeFragmentAddress");
#endif
            return fragmentAddress;
        }

        private static unsafe UInt32 SearchFreeFragmentAddress(int sizeIndex, UInt32 fragmentAddress)
        {
#if NOCOSMOS
            Debug("Begin SearchFreeFragmentAddress for sizeIndex=" + sizeIndex + "in Fragment:");
#else
            Debug("Begin SearchFreeFragmentAddress");
#endif
            Debug(fragmentAddress);
            ++DebugTab;
            ++HeapCounter.SearchTotal;
            UInt32 freeFragmentAddress = 0;            
            var fragmentPtr = (FragmentHeader*)fragmentAddress;
            if (sizeIndex+1 <= fragmentPtr->SizeIndex)
            {
                if (sizeIndex+1 == fragmentPtr->SizeIndex)
                {
                    if (fragmentPtr->IsEmpty)
                    {
                        freeFragmentAddress = fragmentPtr->Child1Address;
                        var child1Ptr = (FragmentHeader*) freeFragmentAddress;
                        child1Ptr->Initialize(sizeIndex);
                        fragmentPtr->HasChild1 = true;
                        var child2Ptr = (FragmentHeader*)fragmentPtr->Child2Address;
                        child2Ptr->Initialize(sizeIndex);
                        child2Ptr->IsChild2OfParent = true;
                        PushFreeFragmentAddress(sizeIndex, fragmentPtr->Child2Address);
                        ++HeapCounter.SearchSuccess;
                    }
                }
                if (freeFragmentAddress == 0)
                {
                    if (fragmentPtr->HasChild1)
                    {
                        freeFragmentAddress = SearchFreeFragmentAddress(sizeIndex, fragmentPtr->Child1Address);
                    }
                    if (freeFragmentAddress == 0)
                    {
                        if (fragmentPtr->HasChild2)
                        {
                            freeFragmentAddress = SearchFreeFragmentAddress(sizeIndex , fragmentPtr->Child2Address);
                        }
                    }
                }
            }
            if (freeFragmentAddress != 0)
            {
                var freeFragmentPtr = (FragmentHeader*) freeFragmentAddress;
                if (freeFragmentPtr->SizeIndex!=sizeIndex)
                {
                    CallException("SearchFreeFragmentAddress: SizeIndex mismatch", freeFragmentAddress);
                }                
            }
            --DebugTab;
            if (freeFragmentAddress != 0)
            {
                Debug("Found:");
                Debug(freeFragmentAddress);
            }
#if NOCOSMOS
            Debug("End SearchFreeFragmentAddress for sizeIndex=" + sizeIndex);
#else
            Debug("End SearchFreeFragmentAddress");
#endif
            return freeFragmentAddress;
        }

        private static unsafe UInt32 CreateFragment(int sizeIndex)
        {
#if NOCOSMOS
            Debug("Begin CreateFragment si=" + sizeIndex);
#else
            Debug("Begin CreateFragment");
#endif
            ++DebugTab;
            if (sizeIndex>=MaxFragmentSizeIndex)
            {
                CallException("CreateFragment: sizeIndex overflow");
            }
            
            if (sizeIndex > MaxFragmentSizeIndex)
            {
                CallException("CreateFragment failed (sizeIndex)");
            }
            UInt32 fragmentAddress = 0;
            UInt32 parentFragmentAddress = GetFreeFragmentAddress(sizeIndex + 1);
            if ((parentFragmentAddress == 0))
            {
                parentFragmentAddress = CreateFragment(sizeIndex + 1);
            }
            if (parentFragmentAddress == 0)
            {
                //Out of memory
                //CallException("CreateFragment failed parentFragmentAddress=0");
            }
            else
            {
                var parentFragmentPtr = (FragmentHeader*) parentFragmentAddress;

                if (parentFragmentPtr->HasChild1 && parentFragmentPtr->HasChild2)
                {
                    //Out of memory
                    return 0;
                }

                parentFragmentPtr->HasChild1 = true;
                fragmentAddress = parentFragmentPtr->Child1Address;
                var child2Address = parentFragmentPtr->Child2Address;
                var child2Ptr = (FragmentHeader*) child2Address;
                child2Ptr->Initialize(sizeIndex);
                child2Ptr->IsChild2OfParent = true;
                PushFreeFragmentAddress(sizeIndex, child2Address);
                if (fragmentAddress == 0)
                {
                    CallException("CreateFragment failed");
                }
                ++HeapCounter.Create;
                var fragmentPtr = (FragmentHeader*) fragmentAddress;
                fragmentPtr->Initialize(sizeIndex);
                --DebugTab;
                Debug("Created:");
                Debug(fragmentAddress);
#if NOCOSMOS
                Debug("End CreateFragment si=" + sizeIndex);
#else
                Debug("End CreateFragment");
#endif
            }
            return fragmentAddress;
        }

        #region FragmentSize

        internal static UInt32 FragmentSize00 { get; set; }
        internal static UInt32 FragmentSize01 { get; set; }
        internal static UInt32 FragmentSize02 { get; set; }
        internal static UInt32 FragmentSize03 { get; set; }
        internal static UInt32 FragmentSize04 { get; set; }
        internal static UInt32 FragmentSize05 { get; set; }
        internal static UInt32 FragmentSize06 { get; set; }
        internal static UInt32 FragmentSize07 { get; set; }
        internal static UInt32 FragmentSize08 { get; set; }
        internal static UInt32 FragmentSize09 { get; set; }
        internal static UInt32 FragmentSize10 { get; set; }
        internal static UInt32 FragmentSize11 { get; set; }
        internal static UInt32 FragmentSize12 { get; set; }
        internal static UInt32 FragmentSize13 { get; set; }
        internal static UInt32 FragmentSize14 { get; set; }
        internal static UInt32 FragmentSize15 { get; set; }
        internal static UInt32 FragmentSize16 { get; set; }
        internal static UInt32 FragmentSize17 { get; set; }
        internal static UInt32 FragmentSize18 { get; set; }
        internal static UInt32 FragmentSize19 { get; set; }
        internal static UInt32 FragmentSize20 { get; set; }
        internal static UInt32 FragmentSize21 { get; set; }
        internal static UInt32 FragmentSize22 { get; set; }
        internal static UInt32 FragmentSize23 { get; set; }
        internal static UInt32 FragmentSize24 { get; set; }
        internal static UInt32 FragmentSize25 { get; set; }
        internal static UInt32 FragmentSize26 { get; set; }
        internal static UInt32 FragmentSize27 { get; set; }
        internal static UInt32 FragmentSize28 { get; set; }
        internal static UInt32 FragmentSize29 { get; set; }
        internal static UInt32 FragmentSize30 { get; set; }
        internal static UInt32 FragmentSize31 { get; set; }

        private static void SetFragmentSize(int sizeIndex, UInt32 size)
        {
            switch (sizeIndex)
            {
                case 0:
                    FragmentSize00 = size;
                    break;
                case 1:
                    FragmentSize01 = size;
                    break;
                case 2:
                    FragmentSize02 = size;
                    break;
                case 3:
                    FragmentSize03 = size;
                    break;
                case 4:
                    FragmentSize04 = size;
                    break;
                case 5:
                    FragmentSize05 = size;
                    break;
                case 6:
                    FragmentSize06 = size;
                    break;
                case 7:
                    FragmentSize07 = size;
                    break;
                case 8:
                    FragmentSize08 = size;
                    break;
                case 9:
                    FragmentSize09 = size;
                    break;
                case 10:
                    FragmentSize10 = size;
                    break;
                case 11:
                    FragmentSize11 = size;
                    break;
                case 12:
                    FragmentSize12 = size;
                    break;
                case 13:
                    FragmentSize13 = size;
                    break;
                case 14:
                    FragmentSize14 = size;
                    break;
                case 15:
                    FragmentSize15 = size;
                    break;
                case 16:
                    FragmentSize16 = size;
                    break;
                case 17:
                    FragmentSize17 = size;
                    break;
                case 18:
                    FragmentSize18 = size;
                    break;
                case 19:
                    FragmentSize19 = size;
                    break;
                case 20:
                    FragmentSize20 = size;
                    break;
                case 21:
                    FragmentSize21 = size;
                    break;
                case 22:
                    FragmentSize22 = size;
                    break;
                case 23:
                    FragmentSize23 = size;
                    break;
                case 24:
                    FragmentSize24 = size;
                    break;
                case 25:
                    FragmentSize25 = size;
                    break;
                case 26:
                    FragmentSize26 = size;
                    break;
                case 27:
                    FragmentSize27 = size;
                    break;
                case 28:
                    FragmentSize28 = size;
                    break;
                case 29:
                    FragmentSize29 = size;
                    break;
                case 30:
                    FragmentSize30 = size;
                    break;
                case 31:
                    FragmentSize31 = size;
                    break;
                default:
                    CallException("SetFragmentAddress");
                    break;
            }
        }

        internal static UInt32 GetFragmentSize(int sizeIndex)
        {
            switch (sizeIndex)
            {
                case 0:
                    return FragmentSize00;
                case 1:
                    return FragmentSize01;
                case 2:
                    return FragmentSize02;
                case 3:
                    return FragmentSize03;
                case 4:
                    return FragmentSize04;
                case 5:
                    return FragmentSize05;
                case 6:
                    return FragmentSize06;
                case 7:
                    return FragmentSize07;
                case 8:
                    return FragmentSize08;
                case 9:
                    return FragmentSize09;
                case 10:
                    return FragmentSize10;
                case 11:
                    return FragmentSize11;
                case 12:
                    return FragmentSize12;
                case 13:
                    return FragmentSize13;
                case 14:
                    return FragmentSize14;
                case 15:
                    return FragmentSize15;
                case 16:
                    return FragmentSize16;
                case 17:
                    return FragmentSize17;
                case 18:
                    return FragmentSize18;
                case 19:
                    return FragmentSize19;
                case 20:
                    return FragmentSize20;
                case 21:
                    return FragmentSize21;
                case 22:
                    return FragmentSize22;
                case 23:
                    return FragmentSize23;
                case 24:
                    return FragmentSize24;
                case 25:
                    return FragmentSize25;
                case 26:
                    return FragmentSize26;
                case 27:
                    return FragmentSize27;
                case 28:
                    return FragmentSize28;
                case 29:
                    return FragmentSize29;
                case 30:
                    return FragmentSize30;
                case 31:
                    return FragmentSize31;
                default:
                    CallException("GetFragmentSize");
                    return 0;
            }
        }

        internal static UInt32 GetFragmentDataSize(int sizeIndex)
        {
            return GetFragmentSize(sizeIndex) - FragmentHeader.HeaderSize;
        }

        private static void CalculateFragmentSizes(UInt32 maxSize)
        {
            int startSizeIndex = 31;
            bool success=false;
            while (startSizeIndex>=0)
            {
                success = true;
                SetFragmentSize(startSizeIndex, maxSize);
                for (int sizeIndex = startSizeIndex - 1; sizeIndex >= 0; sizeIndex--)
                {
                    var childFragmentSize = GetFragmentSize(sizeIndex + 1) / 2 - FragmentHeader.HeaderSize;
                    childFragmentSize -= (PointerSize - (childFragmentSize % PointerSize)); //ensure alignment
                    if ((childFragmentSize==0) ||(childFragmentSize>GetFragmentSize(sizeIndex + 1)))
                    {
                        success = false;
                        break;
                    }
                    SetFragmentSize(sizeIndex, childFragmentSize);
                }
                if (success)
                {
                    MaxFragmentSizeIndex = startSizeIndex;
                    break;
                }
                --startSizeIndex;
            }
            if (!success)
            {
                CallException("CalculateFragmentSizes: no success");
            }
            for (int sizeIndex = MaxFragmentSizeIndex + 1; sizeIndex < 32; sizeIndex++)
            {
                SetFragmentSize(sizeIndex, 0);
            }
#if NOCOSMOS
            var a = DebugActive;
            DebugActive = true;
            Debug("Calculate FragmentSizes");
            for (int sizeIndex = 0; sizeIndex < 32; sizeIndex++)
            {
                Debug(sizeIndex + "=" + GetFragmentSize(sizeIndex));
//                if (GetFragmentSize(sizeIndex) == 0)
//                    break;
            }
            Debug("MaxFragmentSizeIndex="+MaxFragmentSizeIndex);
            DebugActive = a;
#endif
        }

        #endregion

        #region FragmentCache


        private static UInt32 MaxStackValue { get; set; }
        private static UInt32 CacheStackPointerAddress { get; set; }
        private static UInt32 CacheStackPointerMemorySize { get { return PointerSize * (FragmentHeader.MaxSizeIndex + 1); } }
        private static UInt32 CacheSize { get; set; }
        private static UInt32 CacheSizePerIndex { get { return CacheSize / (FragmentHeader.MaxSizeIndex + 1); } }
        private static UInt32 CacheAddress { get; set; }

        private static unsafe void PushFreeFragmentAddress(int sizeIndex, UInt32 fragmentAddress)
        {
            if (fragmentAddress != 0)
            {
#if NOCOSMOS
                Debug("Push si=" + sizeIndex);
#else
                Debug("Push");
#endif
                Debug(fragmentAddress);
                ++HeapCounter.CachePush;

                #region validation

                //general heap logic problem, if one these exceptions occurs 

                var fragmentPtr = (FragmentHeader*)fragmentAddress;
                if (fragmentPtr->SizeIndex != sizeIndex)
                {
                    CallException("PushFreeFragmentAddress: sizeIndex mismatch", fragmentAddress);
                }
                if (!fragmentPtr->IsEmpty)
                {
                    CallException("PushFreeFragmentAddress: not empty", fragmentAddress);
                }
                if (fragmentPtr->HasParent)
                {
                    var parentPtr = (FragmentHeader*) fragmentPtr->ParentAddress;
                    if (parentPtr->HasData)
                    {
                        CallException("PushFreeFragmentAddress: Parent HasData");
                    }
                    if (fragmentPtr->IsChild2OfParent)
                    {
                        if (parentPtr->HasChild2)
                        {
                            CallException("PushFreeFragmentAddress: ChildBit2 in Parent");
                        }
                    }
                    else
                    {
                        if (parentPtr->HasChild1)
                        {
                            CallException("PushFreeFragmentAddress: ChildBit1 in Parent");
                        }
                    }
                }
                if (sizeIndex > MaxFragmentSizeIndex)
                {
                    CallException("PushFreeFragmentAddress: Worng sizeIndex", fragmentAddress);
                }
                #endregion

                var stackPtr = (UInt32*)(CacheStackPointerAddress + sizeIndex * PointerSize);
                if ((*stackPtr) < MaxStackValue )
                {
                    var cacheValuePtr =
                        (UInt32*)
                        (CacheAddress + CacheSizePerIndex * sizeIndex + (*stackPtr) * PointerSize);

                    if ((UInt32)cacheValuePtr > StartAddress)
                    {
#if NOCOSMOS
                        Debug("cacheValuePtr=" + (UInt32)cacheValuePtr);
                        Debug("StartAddress=" + StartAddress);
#endif
                        CallException("PushFreeFragmentAddress: cacheValuePtr out of range");
                    }
                    if ((UInt32)cacheValuePtr < CacheAddress)
                    {
                        CallException("PushFreeFragmentAddress: cacheValuePtr out of range");
                    }

                    (*cacheValuePtr) = fragmentAddress;
#if NOCOSMOS
                    Debug("Push SizeIndex= " + sizeIndex + " cachePtr=" + (UInt32)cacheValuePtr + " <- " + fragmentAddress);
#endif
                    ++(*stackPtr);
                }
            }
        }

        private unsafe static UInt32 PopFreeFragmentAddress(int sizeIndex)
        {
            UInt32 fragmentAddress=0;

            if (sizeIndex > MaxFragmentSizeIndex)
            {
                CallException("PopFreeFragmentAddress: Worng sizeIndex");
            }
            var stackPtr = (UInt32*)(CacheStackPointerAddress + sizeIndex * PointerSize);
            if ((*stackPtr)>MaxStackValue)
            {
#if NOCOSMOS
                DebugActive = true;
                Debug("(*stackPtr)=" + (*stackPtr));
                Debug("sizeIndex=" + sizeIndex);
#endif
                CallException("PopFreeFragmentAddress: Wrong stackValue detetced");
            }
            while ((*stackPtr) > 0)
            {
                --(*stackPtr);
                var cacheValuePtr = (UInt32*)(CacheAddress + CacheSizePerIndex * sizeIndex + (*stackPtr) * PointerSize);
                #region validation

                //general heap logic problem, if one these exceptions occurs 

                if ((UInt32)cacheValuePtr>StartAddress)
                {
#if NOCOSMOS
                    Debug("cacheValuePtr="+(UInt32)cacheValuePtr);
                    Debug("StartAddress="+StartAddress);
#endif
                    CallException("PopFreeFragmentAddress: cacheValuePtr out of range");
                }
                if ((UInt32)cacheValuePtr < CacheAddress)
                {
                    CallException("PopFreeFragmentAddress: cacheValuePtr out of range");
                }
                #endregion

                fragmentAddress =(*cacheValuePtr);
#if NOCOSMOS
                Debug("Pop SizeIndex=" + sizeIndex + " cachePtr=" + (UInt32)cacheValuePtr + " <- " + fragmentAddress); 
#endif
                if (fragmentAddress != 0)
                {
                    var fragmentPtr = (FragmentHeader*) fragmentAddress;
                    if (fragmentPtr->IsEmpty)
                    {
                        ++HeapCounter.CachePop;
#if NOCOSMOS
                        Debug("Pop si=" + sizeIndex);
#else
                    Debug("Pop");
#endif
                        return fragmentAddress;
                    }
                    fragmentAddress = 0;//Cached fragment is already used by Child or Parent - this can happen
                }                
            }
            return fragmentAddress;
        }


        #endregion

        #region Debug

        public static bool DebugActive { get; set; }
        private static int DebugTab { get; set; }//Hack
#if NOCOSMOS
        internal static StringBuilder sb;
        internal static void Debug(string message)
        {
            if (!DebugActive)
                return;
            Console.WriteLine(message);
            if (sb==null)
                sb = new StringBuilder();
            for (int i = 0; i < DebugTab; i++)
            {
                sb.Append(" ");
            }
            sb.AppendLine(message);                        
        }

#else
        internal static void Debug(string message)//Hack
        {
            if (!DebugActive)
                return;
            Console.WriteLine();
            for (int i = 0; i < DebugTab; i++)
            {
                Console.Write(" ");

            }
            Console.Write(message);
//            int j = 0;
//            for (int i = 0; i < 1000000; i++)
//            {
//                ++j;
//                --j;
//            }
        }
#endif
        internal static void Debug(UInt32 address)
        {
            if (!DebugActive)
                return;
            FragmentHeader.Debug(address);
        }



//                internal static void Debug(string message)//Hack
//        {
//            if (!DebugActive)
//                return;
//            Console.WriteLine();
//            for (int i = 0; i < DebugTab; i++)
//            {
//                Console.Write(" ");
//
//            }
//            Console.Write(message);
//            int j = 0;
//            for (int i = 0; i < 1000000; i++)
//            {
//                ++j;
//                --j;
//            }
//        }
//
//        internal static void Debug(string message, UInt32 number)//Hack
//        {
//            if (!DebugActive)
//                return;
//            Console.WriteLine();
//            for (int i = 0; i < DebugTab; i++)
//            {
//                Console.Write(" ");
//
//            }
//            Console.Write(message);
//            Console.Write(" ");
//            WriteNumber(number);
//            int j = 0;
//            for (int i = 0; i < 1000000; i++)
//            {
//                ++j;
//                --j;
//            }
//        }
//
//        internal static void DebugAppend(string message)//Hack
//        {
//            if (!DebugActive)
//                return;
//            Console.Write(" ");
//            Console.Write(message);
//        }
//
//
//        internal static void DebugAppend(UInt32 number)//Hack
//        {
//            if (!DebugActive)
//                return;
//            Console.Write(" ");
//            WriteNumber(number);
//        }
//
//        internal static void DebugAppendAddress(UInt32 number)//Hack
//        {
//            if (!DebugActive)
//                return;
//            if (number == 0)
//            {
//                Console.Write(" 0");
//            }
//            else
//            {
//                DebugAppend(number - StartAddress);
//            }
//        }
//
        public static void WriteNumber(UInt32 aNumber)//Hack
        {
            byte aBits = 32;
            uint xValue = aNumber;
            byte xCurrentBits = aBits;
            Console.Write("0x");
            while (xCurrentBits >= 4)
            {
                xCurrentBits -= 4;
                byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
                string xDigitString = null;
                switch (xCurrentDigit)
                {
                    case 0:
                        xDigitString = "0";
                        goto default;
                    case 1:
                        xDigitString = "1";
                        goto default;
                    case 2:
                        xDigitString = "2";
                        goto default;
                    case 3:
                        xDigitString = "3";
                        goto default;
                    case 4:
                        xDigitString = "4";
                        goto default;
                    case 5:
                        xDigitString = "5";
                        goto default;
                    case 6:
                        xDigitString = "6";
                        goto default;
                    case 7:
                        xDigitString = "7";
                        goto default;
                    case 8:
                        xDigitString = "8";
                        goto default;
                    case 9:
                        xDigitString = "9";
                        goto default;
                    case 10:
                        xDigitString = "A";
                        goto default;
                    case 11:
                        xDigitString = "B";
                        goto default;
                    case 12:
                        xDigitString = "C";
                        goto default;
                    case 13:
                        xDigitString = "D";
                        goto default;
                    case 14:
                        xDigitString = "E";
                        goto default;
                    case 15:
                        xDigitString = "F";
                        goto default;
                    default:
                        Console.Write(xDigitString);
                        break;
                }
            }
        }
        #endregion

        private static bool InException;

        internal static void CallException(string message)//Hack: Exception not allowed because Heap usages
        {
            if (!InException)
            {
                InException = true;
                //Console.WriteLine(sb.ToString());
                Console.WriteLine();
                Console.WriteLine("Heap.cs: Exception:");
                Console.WriteLine(message);

#if NOCOSMOS
                Debug("Exception:");
                Debug(message);
                Debug("StartAddress");
                Debug(StartAddress);
                if (sb != null)
                {
                    File.WriteAllText("Heap.txt", sb.ToString());
                }
#endif
                while (true)
                {
#if NOCOSMOS                    
#else
                    //CPU.Halt();
#endif
                }
            }
        }

        internal static void CallException(string message, UInt32 address)//Hack: Exception not allowed because Heap usages
        {
            if (!InException)
            {
                InException = true;
                //Console.WriteLine(sb.ToString());
                Console.WriteLine();
                Console.WriteLine("Heap.cs: Exception:");
                Console.WriteLine(message);
                Console.WriteLine("Fragment:");
#if NOCOSMOS
                DebugActive = true;

                Debug("Exception:");
                Debug(message);
                Debug(address);
                Debug("StartAddress");
                Debug(StartAddress);
                if (sb != null)
                {
                    File.WriteAllText("Heap.txt", sb.ToString());
                }
#endif

                while (true)
                {
#if NOCOSMOS
#else
                    //CPU.Halt();
#endif
                }
            }
        }
    }
}
