using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel.API;
using Cosmos.Kernel.Dispatch;

namespace Cosmos.Kernel.MM
{
    /// <summary>
    /// Memory manager 
    /// 
    /// Bold attempt at a C# simple but fast MM
    /// If not back to the old c type.
    /// 
    /// note allocating memroy has nothing to do with paging and its best to abstract the paging a bit.
    /// Some memory like Kernel space ,shared IO memory cant be paged
    /// paging the code to disk for the Disk IO , MM etc is really bad news..
    /// 
    /// Be warned with hw paging and swapping its a major undertaking that will break many times.
    /// 
    /// </summary>
    internal class MemoryManager : IMessageEndPoint
    {

        private IPagingManager m_pagedMemory;


        private IPagingManager m_nonpageMemory;  //kernel memory , IO memory 


        //TODO no constructor
        internal MemoryManager()
        {
            Init();
        }

        //process messgaes

        internal void Init()
        {

            uint pagesize = 1 << 20;
            Init(pagesize);
        }



        internal void Init(uint pageSize)
        {

            var totalMem = CPU.AmountOfMemoryInMB;


            m_nonpageMemory = new SimplePageManager(pageSize, UIntPtr.Zero, CPU.EndOfKernel);

            ulong amountPagedMemory = (ulong)CPU.AmountOfMemoryInMB * (1 << 20) - ((ulong)CPU.EndOfKernel + 1024);
            m_pagedMemory = new SoftwarePageManager(pageSize, new UIntPtr(CPU.EndOfKernel + 1024), amountPagedMemory);

            AllocateNonPagedMemory();

            //m_nonpageMemory.MemoryPressureChanged += new EventHandler<EventArgs<MemoryPressure>>(m_nonpageMemory_MemoryPressureChanged);
            //m_pagedMemory.MemoryPressureChanged += new EventHandler<EventArgs<MemoryPressure>>(m_pagedMemory_MemoryPressureChanged);

        }



        // TODO get more accurate info eg from APIC , SMAPINfo?
        /// <summary>
        /// Note by marking the memory we have extra memory here which can be used ..for DMA buffers etc ..
        /// memory at the bottom can be valueable as some devices require it.
        /// 

        /// </summary>
        private void AllocateNonPagedMemory()
        {
            //reserve first K 
            m_nonpageMemory.RequestPages(new MemoryAllocationRequest() { Address = UIntPtr.Zero, NumBytes = 1024, PageType = PageType.Unknown, Sharing = PageSharing.Private });

            //TODO from base.
            m_nonpageMemory.RequestPages(new MemoryAllocationRequest() { Address = new UIntPtr(1024), NumBytes = CPU.EndOfKernel - 1024, PageType = PageType.Kernel, Sharing = PageSharing.Private });

            //VGA  0A000h and 0AFFFFh 
            m_nonpageMemory.RequestPages(new MemoryAllocationRequest() { Address = new UIntPtr(0xC000), NumBytes = 65536, PageType = PageType.IO, Sharing = PageSharing.Private });

            //BIOS 0F000h to 0FFFFh
            m_nonpageMemory.RequestPages(new MemoryAllocationRequest() { Address = new UIntPtr(0xF000), NumBytes = 65536, PageType = PageType.IO, Sharing = PageSharing.Private });
        }



        //TODO move into dispatcher , dispatcher can be smart like deallocate bigest block first or high priority allocates
        /// <summary>
        /// Should this be a seperate dispatcher class eg dispathher ( m_pagedMemory) 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>


        public void Send(KernelMessage message)
        {
            if (message is MMMessage == false)
                throw new InvalidOperationException("Invalid message");


            try
            {
                if (message is MemoryAllocationRequest)
                    ProcessAllocation(message as MemoryAllocationRequest);

                if (message is FreeMemoryRequest)
                    ProcessFree(message as FreeMemoryRequest);

                if (message is QueryPageRequest)
                    ProcessQuery(message as QueryPageRequest);

                if (message is MemoryInformationRequest)
                    ProcessMemoryInformation(message as MemoryInformationRequest);

                throw new InvalidOperationException("Message not supported");

            }
            catch (Exception ex)
            {
                var repMessage = new KernelErrorMessage(ex);
                SendReply(message.Sender, repMessage); 
            }

        }




        private void ProcessQuery(QueryPageRequest query)
        {
            var result = m_pagedMemory.QueryPage(query.Address);

            SendReply(query.Sender , new QueryResponse(result));
        }


        private void ProcessAllocation(MemoryAllocationRequest request)
        {
            var result = m_pagedMemory.RequestPages(request);

            SendReply(request.Sender , new AllocationResponse(result));
        }






        private void ProcessFree(FreeMemoryRequest query)
        {
            if (query.Pages > 0)
                m_pagedMemory.FreePages(query.Sender, new PageAllocation() { Address = query.Address, NumPages = query.Pages });
            else
                m_pagedMemory.FreePages(query.Sender);

        }






        private void ProcessMemoryInformation(MemoryInformationRequest query)
        {
            var result = m_pagedMemory.Details;

            var replyMessage = new MemoryInformationResponse(result);
            SendReply(query.Sender, replyMessage); 

        }


        private void SendReply(Process pr, KernelMessage message)
        {
            pr.InputQueue.Enqueue(message); 
        }


        //TODO make private
        internal IPagingManager PagingManager
        {
            get { return m_pagedMemory; }
        }



        //void m_pagedMemory_MemoryPressureChanged(object sender, EventArgs<MemoryPressure> e)
        //{
        //    ; //ignore
        //}

        //void m_nonpageMemory_MemoryPressureChanged(object sender, EventArgs<MemoryPressure> e)
        //{
        //    ; // ignore
        //}
    }



}
