using System;
using System.Collections.Generic;
using Cosmos.Compiler.Builder;

using Cosmos.Hardware;
using Cosmos.Kernel;
using Cosmos.Sys;

namespace Cosmos.Demo.VGASnake
{
    class Program
    {
        #region Cosmos Builder logic
        [STAThread]
        static void Main(string[] args)
        {
            //Run Builder
            BuildUI.Run();
        }
        #endregion

        class Point
        {
            public uint X;
            public uint Y;
        }
        class Snake
        {
            public List<Point> Tail = new List<Point>();
            public byte ColorIndex;
            public bool Alive;
            public Direction Dir;
        }
        enum Direction : byte
        {
            Up,
            Down,
            Left,
            Right
        }

        static bool CheckCollision(Point p, Snake[] Snakes)
        {
            if (p.X == 0 || p.X == 319)
            {
                return true;
            }
            if (p.Y == 0 || p.Y == 199)
            {
                return true;
            }

            for (int i = 0;i < Snakes.Length;i++)
            {
                for (int j = 0;j < (Snakes[i].Tail.Count - 1);j++)
                {
                    if (p.X == Snakes[i].Tail[j].X && p.Y == Snakes[i].Tail[j].Y)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool UpdateSnake(Snake snake, Snake[] Snakes)
        {
            int DeltaX = 0;
            int DeltaY = 0;

            switch (snake.Dir)
            {
                case Direction.Down:
                    DeltaY = 1;
                    break;
                case Direction.Left:
                    DeltaX = -1;
                    break;
                case Direction.Right:
                    DeltaX = 1;
                    break;
                case Direction.Up:
                    DeltaY = -1;
                    break;
            }

            int newtail = snake.Tail.Count;
            int oldtail = newtail - 1;

            snake.Tail.Add(new Point() { X = (uint)(snake.Tail[oldtail].X + DeltaX), Y = (uint)(snake.Tail[oldtail].Y + DeltaY) });

            if (CheckCollision(snake.Tail[newtail], Snakes))
            {
                for (int i = 0; i < newtail; i++)
                {
                     VGAScreen.SetPixel320x200x8(snake.Tail[i].X, snake.Tail[i].Y, 2);
                }

                snake.Alive = false;

                return false;
            }
            else
            {
                VGAScreen.SetPixel320x200x8(snake.Tail[newtail].X, snake.Tail[newtail].Y, snake.ColorIndex);

                return true;
            }
        }

        // Kernel Entrypoint
        public static void Init()
        {
            //Boot Kernel
            new Boot().Execute();

            Heap.EnableDebug = false;

            VGAScreen.SetMode320x200x8();

            VGAScreen.SetPaletteEntry(0, 0x00, 0x00, 0x00); //Black  (Background)
            VGAScreen.SetPaletteEntry(1, 0xFF, 0xFF, 0xFF); //White  (Walls)
            VGAScreen.SetPaletteEntry(2, 0xFF, 0xBB, 0xBB); //Peach  (Dead Snake)
            VGAScreen.SetPaletteEntry(3, 0x00, 0xFF, 0x00); //Green  (Player 1's Snake)
            VGAScreen.SetPaletteEntry(4, 0x00, 0x00, 0xFF); //Blue   (Player 2's Snake)
            VGAScreen.SetPaletteEntry(5, 0xFF, 0x00, 0x00); //Red    (Player 3's Snake)
            VGAScreen.SetPaletteEntry(6, 0xFF, 0xFF, 0x00); //Yellow (Player 4's Snake)

            VGAScreen.Clear(0);

            Snake[] Snakes = new Snake[4];
            Snakes[0] = new Snake() { Alive = true, ColorIndex = 3 };
            Snakes[0].Tail.Add(new Point() { X = 160, Y = 100 });

            Snakes[1] = new Snake() { Alive = false };
            Snakes[2] = new Snake() { Alive = false };
            Snakes[3] = new Snake() { Alive = false };

            //Construct Walls Horiz
            for (uint x = 0;x < 320;x++)
            {
                VGAScreen.SetPixel320x200x8(x, 0, 1);
                VGAScreen.SetPixel320x200x8(x, 199, 1);
            }
            //Construct Walls Vert
            for (uint y = 1;y < 199;y++)
            {
                VGAScreen.SetPixel320x200x8(0, y, 1);
                VGAScreen.SetPixel320x200x8(319, y, 1);
            }

            while (UpdateSnake(Snakes[0], Snakes))
            {
                PIT.Wait((uint)50);

                ConsoleKey Key = ConsoleKey.NoName;
                if (Keyboard.GetKey(out Key))
                {
                    switch (Key)
                    {
                        case ConsoleKey.LeftArrow:
                            Snakes[0].Dir = Direction.Left;
                            break;
                        case ConsoleKey.RightArrow:
                            Snakes[0].Dir = Direction.Right;
                            break;
                        case ConsoleKey.UpArrow:
                            Snakes[0].Dir = Direction.Up;
                            break;
                        case ConsoleKey.DownArrow:
                            Snakes[0].Dir = Direction.Down;
                            break;
                        case ConsoleKey.Escape:
                            Deboot.Reboot();
                            break;
                    }
                }
            }

            Deboot.Reboot();
        }
    }
}