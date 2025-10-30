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

        // Audio progression tiers - ORIGINAL VALUES
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
                // Use AudioDirector to construct each tier
                for (int tier = 0; tier < 4; tier++)
                {
                    var config = AudioDirector.ConstructTier(tier, "backgroundMusic");
                    if (config.SoundEffect != null) 
                    {
                        _musicProgression.Add(config);
                    }
                }

                Logger.Instance.Info($"Built {_musicProgression.Count} music progression tiers using AudioDirector");
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
            {
                if (AudioManager.Instance.IsInitialized)
                {
                    BuildMusicProgression();
                }
                return;
            }

            // Remove the score difference check - check every update
            // if (Math.Abs(currentScore - _previousScore) < 2)
            //     return;

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
        /// Transition to a new music tier - ACTUALLY CHANGE THE MUSIC
        /// </summary>
        private void TransitionToTier(int tier)
        {
            // Stop current music cleanly
            if (_currentBackgroundMusic != null)
            {
                _currentBackgroundMusic.Stop(true);
                _currentBackgroundMusic.Dispose();
                _currentBackgroundMusic = null;
            }
            
            // Actually apply the new tier configuration
            if (tier < _musicProgression.Count && _musicProgression[tier] != null)
            {
                try
                {
                    // Play the new configuration with tier-specific settings
                    _currentBackgroundMusic = _musicProgression[tier].Play();
                    if (_currentBackgroundMusic != null)
                    {
                        Logger.Instance.Info($"Successfully transitioned to audio tier {tier} with new settings");
                    }
                    else
                    {
                        Logger.Instance.Warning($"Failed to create sound instance for tier {tier}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"Error transitioning to tier {tier}: {ex.Message}");
                }
            }
            else
            {
                Logger.Instance.Warning($"No audio configuration available for tier {tier}");
            }
        }

        /// <summary>
        /// Play laser sound with score-based intensity
        /// </summary>
        public void PlayLaserSound(int currentScore)
        {
            // Use simple AudioManager instead of complex builder pattern for laser sounds
            AudioManager.Instance.PlayLaserSound();
        }

        /// <summary>
        /// Play explosion sound with score-based intensity
        /// </summary>
        public void PlayExplosionSound(int currentScore)
        {
            // Use simple AudioManager instead of complex builder pattern for explosion sounds
            AudioManager.Instance.PlayExplosionSound();
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