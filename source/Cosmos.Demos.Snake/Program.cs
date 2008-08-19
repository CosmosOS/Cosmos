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

        /// <summary>
        /// Object to hold player data
        /// </summary>
        private class Player
        {
            public int X,Y,Direction, PlayerNumber;//,PrevX=-1,PrevY=-1;
            public bool Alive;
        }

        static Player snake = null;
        static bool running = true;

        public static void KeyPress(byte aScanCode, bool aReleased)
        {
            if (snake != null && !aReleased)
            {
                if (aScanCode == 75)
                    snake.Direction = 0;
                else if (aScanCode == 72)
                    snake.Direction = 1;
                else if (aScanCode == 77)
                    snake.Direction = 2;
                else if (aScanCode == 80)
                    snake.Direction = 3;
                else if (aScanCode == 1)
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

        enum Blocked
        {
            Empty=0,
            Snake,
            Fruit,
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

            snake = new Player();
            snake.X = S.Columns / 2;
            snake.Y = S.Rows / 2;
            snake.Direction = 0;
            snake.Alive = true;
            int score = 0;

            Blocked[] isBlocked = new Blocked[S.Columns * S.Rows];

            rand = new Random((int)Cosmos.Hardware.Global.TickCount
                + Cosmos.Hardware.RTC.GetSeconds());

            Player fruit = new Player();

            fruit.X = rand.Next(S.Columns);
            fruit.Y = rand.Next(S.Rows);
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

                switch(snake.Direction)
                {
                    case 0://Left
                        snake.X--;
                        break;
                    case 1://Up
                        snake.Y--;
                        break;
                    case 2://Right
                        snake.X++;
                        break;
                    case 3://Down
                        snake.Y++;
                        break;
                }

                if (snake.X > S.Columns || snake.Y > S.Rows || snake.X < 0 || snake.Y < 0 || isBlocked[snake.X + snake.Y * S.Columns] == Blocked.Snake)
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
            Console.WriteLine("Shutting down snake game");
            Cosmos.Sys.Deboot.ShutDown();
        }
    }
}
