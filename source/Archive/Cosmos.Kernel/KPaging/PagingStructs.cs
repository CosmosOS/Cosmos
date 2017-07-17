using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PageDirectoryEntry
    {
        [FieldOffset(0)]
        public uint PDEValue;

        public uint PageTableAddress
        {
            get
            {
                return (PDEValue & ((uint)0xFFFFF000));
            }
            set
            {
                PDEValue = ((value & ((uint)0xFFFFF000)) | (PDEValue & ((uint)0x00000FFF)));
            }
        }
        public bool Available1
        {
            get
            {
                return ((PDEValue & ((uint)0x400)) != ((uint)0));
            }
            set
            {
                PDEValue = (value ? (PDEValue | ((uint)0x400)) : (PDEValue & ((uint)0xFFFFFBFF)));
            }
        }
        public bool Available0
        {
            get
            {
                return ((PDEValue & ((uint)0x200)) != ((uint)0));
            }
            set
            {
                PDEValue = (value ? (PDEValue | ((uint)0x200)) : (PDEValue & ((uint)0xFFFFFDFF)));
            }
        }
        public bool LargePages
        {
            get
            {
                return ((PDEValue & ((uint)0x100)) != ((uint)0));
            }
            set
            {
                PDEValue = (value ? (PDEValue | ((uint)0x100)) : (PDEValue & ((uint)0xFFFFFEFF)));
            }
        }
        public bool Accessed
        {
            get
            {
                return ((PDEValue & ((uint)0x20)) != ((uint)0));
            }
            set
            {
                PDEValue = (value ? (PDEValue | ((uint)0x20)) : (PDEValue & ((uint)0xFFFFFFDF)));
            }
        }
        public bool CacheDisabled
        {
            get
            {
                return ((PDEValue & ((uint)0x10)) != ((uint)0));
            }
            set
            {
                PDEValue = (value ? (PDEValue | ((uint)0x10)) : (PDEValue & ((uint)0xFFFFFFEF)));
            }
        }
        public bool WriteThrough
        {
            get
            {
                return ((PDEValue & ((uint)0x8)) != ((uint)0));
            }
            set
            {
                PDEValue = (value ? (PDEValue | ((uint)0x8)) : (PDEValue & ((uint)0xFFFFFFF8)));
            }
        }
        public bool UserAccess
        {
            get
            {
                return ((PDEValue & ((uint)0x4)) != ((uint)0));
            }
            set
            {
                PDEValue = (value ? (PDEValue | ((uint)0x4)) : (PDEValue & ((uint)0xFFFFFFFB)));
            }
        }
        public bool ReadOnly
        {
            get
            {
                return ((PDEValue & ((uint)0x2)) != ((uint)0));
            }
            set
            {
                PDEValue = (value ? (PDEValue | ((uint)0x2)) : (PDEValue & ((uint)0xFFFFFFFD)));
            }
        }
        public bool Present
        {
            get
            {
                return ((PDEValue & ((uint)0x1)) != ((uint)0));
            }
            set
            {
                PDEValue = (value ? (PDEValue | ((uint)0x1)) : (PDEValue & ((uint)0xFFFFFFFE)));
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PageTableEntry
    {
        [FieldOffset(0)]
        public uint PTEValue;

        public uint PhysicalAddress
        {
            get
            {
                return (PTEValue & ((uint)0xFFFFF000));
            }
            set
            {
                PTEValue = ((value & ((uint)0xFFFFF000)) | (PTEValue & ((uint)0x00000FFF)));
            }
        }
        public bool Available1
        {
            get
            {
                return ((PTEValue & ((uint)0x400)) != ((uint)0));
            }
            set
            {
                PTEValue = (value ? (PTEValue | ((uint)0x400)) : (PTEValue & ((uint)0xFFFFFBFF)));
            }
        }
        public bool Available0
        {
            get
            {
                return ((PTEValue & ((uint)0x200)) != ((uint)0));
            }
            set
            {
                PTEValue = (value ? (PTEValue | ((uint)0x200)) : (PTEValue & ((uint)0xFFFFFDFF)));
            }
        }
        public bool Global
        {
            get
            {
                return ((PTEValue & ((uint)0x100)) != ((uint)0));
            }
            set
            {
                PTEValue = (value ? (PTEValue | ((uint)0x100)) : (PTEValue & ((uint)0xFFFFFEFF)));
            }
        }
        public bool Dirty
        {
            get
            {
                return ((PTEValue & ((uint)0x40)) != ((uint)0));
            }
            set
            {
                PTEValue = (value ? (PTEValue | ((uint)0x40)) : (PTEValue & ((uint)0xFFFFFFBF)));
            }
        }
        public bool Accessed
        {
            get
            {
                return ((PTEValue & ((uint)0x20)) != ((uint)0));
            }
            set
            {
                PTEValue = (value ? (PTEValue | ((uint)0x20)) : (PTEValue & ((uint)0xFFFFFFDF)));
            }
        }
        public bool CacheDisabled
        {
            get
            {
                return ((PTEValue & ((uint)0x10)) != ((uint)0));
            }
            set
            {
                PTEValue = (value ? (PTEValue | ((uint)0x10)) : (PTEValue & ((uint)0xFFFFFFEF)));
            }
        }
        public bool WriteThrough
        {
            get
            {
                return ((PTEValue & ((uint)0x8)) != ((uint)0));
            }
            set
            {
                PTEValue = (value ? (PTEValue | ((uint)0x8)) : (PTEValue & ((uint)0xFFFFFFF8)));
            }
        }
        public bool UserAccess
        {
            get
            {
                return ((PTEValue & ((uint)0x4)) != ((uint)0));
            }
            set
            {
                PTEValue = (value ? (PTEValue | ((uint)0x4)) : (PTEValue & ((uint)0xFFFFFFFB)));
            }
        }
        public bool ReadOnly
        {
            get
            {
                return ((PTEValue & ((uint)0x2)) != ((uint)0));
            }
            set
            {
                PTEValue = (value ? (PTEValue | ((uint)0x2)) : (PTEValue & ((uint)0xFFFFFFFD)));
            }
        }
        public bool Present
        {
            get
            {
                return ((PTEValue & ((uint)0x1)) != ((uint)0));
            }
            set
            {
                PTEValue = (value ? (PTEValue | ((uint)0x1)) : (PTEValue & ((uint)0xFFFFFFFE)));
            }
        }
    }
}