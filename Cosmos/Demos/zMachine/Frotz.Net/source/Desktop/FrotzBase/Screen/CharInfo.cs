using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frotz.Screen
{
    public struct CharInfo
    {
        private CharDisplayInfo _displayInfo;
        public CharDisplayInfo DisplayInfo
        {
            get { return _displayInfo; }
        }


        private char _character;
        public char Character
        {
            get { return _character; }
        }

        public CharInfo(char Character, CharDisplayInfo DisplayInfo)
        {
            this._character = Character;
            this._displayInfo = DisplayInfo;
        }
    }
}
