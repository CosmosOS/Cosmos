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
    /// Fascade class to make it easy to use MM , note only thing exposed
    /// 
    /// Note can easily replace entire MM 
    /// 
    /// TODO put in a seperate Assembly
    /// TODO if context swithc cheap run in its own thread
    /// </summary>
    public class MemoryManagerService : Service
    {
        private event EventHandler<EventArgs<MemoryPressure>> memoryPressureChanged;


        //event

        static IMessageEndPoint instance;


        private MemoryManagerService()
        {
            var mm = new MemoryManager();
            mm.PagingManager.MemoryPressureChanged += new EventHandler<EventArgs<MemoryPressure>>(OnMemoryPressureChanged);

            instance = mm;

            messageReceived += new EventHandler(MemoryManagerService_messageReceived);
        }



        void MemoryManagerService_messageReceived(object sender, EventArgs e)
        {
            var message = InputQueue.Dequeue();
            instance.Send(message);

        }


        /// <summary>
        /// Internal so only can be set from Kernel
        /// 
        /// If make public need to add some security
        /// </summary>
        internal static IMessageEndPoint SetCustomMM
        {
            set { MemoryManagerService.instance = value; }
        }


        protected void OnMemoryPressureChanged(object sender, EventArgs<MemoryPressure> args)
        {
            if (memoryPressureChanged != null)
                memoryPressureChanged(sender, args);
        }




        event EventHandler<EventArgs<MemoryPressure>> MemoryPressureChanged
        {
            add
            {
                memoryPressureChanged += value;
            }

            remove
            {
                memoryPressureChanged -= value;
            }
        }




    }



}
