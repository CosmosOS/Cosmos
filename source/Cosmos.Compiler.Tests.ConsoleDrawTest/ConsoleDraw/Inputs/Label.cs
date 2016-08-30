using ConsoleDraw.Inputs.Base;
using ConsoleDraw.Windows.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDraw.Inputs
{
    public class Label : Input
    {
        private String Text = "";
        private ConsoleColor TextColour = ConsoleColor.Black;
        public ConsoleColor BackgroundColour = ConsoleColor.Gray;

        public Label(String text, int x, int y, String iD, Window parentWindow) : base(x, y, 1, text.Count(), parentWindow, iD)
        {
            Text = text;
            BackgroundColour = parentWindow.BackgroundColour;
            Selectable = false;
        }

        public override void Draw()
        {
            WindowManager.WirteText(Text, Xpostion, Ypostion, TextColour, BackgroundColour);
        }

        public void SetText(String text)
        {
            Text = text;
            Width = text.Count();
            Draw();
        }
       
    }
}
