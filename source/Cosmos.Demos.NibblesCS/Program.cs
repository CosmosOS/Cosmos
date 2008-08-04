using System;
using Cosmos.Build.Windows;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Hardware;
using System.Diagnostics;
using S = Cosmos.Hardware.TextScreen;

namespace SteveKernel
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            BuildUI.Run();
        }
        #endregion

        /// <summary>
        /// Object to hold player data
        /// </summary>
        private class Player
        {
            public int X, Y, Direction, PlayerNumber;
            public bool Alive;
        }


        // Main entry point of the kernel
        public static void Init()
        {
            Cosmos.Sys.Boot.Default();

            S.ReallyClearScreen();
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            S.SetColors(ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            Console.Write("                                                                                ");
            Console.Write("                                                                                ");
            Console.Write("                 @       @     @       @       @                                ");
            Console.Write("                 @@      @     @       @       @                                ");
            Console.Write("                 @ @     @  @  @       @       @                                ");
            Console.Write("                 @  @    @     @       @       @                                ");
            Console.Write("                 @   @   @  @  @@@@@   @@@@@   @   @@@@    @@@@                 ");
            Console.Write("                 @    @  @  @  @    @  @    @  @  @    @  @                     ");
            Console.Write("                 @     @ @  @  @    @  @    @  @  @@@@@    @@@@                 ");
            Console.Write("                 @      @@  @  @    @  @    @  @  @            @                ");
            Console.Write("                 @       @  @  @@@@@   @@@@@   @   @@@@    @@@@                 ");
            Console.Write("                                                                                ");
            Console.Write("                                                                                ");
            
            S.SetColors(ConsoleColor.White, ConsoleColor.Black);
            Console.WriteLine("");            
            Console.WriteLine("                       Coded for COSMOS by Stephen Remde");

            

            Wait(5000);

            Random myRandom = new Random(
                (int)Cosmos.Hardware.Global.TickCount 
                + Cosmos.Hardware.RTC.GetSeconds());

            int playerCount = 15;
                        
            while (true)
            {
                S.SetColors(ConsoleColor.Black, ConsoleColor.Black);

                S.ReallyClearScreen();

                Player[] myPlayers = new Player[playerCount];

                bool[] isBlocked = new bool[S.Columns * S.Rows];

                for (int i = 0; i < playerCount; i++)
                {
                    myPlayers[i] = new Player()
                    {
                        X = myRandom.Next(S.Columns),
                        Y = myRandom.Next(S.Rows),
                        Direction = myRandom.Next(4),
                        PlayerNumber = i,
                        Alive = true
                    };
                }
                
                bool aPlayerIsAlive = true;

                while (aPlayerIsAlive)
                {
                    Wait(25);

                    aPlayerIsAlive = false;

                    foreach (Player p in myPlayers)
                    {
                        if (p.Alive)
                        {                            
                            S.SetColors((ConsoleColor)(p.PlayerNumber + 1), (ConsoleColor)(p.PlayerNumber + 1));
                
                            S.PutChar(p.Y, p.X, 'X');
                            
                            isBlocked[p.X + p.Y * S.Columns] = true;

                            p.Alive = false;

                            for (int newRotation = 0; newRotation < 4; newRotation++)
                            {
                                int newX = p.X;
                                int newY = p.Y;

                                switch ((p.Direction + newRotation) % 4)
                                {
                                    case 0:
                                        newX--;
                                        break;
                                    case 1:
                                        newY--;
                                        break;
                                    case 2:
                                        newX++;
                                        break;
                                    case 3:
                                        newY++;
                                        break;
                                }

                                if (newX >= 0 && newX < S.Columns && newY >= 0 && newY < S.Rows && !isBlocked[newX + newY * S.Columns])
                                {
                                    p.Alive = true;
                                    p.X = newX;
                                    p.Y = newY;
                                    p.Direction = (p.Direction + newRotation) % 4;
                                    aPlayerIsAlive = true;

                                    break;
                                }
                            }                             
                        }                                  
                    }                 
                }
                
                Wait(2000);
            }             
        }

        private static void Wait(uint milliseconds)
        {
            Cosmos.Hardware.PIT.Wait(milliseconds);
        }
    }
}