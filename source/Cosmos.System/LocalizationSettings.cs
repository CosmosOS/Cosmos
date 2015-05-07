using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System
{
    public static class LocalizationSettings
    {        

        public static void SetKeybordLocalization(Localization.Keybord.IKeybordLocalization loc)
        {
            loc.ResetKeyMap();
            loc.LoadKeyMap();
            HAL.Global.Keyboard.ChangeKeyMap(loc.GetKeyMap());
        }

    }
}
