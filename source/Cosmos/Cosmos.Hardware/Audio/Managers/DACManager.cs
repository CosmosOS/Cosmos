using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware.Audio.Devices;

namespace Cosmos.Hardware.Audio.Managers
{
    public class DACManager
    {
        private DACEntity dacEntity;
        private long dataSize;
        public DACManager(DACEntity dacEntity)
        {
            this.dacEntity = dacEntity;
            this.dataSize = 0;
        }
        protected void prepareDACStreamPlayBack(PCMStream pcmStream, int rate)
        {

        }
    }
}
