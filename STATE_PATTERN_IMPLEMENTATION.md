# State Pattern Implementation for Player

## Overview
The Player class now implements the **State Pattern** with 4 distinct states to manage different gameplay phases.

## States

### 1. IdleState (Initial State)
- **Duration**: 3 seconds
- **Behavior**: 
  - Player is invincible and flashing
  - Cannot move or shoot
  - Automatically transitions to NormalState after 3 seconds
- **Use case**: Match start protection

### 2. NormalState (Standard Gameplay)
- **Duration**: Until death
- **Behavior**:
  - Normal movement and shooting
  - Takes damage from enemies and lasers
  - Transitions to DeadState when health reaches 0
- **Use case**: Regular gameplay

### 3. DeadState
- **Duration**: Until respawn
- **Behavior**:
  - Cannot move or shoot
  - Invincible (already dead)
  - Semi-transparent (alpha = 0.3)
  - After 5 seconds, player can press **R key** to respawn
  - **Respawning resets score to 0**
  - Transitions to RespawnState when R is pressed
- **Use case**: Death and respawn mechanics

### 4. RespawnState
- **Duration**: 3 seconds
- **Behavior**:
  - Player is invincible and flashing
  - Can move and shoot
  - Health restored to full
  - Position reset to center of screen
  - Automatically transitions to NormalState after 3 seconds
- **Use case**: Post-respawn protection

## Key Features

### Invincibility System
- **IdleState**: Invincible for 3 seconds at match start
- **RespawnState**: Invincible for 3 seconds after respawning
- **DeadState**: Technically invincible (already dead)
- **NormalState**: Vulnerable to all damage

### Flashing Visual Effect
- IdleState and RespawnState flash between visible (alpha=1.0) and semi-transparent (alpha=0.3)
- Flash frequency: every 0.2 seconds
- Provides clear visual feedback of invincibility status

### Collision Damage
- **Enemy Contact**: 20 damage per collision (only in NormalState)
- **Laser Hit**: Varies by laser type (Fire/Water/Electric)
- Invincible states ignore all collision damage

### Respawn Mechanics
- 5-second cooldown before respawn is available
- Press **R key** to respawn
- **Score is reset to 0** on respawn (penalty for dying)
- Player respawns at center of screen with full health

## Integration Instructions

### For Game Scenes (Client/Server)
To enable respawn functionality, add this to your game scene's `ProcessInput` or `Update` method:

```csharp
// In your GameScene.cs ProcessInput method:
public override void ProcessInput(GameTime gameTime, InputInformation inputInfo)
{
    // ... existing input handling ...
    
    // Check for respawn input for local player
    if (_localPlayer != null)
    {
        _localPlayer.CheckRespawnInput(
            inputInfo.CurrentKeyboardState, 
            inputInfo.PreviousKeyboardState);
    }
}
```

### Checking Player State
```csharp
// Check if player can fire
if (player.CanFire())
{
    // Fire laser
}

// Check if player is invincible (for debug/UI)
if (player.IsInvincible())
{
    // Display invincibility indicator
}

// Get current state type (for debug)
IPlayerState currentState = player.GetCurrentState();
if (currentState is DeadState)
{
    // Display "Press R to respawn" message
}
```

## Implementation Details

### Files Created
- `IPlayerState.cs` - State interface
- `IdleState.cs` - Initial invincibility state
- `NormalState.cs` - Standard gameplay state
- `DeadState.cs` - Death and respawn waiting state
- `RespawnState.cs` - Post-respawn invincibility state

### Files Modified
- `Player.cs` - Added state management, collision handlers, respawn logic
- `CollisionManager.cs` - Added enemy-to-player collision detection, invincibility checks

### Collision System Updates
The `CollisionManager` now:
1. Checks for enemy-to-player collisions
2. Respects invincibility status (skips damage for invincible players)
3. Applies damage through state pattern methods
4. Handles both laser and enemy collision damage

## Testing Notes

### To Test States:
1. **IdleState**: Start a match - player should flash for 5 seconds
2. **NormalState**: After 5 seconds, collide with enemy - should take damage
3. **DeadState**: Let health reach 0 - player should be semi-transparent and frozen
4. **RespawnState**: Wait 5 seconds, press R - player should respawn flashing at center

### Expected Behavior:
- Score resets to 0 on respawn ✓
- Cannot respawn immediately after death (5 second cooldown) ✓
- Invincible players don't take damage ✓
- Visual flashing indicates invincibility ✓

## Design Pattern Benefits

1. **Open/Closed Principle**: Easy to add new states without modifying existing code
2. **Single Responsibility**: Each state handles its own behavior
3. **Eliminates Conditionals**: No more giant if/else or switch statements for player state
4. **Testable**: Each state can be tested independently
5. **Maintainable**: Clear separation of concerns for different gameplay phases

## Future Enhancements

Potential additional states:
- **StunnedState**: Temporary paralysis from special attacks
- **PowerUpState**: Temporary enhanced abilities
- **ShieldedState**: Alternative invincibility with shield visual
- **SlowedState**: Movement speed reduction debuff
