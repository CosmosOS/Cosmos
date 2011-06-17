using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.FontSupport
{
    public class FontCharacterSet
    {
        List<FontCharacter> foundChars = new List<FontCharacter>();

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
