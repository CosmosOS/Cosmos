using System;
using Cosmos.Compiler.Builder;
using Cosmos.Demo.Pacman.Elements;
using S = Cosmos.Hardware.TextScreen;

namespace Cosmos.Demo.Pacman
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            BuildUI.Run();
        }
        
       

        public static void Init()
        {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();
            S.ReallyClearScreen();
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            S.SetColors(ConsoleColor.Yellow, ConsoleColor.Blue);
            Console.WriteLine("");
            Console.WriteLine(@"  _____        ___        _____               ");
            Console.WriteLine(@" /  _  \      / _ \      / ____|          O   ");
            Console.WriteLine(@" | |_|  |    / / \ \     ||       ---     |   ");
            Console.WriteLine(@" |  ___/    / /___\ \    ||              /|\  ");
            Console.WriteLine(@" | |       / /_____\ \   ||____   ---   / | \ ");
            Console.WriteLine(@" \_/      /_/       \_\  \_____|          |   ");
            Console.WriteLine(@"                                         / \  ");
            Console.WriteLine(@"                                              ");
            S.SetColors(ConsoleColor.White, ConsoleColor.DarkGreen);
            Console.WriteLine(@"Namco/Midway 1980(R)                          ");
            S.SetColors(ConsoleColor.White, ConsoleColor.Black);
            Console.WriteLine(@"Thanks to DarthDie for the Snake project I've based on");
            float nothing = 0;
            Sounds.Intro.Play(nothing);

            
        }

        static Player currentPlayer;

        public static void KeyPress(byte aScanCode, bool aReleased)
        {
            if (currentPlayer.pacMan != null && !aReleased)
            {
                if (aScanCode == (byte)Key.Left)
                    currentPlayer.pacMan.direction = Direction.Left;
                else if (aScanCode == (byte)Key.Up)
                    currentPlayer.pacMan.direction = Direction.Up;
                else if (aScanCode == (byte)Key.Right)
                    currentPlayer.pacMan.direction = Direction.Right;
                else if (aScanCode == (byte)Key.Down)
                    currentPlayer.pacMan.direction = Direction.Down;
            }
        }

    }
}
