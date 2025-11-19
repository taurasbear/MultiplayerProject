using Microsoft.Xna.Framework.Content;
using System;

namespace MultiplayerProject.Source.Helpers.Initialization
{
    /// <summary>
    /// Abstract base class for Template Method pattern.
    /// Defines the skeleton for initializing game object managers.
    /// </summary>
    public abstract class GameObjectInitializer
    {
        /// <summary>
        /// Template method for initialization. Calls steps in order.
        /// </summary>
        public void Initialize(ContentManager content)
        {
            try
            {
                Logger.Instance.Info($"{GetType().Name}: Starting initialization...");
                LoadResources(content);
                SetInitialState();
                Logger.Instance.Info($"{GetType().Name}: Initialization complete.");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"{GetType().Name} error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Load all required resources (must be implemented by subclass).
        /// </summary>
        protected abstract void LoadResources(ContentManager content);
        /// <summary>
        /// Set up initial state (must be implemented by subclass).
        /// </summary>
        protected abstract void SetInitialState();
    }
}
