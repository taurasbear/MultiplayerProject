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
            _configuration.Tempo = Math.Max(0.5f, Math.Min(2.0f, tempo));
            return this;
        }

        /// <summary>
        /// Set intensity level (0.0 = calm, 1.0 = maximum intensity)
        /// Affects volume and pitch
        /// </summary>
        public AudioBuilder WithIntensity(float intensity)
        {
            _configuration.Intensity = Math.Max(0.0f, Math.Min(1.0f, intensity));
            return this;
        }

        /// <summary>
        /// Enable reverb effect
        /// </summary>
        public AudioBuilder WithReverb(bool enable = true)
        {
            _configuration.EnableReverb = enable;
            return this;
        }

        /// <summary>
        /// Set the score threshold at which this configuration becomes active
        /// </summary>
        public AudioBuilder AtScoreThreshold(int score)
        {
            _configuration.ScoreThreshold = Math.Max(0, score);
            return this;
        }

        /// <summary>
        /// Configure audio based on current score
        /// Automatically adjusts volume, tempo, and intensity
        /// </summary>
        public AudioBuilder WithScoreBasedDynamics(int currentScore, int maxScore = 100)
        {
            float scoreRatio = Math.Min(1.0f, (float)currentScore / maxScore);
            
            // Volume increases from 0.5 to 1.0 as score increases
            WithVolume(0.5f + (scoreRatio * 0.5f));
            
            // Tempo increases from 1.0 to 1.5 as score increases
            WithTempo(1.0f + (scoreRatio * 0.5f));
            
            // Intensity matches score ratio
            WithIntensity(scoreRatio);
            
            // Enable reverb at high scores
            if (scoreRatio > 0.7f)
            {
                WithReverb(true);
            }
            
            return this;
        }

        /// <summary>
        /// Set callback for when audio completes
        /// </summary>
        public AudioBuilder OnComplete(Action callback)
        {
            _configuration.OnComplete = callback;
            return this;
        }

        /// <summary>
        /// Build and return the audio configuration
        /// </summary>
        public AudioConfiguration Build()
        {
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