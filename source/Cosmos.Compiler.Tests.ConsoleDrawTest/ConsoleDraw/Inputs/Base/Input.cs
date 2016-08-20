using ConsoleDraw.Windows.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDraw.Inputs.Base
{
    public class Input : AInput
    {
        public Input(int xPostion, int yPostion, int height, int width, Window parentWindow, String iD)
        {
            ParentWindow = parentWindow;
            ID = iD;

            Xpostion = xPostion;
            Ypostion = yPostion;

            Height = height;
            Width = width;
        }

        public override void AddLetter(Char letter) { }
        public override void BackSpace() { }
        public override void CursorMoveLeft() { }
        public override void CursorMoveRight() { }
        public override void CursorMoveUp() { }
        public override void CursorMoveDown() { }
        public override void CursorToStart() { }
        public override void CursorToEnd() { }
        public override void Enter() { }
        public override void Tab() {
            ParentWindow.MoveToNextItem();
        }
        
        public override void Unselect() { }
        public override void Select() { }
        public override void Draw() { }
    }
}
