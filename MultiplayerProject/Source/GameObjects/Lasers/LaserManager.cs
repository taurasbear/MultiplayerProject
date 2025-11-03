using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.Helpers;
using MultiplayerProject.Source.Helpers.Factories;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    public class LaserManager : INetworkedObject
    {
        public List<Laser> Lasers { get { return _laserBeams; } }

        private List<Laser> _laserBeams;

        // texture to hold the laser.
        private Texture2D _laserTexture;

        // govern how fast our laser can fire.
        private TimeSpan _laserSpawnTime;
        private TimeSpan _previousLaserSpawnTime;

        private const float SECONDS_IN_MINUTE = 60f;
        private const float RATE_OF_FIRE = 200f;
        private const float LASER_SPAWN_DISTANCE = 40f;
        
        // Dynamic fire rate based on score
        private float _currentFireRate = RATE_OF_FIRE;

        public string NetworkID { get; set; }

        public LaserManager()
        {
            // Init our laser
            _laserBeams = new List<Laser>();

            _currentFireRate = RATE_OF_FIRE;
            _laserSpawnTime = TimeSpan.FromSeconds(SECONDS_IN_MINUTE / _currentFireRate);
            _previousLaserSpawnTime = TimeSpan.Zero;
        }

        public void Initalise(ContentManager content)
        {
            // Load the texture to serve as the laser
            _laserTexture = content.Load<Texture2D>("laser");
        }

        public void Update(GameTime gameTime)
        {
            // Update laserbeams
            for (var i = _laserBeams.Count - 1; i >= 0; i--)
            {
                _laserBeams[i].Update(gameTime);
                // Remove the beam when its deactivated or is at the end of the screen.
                if (!_laserBeams[i].Active)
                {
                    _laserBeams.Remove(_laserBeams[i]);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the lasers.
            for (int i = 0; i < _laserBeams.Count; i++)
            {
                _laserBeams[i].Draw(spriteBatch);
                //Laser laser = _laserBeams[i];
                //Texture2D texture = new Texture2D(spriteBatch.GraphicsDevice, laser.Width, laser.Height);
                //texture.CreateBorder(1, laser.LaserColor);  // <-- use the laser's color
                //spriteBatch.Draw(texture, laser.Position, Color.White);
            }
        }

        public Laser FireLocalLaserClient(GameObjectFactory factory, GameTime gameTime, Vector2 position, float rotation)
        {
            if (gameTime.TotalGameTime - _previousLaserSpawnTime > _laserSpawnTime)
            {
                _previousLaserSpawnTime = gameTime.TotalGameTime;
                
                // Use builder pattern for laser sound with dynamic properties
                // Note: You'll need to pass current score from GameScene
                AudioManager.Instance.CreateAudioBuilder()
                    .WithSound("laser")
                    .WithVolume(0.6f)
                    .WithPitch(0.0f)
                    .BuildAndPlay();
                
                return AddLaser(factory, position, rotation, "", "");
            }

            return null;
        }
        /// <summary>
        /// Update fire rate based on player score
        /// Higher scores = faster fire rate
        /// </summary>
        public void UpdateFireRate(int playerScore)
        {
            // Increase fire rate based on score
            // Base rate: 200, increases by 50 for every 3 points
            float scoreMultiplier = 1.0f + (playerScore / 3) * 0.25f; // 25% increase per 3 points
            _currentFireRate = RATE_OF_FIRE * scoreMultiplier;

            // Cap the maximum fire rate to prevent it from getting too crazy
            _currentFireRate = Math.Min(_currentFireRate, RATE_OF_FIRE * 3.0f); // Max 3x fire rate

            // Update the spawn time
            _laserSpawnTime = TimeSpan.FromSeconds(SECONDS_IN_MINUTE / _currentFireRate);
        }
        
        public void FireRemoteLaserClient(GameObjectFactory factory, Vector2 position, float rotation, string playerID, DateTime originalTimeFired, string laserID)
        {
            var timeDifference = (originalTimeFired - DateTime.UtcNow).TotalSeconds;

            var laser = AddLaser(factory, position, rotation, laserID, playerID);
            laser.Update((float)timeDifference); // Update it to match the true position
        }

        public Laser FireLaserServer(GameObjectFactory factory, double totalGameSeconds, float deltaTime, Vector2 position, float rotation, string laserID, string playerFiredID)
        {
            // Govern the rate of fire for our lasers
            if (totalGameSeconds - _previousLaserSpawnTime.TotalSeconds > _laserSpawnTime.TotalSeconds)
            {
                _previousLaserSpawnTime = TimeSpan.FromSeconds(totalGameSeconds);

                // Add the laser to our list.
                var laser = AddLaser(factory, position, rotation, laserID, playerFiredID);
                laser.Update(deltaTime); // Update it so it's in the correct position on the server as it is on the client

                return laser;
            }

            return null;
        }

        public Laser AddLaser(GameObjectFactory factory, Vector2 position, float rotation, string laserID, string playerFiredID)
        {
            Laser laser = (Laser)factory.CreateLaser();
            
            Animation laserAnimation = new Animation();
            // Initlize the laser animation
            laserAnimation.Initialize(_laserTexture,
                position,
                rotation,
                46,
                16,
                1,
                30,
                Color.White, // The concrete laser will set its own color in its Initialize method
                1f,
                true);

            Vector2 direction = new Vector2((float)Math.Cos(rotation),
                                     (float)Math.Sin(rotation));
            direction.Normalize();

            // Move the starting position to be slightly in front of the cannon
            var laserPostion = position;
            laserPostion += direction * LASER_SPAWN_DISTANCE;

            laser.Initialize(laserAnimation, laserPostion, rotation);

            if (!string.IsNullOrEmpty(laserID))
                laser.LaserID = laserID;

            laser.PlayerFiredID = playerFiredID;
            _laserBeams.Add(laser);

            return laser;
        }

        public void DeactivateLaser(string laserID)
        {
            for (int i = 0; i < _laserBeams.Count; i++)
            {
                if (_laserBeams[i].LaserID == laserID)
                {
                    _laserBeams[i].Active = false;
                    return;
                }
            }
        }
    }
}
