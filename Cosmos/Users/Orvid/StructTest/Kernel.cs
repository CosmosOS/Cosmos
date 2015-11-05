using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace CrashTest
{
    public class scrn
    {
        public void DrwCrcle(Vec2 v, int i, uint c)
        {
            Sys.Global.Console.WriteLine("value of v: " + v.X + ", " + v.Y);
        }
    }

    public class Kernel : Sys.Kernel
    {
        public static scrn s = new scrn();

        public Kernel()
        {
            base.ClearScreen = false;
        }

        protected override void BeforeRun()
        {
            s.DrwCrcle(new Vec2(150, 100), 20, 30);
        }

        protected override void Run()
        {
        }
    }

    #region Vec2
    public struct Vec2
    {
        public int X;
        public int Y;
        public Vec2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
    #endregion
}
