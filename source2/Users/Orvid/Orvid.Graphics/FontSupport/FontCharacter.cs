using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.FontSupport
{
    public class FontCharacter
    {
        Image[] forms;

        public FontCharacter(Image character, FontFlag flags)
        {
            Int32 i = (Int32)flags;
            forms = new Image[i];
            forms[i] = character;
        }

        public void AddForm(Image character, FontFlag flags)
        {
            Int32 i = (Int32)flags;
            if (i > forms.Length)
            {
                Image[] forms2 = new Image[i];
                Array.Copy(forms, forms2, forms.Length);
                forms = forms2;
            }
            forms[i] = character;
        }

        public Image GetForm(FontFlag flags)
        {
            if ((Int32)flags < forms.Length)
            {
                return (forms[((Int32)flags)]);
            }
            else
            {
                return null;
            }
        }
    }
}
