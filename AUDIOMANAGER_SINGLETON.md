# AudioManager Singleton Implementation

## Overview
This document describes the AudioManager singleton pattern implementation added to the MultiplayerProject to handle background music and sound effects across the entire application.

## Design Pattern: Singleton
The AudioManager follows the Singleton design pattern to ensure:
- **Single Instance**: Only one AudioManager instance exists throughout the application lifetime
- **Global Access**: The AudioManager can be accessed from anywhere in the application via `AudioManager.Instance`
- **Thread Safety**: Uses double-checked locking for thread-safe initialization
- **Resource Management**: Centralized audio resource management and cleanup

## Implementation Details

### Location
- **File**: `Source\Helpers\AudioManager.cs`
- **Namespace**: `MultiplayerProject.Source.Helpers`

### Key Features

#### 1. Background Music Management
- Persistent background music that plays across all scenes
- Automatic looping
- Volume control (0.0 to 1.0)
- Play/Stop/Pause/Resume functionality
- Music can be enabled/disabled globally

#### 2. Sound Effects Management
- Dictionary-based sound effect storage
- Volume control for individual sound effects
- Pre-defined methods for common game sounds:
  - `PlayExplosionSound()` - For enemy/player explosions
  - `PlayLaserSound()` - For laser firing
  - `PlayEnemyHitSound()` - For enemy hits
- Generic `PlaySoundEffect(string soundName, float? volume)` method

#### 3. Configuration Properties
- `IsMusicEnabled` - Enable/disable background music
- `AreSoundEffectsEnabled` - Enable/disable all sound effects
- `MusicVolume` - Background music volume (0.0 - 1.0)
- `SoundEffectVolume` - Sound effects volume (0.0 - 1.0)

### Integration Points

#### 1. Application Startup (`Application.cs`)
```csharp
// Initialize AudioManager in LoadContent()
AudioManager.Instance.Initialize(Content);
AudioManager.Instance.PlayBackgroundMusic();

// Cleanup in OnExiting()
AudioManager.Instance.Dispose();
```

#### 2. Collision System (`CollisionManager.cs`)
```csharp
// Play explosion sound when laser hits enemy
AudioManager.Instance.PlayExplosionSound();
```

#### 3. Laser System (`LaserManager.cs`)
```csharp
// Play laser sound when firing
AudioManager.Instance.PlayLaserSound();
```

## Usage Examples

### Basic Usage
```csharp
// Play background music
AudioManager.Instance.PlayBackgroundMusic();

// Play sound effects
AudioManager.Instance.PlayExplosionSound();
AudioManager.Instance.PlayLaserSound();

// Adjust volumes
AudioManager.Instance.MusicVolume = 0.7f;
AudioManager.Instance.SoundEffectVolume = 0.8f;

// Toggle audio
AudioManager.Instance.IsMusicEnabled = false;
AudioManager.Instance.AreSoundEffectsEnabled = true;
```

### Custom Sound Effects
```csharp
// Play any loaded sound effect
AudioManager.Instance.PlaySoundEffect("customSound", 0.5f);
```

## Adding Audio Files

To add actual audio files to the project:

1. **Add audio files to Content directory**:
   - Background music: `.ogg` or `.wma` files (Song format)
   - Sound effects: `.wav` or `.mp3` files (SoundEffect format)

2. **Update Content.mgcb** to include the audio files

3. **Uncomment and update the Initialize method** in AudioManager.cs:
```csharp
// Load background music
_backgroundMusic = content.Load<Song>("backgroundMusic");

// Load sound effects
_soundEffects["explosion"] = content.Load<SoundEffect>("explosionSound");
_soundEffects["laser"] = content.Load<SoundEffect>("laserSound");
_soundEffects["enemyHit"] = content.Load<SoundEffect>("enemyHitSound");
```

## Benefits of This Implementation

1. **Centralized Audio Management**: All audio logic is contained in one place
2. **Cross-Scene Persistence**: Background music continues seamlessly across different game states
3. **Easy Integration**: Simple API for playing sounds from anywhere in the codebase
4. **Performance**: Singleton ensures no duplicate audio managers or resource loading
5. **Configurability**: Easy volume and enable/disable controls
6. **Extensibility**: Easy to add new sound effects or music tracks

## Thread Safety
The AudioManager uses the double-checked locking pattern for thread-safe singleton initialization, making it safe to use in the multithreaded client-server architecture of the game.

## Memory Management
The AudioManager properly disposes of audio resources in the `Dispose()` method, which is called during application shutdown to prevent memory leaks.