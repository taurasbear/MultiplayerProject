using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.Helpers;
using MultiplayerProject.Source.Helpers.Factories;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    public sealed class LaserManager : EntityManagerBase<Laser>, INetworkedObject
    {
        public string NetworkID { get; set; }

        private Texture2D _laserTexture;
        private TimeSpan _laserSpawnTime;
        private TimeSpan _previousLaserSpawnTime;

        private const float SECONDS_IN_MINUTE = 60f;
        private const float RATE_OF_FIRE = 200f;
        private const float LASER_SPAWN_DISTANCE = 40f;

        private float _currentFireRate = RATE_OF_FIRE;

        public LaserManager() : base()
        {
            _currentFireRate = RATE_OF_FIRE;
            _laserSpawnTime = TimeSpan.FromSeconds(SECONDS_IN_MINUTE / _currentFireRate);
            _previousLaserSpawnTime = TimeSpan.Zero;
        }


        public override void Initalise(ContentManager content)
        {
            _laserTexture = content.Load<Texture2D>("laser");
        }

        protected override void UpdateEntity(Laser laser, GameTime gameTime)
        {
            laser.Update(gameTime);
        }

        protected override bool ShouldRemoveEntity(Laser laser)
        {
            return !laser.Active;
        }

        protected override void DrawEntity(Laser laser, SpriteBatch spriteBatch)
        {
            laser.Draw(spriteBatch);
        }

        public List<Laser> Lasers => GetEntities();

        public Laser FireLocalLaserClient(GameObjectFactory factory, GameTime gameTime, Vector2 position, float rotation)
        {
            if (gameTime.TotalGameTime - _previousLaserSpawnTime > _laserSpawnTime)
            {
                _previousLaserSpawnTime = gameTime.TotalGameTime;

                AudioManager.Instance.CreateAudioBuilder()
                    .WithSound("laser")
                    .WithVolume(0.6f)
                    .WithPitch(0.0f)
                    .BuildAndPlay();

                return AddLaser(factory, position, rotation, "", "");
            }

            return null;
        }

        public void UpdateFireRate(int playerScore)
        {
            float scoreMultiplier = 1.0f + (playerScore / 3) * 0.25f;
            _currentFireRate = RATE_OF_FIRE * scoreMultiplier;
            _currentFireRate = Math.Min(_currentFireRate, RATE_OF_FIRE * 3.0f);
            _laserSpawnTime = TimeSpan.FromSeconds(SECONDS_IN_MINUTE / _currentFireRate);
        }

        public void FireRemoteLaserClient(GameObjectFactory factory, Vector2 position, float rotation, string playerID, DateTime originalTimeFired, string laserID)
        {
            var timeDifference = (originalTimeFired - DateTime.UtcNow).TotalSeconds;
            var laser = AddLaser(factory, position, rotation, laserID, playerID);
            laser.Update((float)timeDifference);
        }

        public Laser FireLaserServer(GameObjectFactory factory, double totalGameSeconds, float deltaTime, Vector2 position, float rotation, string laserID, string playerFiredID)
        {
            if (totalGameSeconds - _previousLaserSpawnTime.TotalSeconds > _laserSpawnTime.TotalSeconds)
            {
                _previousLaserSpawnTime = TimeSpan.FromSeconds(totalGameSeconds);
                var laser = AddLaser(factory, position, rotation, laserID, playerFiredID);
                laser.Update(deltaTime);
                return laser;
            }
            return null;
        }

        public Laser AddLaser(GameObjectFactory factory, Vector2 position, float rotation, string laserID, string playerFiredID)
        {
            Laser laser = (Laser)factory.CreateLaser();

            Animation laserAnimation = new Animation();
            
            // Flyweight pattern: Use shared flyweight data instead of creating duplicate animation data
            // The flyweight handles color, damage, speed, range - we only need position/rotation (extrinsic state)
            laserAnimation.Initialize(_laserTexture, position, rotation, 46, 16, 1, 30, Color.White, 1f, true);

            Vector2 direction = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
            direction.Normalize();

            var laserPosition = position + direction * LASER_SPAWN_DISTANCE;
            laser.Initialize(laserAnimation, laserPosition, rotation);

            if (!string.IsNullOrEmpty(laserID))
                laser.LaserID = laserID;

            laser.PlayerFiredID = playerFiredID;

            // Use base class method to add to collection
            AddEntityToCollection(laser);

            return laser;
        }

        /// <summary>
        /// Add a lightweight laser using TRUE flyweight pattern (no Animation objects)
        /// </summary>
        public LightweightLaser AddLightweightLaser(ElementalType elementalType, Vector2 position, float rotation, string laserID, string playerFiredID)
        {
            // Get the shared flyweight for this elemental type
            LaserFlyweight flyweight = LaserFlyweightFactory.Instance.GetFlyweight(elementalType);

            // Create lightweight laser - only stores position, rotation, IDs
            LightweightLaser laser = string.IsNullOrEmpty(laserID) 
                ? new LightweightLaser() 
                : new LightweightLaser(laserID, playerFiredID);

            Vector2 direction = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
            direction.Normalize();

            var laserPosition = position + direction * LASER_SPAWN_DISTANCE;
            
            // Initialize with shared flyweight - NO Animation object created!
            laser.Initialize(flyweight, laserPosition, rotation);

            if (!string.IsNullOrEmpty(playerFiredID))
                laser.PlayerFiredID = playerFiredID;

            // Note: LightweightLaser is not added to the main collection
            // as EntityManagerBase<Laser> expects Laser type.
            // This method is only used for benchmarking purposes.

            return laser;
        }

        public void DeactivateLaser(string laserID)
        {
            foreach (var laser in entities)
            {
                if (laser.LaserID == laserID)
                {
                    laser.Active = false;
                    return;
                }
            }
        }
    }
}