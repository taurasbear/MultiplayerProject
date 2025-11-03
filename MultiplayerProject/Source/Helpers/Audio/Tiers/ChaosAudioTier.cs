using Microsoft.Xna.Framework.Audio;

namespace MultiplayerProject.Source.Helpers.Audio.Tiers
{
    /// <summary>
    /// Tier 4: Maximum chaos with rapid tempo
    /// Characteristics: Max volume, very fast tempo, highest pitch, centered for full stereo blast
    /// </summary>
    public static class ChaosAudioTier
    {
        public const int SCORE_THRESHOLD = 9;
        public const float VOLUME = 1.0f;   // MAXIMUM
        public const float PITCH = 0.5f;    // HIGHEST PITCH
        public const float PAN = 0.0f;      // CENTERED - full stereo
        public const float TEMPO = 1.6f;    // VERY FAST
        public const float INTENSITY = 1.0f; // MAXIMUM
        public const bool ENABLE_REVERB = true;

        /// <summary>
        /// Build the Tier 4 audio configuration
        /// </summary>
        public static AudioConfiguration Build(string soundName)
        {
            return AudioManager.Instance.CreateAudioBuilder()
                .WithSound(soundName)
                .WithVolume(VOLUME)
                .WithPitch(PITCH)
                .WithPan(PAN)
                .WithTempo(TEMPO)
                .WithIntensity(INTENSITY)
                .WithReverb(ENABLE_REVERB)
                .WithLooping(true)
                .AtScoreThreshold(SCORE_THRESHOLD)
                .Build();
        }
    }
}