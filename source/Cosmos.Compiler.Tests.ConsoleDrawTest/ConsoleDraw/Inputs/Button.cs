using ConsoleDraw.Inputs.Base;
using ConsoleDraw.Windows.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDraw.Inputs
{
    public class Button : Input
    {
        private String Text;
        public ConsoleColor BackgroundColour = ConsoleColor.Gray;
        private ConsoleColor TextColour = ConsoleColor.Black;

        private ConsoleColor SelectedBackgroundColour = ConsoleColor.DarkGray;
        private ConsoleColor SelectedTextColour = ConsoleColor.White;

        private bool Selected = false;

        public Action Action;

        public Button(int x, int y, String text, String iD, Window parentWindow) : base(x, y, 1, text.Count() + 2, parentWindow, iD)
        {
            Text = text;
            BackgroundColour = parentWindow.BackgroundColour;
            Selectable = true;
        }

        public override void Select()
        {
            if (!Selected)
            {
                Selected = true;
                Draw();
            }
        }

        public override void Unselect()
        {
            if (Selected)
            {
                Selected = false;
                Draw();
            }
        }

        public override void Enter()
        {
            if (Action != null) //If an action has been set
                Action();
        }

        public override void Draw()
        {
            if(Selected)
                WindowManager.WirteText('['+Text+']', Xpostion, Ypostion, SelectedTextColour, SelectedBackgroundColour);
            else
                WindowManager.WirteText('[' + Text + ']', Xpostion, Ypostion, TextColour, BackgroundColour);  
        }
        
        public override void CursorMoveDown()
        {
            ParentWindow.MovetoNextItemDown(Xpostion, Ypostion , Width);
        }

        public override void CursorMoveRight()
        {
            ParentWindow.MovetoNextItemRight(Xpostion - 1, Ypostion + Width, 3);

        }

        public override void CursorMoveLeft()
        {
            ParentWindow.MovetoNextItemLeft(Xpostion - 1, Ypostion, 3);
        }

        public override void CursorMoveUp()
        {
            ParentWindow.MovetoNextItemUp(Xpostion, Ypostion, Width);
        }
    }
}
