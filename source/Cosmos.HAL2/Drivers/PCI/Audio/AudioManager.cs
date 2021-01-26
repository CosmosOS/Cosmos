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

            var sb16 = new SoundBlaster16();
            sb16.Enable();
            if (sb16.IsPresent)
            {
                devices.Add(sb16);
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
        /// Play unsigned 8-bit PCM on specifed sound card.
        /// </summary>
        /// <param name="audio"></param>
        public static void PlayAudio(byte[] audio, int SoundCardIndex)
        {
            if (devices.Count == 0)
            {
                throw new InvalidOperationException("No sound cards are installed!");
            }
            if (SoundCardIndex > devices.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            devices[SoundCardIndex].PlaySound(audio);
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
        /// Stops sound on specifed sound card.
        /// </summary>
        public static void StopSound(int SoundCardIndex)
        {
            if (devices.Count == 0)
            {
                throw new InvalidOperationException("No sound cards are installed!");
            }
            if (SoundCardIndex > devices.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            devices[SoundCardIndex].StopSound();
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
