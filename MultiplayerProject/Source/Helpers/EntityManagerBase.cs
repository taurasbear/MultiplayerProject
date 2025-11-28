using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MultiplayerProject.Source.Helpers
{
    public abstract class EntityManagerBase<T> where T : class
    {
        protected List<T> entities;

        protected EntityManagerBase()
        {
            entities = new List<T>();
        }

        public void Update(GameTime gameTime)
        {
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                UpdateEntity(entities[i], gameTime);

                if (ShouldRemoveEntity(entities[i]))
                {
                    OnEntityRemoved(entities[i]);
                    entities.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                DrawEntity(entities[i], spriteBatch);
            }
        }

        protected abstract void UpdateEntity(T entity, GameTime gameTime);
        protected abstract bool ShouldRemoveEntity(T entity);
        protected abstract void DrawEntity(T entity, SpriteBatch spriteBatch);
        public abstract void Initalise(ContentManager content);

        protected virtual void OnEntityRemoved(T entity) { }

        public List<T> GetEntities() => entities;

        protected virtual void AddEntityToCollection(T entity)
        {
            if (entity != null)
            {
                entities.Add(entity);
            }
        }
    }
}