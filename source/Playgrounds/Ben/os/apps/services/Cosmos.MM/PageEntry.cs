using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel.MM
{
    public struct PageEntry
    {
        public const uint PAGE_ENTRY_SIZE = 8; // bytes
        internal PageType pageType;
        internal PageSharing pageSharing;
        internal uint pid; 
        //int numberOFusers;  //dont need as we dont allow multiple writers

        internal static PageEntry EmptyPage = new PageEntry() ;

        public PageEntry(ulong entry)
        {
            ulong sharing = entry & 0xFF00 ;
            sharing = sharing >> 16;

            pageSharing = (PageSharing) sharing;
            pageType  = (PageType) (entry & 0xFF) ; 

            pid = (uint) (entry >> 32);



        }


        static PageEntry()
        {
            EmptyPage.pageSharing = PageSharing.Private;
            EmptyPage.pageType = PageType.Unknown;
            EmptyPage.pid = 0; 
        }




        public ulong GetPageTableEntry()
        {
            ulong value = (ulong) pid << 32;
            value = value | (ulong) pageSharing << 16;
            value = value | (ulong) pageType; 

            return value; 
        }

    }
}
