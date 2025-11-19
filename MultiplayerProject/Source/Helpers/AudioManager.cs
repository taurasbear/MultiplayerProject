using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using MultiplayerProject.Source.Helpers.Audio;

namespace MultiplayerProject.Source.Helpers
{
    /// <summary>
    /// Singleton AudioManager that handles background music and sound effects across the entire application
    /// </summary>
    public class AudioManager : IDisposable
    {
        #region Singleton Implementation
        private static AudioManager instance = null;
        private static readonly object padlock = new object();

        private AudioManager()
        {
            _soundEffects = new Dictionary<string, SoundEffect>();
            _isInitialized = false;
            
            // Add process exit handler to ensure cleanup on unexpected termination
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
        }

        /// <summary>
        /// Event handler for process exit
        /// </summary>
        private static void OnProcessExit(object sender, EventArgs e)
        {
            ForceCleanup();
        }

        /// <summary>
        /// Event handler for domain unload
        /// </summary>
        private static void OnDomainUnload(object sender, EventArgs e)
        {
            ForceCleanup();
        }

        /// <summary>
        /// Finalizer to ensure audio cleanup even if Dispose isn't called
        /// </summary>
        ~AudioManager()
        {
            try
            {
                if (_backgroundMusicInstance != null && _backgroundMusicInstance.State == SoundState.Playing)
                {
                    _backgroundMusicInstance.Stop(true); // Immediate stop
                    _backgroundMusicInstance.Dispose();
                }
                if (_backgroundMusicEffect != null)
                {
                    _backgroundMusicEffect.Dispose();
                }
            }
            catch
            {
                // Ignore exceptions in finalizer
            }
        }

        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new AudioManager();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion

