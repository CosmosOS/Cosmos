using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using System.Threading;

namespace BeepDemo
{
    public class Kernel: Sys.Kernel
    {
		protected override void BeforeRun()
		{
			Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");	
		}
        protected override void Run()
        {
			Console.WriteLine("Run 'Mary Had a Little Lamb'? ");
			string ans = Console.ReadLine();
			if (ans.ToLower() == "y" || ans.ToLower() == "yes")
			{
				BeepTest.Main();
			}
			else
			{
				Console.WriteLine("Default beep:");
				Console.Beep();
				// Does the follwing: Console.Beep((int)Sys.Notes.Default (800 hertz), (int)Sys.Durations.Default (200 milliseconds) );
			}
		}
    }
    class BeepTest
    {
        public static void Main()
        {
            // Declare the first few notes of the song, "Mary Had A Little Lamb".
            Note[] Mary =
            {
                new Note(Tone.B, Duration.QUARTER),
                new Note(Tone.A, Duration.QUARTER),
                new Note(Tone.GbelowC, Duration.QUARTER),
                new Note(Tone.A, Duration.QUARTER),
                new Note(Tone.B, Duration.QUARTER),
                new Note(Tone.B, Duration.QUARTER),
                new Note(Tone.B, Duration.HALF),
                new Note(Tone.A, Duration.QUARTER),
                new Note(Tone.A, Duration.QUARTER),
                new Note(Tone.A, Duration.HALF),
                new Note(Tone.B, Duration.QUARTER),
                new Note(Tone.D, Duration.QUARTER),
                new Note(Tone.D, Duration.HALF)
            };
            // Play the song
            Play(Mary);
        }

        // Play the notes in a song.
        protected static void Play(Note[] aTune)
        {
            foreach (Note n in aTune)
            {
                if (n.NoteTone == Tone.REST)
                {
                    Thread.Sleep((int)n.NoteDuration);
                }
                else
                {
                    Console.Beep((int)n.NoteTone, (int)n.NoteDuration);
                }
            }
        }

        // Define the frequencies of notes in an octave, as well as 
        // silence (rest).
        protected enum Tone
        {
            REST = 0,
            GbelowC = 196,
            A = 220,
            Asharp = 233,
            B = 247,
            C = 262,
            Csharp = 277,
            D = 294,
            Dsharp = 311,
            E = 330,
            F = 349,
            Fsharp = 370,
            G = 392,
            Gsharp = 415,
        }

        // Define the duration of a note in units of milliseconds.
        protected enum Duration
        {
            WHOLE = 1600,
            HALF = WHOLE / 2,
            QUARTER = HALF / 2,
            EIGHTH = QUARTER / 2,
            SIXTEENTH = EIGHTH / 2,
        }

        // Define a note as a aFrequency (tone) and the amount of 
        // aTime (duration) the note plays.
        protected readonly struct Note
        {
            readonly Tone _ToneVal;
            readonly Duration _DurVal;

            // Define a constructor to create a specific note.
            public Note(Tone aFrequency, Duration aTime)
            {
                _ToneVal = aFrequency;
                _DurVal = aTime;
            }

            // Define properties to return the note's tone and duration.
            public Tone NoteTone => _ToneVal;
            public Duration NoteDuration => _DurVal;
        }
    }
    /*
	This example produces the following results:

	This example plays the first few notes of "Mary Had A Little Lamb" 
	through the computer PC Speaker.
	*/
}
