using System;
using Cosmos.Build.Windows;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Hardware;
using System.Diagnostics;
using S = Cosmos.Hardware.TextScreen;

namespace Cosmos.Demos.Snake
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            BuildUI.Run();
        }

        enum Direction
        {
            Left=0,
            Up,
            Right,
            Down
        }

        enum Key
        {
            Escape = 1,
            Up = 72,
            Left = 75,
            Right = 77,
            Down = 80
        }

        enum Blocked
        {
            Empty = 0,
            Snake,
            Fruit,
        }

        /// <summary>
        /// Object to hold player data
        /// </summary>
        private class Player
        {
            public int X,Y;
            public Direction direction;
            public bool Alive;
        }

        static Player snake = null;
        static bool running = true;

        public static void KeyPress(byte aScanCode, bool aReleased)
        {
            if (snake != null && !aReleased)
            {
                if (aScanCode == (byte)Key.Left)
                    snake.direction = Direction.Left;
                else if (aScanCode == (byte)Key.Up)
                    snake.direction = Direction.Up;
                else if (aScanCode == (byte)Key.Right)
                    snake.direction = Direction.Right;
                else if (aScanCode == (byte)Key.Down)
                    snake.direction = Direction.Down;
                else if (aScanCode == (byte)Key.Escape)
                    running = false;
            }
        }

        private class Random
        {
            private int a = 214013;
            private int x = 0x72535;
            private int c = 2531011;

            public Random(int seed)
            {
                x = seed;
            }
            public int Next(int p)
            {
                x = (a * x + c);
                return x % p;
            }
        }

        static Random rand = null;

        public static void Init()
        {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();

            S.ReallyClearScreen();
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            S.SetColors(ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            Console.Write("                                                                                ");
            /*
            Console.Write("                                                                                ");
            Console.Write("                  .d8888b.                    888                               ");
            Console.Write("                 d88P  Y88b                   888                               ");
            Console.Write("                 Y88b.                        888                               ");
            Console.Write("                  .Y888b.   88888b.   8888b.  888  888  .d88b.                  ");
            Console.Write("                     .Y88b. 888 .88b     .88b 888 .88P d8P  Y8b                 ");
            Console.Write("                       /888 888  888 .d888888 888888K  88888888                 ");
            Console.Write("                 Y88b  d88P 888  888 888  888 888 /88b Y8b.                     ");
            Console.Write("                  .Y8888P.  888  888 .Y888888 888  888  .Y8888                  ");*/
            Console.Write("                                                                                ");
            Console.Write("                   _____                   _                                    ");
            Console.Write("                  / ____|                 | |                                   ");
            Console.Write("                 | (___    _ __     __ _  | | __   ___                          ");
            Console.Write("                  \\___ \\  | '_ \\   / _` | | |/ /  / _ \\                         ");
            Console.Write("                  ____) | | | | | | (_| | |   <  |  __/                         ");
            Console.Write("                 |_____/  |_| |_|  \\__,_| |_|\\_\\  \\___|                         ");
            Console.Write("                                                                                ");
            Console.Write("                                                                                ");
            S.SetColors(ConsoleColor.White, ConsoleColor.Black);
            Console.WriteLine("");     
            Console.Write("                 Press enter to begin.                                        ");
            Console.ReadLine();

            Cosmos.Hardware.Keyboard.Initialize(KeyPress);
                
            S.SetColors(ConsoleColor.Black, ConsoleColor.Black);

            S.ReallyClearScreen();

            snake = new Player()
            {
                X = S.Columns / 2,
                Y = S.Rows / 2,
                direction = Direction.Left,
                Alive = true
            };
            int score = 0;

            Blocked[] isBlocked = new Blocked[S.Columns * S.Rows];

            rand = new Random((int)Cosmos.Hardware.Global.TickCount
                + Cosmos.Hardware.RTC.GetSeconds());

            Player fruit = new Player()
            {
                X=rand.Next(S.Columns),
                Y=rand.Next(S.Rows)
            };

            S.SetColors(ConsoleColor.Red, ConsoleColor.Red);
            S.PutChar(fruit.Y, fruit.X, '*');
            isBlocked[fruit.X + fruit.Y * S.Columns] = Blocked.Fruit;

            int pause = 1;

            int[] xpos = new int[100];
            int[] ypos = new int[100];
            int numpos = 0;
            int deleteat = 0;

            while (snake.Alive)
            {
                if (!running)
                    break;

                if (snake.direction == Direction.Up || snake.direction == Direction.Down)
                    Cosmos.Hardware.PIT.Wait(100);//Going up or down is faster then left or right so slow down
                else
                    Cosmos.Hardware.PIT.Wait(50);

                S.SetColors(ConsoleColor.White, ConsoleColor.Black);
                string strscore = score.ToString();
                for (int i = 0; i < strscore.Length; i++)
                {
                    S.PutChar(0, i + 5, strscore[i]);
                }

                S.SetColors(ConsoleColor.Gray, ConsoleColor.Gray);
                S.PutChar(snake.Y, snake.X, 'X');
                xpos[numpos] = snake.X;
                ypos[numpos] = snake.Y;
                numpos++;

                if (pause < 1)
                {
                    if(xpos[deleteat] != -1 && ypos[deleteat] != -1)
                    {
                        S.RemoveChar(ypos[deleteat], xpos[deleteat]);
                        isBlocked[xpos[deleteat] + ypos[deleteat] * S.Columns] = Blocked.Empty;
                        xpos[deleteat] = ypos[deleteat] = -1;
                        deleteat++;
                    }
                }
                else
                    pause--;

                isBlocked[snake.X + snake.Y * S.Columns] = Blocked.Snake;

                switch(snake.direction)
                {
                    case Direction.Left:
                        snake.X--;
                        break;
                    case Direction.Up:
                        snake.Y--;
                        break;
                    case Direction.Right:
                        snake.X++;
                        break;
                    case Direction.Down:
                        snake.Y++;
                        break;
                }

                if (snake.X > S.Columns || snake.Y > S.Rows || snake.X < 0 || snake.Y < 0 
                    || isBlocked[snake.X + snake.Y * S.Columns] == Blocked.Snake)
                {
                    snake.Alive = false;
                }
                else if (isBlocked[snake.X + snake.Y * S.Columns] == Blocked.Fruit)
                {
                    isBlocked[fruit.X + fruit.Y * S.Columns] = Blocked.Empty;
                    fruit.X = rand.Next(S.Columns);
                    fruit.Y = rand.Next(S.Rows);
                    S.SetColors(ConsoleColor.Red, ConsoleColor.Red);
                    S.PutChar(fruit.Y, fruit.X, '*');
                    isBlocked[fruit.X + fruit.Y * S.Columns] = Blocked.Fruit;
                    pause++;
                    score++;
                }
            }
            
            S.ReallyClearScreen();
            S.SetColors(ConsoleColor.White, ConsoleColor.Black);
            //Console.WriteLine("Shutting down snake game");
            //Cosmos.Sys.Deboot.ShutDown();
            Cosmos.Sys.Deboot.Reboot();
        }
    }
}
