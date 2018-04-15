using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.HAL;

namespace Cosmos.System
{
    public static class PCSExtensions
    {
        public static uint MsToHz(this int ms)
        {
            return (uint)(1000 / ms);
        }
        public static uint MsToHz(this uint ms)
        {
            return (uint)(1000 / ms);
        }
    }
    public enum Durations
    {
         Whole = 1600,
         Half = Whole / 2,
         Quarter = Half / 2,
         Eighth = Quarter / 2,
         Sixteenth = Eighth / 2,

         Default = 200,
    }
    public enum Notes
    {
         A0 = 28, // Exactly 27.500
         AS0 = 29,
         B0 = 31,
         C1 = 33,
         CS1 = 35,
         D1 = 37,
         DS1 = 39,
         E1 = 41,
         F1 = 44,
         FS1 = 46,
         G1 = 49,
         GS1 = 52,
         A1 = 55, // Exactly 55.000hz
         AS1 = 58,
         B1 = 62,
         C2 = 65,
         CS2 = 69,
         D2 = 73,
         DS2 = 78,
         E2 = 82,
         F2 = 87,
         FS2 = 92,
         G2 = 98,
         GS2 = 104,
         A2 = 110, // Exactly 110.000hz
         AS2 = 117,
         B2 = 123,
         C3 = 131,
         CS3 = 139,
         D3 = 147,
         DS3 = 156,
         E3 = 165,
         F3 = 175,
         FS3 = 185,
         G3 = 196,
         GS3 = 208,
         A3 = 220, // Exactly 220.000hz
         AS3 = 233,
         B3 = 247,
         C4 = 262,
         CS4 = 277,
         D4 = 294,
         DS4 = 311,
         E4 = 330,
         F4 = 349,
         FS4 = 370,
         G4 = 392,
         GS4 = 415,
         A4 = 440, // Exactly 440.000hz | Concert Pitch
         AS4 = 466,
         B4 = 494,
         C5 = 523,
         CS5 = 554,
         D5 = 587,
         DS5 = 622,
         E5 = 659,

         Default = 800,
    }
    public class PCSpeaker
    {
        public static void Beep()
        {
            Beep((uint) Notes.Default, (uint) Durations.Default);
        }
        public static void Beep(uint frequency)
        {
            Beep(frequency, (uint) Durations.Default);
        }
        public static void Beep(uint frequency, uint duration)
        {
            HAL.PCSpeaker.Beep(frequency, duration);
        }
        public static void Beep(Notes note, Durations duration)
        {
            Beep((uint)note, (uint) duration);
        }
    }
}
