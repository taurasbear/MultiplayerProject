using MultiplayerProject.Source.Helpers.Audio.Tiers;

namespace MultiplayerProject.Source.Helpers.Audio
{
    /// <summary>
    /// Director class that contains all the construction logic for audio configurations
    /// Uses tier data and AudioBuilder to construct AudioConfigurations
    /// Implements the Builder pattern's Director role
    /// </summary>
    public static class AudioDirector
    {
        /// <summary>
        /// Construct an audio configuration for the specified tier
        /// Contains the actual construction steps/recipe
        /// </summary>
        public static AudioConfiguration ConstructTier(int tier, string soundName)
        {
            var builder = AudioManager.Instance.CreateAudioBuilder();
            
            switch (tier)
            {
                case 0:
                    return ConstructCalmAudio(builder, soundName);
                case 1:
                    return ConstructTensionAudio(builder, soundName);
                case 2:
                    return ConstructActionAudio(builder, soundName);
                case 3:
                    return ConstructChaosAudio(builder, soundName);
                default:
                    Logger.Instance.Warning($"Unknown audio tier: {tier}, defaulting to calm");
                    return ConstructCalmAudio(builder, soundName);
            }
        }

        /// <summary>
        /// Construct audio configuration based on score
        /// Automatically determines the appropriate tier
        /// </summary>
        public static AudioConfiguration ConstructForScore(int score, string soundName)
        {
            int tier = GetTierForScore(score);
            return ConstructTier(tier, soundName);
        }

        /// <summary>
        /// Construction recipe for calm audio using CalmAudioTier data
        /// </summary>
        private static AudioConfiguration ConstructCalmAudio(AudioBuilder builder, string soundName)
        {
            return builder
                .WithSound(soundName)
                .WithVolume(CalmAudioTier.VOLUME)
                .WithPitch(CalmAudioTier.PITCH)
                .WithPan(CalmAudioTier.PAN)
                .WithTempo(CalmAudioTier.TEMPO)
                .WithIntensity(CalmAudioTier.INTENSITY)
                .WithReverb(CalmAudioTier.ENABLE_REVERB)
                .WithLooping(true)
                .AtScoreThreshold(CalmAudioTier.SCORE_THRESHOLD)
                .Build();
        }

        /// <summary>
        /// Construction recipe for tension audio using TensionAudioTier data
        /// </summary>
        private static AudioConfiguration ConstructTensionAudio(AudioBuilder builder, string soundName)
        {
            return builder
                .WithSound(soundName)
                .WithVolume(TensionAudioTier.VOLUME)
                .WithPitch(TensionAudioTier.PITCH)
                .WithPan(TensionAudioTier.PAN)
                .WithTempo(TensionAudioTier.TEMPO)
                .WithIntensity(TensionAudioTier.INTENSITY)
                .WithReverb(TensionAudioTier.ENABLE_REVERB)
                .WithLooping(true)
                .AtScoreThreshold(TensionAudioTier.SCORE_THRESHOLD)
                .Build();
        }

        /// <summary>
        /// Construction recipe for action audio using ActionAudioTier data
        /// </summary>
        private static AudioConfiguration ConstructActionAudio(AudioBuilder builder, string soundName)
        {
            return builder
                .WithSound(soundName)
                .WithVolume(ActionAudioTier.VOLUME)
                .WithPitch(ActionAudioTier.PITCH)
                .WithPan(ActionAudioTier.PAN)
                .WithTempo(ActionAudioTier.TEMPO)
                .WithIntensity(ActionAudioTier.INTENSITY)
                .WithReverb(ActionAudioTier.ENABLE_REVERB)
                .WithLooping(true)
                .AtScoreThreshold(ActionAudioTier.SCORE_THRESHOLD)
                .Build();
        }

        /// <summary>
        /// Construction recipe for chaos audio using ChaosAudioTier data
        /// </summary>
        private static AudioConfiguration ConstructChaosAudio(AudioBuilder builder, string soundName)
        {
            return builder
                .WithSound(soundName)
                .WithVolume(ChaosAudioTier.VOLUME)
                .WithPitch(ChaosAudioTier.PITCH)
                .WithPan(ChaosAudioTier.PAN)
                .WithTempo(ChaosAudioTier.TEMPO)
                .WithIntensity(ChaosAudioTier.INTENSITY)
                .WithReverb(ChaosAudioTier.ENABLE_REVERB)
                .WithLooping(true)
                .AtScoreThreshold(ChaosAudioTier.SCORE_THRESHOLD)
                .Build();
        }

        /// <summary>
        /// Get the appropriate tier for a given score
        /// </summary>
        private static int GetTierForScore(int score)
        {
            if (score >= ChaosAudioTier.SCORE_THRESHOLD) return 3;
            if (score >= ActionAudioTier.SCORE_THRESHOLD) return 2;
            if (score >= TensionAudioTier.SCORE_THRESHOLD) return 1;
            return 0;
        }
    }
}