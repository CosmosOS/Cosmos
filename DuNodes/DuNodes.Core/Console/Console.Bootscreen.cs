using System;
using DuNodes.HAL;
using DuNodes.HAL.Core;
using DuNodes.System.Core;

namespace DuNodes.System.Console
{
    public static class Bootscreen
    {
       
        public static void Show(string Info)
        {
            int DLength = 61;

            DLength = DLength - Info.Length;
            DLength = DLength/2;
            var exeter = "";
            for (int i = 0; i < DLength; i++)
            {
                exeter = exeter + "-";
               
            }
            Console.WriteLine("|"+exeter+Info+exeter+"|", ConsoleColor.Blue, true);
            }
        }
    }

