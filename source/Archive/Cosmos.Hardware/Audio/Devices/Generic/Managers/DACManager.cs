using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware2.Audio.Devices.Generic.Managers
{
    public abstract class DACManager
    {
        protected Cosmos.Hardware2.Audio.Devices.Generic.Components.DACEntity dacEntity;
        public DACManager(Cosmos.Hardware2.Audio.Devices.Generic.Components.DACEntity dacEntity)
        {
            this.dacEntity = dacEntity;
        }
        protected abstract bool setDacActiveState(bool state);
    }

}
