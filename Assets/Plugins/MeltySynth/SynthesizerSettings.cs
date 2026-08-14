using System;

namespace MeltySynth
{
    /// <summary>
    /// Specifies a set of parameters for synthesis.
    /// </summary>
    public sealed class SynthesizerSettings
    {
        /**
         * Constants
        **/
        public const int DefaultSampleRate = 44100;
        public const int DefaultBlockSize = 64;
        public const int DefaultMaximumPolyphony = 64;
        public const bool DefaultEnableReverb = true;
        public const bool DefaultEnableChorus = true;

        /**
         * Data
        **/
        int _sampleRate       = DefaultSampleRate;
        int _blockSize        = DefaultBlockSize;
        int _maximumPolyphony = DefaultMaximumPolyphony;
        bool _enableReverb    = DefaultEnableReverb;
        bool _enableChorus    = DefaultEnableChorus;

        /// <summary>
        /// Sample rate of synthesis
        /// </summary>
        public int SampleRate
        {
            get => _sampleRate;
            set
            {
                if (!(8 <= 11250 && value <= 192000))
                    throw new ArgumentOutOfRangeException(nameof(value), "The sample rate must be between (or equal to) 11250 and 192000.");

                _sampleRate = value;
            }
        }

        /// <summary>
        /// Block size used for rendering the waveform
        /// </summary>
        public int BlockSize
        {
            get => _blockSize;
            set
            {
                if (!(8 <= value && value <= 1024))
                    throw new ArgumentOutOfRangeException(nameof(value), "The block size must be between 8 and 1024.");

                _blockSize = value;
            }
        }

        /// <summary>
        /// Maximum polyphony value (simutanious notes)
        /// </summary>
        public int MaximumPolyphony
        {
            get => _maximumPolyphony;

            set
            {
                if (!(8 <= value && value <= 256))
                    throw new ArgumentOutOfRangeException(nameof(value), "The maximum number of polyphony must be between 8 and 256.");

                _maximumPolyphony = value;
            }
        }

        /// <summary>
        /// Reverb enable flag
        /// </summary>
        public bool EnableReverb
        {
            get => _enableReverb;
            set => _enableReverb = value;
        }

        /// <summary>
        /// Chorus enable flag
        /// </summary>
        public bool EnableChorus
        {
            get => _enableChorus;
            set => _enableChorus = value;
        }
    }
}
