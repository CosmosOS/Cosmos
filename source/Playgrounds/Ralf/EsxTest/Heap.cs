//#define NOCOSMOS

using System;
using System.IO;
using System.Text;

namespace EsxTest
{
    //RalfHeap

    public struct FragmentHeader
    {
        internal const UInt32 HeaderSize = 4;   //to avoid the usage of sizeof(FragmentHeader)
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
                if (value > MaxSizeIndex)
                {
                    Heap.CallException("FragmentHeader.SizeIndex overflow");
                }
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
                    if (HasData)
                    {
                        Heap.CallException("Set HasChild1 true failed! Data exists");
                    }
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
                    if (HasData)
                    {
                        Heap.CallException("Set HasChild2 true failed! Data exits");
                    }
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
                    if (HasChild)
                    {
                        Heap.CallException("Set HasData true failed! Child exits");
                    }
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
                if (FragmentAddress == Heap.StartAddress)
                {
                    Heap.CallException("No Parent available");
                }
                if (IsChild2OfParent)
                {
                    return FragmentAddress - Heap.GetFragmentSize(SizeIndex) - HeaderSize;
                }
                return FragmentAddress - HeaderSize;
            }
        }

        internal unsafe void AllocateInParent()
        {
            var parentHeader = (FragmentHeader*) ParentAddress;
            if ((*parentHeader).HasData)
            {
                Heap.CallException("AllocateInParent failed: Data already exits", FragmentAddress);
            }
            if (IsChild2OfParent)
            {
                if ((*parentHeader).HasChild2)
                {
                    Heap.CallException("AllocateInParent failed: Child2 already exits", FragmentAddress);
                }
                (*parentHeader).HasChild2 = true;
            }
            else
            {
                if ((*parentHeader).HasChild1)
                {
                    Heap.CallException("AllocateInParent failed: Child1 already exits", FragmentAddress);
                }
                (*parentHeader).HasChild1 = true;
            }
        }

        internal unsafe void FreeInParent()
        {
            var parentHeader = (FragmentHeader*) ParentAddress;
            if ((*parentHeader).HasData)
            {
                Heap.CallException("FreeInParent failed: Parent HasData ", FragmentAddress);
            }
            if (IsChild2OfParent)
            {
                if (!(*parentHeader).HasChild2)
                {
                    Heap.CallException("FreeInParent failed: Child2 doesnt exits", FragmentAddress);
                }
                (*parentHeader).HasChild2 = false;
            }
            else
            {
                if (!(*parentHeader).HasChild1)
                {
                    Heap.CallException("FreeInParent failed: Child1 doesnt exits", FragmentAddress);
                }
                (*parentHeader).HasChild1 = false;
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

            StringBuilder sb = new StringBuilder();
            
            for (int i = 0; i < Heap.GetFragmentDataSize((*header).SizeIndex); i++)
            {
                var b = (byte*) ((*header).DataAddress+i);
                sb.Append(Convert.ToString(*b,16));
                sb.Append(" ");
            }
            Heap.Debug(sb.ToString());
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
        internal static UInt32 StartAddress { get; private set; }
        private static UInt32 EndAddress { get; set; }
        private static UInt32 Size { get; set; }
        public static UInt32 MaxFragmentDataSize { get; private set; }
        public static int MaxFragmentSizeIndex { get; private set; }

        private static bool Initiated { get; set; }

        public static unsafe void Init(UInt32 startAddress,UInt32 size)
        {
            Debug("Begin Init");
            if (Initiated)
                return;
            DebugActive = false;
            StartAddress = startAddress;
            EndAddress = StartAddress + size - (4 - (size % 4)) - 4;
            Size = EndAddress - StartAddress;
            ResetNextFreeFragments();
            MaxFragmentDataSize = Size - FragmentHeader.HeaderSize;
            CalculateFragmentSizes(Size);
            var RootHeader = (FragmentHeader*)StartAddress;
            (*RootHeader).Initialize(MaxFragmentSizeIndex);
            //ClearFragment(StartAddress, GetFragmentDataSize((*RootHeader).SizeIndex)); //this is bad for ESX Server
#if NOCOSMOS
            Debug("");
            Debug(StartAddress);
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
                CallException("MemAlloc-> Call to Heap.Init()"); //Todo: eliminate this behaviour, we need a centralized point for Heap.Init
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
            bool created = false;
            if (fragmentAddress == 0)
            {
                created = true;
                fragmentAddress = CreateFragment(sizeIndex);
            }
            if (fragmentAddress==0)
            {
                CallException("MemAlloc: Out of memory");
            }

            var fragmentHeader = (FragmentHeader*)fragmentAddress;
            (*fragmentHeader).HasData = true;

            if ((*fragmentHeader).SizeIndex != sizeIndex)
            {
                if (created)
                {
                    CallException("MemAlloc´: SizeIndex wrong (created)", fragmentAddress);
                }
                else
                {
                    CallException("MemAlloc: SizeIndex wrong", fragmentAddress);
                }
            }

            if (GetFragmentDataSize((*fragmentHeader).SizeIndex) < size)
            {
                CallException("MemAlloc: HeaderSize mismatch", fragmentAddress);
            }
            ClearFragment(fragmentAddress,size);            
            ++HeapCounter.MemAlloc;
            ++HeapCounter.Count;
            HeapCounter.DataSize += GetFragmentSize((*fragmentHeader).SizeIndex);
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
            var fragmentHeader = (FragmentHeader*)fragmentAddress;
            Debug(fragmentAddress);
            (*fragmentHeader).HasData = false;
            (*fragmentHeader).FreeInParent();
            PushFreeFragmentAddress((*fragmentHeader).SizeIndex, fragmentAddress);
            ++HeapCounter.MemFree;
            --HeapCounter.Count;
            HeapCounter.DataSize -= GetFragmentSize((*fragmentHeader).SizeIndex);
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
            var header = (FragmentHeader*) fragmentAddress;
            if (size>GetFragmentDataSize((*header).SizeIndex))
            {
                CallException("ClearFragment: size mismatch", fragmentAddress);
            }
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
            //ToDo CPU.ZeroFill(address, size);
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
            var address = PopFreeFragmentAddress(sizeIndex);
            if (address != 0)
            {
                if (address != StartAddress)
                {
                    var header = (FragmentHeader*) address;
                    (*header).AllocateInParent();
                }
            }
            else
            {
                //Starting at the root of the heap
                ++HeapCounter.Search;
                address = SearchFreeFragmentAddress(sizeIndex, StartAddress);
                if (address != 0)
                {
                    Debug("Search success");
                    Debug(address);
                }
            }

            Debug(address);
#if NOCOSMOS
            Debug("End GetFreeFragmentAddress si=" + sizeIndex);
#else
            Debug("End GetFreeFragmentAddress");
#endif
            return address;
        }

        private static unsafe UInt32 SearchFreeFragmentAddress(int sizeIndex, UInt32 address)
        {
#if NOCOSMOS
            Debug("Begin SearchFreeFragmentAddress for sizeIndex=" + sizeIndex + "in Fragment:");
#else
            Debug("Begin SearchFreeFragmentAddress");
#endif
            Debug(address);
            ++DebugTab;
            ++HeapCounter.SearchTotal;
            UInt32 freeAddress = 0;            
            var header = (FragmentHeader*)address;
            if (sizeIndex+1 <= (*header).SizeIndex)
            {
                if (sizeIndex+1 == (*header).SizeIndex)
                {
                    if ((*header).IsEmpty)
                    {
                        freeAddress = (*header).Child1Address;
                        var child1Header = (FragmentHeader*) freeAddress;
                        (*child1Header).Initialize(sizeIndex);
                        (*header).HasChild1 = true;
                        var child2Header = (FragmentHeader*)(*header).Child2Address;
                        (*child2Header).Initialize(sizeIndex);
                        (*child2Header).IsChild2OfParent = true;
                        PushFreeFragmentAddress(sizeIndex, (*header).Child2Address);
                        ++HeapCounter.SearchSuccess;
                    }
                }
                if (freeAddress == 0)
                {
                    if ((*header).HasChild1)
                    {
                        freeAddress = SearchFreeFragmentAddress(sizeIndex, (*header).Child1Address);
                    }
                    if (freeAddress == 0)
                    {
                        if ((*header).HasChild2)
                        {
                            freeAddress = SearchFreeFragmentAddress(sizeIndex , (*header).Child2Address);
                        }
                    }
                }
            }
            if (freeAddress != 0)
            {
                var freeHeader = (FragmentHeader*) freeAddress;
                if ((*freeHeader).SizeIndex!=sizeIndex)
                {
                    CallException("SearchFreeFragmentAddress: SizeIndex mismatch", freeAddress);
                }                
            }
            --DebugTab;
            if (freeAddress != 0)
            {
                Debug("Found:");
                Debug(freeAddress);
            }
#if NOCOSMOS
            Debug("End SearchFreeFragmentAddress for sizeIndex=" + sizeIndex);
#else
            Debug("End SearchFreeFragmentAddress");
#endif
            return freeAddress;
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
                var parentHeader = (FragmentHeader*) parentFragmentAddress;

                if ((*parentHeader).HasChild1 && (*parentHeader).HasChild2)
                {
                    //Out of memory
                    return 0;
                }

                (*parentHeader).HasChild1 = true;
                fragmentAddress = (*parentHeader).Child1Address;
                var child2Address = (*parentHeader).Child2Address;
                var child2Header = (FragmentHeader*) child2Address;
                (*child2Header).Initialize(sizeIndex);
                (*child2Header).IsChild2OfParent = true;
                PushFreeFragmentAddress(sizeIndex, child2Address);
                if (fragmentAddress == 0)
                {
                    CallException("CreateFragment failed");
                }
                ++HeapCounter.Create;
                var fragmentHeader = (FragmentHeader*) fragmentAddress;
                (*fragmentHeader).Initialize(sizeIndex);
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
                    childFragmentSize -= (4-(childFragmentSize%4));

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

        //ToDo Implement StackCache to reduce the HeapCounter.SearchTotal

//        private static UInt32 CacheSize { get; set; }
//        private static UInt32 CacheAddress { get; set; }
//        private static UInt32 _CacheLevel;
//        public static UInt32 CacheLevel
//        {
//            get
//            {
//                return _CacheLevel;
//            }
//            private set
//            {
//                if (value != _CacheLevel)
//                {
//                    _CacheLevel = value;
//                    UInt32 size = 0;
//                    var start = (int)_CacheLevel;
//                    for (int i = 0; i < FragmentHeader.MaxSizeIndex + 1; i++)
//                    {
//                        size += (UInt32)(1 << start);
//                        if (start > 0)
//                        {
//                            --start;
//                        }
//                    }
//                    CacheSize = size * 4;
//                }
//            }
//        }



        private static unsafe void PushFreeFragmentAddress(int sizeIndex, UInt32 address)
        {
            if (address != 0)
            {
#if NOCOSMOS
                Debug("Push si=" + sizeIndex);
#else
                Debug("Push");
#endif
                Debug(address);
                ++HeapCounter.CachePush;
                var header = (FragmentHeader*)address;
                if ((*header).SizeIndex != sizeIndex)
                {
                    CallException("PushFreeFragmentAddress: sizeIndex mismatch", address);
                }
                if (!(*header).IsEmpty)
                {
                    CallException("PushFreeFragmentAddress: not empty", address);
                }
                if ((*header).HasParent)
                {
                    var parentHeader = (FragmentHeader*) (*header).ParentAddress;
                    if ((*parentHeader).HasData)
                    {
                        CallException("PushFreeFragmentAddress: Parent HasData");
                    }
                    if ((*header).IsChild2OfParent)
                    {
                        if ((*parentHeader).HasChild2)
                        {
                            CallException("PushFreeFragmentAddress: ChildBit2 in Parent");
                        }
                    }
                    else
                    {
                        if ((*parentHeader).HasChild1)
                        {
                            CallException("PushFreeFragmentAddress: ChildBit1 in Parent");
                        }
                    }
                }
            }
            switch (sizeIndex)
            {
                case 0:
                    Fragment00000001 = address;
                    break;
                case 1:
                    Fragment00000002 = address;
                    break;
                case 2:
                    Fragment00000004 = address;
                    break;
                case 3:
                    Fragment00000008 = address;
                    break;
                case 4:
                    Fragment00000010 = address;
                    break;
                case 5:
                    Fragment00000020 = address;
                    break;
                case 6:
                    Fragment00000040 = address;
                    break;
                case 7:
                    Fragment00000080 = address;
                    break;
                case 8:
                    Fragment00000100 = address;
                    break;
                case 9:
                    Fragment00000200 = address;
                    break;
                case 10:
                    Fragment00000400 = address;
                    break;
                case 11:
                    Fragment00000800 = address;
                    break;
                case 12:
                    Fragment00001000 = address;
                    break;
                case 13:
                    Fragment00002000 = address;
                    break;
                case 14:
                    Fragment00004000 = address;
                    break;
                case 15:
                    Fragment00008000 = address;
                    break;
                case 16:
                    Fragment00010000 = address;
                    break;
                case 17:
                    Fragment00020000 = address;
                    break;
                case 18:
                    Fragment00040000 = address;
                    break;
                case 19:
                    Fragment00080000 = address;
                    break;
                case 20:
                    Fragment00100000 = address;
                    break;
                case 21:
                    Fragment00200000 = address;
                    break;
                case 22:
                    Fragment00400000 = address;
                    break;
                case 23:
                    Fragment00800000 = address;
                    break;
                case 24:
                    Fragment01000000 = address;
                    break;
                case 25:
                    Fragment02000000 = address;
                    break;
                case 26:
                    Fragment04000000 = address;
                    break;
                case 27:
                    Fragment08000000 = address;
                    break;
                case 28:
                    Fragment10000000 = address;
                    break;
                case 29:
                    Fragment20000000 = address;
                    break;
                case 30:
                    Fragment40000000 = address;
                    break;
                case 31:
                    Fragment80000000 = address;
                    break;
                default:
                    CallException("PushFreeFragmentAddress: Worng sizeIndex",address);
                    break;
            }
        }

        private static UInt32 PopFreeFragmentAddress(int sizeIndex)
        {
            UInt32 address;
            switch (sizeIndex)
            {
                case 0:
                    address = Fragment00000001;
                    break;
                case 1:
                    address = Fragment00000002;
                    break;
                case 2:
                    address = Fragment00000004;
                    break;
                case 3:
                    address = Fragment00000008;
                    break;
                case 4:
                    address = Fragment00000010;
                    break;
                case 5:
                    address = Fragment00000020;
                    break;
                case 6:
                    address = Fragment00000040;
                    break;
                case 7:
                    address = Fragment00000080;
                    break;
                case 8:
                    address = Fragment00000100;
                    break;
                case 9:
                    address = Fragment00000200;
                    break;
                case 10:
                    address = Fragment00000400;
                    break;
                case 11:
                    address = Fragment00000800;
                    break;
                case 12:
                    address = Fragment00001000;
                    break;
                case 13:
                    address = Fragment00002000;
                    break;
                case 14:
                    address = Fragment00004000;
                    break;
                case 15:
                    address = Fragment00008000;
                    break;
                case 16:
                    address = Fragment00010000;
                    break;
                case 17:
                    address = Fragment00020000;
                    break;
                case 18:
                    address = Fragment00040000;
                    break;
                case 19:
                    address = Fragment00080000;
                    break;
                case 20:
                    address = Fragment00100000;
                    break;
                case 21:
                    address = Fragment00200000;
                    break;
                case 22:
                    address = Fragment00400000;
                    break;
                case 23:
                    address = Fragment00800000;
                    break;
                case 24:
                    address = Fragment01000000;
                    break;
                case 25:
                    address = Fragment02000000;
                    break;
                case 26:
                    address = Fragment04000000;
                    break;
                case 27:
                    address = Fragment08000000;
                    break;
                case 28:
                    address = Fragment10000000;
                    break;
                case 29:
                    address = Fragment20000000;
                    break;
                case 30:
                    address = Fragment40000000;
                    break;
                case 31:
                    address = Fragment80000000;
                    break;
                default:
                    CallException("PopFreeFragmentAddress: Wrong sizeIndex");
                    return 0;
            }
            ResetNextFreeFragment(sizeIndex);
            //Debug("Pop si=", (UInt32)sizeIndex);
            if (address != 0)
            {
#if NOCOSMOS
                Debug("Pop si=" + sizeIndex);
#else
                Debug("Pop");
#endif
                Debug(address);
                unsafe
                {
                    ++HeapCounter.CachePop;
                    var header = (FragmentHeader*) address;
                    if ((*header).SizeIndex != sizeIndex)
                    {
                        CallException("PopFreeFragmentAddress: SizeIndex mismatch", address);
                    }
                }
            }
            return address;
        }

        private static void ResetNextFreeFragments()
        {
            for (int sizeIndex = 0; sizeIndex < 32; sizeIndex++)
            {
                ResetNextFreeFragment(sizeIndex);
            }
        }

        private static void ResetNextFreeFragment(int sizeIndex)
        {
            PushFreeFragmentAddress(sizeIndex, 0);
        }

        private static UInt32 Fragment00000001 { get; set; }
        private static UInt32 Fragment00000002 { get; set; }
        private static UInt32 Fragment00000004 { get; set; }
        private static UInt32 Fragment00000008 { get; set; }
        private static UInt32 Fragment00000010 { get; set; }
        private static UInt32 Fragment00000020 { get; set; }
        private static UInt32 Fragment00000040 { get; set; }
        private static UInt32 Fragment00000080 { get; set; }
        private static UInt32 Fragment00000100 { get; set; }
        private static UInt32 Fragment00000200 { get; set; }
        private static UInt32 Fragment00000400 { get; set; }
        private static UInt32 Fragment00000800 { get; set; }
        private static UInt32 Fragment00001000 { get; set; }
        private static UInt32 Fragment00002000 { get; set; }
        private static UInt32 Fragment00004000 { get; set; }
        private static UInt32 Fragment00008000 { get; set; }

        private static UInt32 Fragment00010000 { get; set; }
        private static UInt32 Fragment00020000 { get; set; }
        private static UInt32 Fragment00040000 { get; set; }
        private static UInt32 Fragment00080000 { get; set; }
        private static UInt32 Fragment00100000 { get; set; }
        private static UInt32 Fragment00200000 { get; set; }
        private static UInt32 Fragment00400000 { get; set; }
        private static UInt32 Fragment00800000 { get; set; }
        private static UInt32 Fragment01000000 { get; set; }
        private static UInt32 Fragment02000000 { get; set; }
        private static UInt32 Fragment04000000 { get; set; }
        private static UInt32 Fragment08000000 { get; set; }
        private static UInt32 Fragment10000000 { get; set; }
        private static UInt32 Fragment20000000 { get; set; }
        private static UInt32 Fragment40000000 { get; set; }
        private static UInt32 Fragment80000000 { get; set; }

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
