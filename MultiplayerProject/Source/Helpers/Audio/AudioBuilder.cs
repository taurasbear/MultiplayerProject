using Microsoft.Xna.Framework.Audio;
using System;

namespace MultiplayerProject.Source.Helpers.Audio
{
    /// <summary>
    /// Builder for creating complex audio configurations with score-based dynamics
    /// </summary>
    public class AudioBuilder
    {
        private readonly AudioConfiguration _configuration;

        public AudioBuilder()
        {
            _configuration = new AudioConfiguration
            {
                Volume = 1.0f,
                Pitch = 0.0f,
                Pan = 0.0f,
                IsLooped = false,
                FadeInDuration = 0.0f,
                FadeOutDuration = 0.0f,
                DelayBeforePlay = 0.0f,
                Tempo = 1.0f,
                Intensity = 0.0f,
                EnableReverb = false,
                ScoreThreshold = 0
            };
        }

        /// <summary>
        /// Set the sound effect to play
        /// </summary>
        public AudioBuilder WithSound(SoundEffect soundEffect)
        {
            _configuration.SoundEffect = soundEffect;
            return this;
        }

        /// <summary>
        /// Set the sound effect by name (looks up from AudioManager)
        /// </summary>
        public AudioBuilder WithSound(string soundName)
        {
            if (AudioManager.Instance.IsInitialized && 
                AudioManager.Instance.TryGetSoundEffect(soundName, out var soundEffect))
            {
                _configuration.SoundEffect = soundEffect;
            }
            // Silently fail if sound not found - don't log warning
            return this;
        }

        /// <summary>
        /// Set the volume (0.0 to 1.0)
        /// </summary>
        public AudioBuilder WithVolume(float volume)
        {
            _configuration.Volume = Math.Max(0.0f, Math.Min(1.0f, volume));
            return this;
        }

        /// <summary>
        /// Set the pitch (-1.0 to 1.0)
        /// </summary>
        public AudioBuilder WithPitch(float pitch)
        {
            _configuration.Pitch = Math.Max(-1.0f, Math.Min(1.0f, pitch));
            return this;
        }

        /// <summary>
        /// Set the pan (-1.0 left to 1.0 right)
        /// </summary>
        public AudioBuilder WithPan(float pan)
        {
            _configuration.Pan = Math.Max(-1.0f, Math.Min(1.0f, pan));
            return this;
        }

        /// <summary>
        /// Enable looping
        /// </summary>
        public AudioBuilder WithLooping(bool loop = true)
        {
            _configuration.IsLooped = loop;
            return this;
        }

        /// <summary>
        /// Add fade-in effect (duration in seconds)
        /// </summary>
        public AudioBuilder WithFadeIn(float duration)
        {
            _configuration.FadeInDuration = Math.Max(0.0f, duration);
            return this;
        }

        /// <summary>
        /// Add fade-out effect (duration in seconds)
        /// </summary>
        public AudioBuilder WithFadeOut(float duration)
        {
            _configuration.FadeOutDuration = Math.Max(0.0f, duration);
            return this;
        }

        /// <summary>
        /// Add delay before playing (in seconds)
        /// </summary>
        public AudioBuilder WithDelay(float delay)
        {
            _configuration.DelayBeforePlay = Math.Max(0.0f, delay);
            return this;
        }

        /// <summary>
        /// Set tempo/speed multiplier (0.5 = half speed, 2.0 = double speed)
        /// </summary>
        public AudioBuilder WithTempo(float tempo)
        {
            _configuration.Tempo = Math.Max(0.1f, Math.Min(2.0f, tempo));
            return this;
        }

        /// <summary>
        /// Set the intensity level (0.0 to 1.0)
        /// </summary>
        public AudioBuilder WithIntensity(float intensity)
        {
            _configuration.Intensity = Math.Max(0.0f, Math.Min(1.0f, intensity));
            return this;
        }

        /// <summary>
        /// Enable or disable reverb effect
        /// </summary>
        public AudioBuilder WithReverb(bool enableReverb)
        {
            _configuration.EnableReverb = enableReverb;
            return this;
        }

        /// <summary>
        /// Set the score threshold for this configuration
        /// </summary>
        public AudioBuilder AtScoreThreshold(int threshold)
        {
            _configuration.ScoreThreshold = threshold;
            return this;
        }

        /// <summary>
        /// Build the final audio configuration
        /// </summary>
        public AudioConfiguration Build()
        {
            // Validate that we have a sound effect
            if (_configuration.SoundEffect == null)
            {
                Logger.Instance.Warning("AudioBuilder: No sound effect set, cannot build audio configuration");
                return null;
            }

            // Validate volume range
            if (_configuration.Volume < 0.0f || _configuration.Volume > 1.0f)
            {
                Logger.Instance.Warning($"AudioBuilder: Volume out of range ({_configuration.Volume}), clamping to valid range");
                _configuration.Volume = Math.Max(0.0f, Math.Min(1.0f, _configuration.Volume));
            }

            // Validate pitch range
            if (_configuration.Pitch < -1.0f || _configuration.Pitch > 1.0f)
            {
                Logger.Instance.Warning($"AudioBuilder: Pitch out of range ({_configuration.Pitch}), clamping to valid range");
                _configuration.Pitch = Math.Max(-1.0f, Math.Min(1.0f, _configuration.Pitch));
            }

            // Validate pan range
            if (_configuration.Pan < -1.0f || _configuration.Pan > 1.0f)
            {
                Logger.Instance.Warning($"AudioBuilder: Pan out of range ({_configuration.Pan}), clamping to valid range");
                _configuration.Pan = Math.Max(-1.0f, Math.Min(1.0f, _configuration.Pan));
            }

            // Validate tempo range
            if (_configuration.Tempo < 0.1f || _configuration.Tempo > 2.0f)
            {
                Logger.Instance.Warning($"AudioBuilder: Tempo out of range ({_configuration.Tempo}), clamping to valid range");
                _configuration.Tempo = Math.Max(0.1f, Math.Min(2.0f, _configuration.Tempo));
            }

            // Validate intensity range
            if (_configuration.Intensity < 0.0f || _configuration.Intensity > 1.0f)
            {
                Logger.Instance.Warning($"AudioBuilder: Intensity out of range ({_configuration.Intensity}), clamping to valid range");
                _configuration.Intensity = Math.Max(0.0f, Math.Min(1.0f, _configuration.Intensity));
            }

            Logger.Instance.Debug($"Built audio configuration successfully: Volume={_configuration.Volume:F2}, Pitch={_configuration.Pitch:F2}");
            return _configuration;
        }

        /// <summary>
        /// Build and immediately play the audio
        /// </summary>
        public SoundEffectInstance BuildAndPlay()
        {
            if (_configuration.SoundEffect == null)
            {
                // Silently return null if no sound effect was set
                return null;
            }
            return _configuration.Play();
        }
    }
}