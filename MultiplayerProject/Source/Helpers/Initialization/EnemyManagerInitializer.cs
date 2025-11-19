using Microsoft.Xna.Framework.Content;
using MultiplayerProject.Source.GameObjects.Enemy;
using MultiplayerProject.Source.Helpers.Initialization;

namespace MultiplayerProject.Source.Helpers.Initialization
{
    /// <summary>
    /// Sealed initializer for EnemyManager using Template Method pattern.
    /// </summary>
    public sealed class EnemyManagerInitializer : GameObjectInitializer
    {
        private readonly EnemyManager _enemyManager;
        public EnemyManagerInitializer(EnemyManager enemyManager)
        {
            _enemyManager = enemyManager;
        }
        protected override void LoadResources(ContentManager content)
        {
            Logger.Instance.Info("EnemyManagerInitializer: Loading resources...");
            if (content != null)
                _enemyManager.Initalise(content);
        }
        protected override void SetInitialState()
        {
            Logger.Instance.Info("EnemyManagerInitializer: Setting initial state...");
            // Reset the enemy list and movement pattern
            _enemyManager.Enemies.Clear();
            // Optionally reset movement pattern index or other state if needed
        }
        
    }
}