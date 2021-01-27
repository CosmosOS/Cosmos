using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.HAL.Drivers.PCI.Audio.Generic
{
    public abstract class SoundCard
    {
        /// <summary>
        /// The volume of the sound card. In this format: 0xLF - L = Left, R = Right
        /// </summary>
        public abstract byte Volume { get; set; }
        /// <summary>
        /// Enables the sound card.
        /// </summary>
        public abstract void Enable();
        /// <summary>
        /// Disables the sound card.
        /// </summary>
        public abstract void Disable();
        /// <summary>
        /// Plays unsigned 8 bit pcm audio on the sound card.
        /// </summary>
        /// <param name="audio"></param>
        public abstract void PlaySound(byte[] audio);
        /// <summary>
        /// Stops all of the playing sound
        /// </summary>
        public abstract void StopSound();
        /// <summary>
        /// The name of the sound card
        /// </summary>
        public abstract string Name { get; }
    }
}
