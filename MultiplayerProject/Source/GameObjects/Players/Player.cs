using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.GameObjects.Players.States;
using MultiplayerProject.Source.Visitors;

namespace MultiplayerProject.Source
{
    public class Player : GameObject, INetworkedObject, IPlayer, IVisitable
    {
        public override bool Active { get; set; }
        public int Health { get; set; }
        public PlayerColour Colour { get; set; }
        public string PlayerName { get; set; } = "Player";

        public int Width { get; set; }
        public int Height { get; set; }

        // State pattern
        private IPlayerState _currentState;
        public int Score { get; set; } // Track player score for respawn penalty

        public Vector2 Position { get { return PlayerState.Position; } }
        public float Rotation { get { return PlayerState.Rotation; } }
        public float Speed { get { return PlayerState.Speed; } }

        public string NetworkID { get; set; }
        public int LastSequenceNumberProcessed { get; set; }
        public KeyboardMovementInput LastKeyboardMovementInput { get; set; }

        public struct ObjectState
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float Speed;
        }

        private Animation PlayerAnimation;
        protected ObjectState PlayerState;

        public Player()
        {
            PlayerState.Position = Vector2.Zero;
            PlayerState.Velocity = Vector2.Zero;
            PlayerState.Rotation = 0;

            Width = 115;
            Height = 69;

            Active = true;
            Health = Application.PLAYER_STARTING_HEALTH;
            Score = 0;
            
            // Start in IdleState (invincible at match start)
            _currentState = new IdleState();
            _currentState.Enter(this);
        }

        public Player(PlayerColour colour) : this()
        {
            Colour = colour;
        }

        public void Initialize(ContentManager content)
        {
            Animation playerAnimation = new Animation();
            Texture2D playerTexture = content.Load<Texture2D>("shipAnimation");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 0, Width, Height, 8, 30,
                new Color(Colour.R, Colour.G, Colour.B), 1f, true);

