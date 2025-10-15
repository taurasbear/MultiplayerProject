using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace MultiplayerProject.Source.Helpers.Audio
{
    /// <summary>
    /// Controls dynamic audio changes based on game score
    /// Uses Builder pattern to construct appropriate audio configurations
    /// </summary>
    public class ScoreBasedAudioController
    {
        private SoundEffectInstance _currentBackgroundMusic;
        private List<AudioConfiguration> _musicProgression;
        private int _currentTier;
        private int _previousScore;
        private bool _isInitialized;

        // Audio progression tiers
        private const int TIER_1_SCORE = 0;   // Calm
        private const int TIER_2_SCORE = 3;   // Building tension
        private const int TIER_3_SCORE = 6;   // High intensity
        private const int TIER_4_SCORE = 9;   // Maximum intensity

        public ScoreBasedAudioController()
        {
            _musicProgression = new List<AudioConfiguration>();
            _currentTier = 0;
            _previousScore = 0;
            _isInitialized = false;
        }

        /// <summary>
        /// Build the progression of music configurations using the Builder pattern
        /// This should be called AFTER AudioManager is initialized
        /// </summary>
        private void BuildMusicProgression()
        {
            if (!AudioManager.Instance.IsInitialized)
            {
                Logger.Instance.Warning("Cannot build music progression - AudioManager not initialized");
                return;
            }

            _musicProgression.Clear();

            // Tier 1: Calm, base music
            var tier1 = AudioManager.Instance.CreateAudioBuilder()
                .WithSound("backgroundMusic")
                .WithVolume(0.4f)
                .WithTempo(1.0f)
                .WithIntensity(0.0f)
                .WithLooping(true)
                .AtScoreThreshold(TIER_1_SCORE)
                .Build();
            _musicProgression.Add(tier1);

            // Tier 2: Building tension
            var tier2 = AudioManager.Instance.CreateAudioBuilder()
                .WithSound("backgroundMusic")
                .WithVolume(0.6f)
                .WithTempo(1.2f)
                .WithIntensity(0.33f)
                .WithLooping(true)
                .AtScoreThreshold(TIER_2_SCORE)
                .Build();
            _musicProgression.Add(tier2);

            // Tier 3: High intensity
            var tier3 = AudioManager.Instance.CreateAudioBuilder()
                .WithSound("backgroundMusic")
                .WithVolume(0.8f)
                .WithTempo(1.4f)
                .WithIntensity(0.66f)
                .WithReverb(true)
                .WithLooping(true)
                .AtScoreThreshold(TIER_3_SCORE)
                .Build();
            _musicProgression.Add(tier3);

            // Tier 4: Maximum intensity
            var tier4 = AudioManager.Instance.CreateAudioBuilder()
                .WithSound("backgroundMusic")
                .WithVolume(1.0f)
                .WithTempo(1.5f)
                .WithIntensity(1.0f)
                .WithReverb(true)
                .WithLooping(true)
                .AtScoreThreshold(TIER_4_SCORE)
                .Build();
            _musicProgression.Add(tier4);

            Logger.Instance.Info($"Built {_musicProgression.Count} music progression tiers");
            _isInitialized = true;
        }

        /// <summary>
        /// Update audio based on current score
        /// </summary>
        public void Update(int currentScore)
        {
            if (!_isInitialized)
                return;

            if (currentScore == _previousScore)
                return;

            // Determine which tier we should be in
            int newTier = GetTierForScore(currentScore);

            if (newTier != _currentTier)
            {
                Logger.Instance.Info($"Score changed from {_previousScore} to {currentScore}, transitioning from tier {_currentTier} to tier {newTier}");
                TransitionToTier(newTier);
                _currentTier = newTier;
            }

            _previousScore = currentScore;
        }

        /// <summary>
        /// Get the appropriate tier for a given score
        /// </summary>
        private int GetTierForScore(int score)
        {
            if (score >= TIER_4_SCORE) return 3;
            if (score >= TIER_3_SCORE) return 2;
            if (score >= TIER_2_SCORE) return 1;
            return 0;
        }

        /// <summary>
        /// Transition to a new music tier
        /// </summary>
        private void TransitionToTier(int tier)
        {
            Logger.Instance.Info($"Would transition to tier {tier} (feature disabled - using main background music)");
        }

        /// <summary>
        /// Play laser sound with score-based intensity
        /// </summary>
        public void PlayLaserSound(int currentScore)
        {
            if (!AudioManager.Instance.IsInitialized)
                return;

            AudioManager.Instance.CreateAudioBuilder()
                .WithSound("laser")
                .WithScoreBasedDynamics(currentScore, 10)
                .BuildAndPlay();
        }

        /// <summary>
        /// Play explosion sound with score-based intensity
        /// </summary>
        public void PlayExplosionSound(int currentScore)
        {
            if (!AudioManager.Instance.IsInitialized)
                return;

            AudioManager.Instance.CreateAudioBuilder()
                .WithSound("explosion")
                .WithScoreBasedDynamics(currentScore, 10)
                .WithReverb(currentScore >= TIER_3_SCORE)
                .BuildAndPlay();
        }

        /// <summary>
        /// Start the audio controller
        /// </summary>
        public void Start()
        {
            if (!AudioManager.Instance.IsInitialized)
            {
                Logger.Instance.Warning("Cannot start ScoreBasedAudioController - AudioManager not initialized");
                return;
            }

            BuildMusicProgression();
            
            Logger.Instance.Info("ScoreBasedAudioController started (using main background music)");
        }

        /// <summary>
        /// Stop all audio
        /// </summary>
        public void Stop()
        {
            if (_currentBackgroundMusic != null)
            {
                _currentBackgroundMusic.Stop(true);
                _currentBackgroundMusic.Dispose();
                _currentBackgroundMusic = null;
            }
        }
    }
}