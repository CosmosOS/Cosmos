using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Localization.Keybord
{
    public abstract class IKeybordLocalization
    {
        private List<Cosmos.HAL.Keyboard.KeyMapping> mKeys;

        public abstract void LoadKeyMap();

        public void ResetKeyMap()
        {
            mKeys.Clear();
        }

        public List<Cosmos.HAL.Keyboard.KeyMapping> GetKeyMap()
        {
            return mKeys;
        }

        public IKeybordLocalization()
        {
            mKeys = new List<Cosmos.HAL.Keyboard.KeyMapping>(164);
        }

        public void MapKey(uint p, char p_2, ConsoleKey p_3)
        {
            mKeys.Add(new Cosmos.HAL.Keyboard.KeyMapping(p, p_2, p_3));
           
        }
        public void MapKeyWithShift(uint p, char p_2, ConsoleKey p_3)
        {
            MapKey(p, p_2, p_3);
            MapKey(p << 16, p_2, p_3);
        }
        public void MapKey(uint p, ConsoleKey p_3)
        {
            MapKey(p, '\0', p_3);
        }
        public void MapKeyWithShift(uint p, ConsoleKey p_3)
        {
            MapKeyWithShift(p, '\0', p_3);
        }

    }
}
