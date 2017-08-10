using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Documents;

namespace WPFMachine.Screen
{
    internal class ZBlankContainer : InlineUIContainer
    {
        internal ZBlankContainer(int Width)
        {
            this.Width = Width;
            var c = new System.Windows.Controls.Canvas();
            c.Width = Width;

            this.Child = c;
        }

        internal int Width { get; private set; }
    }
}
