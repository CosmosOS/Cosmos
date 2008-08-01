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
        private float rate;
        public DACManager(DACEntity dacEntity)
        {
            this.dacEntity = dacEntity;
            this.rate = 0;
        }
        protected void prepareDACStreamPlayBack(PCMStream pcmStream, int rate)
        {

        }
    }
}
