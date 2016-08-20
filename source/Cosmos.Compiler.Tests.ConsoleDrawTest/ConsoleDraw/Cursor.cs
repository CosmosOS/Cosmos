using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ConsoleDraw
{
    public class Cursor
    {
        public bool _cursorShow;
        public int _x;
        public int _y;
        //public Timer blink;
        public Char blinkLetter;
        public ConsoleColor _background;
        private bool visible;

        public void PlaceCursor(int x, int y, char letter, ConsoleColor background = ConsoleColor.Blue)
        {
            Console.CursorVisible = true;
            visible = true;
            _x = x;
            _y = y;
            Console.CursorLeft = x;
            Console.CursorTop = y;

        }

        public void RemoveCursor()
        {
            if (visible)
            {
                Console.CursorVisible = false;

                visible = false;
            }
            
        }
    }
}
