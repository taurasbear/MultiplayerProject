using Microsoft.Xna.Framework.Audio;

namespace MultiplayerProject.Source.Helpers.Audio.Tiers
{
    /// <summary>
    /// Tier 1: Calm ambient atmosphere
    /// Characteristics: Low volume, slow tempo, low pitch, no reverb
    /// </summary>
    public static class CalmAudioTier
    {
        public const int SCORE_THRESHOLD = 0;
        public const float VOLUME = 0.3f;
        public const float PITCH = -0.2f;
        public const float PAN = 0.0f;
        public const float TEMPO = 0.8f;
        public const float INTENSITY = 0.0f;
        public const bool ENABLE_REVERB = false;

        /// <summary>
        /// Build the Tier 1 audio configuration
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