            PlayerAnimation = playerAnimation;
        }

        public void Initialize(ContentManager content, PlayerColour colour)
        {
            Colour = colour;
            Initialize(content);
        }

        public override void Update(GameTime gameTime)
        {
            // Update current state
            _currentState?.Update(this, gameTime);

            if (PlayerAnimation != null)
            {
                PlayerAnimation.Position = PlayerState.Position;
                PlayerAnimation.Rotation = PlayerState.Rotation;

                // Apply alpha for flashing effect
                float alpha = _currentState?.GetAlpha() ?? 1.0f;
                Color currentColor = new Color(Colour.R, Colour.G, Colour.B);
                // Apply alpha by modifying the color's alpha channel
                Color colorWithAlpha = currentColor * alpha;
                PlayerAnimation.SetColor(colorWithAlpha);

                PlayerAnimation.Update(gameTime);
            }
        }

        public void Update(float deltaTime)
        {
            Update(ref PlayerState, deltaTime);
        }

        public void Update(ref ObjectState state, float deltaTime)
        {
            if (state.Speed > Application.PLAYER_MAX_SPEED)
                state.Speed = Application.PLAYER_MAX_SPEED;
            else if (state.Speed < -Application.PLAYER_MAX_SPEED)
                state.Speed = -Application.PLAYER_MAX_SPEED;

            Vector2 direction = new Vector2((float)Math.Cos(state.Rotation),
                        (float)Math.Sin(state.Rotation));
            direction.Normalize();

            state.Position += direction * state.Speed;
            state.Speed *= Application.PLAYER_DECELERATION_AMOUNT;

            state.Position.X = MathHelper.Clamp(state.Position.X, 0, Application.WINDOW_WIDTH);
            state.Position.Y = MathHelper.Clamp(state.Position.Y, 0, Application.WINDOW_HEIGHT);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            PlayerAnimation.Draw(spriteBatch);
        }

        public virtual void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            PlayerAnimation.Draw(spriteBatch);
        }

        public void SetPlayerState(PlayerUpdatePacket packet)
        {
            PlayerState.Position = new Vector2(packet.XPosition, packet.YPosition);
            PlayerState.Rotation = packet.Rotation;
            PlayerState.Speed = packet.Speed;
        }

        public void ApplyInputToPlayer(KeyboardMovementInput input, float deltaTime)
        {
            // Delegate input handling to current state
            _currentState?.HandleInput(this, input, deltaTime);
        }

        // Internal method for states to use
        internal void ApplyInputToPlayerInternal(KeyboardMovementInput input, float deltaTime)
        {
            ApplyInputToPlayer(ref PlayerState, input, deltaTime);
        }

        public void ApplyInputToPlayer(ref ObjectState state, KeyboardMovementInput input, float deltaTime)
        {
            if (input.DownPressed)
            {
                state.Speed -= Application.PLAYER_ACCELERATION_SPEED * deltaTime;
            }

            if (input.UpPressed)
            {
                state.Speed += Application.PLAYER_ACCELERATION_SPEED * deltaTime;
            }

            if (input.LeftPressed)
            {
                state.Rotation -= Application.PLAYER_ROTATION_SPEED * deltaTime;
            }

            if (input.RightPressed)
            {
                state.Rotation += Application.PLAYER_ROTATION_SPEED * deltaTime;
            }
        }

        public PlayerUpdatePacket BuildUpdatePacket()
        {
            Vector2 pos = new Vector2((float)Math.Round((decimal)PlayerState.Position.X, 1), (float)Math.Round((decimal)PlayerState.Position.Y, 1));
            float speed = (float)Math.Round((decimal)PlayerState.Speed, 1);
            float rot = (float)Math.Round((decimal)PlayerState.Rotation, 1);
            return NetworkPacketFactory.Instance.MakePlayerUpdatePacket(pos.X, pos.Y, speed, rot);
        }

        public virtual bool GetHasShield()
        {
            return false;
        }

        public virtual float GetFireRateMultiplier()
        {
            return 1.0f;
        }

        public virtual void TakeDamage(int damage)
        {
            Health -= damage;
        }

        public void SetColour(PlayerColour colour)
        {
            this.Colour = colour;
        }

        public void Accept(IGameObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        // ============ STATE PATTERN METHODS ============

        /// <summary>
        /// Change to a new state
        /// </summary>
        public void ChangeState(IPlayerState newState)
        {
            _currentState?.Exit(this);
            _currentState = newState;
            _currentState?.Enter(this);
        }

        /// <summary>
        /// Get current state (for debugging/testing)
        /// </summary>
        public IPlayerState GetCurrentState()
        {
            return _currentState;
        }

        /// <summary>
        /// Check if player can fire based on current state
        /// </summary>
        public bool CanFire()
        {
            return _currentState?.CanFire() ?? true;
        }

        /// <summary>
        /// Check if player is invincible based on current state
        /// </summary>
        public bool IsInvincible()
        {
            return _currentState?.IsInvincible() ?? false;
        }

        /// <summary>
        /// Handle collision with enemy
        /// </summary>
        public void HandleEnemyCollision(int damage)
        {
            _currentState?.HandleEnemyCollision(this, damage);
        }

        /// <summary>
        /// Handle collision with laser
        /// </summary>
        public void HandleLaserCollision(int damage)
        {
            _currentState?.HandleLaserCollision(this, damage);
        }

        /// <summary>
        /// Set player position (used by states for respawn)
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            PlayerState.Position = position;
        }

        /// <summary>
        /// Check for respawn input (R key). Should be called from game scene with full keyboard state.
        /// Returns true if respawn was triggered (so caller can notify server).
        /// </summary>
        public bool CheckRespawnInput(KeyboardState currentKeyboard, KeyboardState previousKeyboard)
        {
            if (_currentState is DeadState deadState)
            {
                if (deadState.CheckRespawnInput(Keys.R, currentKeyboard, previousKeyboard))
                {
                    // Reset score to 0 on respawn
                    Score = 0;
                    Console.WriteLine($"[STATE] Player {PlayerName} respawning - Score reset to 0");
                    
                    // Transition to RespawnState
                    ChangeState(new RespawnState());
                    return true; // Notify caller that respawn happened
                }
            }
            return false;
        }
    }
}