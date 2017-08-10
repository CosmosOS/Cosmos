using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.FontSupport.Old
{
    public class FontCharacterSet
    {
        FontCharacter[] foundChars = new FontCharacter[ushort.MaxValue];
        uint chars = 0;

        public void AddCharacter(int charNumber, Image ch, FontFlag flags)
        {
            if (foundChars[charNumber] == null)
            {
                foundChars[charNumber] = new FontCharacter(ch, flags);
            }
            else
            {
                foundChars[charNumber].AddForm(ch, flags);
            }
            chars++;
        }

        public Image GetCharacter(int charNumber, FontFlag flags)
        {
            if (foundChars[charNumber] != null)
            {
                if (foundChars[charNumber].GetForm(flags) != null)
                {
                    return foundChars[charNumber].GetForm(flags);
                }
                else
                {
                    throw new Exception("Form Not Found!");
                }
            }
            else
            {
                throw new Exception("Character non-existant!");
            }
        }

    }
}
