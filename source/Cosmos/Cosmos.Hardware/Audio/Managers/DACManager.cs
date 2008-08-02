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
        private byte dacAddr;
        private byte dacSizeAddr;
        bool dacEnabled;
        public DACManager(DACEntity dacEntity, bool dacEnabled , byte dacAddr, byte dacSizeAddr)
        {
            this.dacEntity = dacEntity;
            this.dacAddr = dacAddr;
            this.dacSizeAddr = dacSizeAddr;
            this.dacEnabled = dacEnabled;
        }

        public bool setDACStateEnabled(bool state)
        {
            dacEnabled = state;
            return dacEnabled;
        }
    }
}
