using ConsoleDraw.Windows.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDraw.Inputs.Base
{
    public abstract class AInput
    {
        abstract public void Draw();

        public abstract void Select();
        public abstract void Unselect();

        public abstract void AddLetter(Char letter);
        public abstract void BackSpace();
        public abstract void CursorMoveLeft();
        public abstract void CursorMoveUp();
        public abstract void CursorMoveDown();
        public abstract void CursorMoveRight();
        public abstract void CursorToStart();
        public abstract void CursorToEnd();
        public abstract void Enter();
        public abstract void Tab();

        public int Xpostion;
        public int Ypostion;
        public int Width;
        public int Height;

        public bool Selectable { get; set; }

        public String ID;
        public Window ParentWindow;
    }
}
