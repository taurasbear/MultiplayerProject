using Microsoft.Xna.Framework.Audio;
using System;

namespace MultiplayerProject.Source.Helpers.Audio
{
    /// <summary>
    /// Represents a fully configured audio instance with dynamic properties
    /// </summary>
    public class AudioConfiguration
    {
        public SoundEffect SoundEffect { get; internal set; }
        public float Volume { get; internal set; }
        public float Pitch { get; internal set; }
        public float Pan { get; internal set; }
        public bool IsLooped { get; internal set; }
        public float FadeInDuration { get; internal set; }
        public float FadeOutDuration { get; internal set; }
        public float DelayBeforePlay { get; internal set; }
        public Action OnComplete { get; internal set; }
        
        // Score-based dynamic properties
        public float Tempo { get; internal set; }
        public float Intensity { get; internal set; }
        public bool EnableReverb { get; internal set; }
        public int ScoreThreshold { get; internal set; }

        internal AudioConfiguration() { }

        /// <summary>
        /// Play the configured audio
        /// </summary>
        public SoundEffectInstance Play()
        {
            if (SoundEffect == null)
            {
                // Silently return null instead of throwing exception
                return null;
            }

            var instance = SoundEffect.CreateInstance();
            
            instance.Volume = Volume;
            instance.Pan = Pan;
            instance.IsLooped = IsLooped;
            
            float dynamicPitch = Pitch + (Tempo - 1.0f) + (Intensity * 0.2f);
            dynamicPitch = Math.Max(-1.0f, Math.Min(1.0f, dynamicPitch));
            instance.Pitch = dynamicPitch;

            instance.Play();
            return instance;
        }

        public string GetDescription()
        {
            return $"Volume: {Volume:F2}, Pitch: {Pitch:F2}, Tempo: {Tempo:F2}, Intensity: {Intensity:F2}, Score: {ScoreThreshold}";
        }
    }
}