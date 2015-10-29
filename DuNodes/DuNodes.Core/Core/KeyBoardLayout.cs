using System;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.HAL.ScanMaps;

namespace DuNodes.System.Core
{
    // INFO: We recommend to set the keylayout in the BeforeRun() method to make sure that
    //       the arrow keys does not appear as a pretty fuckedup random unicode char..
    public static class KeyBoardLayout
    {
  
        public enum KeyLayouts : byte { QWERTY, AZERTY };
   
      
        public static void SwitchKeyLayout(KeyLayouts layout)
        {
            switch (layout)
            {
                case KeyLayouts.AZERTY:
                    Global.Keyboard.SetKeyLayout(new FR_Standard());   break;
                case KeyLayouts.QWERTY:
                    Global.Keyboard.SetKeyLayout(new US_Standard()); break;
             
            }
        }
      
    }
}
