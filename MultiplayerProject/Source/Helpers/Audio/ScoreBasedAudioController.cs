using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using MultiplayerProject.Source.Helpers.Audio.Tiers;

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

            try
            {
                // Use tier classes to build configurations
                var tier1 = CalmAudioTier.Build("backgroundMusic");
                if (tier1.SoundEffect != null) _musicProgression.Add(tier1);

                var tier2 = TensionAudioTier.Build("backgroundMusic");
                if (tier2.SoundEffect != null) _musicProgression.Add(tier2);

                var tier3 = ActionAudioTier.Build("backgroundMusic");
                if (tier3.SoundEffect != null) _musicProgression.Add(tier3);

                var tier4 = ChaosAudioTier.Build("backgroundMusic");
                if (tier4.SoundEffect != null) _musicProgression.Add(tier4);

                Logger.Instance.Info($"Built {_musicProgression.Count} music progression tiers using tier classes");
                _isInitialized = _musicProgression.Count > 0;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to build music progression: {ex.Message}");
                _isInitialized = false;
            }
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
            // Stop current music
            if (_currentBackgroundMusic != null)
            {
                _currentBackgroundMusic.Stop(true);
                _currentBackgroundMusic.Dispose();
                _currentBackgroundMusic = null;
            }
            
            AudioManager.Instance.StopBackgroundMusic();
            
            // Play the new tier
            if (tier >= 0 && tier < _musicProgression.Count)
            {
                _currentBackgroundMusic = _musicProgression[tier].Play();
                Logger.Instance.Info($"Transitioned to music tier {tier}");
            }
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