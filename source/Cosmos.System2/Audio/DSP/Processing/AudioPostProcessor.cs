using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.HAL.Audio;

namespace Cosmos.System.Audio.DSP.Processing
{
    /// <summary>
    /// The <see cref="AudioPostProcessor"/> class allows for post-processing
    /// of audio buffers.
    /// </summary>
    public abstract class AudioPostProcessor
    {
        /// <summary>
        /// Processes the given audio buffer.
        /// </summary>
        /// <param name="buffer">The buffer to process.</param>
        public abstract void Process(AudioBuffer buffer);
    }
}
