using Microsoft.Xna.Framework.Audio;

namespace MultiplayerProject.Source.Helpers.Audio.Tiers
{
    /// <summary>
    /// Tier 3: High action with pitch variation
    /// Characteristics: High volume, faster tempo, high pitch, left pan (opposite of tier 2)
    /// Contains only configuration data - no construction logic
    /// </summary>
    public static class ActionAudioTier
    {
        public const int SCORE_THRESHOLD = 6;
        public const float VOLUME = 0.75f;
        public const float PITCH = 0.3f;  // HIGH PITCH
        public const float PAN = -0.3f;   // LEFT PAN (opposite of tier 2)
        public const float TEMPO = 1.3f;  // FASTER
        public const float INTENSITY = 0.6f;
        public const bool ENABLE_REVERB = true;
    }
}