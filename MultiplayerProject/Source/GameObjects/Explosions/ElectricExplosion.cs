using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source.GameObjects.Explosions
{
    public class ElectricExplosion : Explosion
    {
        public ElectricExplosion() { }
        private readonly float _scale = 1.1f;
        public override void Initialize(Animation baseAnimation, Vector2 centerPosition, Color color)
        {
            Animation animationCopy = new Animation();
            animationCopy.Initialize(
                texture: baseAnimation.Texture,
                position: baseAnimation.Position,
                rotation: baseAnimation.Rotation,
                frameWidth: baseAnimation.FrameWidth,
                frameHeight: baseAnimation.FrameHeight,
                frameCount: baseAnimation.FrameCount,
                frameTime: baseAnimation.FrameTime,
                color: color,
                scale: baseAnimation.Scale * _scale,
                looping: false
            );
            base.Initialize(animationCopy, centerPosition, color);
        }
    }
}