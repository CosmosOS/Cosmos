using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.HAL.Audio
{
    /// <summary>
    /// Represents an object that can provide audio buffers.
    /// </summary>
    public interface IAudioBufferProvider
    {
        /// <summary>
        /// Requests an audio buffer.
        /// </summary>
        public void RequestBuffer(
            AudioBuffer buffer
        );
    }
}
