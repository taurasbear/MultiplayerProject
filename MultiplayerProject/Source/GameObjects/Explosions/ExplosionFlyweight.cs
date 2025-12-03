using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MultiplayerProject.Source.GameObjects.Explosions
{
    /// <summary>
    /// Flyweight for Explosion - Contains shared intrinsic state for all explosions
    /// This reduces memory by sharing the heavy texture and animation properties
    /// </summary>
    public class ExplosionFlyweight
    {
        // Intrinsic state - shared among all explosions
        public Texture2D Texture { get; private set; }
        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }
        public int FrameCount { get; private set; }
        public int FrameTime { get; private set; }

        public ExplosionFlyweight(Texture2D texture, int frameWidth, int frameHeight, int frameCount, int frameTime)
        {
            Texture = texture;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            FrameCount = frameCount;
            FrameTime = frameTime;
        }

        /// <summary>
        /// Create an animation using the shared flyweight data and unique extrinsic state
        /// </summary>
        public Animation CreateAnimation(Vector2 position, Color color, float scale, bool looping)
        {
            Animation animation = new Animation();
            animation.Initialize(
                Texture,
                position,
                0,
                FrameWidth,
                FrameHeight,
                FrameCount,
                FrameTime,
                color,
                scale,
                looping
            );
            return animation;
        }

        /// <summary>
        /// Draw explosion using flyweight's shared intrinsic state and provided extrinsic state
        /// This is the TRUE flyweight approach - no Animation object needed per explosion
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float scale, int currentFrame)
        {
            // Calculate source rectangle based on current frame
            Rectangle sourceRect = new Rectangle(currentFrame * FrameWidth, 0, FrameWidth, FrameHeight);
            
            // Calculate destination rectangle
            Rectangle destinationRect = new Rectangle(
                (int)position.X - (int)(FrameWidth * scale) / 2,
                (int)position.Y - (int)(FrameHeight * scale) / 2,
                (int)(FrameWidth * scale),
                (int)(FrameHeight * scale)
            );

            spriteBatch.Draw(Texture, destinationRect, sourceRect, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }
    }
}
