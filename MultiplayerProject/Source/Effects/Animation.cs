using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    public class Animation
    {
        private Texture2D _spriteStrip;
        private int _frameCount;
        private int _frameTime;
        private int _currentFrame;
        private int _elapsedTime;
        private Color _color;

        public bool Looping { get; set; }
        public float Scale { get; set; } = 1f;
        public Vector2 Position;
        public float Rotation;
        public int FrameWidth;
        public int FrameHeight;
        public bool Active { get; private set; }
        public bool IsFinished { get; private set; }

        private Rectangle _sourceRect;
        private Rectangle _destinationRect;

        // Expose needed properties for cloning/copying
        public Texture2D Texture => _spriteStrip;
        public int FrameTime => _frameTime;
        public int FrameCount => _frameCount;

        public void Initialize(Texture2D texture, Vector2 position, float rotation, int frameWidth, int frameHeight,
            int frameCount, int frameTime, Color color, float scale, bool looping)
        {
            _spriteStrip = texture;
            Position = position;
            Rotation = rotation;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            _frameCount = frameCount;
            _frameTime = frameTime;
            _color = color;
            Scale = scale;
            Looping = looping;

            _currentFrame = 0;
            _elapsedTime = 0;
            Active = true;
            IsFinished = false;
        }

        public void Update(GameTime gameTime)
        {
            if (!Active) return;

            _elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_elapsedTime > _frameTime)
            {
                _currentFrame++;

                if (_currentFrame >= _frameCount)
                {
                    if (Looping)
                    {
                        _currentFrame = 0;
                    }
                    else
                    {
                        _currentFrame = _frameCount - 1;
                        Active = false;
                        IsFinished = true;
                    }
                }

                _elapsedTime = 0;
            }

            _sourceRect = new Rectangle(_currentFrame * FrameWidth, 0, FrameWidth, FrameHeight);
            _destinationRect = new Rectangle(
                (int)Position.X - (int)(FrameWidth * Scale) / 2,
                (int)Position.Y - (int)(FrameHeight * Scale) / 2,
                (int)(FrameWidth * Scale),
                (int)(FrameHeight * Scale));

            if (!Looping && _currentFrame >= FrameCount - 1)
            {
                IsFinished = true;
                Active = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Active || _spriteStrip == null) return;

            spriteBatch.Draw(_spriteStrip, _destinationRect, _sourceRect, _color, Rotation, Vector2.Zero, SpriteEffects.None, 0f);
        }

        // Methods needed by Explosion subclasses
        public void SetColor(Color color) => _color = color;

        public void Reset()
        {
            _currentFrame = 0;
            _elapsedTime = 0;
            Active = true;
            IsFinished = false;
        }
    }
}
