using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.HAL;

namespace Cosmos.System
{
    /// <summary>
    /// PC speaker helper class.
    /// </summary>
    public static class PCSExtensions
    {
        /// <summary>
        /// Convert milliseconds (ms) to hertz (Hz).
        /// </summary>
        /// <param name="ms">A milliseconds value, must be > 0.</param>
        /// <returns>integer value.</returns>
        public static uint MsToHz(this int ms)
        {
            return (uint)(1000 / ms); //TODO: maybe throw exception on <= 0 ms value
        }

        /// <summary>
        /// Convert milliseconds (ms) to hertz (Hz).
        /// </summary>
        /// <param name="ms">A milliseconds value, must be > 0.</param>
        /// <returns>integer value.</returns>
        public static uint MsToHz(this uint ms)
        {
            return (uint)(1000 / ms);
        }
    }

    /// <summary>
    /// Possible duration types.
    /// <seealso href="https://en.wikipedia.org/wiki/Duration_(music)"/>
    /// </summary>
    public enum Durations
    {
         Whole = 1600,
         Half = Whole / 2,
         Quarter = Half / 2,
         Eighth = Quarter / 2,
         Sixteenth = Eighth / 2,

         Default = 200,
    }

    /// <summary>
    /// Possible note types.
    /// <seealso href="https://en.wikipedia.org/wiki/Musical_note"/>
    /// <seealso href="https://en.wikipedia.org/wiki/Audio_frequency"/>
    /// </summary>
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
        F5 = 698,
        FS5 = 739,
        G5 = 783,
        GS5 = 830,
        A5 = 880, // Exactly 880.000hz
        AS5 = 932,
        B5 = 987,

        C6 = 1046,
        CS6 = 1108,
        D6 = 1174,
        DS6 = 1244,
        E6 = 1318,
        F6 = 1396,
        FS6 = 1479,
        G6 = 1567,
        GS6 = 1661,
        A6 = 1760, // Exactly 1760.000hz
        AS6 = 1864,
        B6 = 1975,

        Default = 800,
    }

    // TODO: continue exception list, once HAL is documented.
    /// <summary>
    /// PC speaker class.
    /// </summary>
    public class PCSpeaker
    {
        // TODO: continue exception list, once HAL is documented.
        /// <summary>
        /// Play beep sound, at 800hz for one eighth.
        /// </summary>
        public static void Beep()
        {
            Beep((uint) Notes.Default, (uint) Durations.Default);
        }

        // TODO: continue exception list, once HAL is documented.
        /// <summary>
        /// Play beep sound, at a specified frequency for one eighth.
        /// </summary>
        /// <param name="frequency">Audio frequency in Hz, must be between 37 and 32767Hz.</param>
        public static void Beep(uint frequency)
        {
            Beep(frequency, (uint) Durations.Default);
        }

        // TODO: continue exception list, once HAL is documented.
        /// <summary>
        /// Play beep sound, at a specified frequency for a specified duration.
        /// </summary>
        /// <param name="frequency">Audio frequency in Hz, must be between 37 and 32767Hz.</param>
        /// <param name="duration">Beep duration, must be > 0.</param>
        public static void Beep(uint frequency, uint duration)
        {
            HAL.PCSpeaker.Beep(frequency, duration);
        }

        // TODO: continue exception list, once HAL is documented.
        /// <summary>
        /// Play beep sound, at a specified note for a specified duration.
        /// </summary>
        /// <param name="note">A note to play.</param>
        /// <param name="duration">Beep duration, must be > 0.</param>
        public static void Beep(Notes note, Durations duration)
        {
            Beep((uint)note, (uint) duration);
        }

        // TODO: continue exception list, once HAL is documented.
        /// <summary>
        /// Play beep sound, at a specified frequency for one eighth.
        /// </summary>
        /// <param name="note">A note to play.</param>
        public static void Beep(Notes note)
        {
            Beep((uint)note, (uint) Durations.Default);
        }
    }
}
