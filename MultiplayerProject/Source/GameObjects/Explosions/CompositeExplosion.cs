using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source.GameObjects.Explosions
{
    public class CompositeExplosion : Explosion
    {
        private List<Explosion> _childExplosions = new List<Explosion>();
        private List<float> _childDelays = new List<float>();
        private List<bool> _childTriggered = new List<bool>();
        private float _totalElapsedTime = 0f;
        private Random _random = new Random();
        private bool _initialized = false;

        public CompositeExplosion()
        {
            Damage = 0f;
            Radius = 1.5f;
            Duration = 2.0f; 
        }
        public void AddExplosion(Explosion explosion, float minDelay = 0f, float maxDelay = 0.5f)
        {
            _childExplosions.Add(explosion);
            _childDelays.Add((float)(_random.NextDouble() * (maxDelay - minDelay) + minDelay));
            _childTriggered.Add(false);
        }

        public void AddInitializedExplosion(Explosion explosion, float delay = 0f)
        {
            _childExplosions.Add(explosion);
            _childDelays.Add(delay);
            _childTriggered.Add(false);
        }

        public override void Initialize(Animation animation, Vector2 centerPosition, Color color)
        {
            base.Initialize(null, centerPosition, color);
            _totalElapsedTime = 0f;
            _initialized = true;

            bool hadChildren = _childExplosions.Count > 0;
            
            if (hadChildren)
            {
                for (int i = 0; i < _childExplosions.Count; i++)
                {
                    var child = _childExplosions[i];
                    
                    if (child.ExplosionAnimation == null && !(child is CompositeExplosion))
                    {
                        float angle = (float)(_random.NextDouble() * Math.PI * 2);
                        float distance = (float)(_random.NextDouble() * 30f);
                        Vector2 offset = new Vector2(
                            (float)Math.Cos(angle) * distance,
                            (float)Math.Sin(angle) * distance
                        );

                        if (animation != null)
                        {
                            Animation childAnimation = new Animation();
                            childAnimation.Initialize(
                                texture: animation.Texture,
                                position: centerPosition + offset,
                                rotation: animation.Rotation,
                                frameWidth: animation.FrameWidth,
                                frameHeight: animation.FrameHeight,
                                frameCount: animation.FrameCount,
                                frameTime: animation.FrameTime,
                                color: color,
                                scale: animation.Scale * (float)(0.7 + _random.NextDouble() * 0.6),
                                looping: false
                            );
                            child.Initialize(childAnimation, centerPosition + offset, color);
                        }
                    }
                }
            }
            else
            {
                CreateDefaultSequence(animation, centerPosition, color);
            }
        }

        private void CreateDefaultSequence(Animation baseAnimation, Vector2 centerPosition, Color color)
        {
            int explosionCount = _random.Next(3, 6);
            float radius = 30f; 

            for (int i = 0; i < explosionCount; i++)
            {
                float angle = (float)(_random.NextDouble() * Math.PI * 2);
                float distance = (float)(_random.NextDouble() * radius);
                Vector2 offset = new Vector2(
                    (float)Math.Cos(angle) * distance,
                    (float)Math.Sin(angle) * distance
                );

                Explosion childExplosion = new Explosion();
                
                if (baseAnimation != null)
                {
                    Animation childAnimation = new Animation();
                    childAnimation.Initialize(
                        texture: baseAnimation.Texture,
                        position: centerPosition + offset,
                        rotation: baseAnimation.Rotation,
                        frameWidth: baseAnimation.FrameWidth,
                        frameHeight: baseAnimation.FrameHeight,
                        frameCount: baseAnimation.FrameCount,
                        frameTime: baseAnimation.FrameTime,
                        color: color,
                        scale: baseAnimation.Scale * (float)(0.7 + _random.NextDouble() * 0.6), 
                        looping: false
                    );
                    childExplosion.Initialize(childAnimation, centerPosition + offset, color);
                }

                AddExplosion(childExplosion, i * 0.1f, (i + 1) * 0.2f);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!_initialized || !Active)
                return;

            _totalElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < _childExplosions.Count; i++)
            {
                if (!_childTriggered[i] && _totalElapsedTime >= _childDelays[i])
                {
                    _childExplosions[i].Active = true;
                    _childTriggered[i] = true;
                }

                if (_childTriggered[i])
                {
                    _childExplosions[i].Update(gameTime);
                }
            }

            bool allFinished = true;
            foreach (var child in _childExplosions)
            {
                if (child.Active)
                {
                    allFinished = false;
                    break;
                }
            }

            if (allFinished && _childTriggered.Count == _childExplosions.Count)
            {
                Active = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Active)
                return;

            for (int i = 0; i < _childExplosions.Count; i++)
            {
                if (_childTriggered[i] && _childExplosions[i].Active)
                {
                    _childExplosions[i].Draw(spriteBatch);
                }
            }
        }
    }
}
