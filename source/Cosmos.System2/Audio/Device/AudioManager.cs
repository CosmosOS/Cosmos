using Cosmos.HAL;
using Cosmos.HAL.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Audio
{
    /// <summary>
    /// Responsible for interfacing with a single audio driver.
    /// </summary>
    public class AudioManager : IAudioBufferProvider {
        bool enabled;
        AudioDriver output = null;

        /// <summary>
        /// A value indicating whether this audio manager is enabled.
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (!enabled && value) {
                    Enable();
                }
                else if (enabled && !value) {
                    Disable();
                }
            }
        }

        /// <summary>
        /// The audio driver to supply the output audio buffer to.
        /// </summary>
        public AudioDriver Output {
            get => output;
            set {

                if (output != null)
                {
                    output.BufferProvider = null;
                    output.Disable();
                }

                output = value;

                if (output != null)
                {
                    output.BufferProvider = this;

                    if (Enabled) {
                        output.Enable();
                    }
                }
            }
        }

        /// <summary>
        /// The audio stream to use when reading to the buffer.
        /// </summary>
        public AudioStream Stream { get; set; }

        /// <summary>
        /// Enables this audio manager.
        /// </summary>
        public void Enable()
        {
            Output?.Enable();
            enabled = true;
        }

        /// <summary>
        /// Disables this audio manager.
        /// </summary>
        public void Disable()
        {
            Output?.Disable();
            enabled = false;
        }

        public void RequestBuffer(AudioBuffer buffer)
        {
            if (Stream != null) {
                Stream.Read(buffer);
            }
            else {
                // No stream defined - flush the buffer so we get silence
                buffer.Flush();
            }
        }
    }
}
