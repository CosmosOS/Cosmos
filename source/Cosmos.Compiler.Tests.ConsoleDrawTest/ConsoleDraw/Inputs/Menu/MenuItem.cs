using ConsoleDraw.Inputs.Base;
using ConsoleDraw.Windows.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDraw.Inputs
{
    public class MenuItem : Input
    {
        private String Text = "";
        private ConsoleColor TextColour = ConsoleColor.White;
        private ConsoleColor BackgroudColour = ConsoleColor.DarkGray;
        private ConsoleColor SelectedTextColour = ConsoleColor.Black;
        private ConsoleColor SelectedBackgroundColour = ConsoleColor.Gray;

        private bool Selected = false;
        public Action Action;

        public MenuItem(String text, String iD, Window parentWindow)
            : base(0, 0, 1, 0, parentWindow, iD)
        {
            Text = text; 

            Selectable = true;
        }

        public override void Draw()
        {
            var paddedText = ('[' +Text + ']').PadRight(Width, ' ');

            if (Selected)
                WindowManager.WirteText(paddedText, Xpostion, Ypostion, SelectedTextColour, SelectedBackgroundColour);
            else
                WindowManager.WirteText(paddedText, Xpostion, Ypostion, TextColour, BackgroudColour);
        }

        public override void Select()
        {
            if (!Selected)
            {
                Selected = true;
                Draw();

               // new MenuDropdown(Xpostion + 1, Ypostion, ParentWindow);
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

        public override void BackSpace()
        {
            //ParentWindow.ParentWindow.MoveToNextItem();
            ParentWindow.SelectFirstItem();
            ParentWindow.ExitWindow();
        }

        public override void Enter()
        {
            if (Action != null)
            {
                ParentWindow.SelectFirstItem();
                Action();
            }
        }

        public override void CursorMoveDown()
        {
            ParentWindow.MoveToNextItem();
        }
        public override void CursorMoveUp()
        {
            ParentWindow.MoveToLastItem();
        }

        public override void CursorMoveRight()
        {
            ParentWindow.SelectFirstItem();
            ParentWindow.ExitWindow();
            ParentWindow.ParentWindow.MoveToNextItem();
        }

        public override void CursorMoveLeft()
        {
            ParentWindow.SelectFirstItem();
            ParentWindow.ExitWindow();
            ParentWindow.ParentWindow.MoveToLastItem();
        }
    }
}