        #region Private Fields
        private Song _backgroundMusic;
        private SoundEffect _backgroundMusicEffect; // Alternative approach for background music
        private SoundEffectInstance _backgroundMusicInstance;
        private Dictionary<string, SoundEffect> _soundEffects;
        private bool _isInitialized;
        private bool _isMusicEnabled = true;
        private bool _areSoundEffectsEnabled = true;
        private float _musicVolume = 0.5f;
        private float _soundEffectVolume = 0.7f;
        private bool _isMusicPlaying = false; // Track music state manually
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets whether background music is enabled
        /// </summary>
        public bool IsMusicEnabled
        {
            get { return _isMusicEnabled; }
            set
            {
                _isMusicEnabled = value;
                if (!_isMusicEnabled)
                {
                    StopBackgroundMusic();
                }
                else if (_backgroundMusic != null || _backgroundMusicEffect != null)
                {
                    PlayBackgroundMusic();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether sound effects are enabled
        /// </summary>
        public bool AreSoundEffectsEnabled
        {
            get { return _areSoundEffectsEnabled; }
            set { _areSoundEffectsEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the background music volume (0.0 to 1.0)
        /// </summary>
        public float MusicVolume
        {
            get { return _musicVolume; }
            set
            {
                _musicVolume = Math.Max(0.0f, Math.Min(1.0f, value));
                MediaPlayer.Volume = _musicVolume;
                
                // Also update SoundEffect volume if using that approach
                if (_backgroundMusicInstance != null)
                {
                    _backgroundMusicInstance.Volume = _musicVolume;
                }
            }
        }

        /// <summary>
        /// Gets or sets the sound effects volume (0.0 to 1.0)
        /// </summary>
        public float SoundEffectVolume
        {
            get { return _soundEffectVolume; }
            set
            {
                _soundEffectVolume = Math.Max(0.0f, Math.Min(1.0f, value));
            }
        }

        /// <summary>
        /// Gets whether the AudioManager has been initialized
        /// </summary>
        public bool IsInitialized
        {
            get { return _isInitialized; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the AudioManager with content manager
        /// </summary>
        /// <param name="content">ContentManager to load audio assets</param>
        public void Initialize(ContentManager content)
        {
            try
            {
                // Try to load background music as Song first
                try
                {
                    _backgroundMusic = content.Load<Song>("backgroundMusic");
                    Logger.Instance.Info("Background music loaded successfully as Song");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Warning($"Song background music not found: {ex.Message}");
                    _backgroundMusic = null;
                    
                    // Try loading background music as SoundEffect instead
                    try
                    {
                        string audioPath = Path.Combine(content.RootDirectory, "backgroundMusic.wav");
                        if (File.Exists(audioPath))
                        {
                            using (var fileStream = new FileStream(audioPath, FileMode.Open))
                            {
                                _backgroundMusicEffect = SoundEffect.FromStream(fileStream);
                                _backgroundMusicInstance = _backgroundMusicEffect.CreateInstance();
                                _backgroundMusicInstance.IsLooped = false;
                                _backgroundMusicInstance.Volume = _musicVolume;
                                
                                // ADD THIS: Register in dictionary so builder can find it
                                _soundEffects["backgroundMusic"] = _backgroundMusicEffect;
                                
                                Logger.Instance.Info("Background music loaded successfully as SoundEffect (no loop)");
                            }
                        }
                        else
                        {
                            Logger.Instance.Warning($"Background music file not found at: {audioPath}");
                        }
                    }
                    catch (Exception fileEx)
                    {
                        Logger.Instance.Warning($"Failed to load background music from file: {fileEx.Message}");
                    }
                }

                // Try to load sound effects
                try
                {
                    _soundEffects["explosion"] = content.Load<SoundEffect>("explosion");
                    Logger.Instance.Info("Explosion sound loaded successfully");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Warning($"Explosion sound not found: {ex.Message}");
                }

                try
                {
                    // Try loading using the content pipeline first
                    _soundEffects["laser"] = content.Load<SoundEffect>("laserSound");
                    Logger.Instance.Info("Laser sound loaded successfully from content pipeline");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Warning($"Content pipeline laser sound not found: {ex.Message}");
                    
                    // Try loading directly from file as fallback
                    try
                    {
                        string audioPath = Path.Combine(content.RootDirectory, "laserSound.wav");
                        if (File.Exists(audioPath))
                        {
                            using (var fileStream = new FileStream(audioPath, FileMode.Open))
                            {
                                _soundEffects["laser"] = SoundEffect.FromStream(fileStream);
                                Logger.Instance.Info("Laser sound loaded successfully from file");
                            }
                        }
                        else
                        {
                            Logger.Instance.Warning($"Laser sound file not found at: {audioPath}");
                        }
                    }
                    catch (Exception fileEx)
                    {
                        Logger.Instance.Warning($"Failed to load laser sound from file: {fileEx.Message}");
                    }
                }

                try
                {
                    _soundEffects["enemyHit"] = content.Load<SoundEffect>("enemyHitSound");
                    Logger.Instance.Info("Enemy hit sound loaded successfully");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Warning($"Enemy hit sound not found: {ex.Message}");
                }

                // Set initial volume
                MediaPlayer.Volume = _musicVolume;
                MediaPlayer.IsRepeating = true;

                _isInitialized = true;
                Logger.Instance.Info($"AudioManager initialized - Music: {(_backgroundMusic != null ? "Yes" : "No")}, Sound Effects: {_soundEffects.Count}");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to initialize AudioManager: {ex.Message}");
                _isInitialized = false;
            }
        }
        #endregion

        #region Update Method
        /// <summary>
        /// Update method to handle manual music looping and state management
        /// Call this from the game's Update loop
        /// </summary>
        public void Update()
        {
            if (!_isInitialized || !_isMusicEnabled)
                return;

            try
            {
                // Handle manual looping for SoundEffect background music
                if (_backgroundMusicInstance != null)
                {
                    if (_isMusicPlaying && _backgroundMusicInstance.State == SoundState.Stopped)
                    {
                        // Music finished, restart it (manual looping)
                        _backgroundMusicInstance.Play();
                        //Logger.Instance.Trace("Background music restarted (manual loop)");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error in AudioManager Update: {ex.Message}");
            }
        }
        #endregion

        #region Background Music Methods
        /// <summary>
        /// Play the background music
        /// </summary>
        public void PlayBackgroundMusic()
        {
            if (!_isInitialized || !_isMusicEnabled)
                return;

            try
            {
                // Try Song approach first
                if (_backgroundMusic != null)
                {
                    if (MediaPlayer.State != MediaState.Playing)
                    {
                        MediaPlayer.Play(_backgroundMusic);
                        _isMusicPlaying = true;
                        Logger.Instance.Info("Background music started (Song)");
                    }
                }
                // Fall back to SoundEffect approach
                else if (_backgroundMusicInstance != null)
                {
                    if (_backgroundMusicInstance.State != SoundState.Playing)
                    {
                        _backgroundMusicInstance.Play();
                        _isMusicPlaying = true;
                        Logger.Instance.Info("Background music started (SoundEffect - no loop)");
                    }
                }
                else
                {
                    Logger.Instance.Warning("No background music loaded to play");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to play background music: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop the background music
        /// </summary>
        public void StopBackgroundMusic()
        {
            try
            {
                _isMusicPlaying = false; // Always set this flag to false first
                
                // Stop Song if playing
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Stop();
                    Logger.Instance.Info("Background music stopped (Song)");
                }
                
                // Stop SoundEffect if playing - use immediate stop
                if (_backgroundMusicInstance != null)
                {
                    if (_backgroundMusicInstance.State == SoundState.Playing)
                    {
                        _backgroundMusicInstance.Stop(true); // Immediate stop, don't fade
                        Logger.Instance.Info("Background music stopped (SoundEffect)");
                    }
                    // Also reset the instance state
                    _backgroundMusicInstance.Stop(true);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to stop background music: {ex.Message}");
            }
        }

        /// <summary>
        /// Pause the background music
        /// </summary>
        public void PauseBackgroundMusic()
        {
            try
            {
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Pause();
                    Logger.Instance.Info("Background music paused");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to pause background music: {ex.Message}");
            }
        }

        /// <summary>
        /// Resume the background music
        /// </summary>
        public void ResumeBackgroundMusic()
        {
            if (!_isInitialized || !_isMusicEnabled)
                return;

            try
            {
                if (MediaPlayer.State == MediaState.Paused)
                {
                    MediaPlayer.Resume();
                    Logger.Instance.Info("Background music resumed");
                }
                else if (MediaPlayer.State == MediaState.Stopped && _backgroundMusic != null)
                {
                    PlayBackgroundMusic();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to resume background music: {ex.Message}");
            }
        }
        #endregion

        #region Sound Effects Methods
        /// <summary>
        /// Play a sound effect by name
        /// </summary>
        /// <param name="soundName">Name of the sound effect to play</param>
        /// <param name="volume">Volume override (optional, uses default if not specified)</param>
        public void PlaySoundEffect(string soundName, float? volume = null)
        {
            if (!_isInitialized || !_areSoundEffectsEnabled || !_soundEffects.ContainsKey(soundName))
                return;

            try
            {
                float playVolume = volume ?? _soundEffectVolume;
                playVolume = Math.Max(0.0f, Math.Min(1.0f, playVolume));
                
                _soundEffects[soundName].Play(playVolume, 0.0f, 0.0f);
                Logger.Instance.Trace($"Played sound effect: {soundName}");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Failed to play sound effect '{soundName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Play explosion sound effect
        /// </summary>
        public void PlayExplosionSound()
        {
            PlaySoundEffect("explosion");
        }

        /// <summary>
        /// Play laser sound effect
        /// </summary>
        public void PlayLaserSound()
        {
            PlaySoundEffect("laser");
        }

        /// <summary>
        /// Play enemy hit sound effect
        /// </summary>
        public void PlayEnemyHitSound()
        {
            PlaySoundEffect("enemyHit");
        }

        /// <summary>
        /// Toggle background music on/off for testing
        /// </summary>
        public void ToggleBackgroundMusic()
        {
            IsMusicEnabled = !IsMusicEnabled;
            Logger.Instance.Info($"Background music toggled: {(IsMusicEnabled ? "ON" : "OFF")}");
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Clean up audio resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                _isMusicPlaying = false; // Stop any manual looping
                StopBackgroundMusic();
                
                // Dispose of background music instances
                if (_backgroundMusicInstance != null)
                {
                    _backgroundMusicInstance.Dispose();
                    _backgroundMusicInstance = null;
                }
                
                if (_backgroundMusicEffect != null)
                {
                    _backgroundMusicEffect.Dispose();
                    _backgroundMusicEffect = null;
                }
                
                // Dispose of all sound effects
                foreach (var soundEffect in _soundEffects.Values)
                {
                    soundEffect?.Dispose();
                }
                _soundEffects.Clear();
                
                _isInitialized = false;
                Logger.Instance.Info("AudioManager disposed completely");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error disposing AudioManager: {ex.Message}");
            }
        }

        /// <summary>
        /// Static cleanup method to ensure all audio stops when application closes
        /// </summary>
        public static void ForceCleanup()
        {
            try
            {
                // Stop MediaPlayer first
                try
                {
                    if (MediaPlayer.State == MediaState.Playing)
                    {
                        MediaPlayer.Stop();
                    }
                }
                catch { }

                // If there's an instance, dispose it aggressively
                if (instance != null)
                {
                    try
                    {
                        // Stop background music instance immediately
                        if (instance._backgroundMusicInstance != null)
                        {
                            instance._backgroundMusicInstance.Stop(true); // Immediate stop
                            instance._backgroundMusicInstance.Dispose();
                            instance._backgroundMusicInstance = null;
                        }

                        // Dispose background music effect
                        if (instance._backgroundMusicEffect != null)
                        {
                            instance._backgroundMusicEffect.Dispose();
                            instance._backgroundMusicEffect = null;
                        }

                        // Dispose all sound effects
                        foreach (var soundEffect in instance._soundEffects.Values)
                        {
                            try
                            {
                                soundEffect?.Dispose();
                            }
                            catch { }
                        }
                        instance._soundEffects.Clear();
                    }
                    catch { }

                    instance = null;
                }

                // Force garbage collection to clean up any remaining audio resources
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch (Exception ex)
            {
                // Can't log here as Logger might be disposed
                System.Console.WriteLine($"Error in ForceCleanup: {ex.Message}");
            }
        }
        #endregion

        /// <summary>
        /// Create a new audio builder for complex audio configurations
        /// </summary>
        public AudioBuilder CreateAudioBuilder()
        {
            return new AudioBuilder();
        }

        /// <summary>
        /// Try to get a sound effect by name (for builder pattern)
        /// </summary>
        internal bool TryGetSoundEffect(string soundName, out SoundEffect soundEffect)
        {
            return _soundEffects.TryGetValue(soundName, out soundEffect);
        }
    }
}