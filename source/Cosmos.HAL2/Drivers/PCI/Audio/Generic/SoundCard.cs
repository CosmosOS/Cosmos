using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.HAL.Drivers.PCI.Audio.Generic
{
    public abstract class SoundCard
    {
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
    }
}
