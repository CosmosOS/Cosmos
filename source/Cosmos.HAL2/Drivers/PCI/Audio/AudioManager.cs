using Cosmos.HAL.Drivers;
using Cosmos.HAL.Drivers.PCI.Audio.Generic;
using System;
using System.Collections.Generic;

namespace Cosmos.HAL.Drivers.PCI.Audio
{
    /// <summary>
    /// Audio mananger class. Right now, it only supports sound blaster 16.
    /// </summary>
    public class AudioManager
    {
        public static List<SoundCard> devices = new List<SoundCard>();

        public static void Init()
        {
            devices.Clear(); //just in case
            try
            {
                Console.WriteLine("Searching for sound cards");

                var sb16 = new SoundBlaster16();
                sb16.Enable();
                if (sb16.IsPresent)
                {
                    Console.WriteLine("Found Sound blaster 16!");
                    devices.Add(sb16);
                }

                if (devices.Count == 0)
                {
                    Console.WriteLine("No supported sound cards sound.");
                }
                else
                {
                    Console.WriteLine("Sound card(s) initialized successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while initializing sound: " + ex.Message);
            }
        }
        /// <summary>
        /// Play unsigned 8-bit PCM on first Sound card
        /// </summary>
        /// <param name="audio"></param>
        public static void PlayAudio(byte[] audio)
        {
            if (devices.Count == 0)
            {
                throw new InvalidOperationException("No sound cards are installed!");
            }
            devices[0].PlaySound(audio);
        }
        /// <summary>
        /// Stops all sound on all sound cards
        /// </summary>
        public static void StopAllSound()
        {
            foreach (var item in devices)
            {
                item.StopSound();
            }
        }
        /// <summary>
        /// Disables all sound cards. Should be called on system shutdown or reboot.
        /// </summary>
        public static void DisableAll()
        {
            foreach (var item in devices)
            {
                item.StopSound();
                item.Disable();
            }
        }
    }
}
