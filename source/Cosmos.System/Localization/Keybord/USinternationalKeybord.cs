using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Localization.Keybord
{
    public class USinternationalKeybord : IKeybordLocalization
    {
        public override void LoadKeyMap()
        {

            #region Letters
            MapKey(0x10, 'q', ConsoleKey.Q);
            MapKey(0x100000, 'Q', ConsoleKey.Q);
            MapKey(0x11, 'w', ConsoleKey.W);
            MapKey(0x110000, 'W', ConsoleKey.W);
            MapKey(0x12, 'e', ConsoleKey.E);
            MapKey(0x120000, 'E', ConsoleKey.E);
            MapKey(0x13, 'r', ConsoleKey.R);
            MapKey(0x130000, 'R', ConsoleKey.R);
            MapKey(0x14, 't', ConsoleKey.T);
            MapKey(0x140000, 'T', ConsoleKey.T);
            MapKey(0x15, 'y', ConsoleKey.Y);
            MapKey(0x150000, 'Y', ConsoleKey.Y);
            MapKey(0x16, 'u', ConsoleKey.U);
            MapKey(0x160000, 'U', ConsoleKey.U);
            MapKey(0x17, 'i', ConsoleKey.I);
            MapKey(0x170000, 'I', ConsoleKey.I);
            MapKey(0x18, 'o', ConsoleKey.O);
            MapKey(0x180000, 'O', ConsoleKey.O);
            MapKey(0x19, 'p', ConsoleKey.P);
            MapKey(0x190000, 'P', ConsoleKey.P);

            MapKey(0x1E, 'a', ConsoleKey.A);
            MapKey(0x1E0000, 'A', ConsoleKey.A);
            MapKey(0x1F, 's', ConsoleKey.S);
            MapKey(0x1F0000, 'S', ConsoleKey.S);
            MapKey(0x20, 'd', ConsoleKey.D);
            MapKey(0x200000, 'D', ConsoleKey.D);
            MapKey(0x21, 'f', ConsoleKey.F);
            MapKey(0x210000, 'F', ConsoleKey.F);
            MapKey(0x22, 'g', ConsoleKey.G);
            MapKey(0x220000, 'G', ConsoleKey.G);
            MapKey(0x23, 'h', ConsoleKey.H);
            MapKey(0x230000, 'H', ConsoleKey.H);
            MapKey(0x24, 'j', ConsoleKey.J);
            MapKey(0x240000, 'J', ConsoleKey.J);
            MapKey(0x25, 'k', ConsoleKey.K);
            MapKey(0x250000, 'K', ConsoleKey.K);
            MapKey(0x26, 'l', ConsoleKey.L);
            MapKey(0x260000, 'L', ConsoleKey.L);

            MapKey(0x2C, 'z', ConsoleKey.Z);
            MapKey(0x2C0000, 'Z', ConsoleKey.Z);
            MapKey(0x2D, 'x', ConsoleKey.X);
            MapKey(0x2D0000, 'X', ConsoleKey.X);
            MapKey(0x2E, 'c', ConsoleKey.C);
            MapKey(0x2E0000, 'C', ConsoleKey.C);
            MapKey(0x2F, 'v', ConsoleKey.V);
            MapKey(0x2F0000, 'V', ConsoleKey.V);
            MapKey(0x30, 'b', ConsoleKey.B);
            MapKey(0x300000, 'B', ConsoleKey.B);
            MapKey(0x31, 'n', ConsoleKey.N);
            MapKey(0x310000, 'N', ConsoleKey.N);
            MapKey(0x32, 'm', ConsoleKey.M);
            MapKey(0x320000, 'M', ConsoleKey.M);
            #endregion

            #region digits
            //MapKey(0x1, '`');
            //MapKey(0x10000, '~');
            MapKey(0x29, '`', ConsoleKey.NoName);
            MapKey(0x290000, '~', ConsoleKey.NoName);
            MapKey(0x2, '1', ConsoleKey.D1);
            MapKey(0x20000, '!', ConsoleKey.D1);
            MapKey(0x3, '2', ConsoleKey.D2);
            MapKey(0x30000, '@', ConsoleKey.D2);
            MapKey(0x4, '3', ConsoleKey.D3);
            MapKey(0x40000, '#', ConsoleKey.D3);
            MapKey(0x5, '4', ConsoleKey.D4);
            MapKey(0x50000, '$', ConsoleKey.D5);
            MapKey(0x6, '5', ConsoleKey.D5);
            MapKey(0x60000, '%', ConsoleKey.D5);
            MapKey(0x7, '6', ConsoleKey.D6);
            MapKey(0x70000, '^', ConsoleKey.D6);
            MapKey(0x8, '7', ConsoleKey.D7);
            MapKey(0x80000, '&', ConsoleKey.D7);
            MapKey(0x9, '8', ConsoleKey.D8);
            MapKey(0x90000, '*', ConsoleKey.D8);
            MapKey(0xA, '9', ConsoleKey.D9);
            MapKey(0xA0000, '(', ConsoleKey.D9);
            MapKey(0xB, '0', ConsoleKey.D0);
            MapKey(0xB0000, ')', ConsoleKey.D0);

            #endregion

            #region Special
            MapKeyWithShift(0x0E, '\u0968', ConsoleKey.Backspace);               //Backspace
            MapKeyWithShift(0x0F, '\t', ConsoleKey.Tab);                         //Tabulator
            MapKeyWithShift(0x1C, '\n', ConsoleKey.Enter);                       //Enter
            MapKeyWithShift(0x39, ' ', ConsoleKey.Spacebar);                     //Space
            MapKeyWithShift(0x4b, '\u2190', ConsoleKey.LeftArrow);               //Left arrow
            MapKeyWithShift(0x48, '\u2191', ConsoleKey.UpArrow);                 //Up arrow
            MapKeyWithShift(0x4d, '\u2192', ConsoleKey.RightArrow);              //Right arrow
            MapKeyWithShift(0x50, '\u2193', ConsoleKey.DownArrow);               //Down arrow

            MapKeyWithShift(0x5b, ConsoleKey.LeftWindows);
            MapKeyWithShift(0x5c, ConsoleKey.RightWindows);
            //MapKey(0x5d, ConsoleKey.NoName);                                   //Context Menu  

            MapKeyWithShift(0x52, ConsoleKey.Insert);
            MapKeyWithShift(0x47, ConsoleKey.Home);
            MapKeyWithShift(0x49, ConsoleKey.PageUp);
            MapKeyWithShift(0x53, ConsoleKey.Delete);
            MapKeyWithShift(0x4f, ConsoleKey.End);
            MapKeyWithShift(0x51, ConsoleKey.PageDown);

            MapKeyWithShift(0x37, ConsoleKey.PrintScreen);
            //MapKeyWithShift(0x46, ConsoleKey.NoName);                          //Scroll Lock
            //MapKeyWithShift(0x3a, ConsoleKey.NoName);                          //Caps Lock
            MapKeyWithShift(0x45, ConsoleKey.Pause);

            MapKeyWithShift(0x3b, ConsoleKey.F1);
            MapKeyWithShift(0x3c, ConsoleKey.F2);
            MapKeyWithShift(0x3d, ConsoleKey.F3);
            MapKeyWithShift(0x3e, ConsoleKey.F4);
            MapKeyWithShift(0x3f, ConsoleKey.F5);
            MapKeyWithShift(0x40, ConsoleKey.F6);
            MapKeyWithShift(0x41, ConsoleKey.F7);
            MapKeyWithShift(0x42, ConsoleKey.F8);
            MapKeyWithShift(0x43, ConsoleKey.F9);
            MapKeyWithShift(0x44, ConsoleKey.F10);
            MapKeyWithShift(0x57, ConsoleKey.F11);
            MapKeyWithShift(0x58, ConsoleKey.F12);

            MapKeyWithShift(0x1, ConsoleKey.Escape);
            #endregion

            #region Punctuation and Signs
            MapKey(0x27, ';', ConsoleKey.NoName);
            MapKey(0x270000, ':', ConsoleKey.NoName);
            MapKey(0x28, '\'', ConsoleKey.NoName);
            MapKey(0x280000, '"', ConsoleKey.NoName);
            MapKey(0x2B, '\\', ConsoleKey.NoName);
            MapKey(0x2B0000, '|', ConsoleKey.NoName);
            MapKey(0x33, ',', ConsoleKey.OemComma);
            MapKey(0x330000, '<', ConsoleKey.OemComma);
            MapKey(0x34, '.', ConsoleKey.OemPeriod);
            MapKey(0x340000, '>', ConsoleKey.OemPeriod);
            MapKey(0x35, '/', ConsoleKey.Divide);
            MapKey(0x350000, '?', ConsoleKey.Divide);
            //MapKey(0x4A, '-');
            MapKey(0x0C, '-', ConsoleKey.Subtract);
            MapKey(0x0C0000, '_', ConsoleKey.Subtract);
            MapKey(0x0D, '=', ConsoleKey.OemPlus);
            MapKey(0x0D0000, '+', ConsoleKey.OemPlus);
            //MapKey(0x4E, '+');
            MapKey(0x1A, '[', ConsoleKey.NoName);
            MapKey(0x1A0000, '{', ConsoleKey.NoName);
            MapKey(0x1B, ']', ConsoleKey.NoName);
            MapKey(0x1B0000, '}', ConsoleKey.NoName);

            MapKeyWithShift(0x4c, '5', ConsoleKey.NumPad5);

            MapKeyWithShift(0x4a, '-', ConsoleKey.OemMinus);
            MapKeyWithShift(0x4e, '+', ConsoleKey.OemPlus);

            MapKeyWithShift(0x37, '*', ConsoleKey.Multiply);
            #endregion
        }
    }
}
