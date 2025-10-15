﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    public class Animation
    {
        // Width of a given frame
        public int FrameWidth;

        // Height of a given frame
        public int FrameHeight;

        // The state of the Animation
        public bool Active;

        // Determines if the animation will keep playing or deactivate after one run
        public bool Looping;

        // Width of a given frame
        public Vector2 Position;

        // Rotation of the animation
        public float Rotation;

        // The scale used to display the sprite strip
        public float Scale;

        // The image representing the collection of images used for animation
        private Texture2D _spriteStrip;

        // The time since we last updated the frame
        private int _elapsedTime;

        // The time we display a frame until the next one
        private int _frameTime;

        // The number of frames that the animation contains
        private int _frameCount;

        // The index of the current frame we are displaying
        private int _currentFrame;

        // The color of the frame we will be displaying
        private Color _color;

        // The area of the image strip we want to display
        private Rectangle _sourceRect = new Rectangle();

        // The area where we want to display the image strip in the game
        private Rectangle _destinationRect = new Rectangle();

        public void Initialize(Texture2D texture, Vector2 position, float rotation, int frameWidth, int frameHeight, int frameCount, int frametime, Color color, float scale, bool looping)
        {
            // Keep a local copy of the values passed in
            _color = color;
            _frameCount = frameCount;
            _frameTime = frametime;
            Scale = scale;

            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            Looping = looping;
            Position = position;
            Rotation = rotation;

            _spriteStrip = texture;

            // Set the time to zero
            _elapsedTime = 0;
            _currentFrame = 0;

            // Set the Animation to active by default
            Active = true;
        }

        public void Update(GameTime gameTime)
        {
            // Do not update the game if we are not active
            if (Active == false) return;

            // Update the elapsed time
            _elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            // If the elapsed time is larger than the frame time
            // we need to switch frames
            if (_elapsedTime > _frameTime)
            {
                // Move to the next frame
                _currentFrame++;

                // If the currentFrame is equal to frameCount reset currentFrame to zero
                if (_currentFrame == _frameCount)
                {
                    _currentFrame = 0;

                    // If we are not looping deactivate the animation
                    if (Looping == false)
                        Active = false;
                }

                // Reset the elapsed time to zero      
                _elapsedTime = 0;
            }

            // Grab the correct frame in the image strip by multiplying the currentFrame index by the Frame width
            _sourceRect = new Rectangle(_currentFrame * FrameWidth, 0, FrameWidth, FrameHeight);

            // Grab the correct frame in the image strip by multiplying the currentFrame index by the frame width
            //Logger.Instance.Warning($"--> Animation scale: {Scale}");
            _destinationRect = new Rectangle((int)Position.X - (int)(FrameWidth * Scale) / 2,
                (int)Position.Y - (int)(FrameHeight * Scale) / 2,
                (int)(FrameWidth * Scale),
                (int)(FrameHeight * Scale));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Only draw the animation when we are active
            if (Active && _spriteStrip != null)
            {
                try // This try-catch is here in an attempt to catch an intermittent error
                {
                    Vector2 origin = new Vector2(FrameWidth / 2, FrameHeight / 2);
                    spriteBatch.Draw(_spriteStrip, _destinationRect, _sourceRect, _color, Rotation, Vector2.Zero, SpriteEffects.None, 0);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Animation Error occured: " + e.Message);
                }
            }
        }
    }
}
