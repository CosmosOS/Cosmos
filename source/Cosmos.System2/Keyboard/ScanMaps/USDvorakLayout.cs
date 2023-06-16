using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.HAL;

namespace Cosmos.System.ScanMaps
{
    /// <summary>
    /// US_Dvorak class. Represent US_Dvorak keyboard layout.
    /// </summary>
    public class US_Dvorak : ScanMapBase
    {
        /// <summary>
        /// Create new instance of the <see cref="US_Dvorak"/> class.
        /// </summary>
        public US_Dvorak()
        {
        }

        /// <summary>
        /// Init key list.
        /// </summary>
        protected override void InitKeys()
        {
            _keys = new List<KeyMapping>();

            #region Keys

            /*     Scan  Norm Shift Ctrl Alt     Num  Caps ShCaps ShNum ConsoleKeyEx */
            _keys.Add(new KeyMapping(0x00, ConsoleKeyEx.NoName));
            _keys.Add(new KeyMapping(0x01, ConsoleKeyEx.Escape));
            /* 1 -> 9 */
            _keys.Add(new KeyMapping(0x02, '1', '!', '1', '1', '!', '1', ConsoleKeyEx.D1));
            _keys.Add(new KeyMapping(0x03, '2', '@', '2', '2', '@', '2', ConsoleKeyEx.D2));
            _keys.Add(new KeyMapping(0x04, '3', '#', '3', '3', '#', '3', ConsoleKeyEx.D3));
            _keys.Add(new KeyMapping(0x05, '4', '$', '4', '4', '$', '4', ConsoleKeyEx.D4));
            _keys.Add(new KeyMapping(0x06, '5', '%', '5', '5', '%', '5', ConsoleKeyEx.D5));
            _keys.Add(new KeyMapping(0x07, '6', '^', '6', '6', '^', '6', ConsoleKeyEx.D6));
            _keys.Add(new KeyMapping(0x08, '7', '&', '7', '7', '&', '7', ConsoleKeyEx.D7));
            _keys.Add(new KeyMapping(0x09, '8', '*', '8', '8', '*', '8', ConsoleKeyEx.D8));
            _keys.Add(new KeyMapping(0x0A, '9', '(', '9', '9', '(', '9', ConsoleKeyEx.D9));
            _keys.Add(new KeyMapping(0x0B, '0', ')', '0', '0', ')', '0', ConsoleKeyEx.D0));
            /* -, =, Bksp, Tab */
            _keys.Add(new KeyMapping(0x0C, '[', '{', '[', '{', '[', '{', ConsoleKeyEx.LBracket));
            _keys.Add(new KeyMapping(0x0D, ']', '}', ']', '}', ']', '}', ConsoleKeyEx.RBracket));
            //_keys.Add(new KeyMapping(0x0C, '-', '_', '-', '-', '_', '-', ConsoleKeyEx.Minus));
            //_keys.Add(new KeyMapping(0x0D, '=', '+', '=', '=', '+', '=', ConsoleKeyEx.Equal));
            _keys.Add(new KeyMapping(0x0E, ConsoleKeyEx.Backspace));
            _keys.Add(new KeyMapping(0x0F, ConsoleKeyEx.Tab));
            /*      QWERTYUIOP[] */
            _keys.Add(new KeyMapping(0x10, '\'', '"', '\'', '\'', '"', '"', ConsoleKeyEx.Apostrophe)); //'q', 'Q', 'q', 'Q', 'q', 'Q', ConsoleKeyEx.Q));
            _keys.Add(new KeyMapping(0x11, ',', '<', ',', ',', '<', '<', ConsoleKeyEx.Comma)); //'w', 'W', 'w', 'W', 'w', 'W', ConsoleKeyEx.W));
            _keys.Add(new KeyMapping(0x12, '.', '>', '.', '.', '>', '>', ConsoleKeyEx.Period)); //'e', 'E', 'e', 'E', 'e', 'E', ConsoleKeyEx.E));
            _keys.Add(new KeyMapping(0x13, 'p', 'P', 'p', 'P', 'p', 'P', ConsoleKeyEx.P)); //'r', 'R', 'r', 'R', 'r', 'R', ConsoleKeyEx.R));
            _keys.Add(new KeyMapping(0x14, 'y', 'Y', 'y', 'Y', 'y', 'Y', ConsoleKeyEx.Y)); //'t', 'T', 't', 'T', 't', 'T', ConsoleKeyEx.T));
            _keys.Add(new KeyMapping(0x15, 'f', 'F', 'f', 'F', 'f', 'F', ConsoleKeyEx.F)); //'y', 'Y', 'y', 'Y', 'y', 'Y', ConsoleKeyEx.Y));
            _keys.Add(new KeyMapping(0x16, 'g', 'G', 'g', 'G', 'g', 'G', ConsoleKeyEx.G)); //'u', 'U', 'u', 'U', 'u', 'U', ConsoleKeyEx.U));
            _keys.Add(new KeyMapping(0x17, 'c', 'C', 'c', 'C', 'c', 'C', ConsoleKeyEx.C)); //'i', 'I', 'i', 'I', 'i', 'I', ConsoleKeyEx.I));
            _keys.Add(new KeyMapping(0x18, 'r', 'R', 'r', 'R', 'r', 'R', ConsoleKeyEx.R)); //'o', 'O', 'o', 'O', 'o', 'O', ConsoleKeyEx.O));
            _keys.Add(new KeyMapping(0x19, 'l', 'L', 'l', 'L', 'l', 'L', ConsoleKeyEx.L)); //'p', 'P', 'p', 'P', 'p', 'P', ConsoleKeyEx.P));
            _keys.Add(new KeyMapping(0x1A, '/', '?', '/', '/', '?', '/', ConsoleKeyEx.Slash)); //'[', '{', '[', '{', '[', '{', ConsoleKeyEx.LBracket));
            _keys.Add(new KeyMapping(0x1B, '=', '+', '=', '=', '+', '=', ConsoleKeyEx.Equal)); //']', '}', ']', '}', ']', '}', ConsoleKeyEx.RBracket));
            /* ENTER, CTRL */
            _keys.Add(new KeyMapping(0x1C, ConsoleKeyEx.Enter));
            _keys.Add(new KeyMapping(0x1D, ConsoleKeyEx.LCtrl));
            /* ASDFGHJKL;'` */
            _keys.Add(new KeyMapping(0x1E, 'a', 'A', 'a', 'A', 'a', 'A', ConsoleKeyEx.A));
            _keys.Add(new KeyMapping(0x1F, 'o', 'O', 'o', 'O', 'o', 'O', ConsoleKeyEx.O)); //'s', 'S', 's', 'S', 's', 'S', ConsoleKeyEx.S));
            _keys.Add(new KeyMapping(0x20, 'e', 'E', 'e', 'E', 'e', 'E', ConsoleKeyEx.E)); //'d', 'D', 'd', 'D', 'd', 'D', ConsoleKeyEx.D));
            _keys.Add(new KeyMapping(0x21, 'u', 'U', 'u', 'U', 'u', 'U', ConsoleKeyEx.U)); //'f', 'F', 'f', 'F', 'f', 'F', ConsoleKeyEx.F));
            _keys.Add(new KeyMapping(0x22, 'i', 'I', 'i', 'I', 'i', 'I', ConsoleKeyEx.I)); //'g', 'G', 'g', 'G', 'g', 'G', ConsoleKeyEx.G));
            _keys.Add(new KeyMapping(0x23, 'd', 'D', 'd', 'D', 'd', 'D', ConsoleKeyEx.D)); //'h', 'H', 'h', 'H', 'h', 'H', ConsoleKeyEx.H));
            _keys.Add(new KeyMapping(0x24, 'h', 'H', 'h', 'H', 'h', 'H', ConsoleKeyEx.H)); //'j', 'J', 'j', 'J', 'j', 'J', ConsoleKeyEx.J));
            _keys.Add(new KeyMapping(0x25, 't', 'T', 't', 'T', 't', 'T', ConsoleKeyEx.T)); //'k', 'K', 'k', 'K', 'k', 'K', ConsoleKeyEx.K));
            _keys.Add(new KeyMapping(0x26, 'n', 'N', 'n', 'N', 'n', 'N', ConsoleKeyEx.N)); //'l', 'L', 'l', 'L', 'l', 'L', ConsoleKeyEx.L));
            _keys.Add(new KeyMapping(0x27, 's', 'S', 's', 'S', 's', 'S', ConsoleKeyEx.S)); //';', ':', ';', ';', ':', ':', ConsoleKeyEx.Semicolon));
            _keys.Add(new KeyMapping(0x28, '-', '_', '-', '-', '_', '-', ConsoleKeyEx.Minus)); //'\'', '"', '\'', '\'', '"', '"', ConsoleKeyEx.Apostrophe));
            _keys.Add(new KeyMapping(0x29, '`', '~', '`', '`', '~', '~', ConsoleKeyEx.Backquote));
            /* Left Shift*/
            _keys.Add(new KeyMapping(0x2A, ConsoleKeyEx.LShift));
            /* \ZXCVBNM,./ */
            _keys.Add(new KeyMapping(0x2B, '\\', '|', '\\', '\\', '|', '|', ConsoleKeyEx.Backslash));
            _keys.Add(new KeyMapping(0x2C, ';', ':', ';', ';', ':', ':', ConsoleKeyEx.Semicolon)); //'z', 'Z', 'z', 'Z', 'z', 'Z', ConsoleKeyEx.Z));
            _keys.Add(new KeyMapping(0x2D, 'q', 'Q', 'q', 'Q', 'q', 'Q', ConsoleKeyEx.Q)); //'x', 'X', 'x', 'X', 'x', 'X', ConsoleKeyEx.X));
            _keys.Add(new KeyMapping(0x2E, 'j', 'J', 'j', 'J', 'j', 'J', ConsoleKeyEx.J)); //'c', 'C', 'c', 'C', 'c', 'C', ConsoleKeyEx.C));
            _keys.Add(new KeyMapping(0x2F, 'k', 'K', 'k', 'K', 'k', 'K', ConsoleKeyEx.K)); //'v', 'V', 'v', 'V', 'v', 'V', ConsoleKeyEx.V));
            _keys.Add(new KeyMapping(0x30, 'x', 'X', 'x', 'X', 'x', 'X', ConsoleKeyEx.X)); //'b', 'B', 'b', 'B', 'b', 'B', ConsoleKeyEx.B));
            _keys.Add(new KeyMapping(0x31, 'b', 'B', 'b', 'B', 'b', 'B', ConsoleKeyEx.B)); //'n', 'N', 'n', 'N', 'n', 'N', ConsoleKeyEx.N));
            _keys.Add(new KeyMapping(0x32, 'm', 'M', 'm', 'M', 'm', 'M', ConsoleKeyEx.M));
            _keys.Add(new KeyMapping(0x33, 'w', 'W', 'w', 'W', 'w', 'W', ConsoleKeyEx.W)); //',', '<', ',', ',', '<', '<', ConsoleKeyEx.Comma));
            _keys.Add(new KeyMapping(0x34, 'v', 'V', 'v', 'V', 'v', 'V', ConsoleKeyEx.V)); //'.', '>', '.', '.', '>', '>', ConsoleKeyEx.Period));
            _keys.Add(new KeyMapping(0x35, 'z', 'Z', 'z', 'Z', 'z', 'Z', ConsoleKeyEx.Z)); //'/', '?', '/', '/', '?', '/', ConsoleKeyEx.Slash)); // also numpad divide
            /* Right Shift */
            _keys.Add(new KeyMapping(0x36, ConsoleKeyEx.RShift));
            /* Print Screen */
            _keys.Add(new KeyMapping(0x37, '*', '*', '*', '*', '*', '*', ConsoleKeyEx.NumMultiply));
            // also numpad multiply
            /* Alt  */
            _keys.Add(new KeyMapping(0x38, ConsoleKeyEx.LAlt));
            /* Space */
            _keys.Add(new KeyMapping(0x39, ' ', ConsoleKeyEx.Spacebar));
            /* Caps */
            _keys.Add(new KeyMapping(0x3A, ConsoleKeyEx.CapsLock));
            /* F1-F12 */
            _keys.Add(new KeyMapping(0x3B, ConsoleKeyEx.F1));
            _keys.Add(new KeyMapping(0x3C, ConsoleKeyEx.F2));
            _keys.Add(new KeyMapping(0x3D, ConsoleKeyEx.F3));
            _keys.Add(new KeyMapping(0x3E, ConsoleKeyEx.F4));
            _keys.Add(new KeyMapping(0x3F, ConsoleKeyEx.F5));
            _keys.Add(new KeyMapping(0x40, ConsoleKeyEx.F6));
            _keys.Add(new KeyMapping(0x41, ConsoleKeyEx.F7));
            _keys.Add(new KeyMapping(0x42, ConsoleKeyEx.F8));
            _keys.Add(new KeyMapping(0x43, ConsoleKeyEx.F9));
            _keys.Add(new KeyMapping(0x44, ConsoleKeyEx.F10));
            _keys.Add(new KeyMapping(0x57, ConsoleKeyEx.F11));
            _keys.Add(new KeyMapping(0x58, ConsoleKeyEx.F12));
            /* Num Lock, Scrl Lock */
            _keys.Add(new KeyMapping(0x45, ConsoleKeyEx.NumLock));
            _keys.Add(new KeyMapping(0x46, ConsoleKeyEx.ScrollLock));
            /* HOME, Up, Pgup, -kpad, left, center, right, +keypad, end, down, pgdn, ins, del */
            _keys.Add(new KeyMapping(0x47, '\0', '\0', '7', '\0', '\0', '\0', ConsoleKeyEx.Home, ConsoleKeyEx.Num7));
            _keys.Add(new KeyMapping(0x48, '\0', '\0', '8', '\0', '\0', '\0', ConsoleKeyEx.UpArrow, ConsoleKeyEx.Num8));
            _keys.Add(new KeyMapping(0x49, '\0', '\0', '9', '\0', '\0', '\0', ConsoleKeyEx.PageUp, ConsoleKeyEx.Num9));
            _keys.Add(new KeyMapping(0x4A, '-', '-', '-', '-', '-', '-', ConsoleKeyEx.NumMinus));
            _keys.Add(new KeyMapping(0x4B, '\0', '\0', '4', '\0', '\0', '\0', ConsoleKeyEx.LeftArrow, ConsoleKeyEx.Num4));
            _keys.Add(new KeyMapping(0x4C, '\0', '\0', '5', '\0', '\0', '\0', ConsoleKeyEx.Num5));
            _keys.Add(new KeyMapping(0x4D, '\0', '\0', '6', '\0', '\0', '\0', ConsoleKeyEx.RightArrow, ConsoleKeyEx.Num6));
            _keys.Add(new KeyMapping(0x4E, '+', '+', '+', '+', '+', '+', ConsoleKeyEx.NumPlus));
            _keys.Add(new KeyMapping(0x4F, '\0', '\0', '1', '\0', '\0', '\0', ConsoleKeyEx.End, ConsoleKeyEx.Num1));
            _keys.Add(new KeyMapping(0x50, '\0', '\0', '2', '\0', '\0', '\0', ConsoleKeyEx.DownArrow, ConsoleKeyEx.Num2));
            _keys.Add(new KeyMapping(0x51, '\0', '\0', '3', '\0', '\0', '\0', ConsoleKeyEx.PageDown, ConsoleKeyEx.Num3));
            _keys.Add(new KeyMapping(0x52, '\0', '\0', '0', '\0', '\0', '\0', ConsoleKeyEx.Insert, ConsoleKeyEx.Num0));
            _keys.Add(new KeyMapping(0x53, '\0', '\0', '.', '\0', '\0', '\0', ConsoleKeyEx.Delete,
                ConsoleKeyEx.NumPeriod));

            _keys.Add(new KeyMapping(0x5b, ConsoleKeyEx.LWin));
            _keys.Add(new KeyMapping(0x5c, ConsoleKeyEx.RWin));

            #endregion
        }
    }
}
