/*
Copyright (c) 2012-2013, dewitcher Team
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice
   this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using Cosmos.Hardware;

namespace DuNodes.Kernel.Base.Core
{
    // INFO: We recommend to set the keylayout in the BeforeRun() method to make sure that
    //       the arrow keys does not appear as a pretty fuckedup random unicode char..
    public static class KeyLayout
    {
        internal static List<Cosmos.Hardware.Keyboard.KeyMapping> keys;
        public enum KeyLayouts : byte { QWERTY, QWERTZ, AZERTY };
        private static uint KeyCount;
        private static void ChangeKeyMap()
        {
            Global.Keyboard.ChangeKeyMap(keys);
        }
        public static void SwitchKeyLayout(KeyLayouts layout)
        {
            switch(layout)
            {
                case KeyLayouts.AZERTY:
                    AZERTY(); break;
                case KeyLayouts.QWERTY:
                    QWERTY(); break;
                case KeyLayouts.QWERTZ:
                    QWERTZ(); break;
            }
        }
        private static void AddKey(uint p, char p_2, ConsoleKey p_3)
        {
            keys.Add(new Keyboard.KeyMapping(p, p_2, p_3));
            KeyCount += 1u;
        }
        private static void AddKeyWithShift(uint p, char p_2, ConsoleKey p_3)
        {
            AddKey(p, p_2, p_3);
            AddKey(p << 16, p_2, p_3);
        }
        private static void AddKey(uint p, ConsoleKey p_3)
        {
            AddKey(p, '\0', p_3);
        }
        private static void AddKeyWithShift(uint p, ConsoleKey p_3)
        {
            AddKeyWithShift(p, '\0', p_3);
        }
        public static void QWERTY()
        {
            keys = new List<Keyboard.KeyMapping>(164);
            #region Keys
            AddKey(16u, 'q', ConsoleKey.Q);
            AddKey(1048576u, 'Q', ConsoleKey.Q);
            AddKey(17u, 'w', ConsoleKey.W);
            AddKey(1114112u, 'W', ConsoleKey.W);
            AddKey(18u, 'e', ConsoleKey.E);
            AddKey(1179648u, 'E', ConsoleKey.E);
            AddKey(19u, 'r', ConsoleKey.R);
            AddKey(1245184u, 'R', ConsoleKey.R);
            AddKey(20u, 't', ConsoleKey.T);
            AddKey(1310720u, 'T', ConsoleKey.T);
            AddKey(21u, 'y', ConsoleKey.Y);
            AddKey(1376256u, 'Y', ConsoleKey.Y);
            AddKey(22u, 'u', ConsoleKey.U);
            AddKey(1441792u, 'U', ConsoleKey.U);
            AddKey(23u, 'i', ConsoleKey.I);
            AddKey(1507328u, 'I', ConsoleKey.I);
            AddKey(24u, 'o', ConsoleKey.O);
            AddKey(1572864u, 'O', ConsoleKey.O);
            AddKey(25u, 'p', ConsoleKey.P);
            AddKey(1638400u, 'P', ConsoleKey.P);
            AddKey(30u, 'a', ConsoleKey.A);
            AddKey(1966080u, 'A', ConsoleKey.A);
            AddKey(31u, 's', ConsoleKey.S);
            AddKey(2031616u, 'S', ConsoleKey.S);
            AddKey(32u, 'd', ConsoleKey.D);
            AddKey(2097152u, 'D', ConsoleKey.D);
            AddKey(33u, 'f', ConsoleKey.F);
            AddKey(2162688u, 'F', ConsoleKey.F);
            AddKey(34u, 'g', ConsoleKey.G);
            AddKey(2228224u, 'G', ConsoleKey.G);
            AddKey(35u, 'h', ConsoleKey.H);
            AddKey(2293760u, 'H', ConsoleKey.H);
            AddKey(36u, 'j', ConsoleKey.J);
            AddKey(2359296u, 'J', ConsoleKey.J);
            AddKey(37u, 'k', ConsoleKey.K);
            AddKey(2424832u, 'K', ConsoleKey.K);
            AddKey(38u, 'l', ConsoleKey.L);
            AddKey(2490368u, 'L', ConsoleKey.L);
            AddKey(44u, 'z', ConsoleKey.Z);
            AddKey(2883584u, 'Z', ConsoleKey.Z);
            AddKey(45u, 'x', ConsoleKey.X);
            AddKey(2949120u, 'X', ConsoleKey.X);
            AddKey(46u, 'c', ConsoleKey.C);
            AddKey(3014656u, 'C', ConsoleKey.C);
            AddKey(47u, 'v', ConsoleKey.V);
            AddKey(3080192u, 'V', ConsoleKey.V);
            AddKey(48u, 'b', ConsoleKey.B);
            AddKey(3145728u, 'B', ConsoleKey.B);
            AddKey(49u, 'n', ConsoleKey.N);
            AddKey(3211264u, 'N', ConsoleKey.N);
            AddKey(50u, 'm', ConsoleKey.M);
            AddKey(3276800u, 'M', ConsoleKey.M);
            AddKey(41u, '`', ConsoleKey.NoName);
            AddKey(2686976u, '~', ConsoleKey.NoName);
            AddKey(2u, '1', ConsoleKey.D1);
            AddKey(131072u, '!', ConsoleKey.D1);
            AddKey(3u, '2', ConsoleKey.D2);
            AddKey(196608u, '@', ConsoleKey.D2);
            AddKey(4u, '3', ConsoleKey.D3);
            AddKey(262144u, '#', ConsoleKey.D3);
            AddKey(5u, '4', ConsoleKey.D4);
            AddKey(327680u, '$', ConsoleKey.D5);
            AddKey(6u, '5', ConsoleKey.D5);
            AddKey(393216u, '%', ConsoleKey.D5);
            AddKey(7u, '6', ConsoleKey.D6);
            AddKey(458752u, '^', ConsoleKey.D6);
            AddKey(8u, '7', ConsoleKey.D7);
            AddKey(524288u, '&', ConsoleKey.D7);
            AddKey(9u, '8', ConsoleKey.D8);
            AddKey(589824u, '*', ConsoleKey.D8);
            AddKey(10u, '9', ConsoleKey.D9);
            AddKey(655360u, '(', ConsoleKey.D9);
            AddKey(11u, '0', ConsoleKey.D0);
            AddKey(720896u, ')', ConsoleKey.D0);
            AddKeyWithShift(14u, '\b', ConsoleKey.Backspace);
            AddKeyWithShift(15u, '\t', ConsoleKey.Tab);
            AddKeyWithShift(28u, '\n', ConsoleKey.Enter);
            AddKeyWithShift(57u, ' ', ConsoleKey.Spacebar);
            AddKeyWithShift(75u, '\0', ConsoleKey.LeftArrow);
            AddKeyWithShift(72u, '\0', ConsoleKey.UpArrow);
            AddKeyWithShift(77u, '\0', ConsoleKey.RightArrow);
            AddKeyWithShift(80u, '\0', ConsoleKey.DownArrow);
            AddKeyWithShift(91u, ConsoleKey.LeftWindows);
            AddKeyWithShift(92u, ConsoleKey.RightWindows);
            AddKeyWithShift(82u, ConsoleKey.Insert);
            AddKeyWithShift(71u, ConsoleKey.Home);
            AddKeyWithShift(73u, ConsoleKey.PageUp);
            AddKeyWithShift(83u, ConsoleKey.Delete);
            AddKeyWithShift(79u, ConsoleKey.End);
            AddKeyWithShift(81u, ConsoleKey.PageDown);
            AddKeyWithShift(55u, ConsoleKey.PrintScreen);
            AddKeyWithShift(69u, ConsoleKey.Pause);
            AddKeyWithShift(59u, ConsoleKey.F1);
            AddKeyWithShift(60u, ConsoleKey.F2);
            AddKeyWithShift(61u, ConsoleKey.F3);
            AddKeyWithShift(62u, ConsoleKey.F4);
            AddKeyWithShift(63u, ConsoleKey.F5);
            AddKeyWithShift(64u, ConsoleKey.F6);
            AddKeyWithShift(65u, ConsoleKey.F7);
            AddKeyWithShift(66u, ConsoleKey.F8);
            AddKeyWithShift(67u, ConsoleKey.F9);
            AddKeyWithShift(68u, ConsoleKey.F10);
            AddKeyWithShift(87u, ConsoleKey.F11);
            AddKeyWithShift(88u, ConsoleKey.F12);
            AddKeyWithShift(1u, ConsoleKey.Escape);
            AddKey(39u, ';', ConsoleKey.NoName);
            AddKey(2555904u, ':', ConsoleKey.NoName);
            AddKey(40u, '\'', ConsoleKey.NoName);
            AddKey(2621440u, '"', ConsoleKey.NoName);
            AddKey(43u, '\\', ConsoleKey.NoName);
            AddKey(2818048u, '|', ConsoleKey.NoName);
            AddKey(51u, ',', ConsoleKey.OemComma);
            AddKey(3342336u, '<', ConsoleKey.OemComma);
            AddKey(52u, '.', ConsoleKey.OemPeriod);
            AddKey(3407872u, '>', ConsoleKey.OemPeriod);
            AddKey(53u, '/', ConsoleKey.Divide);
            AddKey(3473408u, '?', ConsoleKey.Divide);
            AddKey(12u, '-', ConsoleKey.Subtract);
            AddKey(786432u, '_', ConsoleKey.Subtract);
            AddKey(13u, '=', ConsoleKey.OemPlus);
            AddKey(851968u, '+', ConsoleKey.OemPlus);
            AddKey(26u, '[', ConsoleKey.NoName);
            AddKey(1703936u, '{', ConsoleKey.NoName);
            AddKey(27u, ']', ConsoleKey.NoName);
            AddKey(1769472u, '}', ConsoleKey.NoName);
            AddKeyWithShift(76u, '5', ConsoleKey.NumPad5);
            AddKeyWithShift(74u, '-', ConsoleKey.OemMinus);
            AddKeyWithShift(78u, '+', ConsoleKey.OemPlus);
            AddKeyWithShift(55u, '*', ConsoleKey.Multiply);
            #endregion
            ChangeKeyMap();
        }

        /// <summary>
        /// The QWERTZ-Implementation is not 100% finished.
        /// Most keys will work, some keys will still return QWERTY-Chars.
        /// </summary>
        public static void QWERTZ()
        {
            keys = new List<Keyboard.KeyMapping>(164);
            #region Keys
            // Q W E R T Z U I O P
            AddKey(16u, 'q', ConsoleKey.Q);
            AddKey(1048576u, 'Q', ConsoleKey.Q);
            AddKey(17u, 'w', ConsoleKey.W);
            AddKey(1114112u, 'W', ConsoleKey.W);
            AddKey(18u, 'e', ConsoleKey.E);
            AddKey(1179648u, 'E', ConsoleKey.E);
            AddKey(19u, 'r', ConsoleKey.R);
            AddKey(1245184u, 'R', ConsoleKey.R);
            AddKey(20u, 't', ConsoleKey.T);
            AddKey(1310720u, 'T', ConsoleKey.T);
            AddKey(21u, 'z', ConsoleKey.Z);
            AddKey(1376256u, 'Z', ConsoleKey.Z);
            AddKey(22u, 'u', ConsoleKey.U);
            AddKey(1441792u, 'U', ConsoleKey.U);
            AddKey(23u, 'i', ConsoleKey.I);
            AddKey(1507328u, 'I', ConsoleKey.I);
            AddKey(24u, 'o', ConsoleKey.O);
            AddKey(1572864u, 'O', ConsoleKey.O);
            AddKey(25u, 'p', ConsoleKey.P);
            AddKey(1638400u, 'P', ConsoleKey.P);

            // A S D F G H J K L
            AddKey(30u, 'a', ConsoleKey.A);
            AddKey(1966080u, 'A', ConsoleKey.A);
            AddKey(31u, 's', ConsoleKey.S);
            AddKey(2031616u, 'S', ConsoleKey.S);
            AddKey(32u, 'd', ConsoleKey.D);
            AddKey(2097152u, 'D', ConsoleKey.D);
            AddKey(33u, 'f', ConsoleKey.F);
            AddKey(2162688u, 'F', ConsoleKey.F);
            AddKey(34u, 'g', ConsoleKey.G);
            AddKey(2228224u, 'G', ConsoleKey.G);
            AddKey(35u, 'h', ConsoleKey.H);
            AddKey(2293760u, 'H', ConsoleKey.H);
            AddKey(36u, 'j', ConsoleKey.J);
            AddKey(2359296u, 'J', ConsoleKey.J);
            AddKey(37u, 'k', ConsoleKey.K);
            AddKey(2424832u, 'K', ConsoleKey.K);
            AddKey(38u, 'l', ConsoleKey.L);
            AddKey(2490368u, 'L', ConsoleKey.L);

            // Y X C V B N M
            AddKey(44u, 'y', ConsoleKey.Y);
            AddKey(2883584u, 'Y', ConsoleKey.Y);
            AddKey(45u, 'x', ConsoleKey.X);
            AddKey(2949120u, 'X', ConsoleKey.X);
            AddKey(46u, 'c', ConsoleKey.C);
            AddKey(3014656u, 'C', ConsoleKey.C);
            AddKey(47u, 'v', ConsoleKey.V);
            AddKey(3080192u, 'V', ConsoleKey.V);
            AddKey(48u, 'b', ConsoleKey.B);
            AddKey(3145728u, 'B', ConsoleKey.B);
            AddKey(49u, 'n', ConsoleKey.N);
            AddKey(3211264u, 'N', ConsoleKey.N);
            AddKey(50u, 'm', ConsoleKey.M);
            AddKey(3276800u, 'M', ConsoleKey.M);

            // ÃœÃ–Ã„
            // -- Nothing yet

            // ^ 1 2 3 4 5 6 7 8 9 0
            // Â° ! " Â§ $ % & / ( ) =
            AddKey(41u, '^', ConsoleKey.NoName);
            AddKey(2686976u, '\0', ConsoleKey.NoName);
            AddKey(2u, '1', ConsoleKey.D1); 
            AddKey(131072u, '!', ConsoleKey.D1);
            AddKey(3u, '2', ConsoleKey.D2);
            AddKey(196608u, '\"', ConsoleKey.D2);
            AddKey(4u, '3', ConsoleKey.D3);
            AddKey(262144u, '$', ConsoleKey.D3);
            AddKey(5u, '4', ConsoleKey.D4);
            AddKey(327680u, '$', ConsoleKey.D5);
            AddKey(6u, '5', ConsoleKey.D5);
            AddKey(393216u, '%', ConsoleKey.D5);
            AddKey(7u, '6', ConsoleKey.D6);
            AddKey(458752u, '&', ConsoleKey.D6);
            AddKey(8u, '7', ConsoleKey.D7);
            AddKey(524288u, '/', ConsoleKey.Divide);
            AddKey(9u, '8', ConsoleKey.D8);
            AddKey(589824u, '(', ConsoleKey.D8);
            AddKey(10u, '9', ConsoleKey.D9);
            AddKey(655360u, ')', ConsoleKey.D9);
            AddKey(11u, '0', ConsoleKey.D0);
            AddKey(720896u, '=', ConsoleKey.D0);

            // + * # ' - _
            AddKey(27u, '+', ConsoleKey.OemPlus);
            AddKey(1769472u, '*', ConsoleKey.Multiply);
            AddKey(43u, '#', ConsoleKey.NoName);
            AddKey(2555904u, '\'', ConsoleKey.NoName);
            AddKey(53u, '-', ConsoleKey.Subtract);
            AddKey(3473408u, '_', ConsoleKey.Subtract);

            // ; , : .
            AddKey(3342336u, ';', ConsoleKey.OemComma);
            AddKey(51u, ',', ConsoleKey.OemComma);
            AddKey(3407872u, ':', ConsoleKey.OemPeriod);
            AddKey(52u, '.', ConsoleKey.OemPeriod);

            // < > | ~
            // -- DOES NOT EXIST
            // -- DOES NOT EXIST
            // -- DOES NOT EXIST
            AddKey(27u, '~', ConsoleKey.OemPlus);

            // Special keys
            AddKeyWithShift(14u, '\b', ConsoleKey.Backspace);
            AddKeyWithShift(15u, '\t', ConsoleKey.Tab);
            AddKeyWithShift(28u, '\n', ConsoleKey.Enter);
            AddKeyWithShift(57u, ' ', ConsoleKey.Spacebar);
            AddKeyWithShift(75u, '\0', ConsoleKey.LeftArrow);
            AddKeyWithShift(72u, '\0', ConsoleKey.UpArrow);
            AddKeyWithShift(77u, '\0', ConsoleKey.RightArrow);
            AddKeyWithShift(80u, '\0', ConsoleKey.DownArrow);
            AddKeyWithShift(91u, ConsoleKey.LeftWindows);
            AddKeyWithShift(92u, ConsoleKey.RightWindows);
            AddKeyWithShift(82u, ConsoleKey.Insert);
            AddKeyWithShift(71u, ConsoleKey.Home);
            AddKeyWithShift(73u, ConsoleKey.PageUp);
            AddKeyWithShift(83u, ConsoleKey.Delete);
            AddKeyWithShift(79u, ConsoleKey.End);
            AddKeyWithShift(81u, ConsoleKey.PageDown);
            AddKeyWithShift(55u, ConsoleKey.PrintScreen);
            AddKeyWithShift(69u, ConsoleKey.Pause);
            AddKeyWithShift(59u, ConsoleKey.F1);
            AddKeyWithShift(60u, ConsoleKey.F2);
            AddKeyWithShift(61u, ConsoleKey.F3);
            AddKeyWithShift(62u, ConsoleKey.F4);
            AddKeyWithShift(63u, ConsoleKey.F5);
            AddKeyWithShift(64u, ConsoleKey.F6);
            AddKeyWithShift(65u, ConsoleKey.F7);
            AddKeyWithShift(66u, ConsoleKey.F8);
            AddKeyWithShift(67u, ConsoleKey.F9);
            AddKeyWithShift(68u, ConsoleKey.F10);
            AddKeyWithShift(87u, ConsoleKey.F11);
            AddKeyWithShift(88u, ConsoleKey.F12);
            AddKeyWithShift(1u, ConsoleKey.Escape);

            // ÃŸ ? \
            // -- Not yet implemented

            // Other keys
            AddKeyWithShift(76u, '5', ConsoleKey.NumPad5);
            #endregion
            ChangeKeyMap();
        }
		
        public static void AZERTY()
        {
            keys = new List<Keyboard.KeyMapping>(164);
            #region Keys
            // A Z E R T Y U I O P
            AddKey(16u, 'a', ConsoleKey.A);
            AddKey(1048576u, 'A', ConsoleKey.A);
            AddKey(17u, 'z', ConsoleKey.Z);
            AddKey(1114112u, 'Z', ConsoleKey.Z);
            AddKey(18u, 'e', ConsoleKey.E);
            AddKey(1179648u, 'E', ConsoleKey.E);
            AddKey(19u, 'r', ConsoleKey.R);
            AddKey(1245184u, 'R', ConsoleKey.R);
            AddKey(20u, 't', ConsoleKey.T);
            AddKey(1310720u, 'T', ConsoleKey.T);
            AddKey(21u, 'y', ConsoleKey.Y);
            AddKey(1376256u, 'Y', ConsoleKey.Y);
            AddKey(22u, 'u', ConsoleKey.U);
            AddKey(1441792u, 'U', ConsoleKey.U);
            AddKey(23u, 'i', ConsoleKey.I);
            AddKey(1507328u, 'I', ConsoleKey.I);
            AddKey(24u, 'o', ConsoleKey.O);
            AddKey(1572864u, 'O', ConsoleKey.O);
            AddKey(25u, 'p', ConsoleKey.P);
            AddKey(1638400u, 'P', ConsoleKey.P);

            // Q S D F G H J K L M
            AddKey(30u, 'q', ConsoleKey.Q);
            AddKey(1966080u, 'Q', ConsoleKey.Q);
            AddKey(31u, 's', ConsoleKey.S);
            AddKey(2031616u, 'S', ConsoleKey.S);
            AddKey(32u, 'd', ConsoleKey.D);
            AddKey(2097152u, 'D', ConsoleKey.D);
            AddKey(33u, 'f', ConsoleKey.F);
            AddKey(2162688u, 'F', ConsoleKey.F);
            AddKey(34u, 'g', ConsoleKey.G);
            AddKey(2228224u, 'G', ConsoleKey.G);
            AddKey(35u, 'h', ConsoleKey.H);
            AddKey(2293760u, 'H', ConsoleKey.H);
            AddKey(36u, 'j', ConsoleKey.J);
            AddKey(2359296u, 'J', ConsoleKey.J);
            AddKey(37u, 'k', ConsoleKey.K);
            AddKey(2424832u, 'K', ConsoleKey.K);
            AddKey(38u, 'l', ConsoleKey.L);
            AddKey(2490368u, 'L', ConsoleKey.L);
            AddKey(39u, 'm', ConsoleKey.M);

            // W X C V B N
            AddKey(44u, 'w', ConsoleKey.W);
            AddKey(2883584u, 'W', ConsoleKey.W);
            AddKey(45u, 'x', ConsoleKey.X);
            AddKey(2949120u, 'X', ConsoleKey.X);
            AddKey(46u, 'c', ConsoleKey.C);
            AddKey(3014656u, 'C', ConsoleKey.C);
            AddKey(47u, 'v', ConsoleKey.V);
            AddKey(3080192u, 'V', ConsoleKey.V);
            AddKey(48u, 'b', ConsoleKey.B);
            AddKey(3145728u, 'B', ConsoleKey.B);
            AddKey(49u, 'n', ConsoleKey.N);
            AddKey(3211264u, 'N', ConsoleKey.N);
            

            // ÜÖÄ
            // -- Nothing yet

            // ^ 1 2 3 4 5 6 7 8 9 0
            // ° ! " § $ % & / ( ) =
            AddKey(41u, '²', ConsoleKey.NoName);
            //AddKey(2686976u, '°', ConsoleKey.NoName);
            AddKey(2u, '&', ConsoleKey.D1);
            AddKey(131072u, '1', ConsoleKey.D1);
            AddKey(3u, 'é', ConsoleKey.D2);
            AddKey(196608u, '2', ConsoleKey.D2);
            AddKey(4u, '"', ConsoleKey.D3);
            AddKey(262144u, '3', ConsoleKey.D3);
            AddKey(5u, '\'', ConsoleKey.D4);
            AddKey(327680u, '4', ConsoleKey.D5);
            AddKey(6u, '(', ConsoleKey.D5);
            AddKey(393216u, '5', ConsoleKey.D5);
            AddKey(7u, '-', ConsoleKey.D6);
            AddKey(458752u, '6', ConsoleKey.D6);
            AddKey(8u, 'è', ConsoleKey.D7);
            AddKey(524288u, '7', ConsoleKey.Divide);
            AddKey(9u, '_', ConsoleKey.D8);
            AddKey(589824u, '8', ConsoleKey.D8);
            AddKey(10u, 'ç', ConsoleKey.D9);
            AddKey(655360u, '9', ConsoleKey.D9);
            AddKey(11u, 'à', ConsoleKey.D0);
            AddKey(720896u, '0', ConsoleKey.D0);

            // + * # ' - _
            AddKey(27u, '$', ConsoleKey.OemPlus);
            AddKey(1769472u, '£', ConsoleKey.Multiply);
            AddKey(43u, '#', ConsoleKey.NoName);
            AddKey(2555904u, '\'', ConsoleKey.NoName);
            AddKey(53u, '-', ConsoleKey.Subtract);
            AddKey(3473408u, '_', ConsoleKey.Subtract);

            // ; , : .
            AddKey(50u, ',', ConsoleKey.OemComma);
            AddKey(3342336u, '.', ConsoleKey.OemComma);
            AddKey(51u, ';', ConsoleKey.OemComma);
            AddKey(3407872u, '/', ConsoleKey.OemPeriod);
            AddKey(52u, ':', ConsoleKey.OemPeriod);

            // < > | ~
            // -- DOES NOT EXIST
            // -- DOES NOT EXIST
            // -- DOES NOT EXIST
            AddKey(27u, '¤', ConsoleKey.OemPlus);

            // Special keys
            AddKeyWithShift(14u, '?', ConsoleKey.Backspace);
            AddKeyWithShift(15u, '\t', ConsoleKey.Tab);
            AddKeyWithShift(28u, '\n', ConsoleKey.Enter);
            AddKeyWithShift(57u, ' ', ConsoleKey.Spacebar);
            AddKeyWithShift(75u, '?', ConsoleKey.LeftArrow);
            AddKeyWithShift(72u, '?', ConsoleKey.UpArrow);
            AddKeyWithShift(77u, '?', ConsoleKey.RightArrow);
            AddKeyWithShift(80u, '?', ConsoleKey.DownArrow);
            AddKeyWithShift(91u, ConsoleKey.LeftWindows);
            AddKeyWithShift(92u, ConsoleKey.RightWindows);
            AddKeyWithShift(82u, ConsoleKey.Insert);
            AddKeyWithShift(71u, ConsoleKey.Home);
            AddKeyWithShift(73u, ConsoleKey.PageUp);
            AddKeyWithShift(83u, ConsoleKey.Delete);
            AddKeyWithShift(79u, ConsoleKey.End);
            AddKeyWithShift(81u, ConsoleKey.PageDown);
            AddKeyWithShift(55u, ConsoleKey.PrintScreen);
            AddKeyWithShift(69u, ConsoleKey.Pause);
            AddKeyWithShift(59u, ConsoleKey.F1);
            AddKeyWithShift(60u, ConsoleKey.F2);
            AddKeyWithShift(61u, ConsoleKey.F3);
            AddKeyWithShift(62u, ConsoleKey.F4);
            AddKeyWithShift(63u, ConsoleKey.F5);
            AddKeyWithShift(64u, ConsoleKey.F6);
            AddKeyWithShift(65u, ConsoleKey.F7);
            AddKeyWithShift(66u, ConsoleKey.F8);
            AddKeyWithShift(67u, ConsoleKey.F9);
            AddKeyWithShift(68u, ConsoleKey.F10);
            AddKeyWithShift(87u, ConsoleKey.F11);
            AddKeyWithShift(88u, ConsoleKey.F12);
            AddKeyWithShift(1u, ConsoleKey.Escape);

            // ß ? \
            // -- Not yet implemented

            // Other keys
            AddKeyWithShift(76u, '5', ConsoleKey.NumPad5);
            #endregion
            ChangeKeyMap();
        }
    }
}
