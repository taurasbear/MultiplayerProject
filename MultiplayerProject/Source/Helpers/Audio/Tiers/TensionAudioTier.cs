using Microsoft.Xna.Framework.Audio;

namespace MultiplayerProject.Source.Helpers.Audio.Tiers
{
    /// <summary>
    /// Tier 2: Rising tension with spatial audio
    /// Characteristics: Medium volume, normal tempo, right pan, reverb enabled
    /// </summary>
    public static class TensionAudioTier
    {
        public const int SCORE_THRESHOLD = 3;
        public const float VOLUME = 0.5f;
        public const float PITCH = 0.0f;
        public const float PAN = 0.3f;  // RIGHT PAN - spatial effect
        public const float TEMPO = 1.0f;
        public const float INTENSITY = 0.25f;
        public const bool ENABLE_REVERB = true;  // REVERB ON

        /// <summary>
        /// Build the Tier 2 audio configuration
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