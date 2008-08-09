using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DokuTest
{
    public class ES1370
    {
        public static void test()
        {
            Console.WriteLine("Founding any ES1370 audio cards");
            var xAudioCards = Cosmos.Hardware.Audio.Devices.ES1370.ES1370.FindAll();
            if(xAudioCards!=null)
                Console.WriteLine("Found one!");
            var xAudioCard = xAudioCards[0];
            Console.WriteLine("Enabling first audio card...");
            xAudioCard.Enable();
            xAudioCard.InitializeDriver();
            xAudioCard.DumpRegisters();
            Console.WriteLine("Test sine waves");
            //Cosmos.Hardware.Audio.PCMStream sineWave = SampleSounds.SoundSamples.generateSineWaveForm(440, 44100, 0,10000);
            //xAudioCard.playStream(sineWave);
            Console.WriteLine("Disabling first audio card...");
            xAudioCard.Disable();
            xAudioCard.DumpRegisters();
        }
    }
}
