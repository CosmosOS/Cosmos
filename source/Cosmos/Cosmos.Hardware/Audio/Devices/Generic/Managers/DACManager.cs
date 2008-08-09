using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.Audio.Devices.Generic.Managers
{
    public abstract class DACManager
    {
        protected Cosmos.Hardware.Audio.Devices.Generic.Components.DACEntity dacEntity;
        public DACManager(Cosmos.Hardware.Audio.Devices.Generic.Components.DACEntity dacEntity)
        {
            this.dacEntity = dacEntity;
        }
        protected abstract bool setDacActiveState(bool state);
    }

}
