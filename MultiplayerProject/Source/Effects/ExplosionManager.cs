using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.Helpers;
using MultiplayerProject.Source.Helpers.Factories;
using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.GameObjects.Explosions;
using System;

namespace MultiplayerProject.Source
{
    public sealed class ExplosionManager : EntityManagerBase<Explosion>
    {
        private Texture2D _explosionTexture;
        private Random _random = new Random();

        public ExplosionManager() : base()
        {
        }

        public override void Initalise(ContentManager content)
        {
            _explosionTexture = content.Load<Texture2D>("explosion");
            
            // Initialize Flyweight factory for memory optimization
            GameObjects.Explosions.ExplosionFlyweightFactory.Instance.Initialize(content);
        }

        protected override void UpdateEntity(Explosion explosion, GameTime gameTime)
        {
            explosion.Update(gameTime);
        }

        protected override bool ShouldRemoveEntity(Explosion explosion)
        {
            return !explosion.Active;
        }
        protected override void DrawEntity(Explosion explosion, SpriteBatch spriteBatch)
        {
            explosion.Draw(spriteBatch);
        }

        public void AddExplosion(Vector2 position, GameObjectFactory factory, Color color)
        {
            if (_random.Next(100) < 30)
            {
                AddCompositeExplosion(position, color);
            }
            else
            {
                // Regular explosion
                Explosion explosion = (Explosion)factory.CreateExplosion();

                // Use Flyweight pattern to create animation with shared intrinsic state
                // This saves memory by reusing the same texture and properties for all explosions
                Animation baseAnimation;
                
                if (GameObjects.Explosions.ExplosionFlyweightFactory.Instance.IsInitialized)
                {
                    var flyweight = GameObjects.Explosions.ExplosionFlyweightFactory.Instance.GetFlyweight();
                    baseAnimation = flyweight.CreateAnimation(position, color, 1.0f, false);
                }
                else
                {
                    // Fallback if flyweight not initialized (shouldn't happen)
                    baseAnimation = new Animation();
                    baseAnimation.Initialize(
                        _explosionTexture,
                        position,
                        0,
                        134,
                        134,
                        12,
                        30,
                        color,
                        1.0f,
                        false
                    );
                }

                explosion.Initialize(baseAnimation, position, color);

                // Use base class method to add to collection
                AddEntityToCollection(explosion);
            }
        }

        private void AddCompositeExplosion(Vector2 position, Color color, int depth = 0)
        {
            const int maxDepth = 2;
            CompositeExplosion composite = new CompositeExplosion();

            int childCount = _random.Next(2, 5);

            for (int i = 0; i < childCount; i++)
            {
                float delay = (float)(_random.NextDouble() * 0.3 + i * 0.15);
                
                bool createNested = depth < maxDepth && _random.Next(100) < 40;

                if (createNested)
                {
                    CompositeExplosion nestedComposite = new CompositeExplosion();
                    
                    float angle = (float)(_random.NextDouble() * Math.PI * 2);
                    float distance = (float)(_random.NextDouble() * 40f);
                    Vector2 offset = new Vector2(
                        (float)Math.Cos(angle) * distance,
                        (float)Math.Sin(angle) * distance
                    );

                    BuildNestedComposite(nestedComposite, position + offset, color, depth + 1);
                    
                    composite.AddInitializedExplosion(nestedComposite, delay);
                }
                else
                {
                    Explosion leafExplosion = CreateRandomLeafExplosion();
                    composite.AddExplosion(leafExplosion, delay, delay + 0.1f);
                }
            }

            Animation baseAnimation = CreateBaseAnimation(position, color);
            composite.Initialize(baseAnimation, position, color);

            AddEntityToCollection(composite);
        }

        private void BuildNestedComposite(CompositeExplosion composite, Vector2 position, Color color, int depth)
        {
            const int maxDepth = 2;
            int childCount = _random.Next(2, 4);

            for (int i = 0; i < childCount; i++)
            {
                float delay = (float)(_random.NextDouble() * 0.2 + i * 0.1);
                
                bool createNested = depth < maxDepth && _random.Next(100) < 30;

                if (createNested)
                {
                    CompositeExplosion nestedComposite = new CompositeExplosion();
                    
                    float angle = (float)(_random.NextDouble() * Math.PI * 2);
                    float distance = (float)(_random.NextDouble() * 30f);
                    Vector2 offset = new Vector2(
                        (float)Math.Cos(angle) * distance,
                        (float)Math.Sin(angle) * distance
                    );

                    BuildNestedComposite(nestedComposite, position + offset, color, depth + 1);
                    composite.AddInitializedExplosion(nestedComposite, delay);
                }
                else
                {
                    Explosion leafExplosion = CreateRandomLeafExplosion();
                    composite.AddExplosion(leafExplosion, delay, delay + 0.1f);
                }
            }

            Animation baseAnimation = CreateBaseAnimation(position, color);
            composite.Initialize(baseAnimation, position, color);
        }

        private Explosion CreateRandomLeafExplosion()
        {
            int type = _random.Next(3);
            switch (type)
            {
                case 0:
                    return new FireExplosion();
                case 1:
                    return new WaterExplosion();
                case 2:
                    return new ElectricExplosion();
                default:
                    return new FireExplosion();
            }
        }

        private Animation CreateBaseAnimation(Vector2 position, Color color)
        {
            if (GameObjects.Explosions.ExplosionFlyweightFactory.Instance.IsInitialized)
            {
                var flyweight = GameObjects.Explosions.ExplosionFlyweightFactory.Instance.GetFlyweight();
                return flyweight.CreateAnimation(position, color, 1.0f, false);
            }
            else
            {
                Animation animation = new Animation();
                animation.Initialize(
                    _explosionTexture,
                    position,
                    0,
                    134,
                    134,
                    12,
                    30,
                    color,
                    1.0f,
                    false
                );
                return animation;
            }
        }

        /// <summary>
        /// Add a lightweight explosion using TRUE flyweight pattern (no Animation objects)
        /// This creates explosions that only store position, color, and current frame
        /// All rendering data comes from the shared flyweight
        /// </summary>
        public LightweightExplosion AddLightweightExplosion(Vector2 position, Color color)
        {
            // Get the shared flyweight
            var flyweight = GameObjects.Explosions.ExplosionFlyweightFactory.Instance.GetFlyweight();

            // Create lightweight explosion - only stores position, color, frame
            LightweightExplosion explosion = new LightweightExplosion();
            explosion.Initialize(flyweight, position, color);

            // Note: LightweightExplosion is not added to the main collection
            // as EntityManagerBase<Explosion> expects Explosion type.
            // This method is only used for benchmarking purposes.

            return explosion;
        }

        /// <summary>
        /// Server-side explosion creation without graphics (for statistics tracking only)
        /// This allows the server to track explosions in the visitor pattern without needing textures
        /// </summary>
        public void AddServerExplosion(Vector2 position, GameObjectFactory factory, Color color)
        {
            // Get the correct explosion type from the factory
            Explosion explosion = (Explosion)factory.CreateExplosion();

            // Initialize for server-side use (no graphics, timer-based)
            explosion.InitializeServerSide(position, color, 1.0f); // 1 second duration
            
            // Use base class method to add to collection
            AddEntityToCollection(explosion);
        }
    }
}