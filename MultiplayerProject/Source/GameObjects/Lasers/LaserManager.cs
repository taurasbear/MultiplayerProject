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
        private Dictionary<string, List<Laser>> _playerLasers;

        // texture to hold the laser.
        private Texture2D _laserTexture;

        // govern how fast our laser can fire.
        private TimeSpan _laserSpawnTime;
        private TimeSpan _previousLaserSpawnTime;

        private const float SECONDS_IN_MINUTE = 60f;
        private const float RATE_OF_FIRE = 200f;
        private const float LASER_SPAWN_DISTANCE = 40f;

        public string NetworkID { get; set; }

        public LaserManager()
        {
            // Init our laser
            _laserBeams = new List<Laser>();
            _playerLasers = new Dictionary<string, List<Laser>>();

            _laserSpawnTime = TimeSpan.FromSeconds(SECONDS_IN_MINUTE / RATE_OF_FIRE);
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
            for (var i = 0; i < _laserBeams.Count; i++)
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

        public Laser FireLocalLaserClient(GameTime gameTime, Vector2 position, float rotation, PlayerColour colour)
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
                
                return AddLaser(position, rotation, "", "", colour);
            }

            return null;
        }

        public void FireRemoteLaserClient(Vector2 position, float rotation, string playerID, DateTime originalTimeFired, string laserID, PlayerColour colour)
        {
            var timeDifference = (originalTimeFired - DateTime.UtcNow).TotalSeconds;

            var laser = AddLaser(position, rotation, laserID, playerID, colour);
            laser.Update((float)timeDifference); // Update it to match the true position

            if (!_playerLasers.ContainsKey(playerID))
            {
                _playerLasers.Add(playerID, new List<Laser>());
            }

            _playerLasers[playerID].Add(laser);
        }

        public Laser FireLaserServer(double totalGameSeconds, float deltaTime, Vector2 position, float rotation, string laserID, string playerFiredID)
        {
            // Govern the rate of fire for our lasers
            if (totalGameSeconds - _previousLaserSpawnTime.TotalSeconds > _laserSpawnTime.TotalSeconds)
            {
                _previousLaserSpawnTime = TimeSpan.FromSeconds(totalGameSeconds);

                // Add the laser to our list.
                var laser = AddLaser(position, rotation, laserID, playerFiredID, NetworkPacketFactory.Instance.MakePlayerColour(255, 255, 255));
                laser.Update(deltaTime); // Update it so it's in the correct position on the server as it is on the client

                return laser;
            }

            return null;
        }

        public Laser AddLaser(Vector2 position, float rotation, string laserID, string playerFiredID, PlayerColour colour)
        {
            Laser laser = CreateLaserFromColor(colour, laserID, playerFiredID);

            Animation laserAnimation = new Animation();
            // Initlize the laser animation
            laserAnimation.Initialize(_laserTexture,
                position,
                rotation,
                46,
                16,
                1,
                30,
                new Color(colour.R, colour.G, colour.B),
                1f,
                true);

            //Laser laser;
            //if (string.IsNullOrEmpty(laserID))
            //    laser = new Laser();
            //else
            //    laser = new Laser(laserID, playerFiredID);

            Vector2 direction = new Vector2((float)Math.Cos(rotation),
                                     (float)Math.Sin(rotation));
            direction.Normalize();

            // Move the starting position to be slightly in front of the cannon
            var laserPostion = position;
            laserPostion += direction * LASER_SPAWN_DISTANCE;

            // Init the laser
            laser.Initialize(laserAnimation, laserPostion, rotation);
            laser.LaserColor = new Color(colour.R, colour.G, colour.B);  // <-- store in laser
            laser.PlayerFiredID = playerFiredID;
            _laserBeams.Add(laser);

            return laser;
        }
        private Laser CreateLaserFromColor(PlayerColour colour, string laserID, string playerFiredID)
        {
            GameObjectFactory factory;

            // Determine which factory to use based on color
            var color = new Color(colour.R, colour.G, colour.B);

            if (color == Color.Red)
            {
                factory = new RedFactory();
            }
            else if (color == Color.Blue)
            {
                factory = new BlueFactory();
            }
            else if (color == Color.Green)
            {
                factory = new GreenFactory();
            }
            else
            {
                // Default to Red factory for any other colors
                factory = new RedFactory();
            }

            // Get laser from factory and cast to Laser
            Laser laser = (Laser)factory.GetLaser();

            // Set the IDs if they were provided
            if (!string.IsNullOrEmpty(laserID))
            {
                laser.LaserID = laserID;
            }
            if (!string.IsNullOrEmpty(playerFiredID))
            {
                laser.PlayerFiredID = playerFiredID;
            }

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
