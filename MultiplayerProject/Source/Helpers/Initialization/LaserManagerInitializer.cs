using Microsoft.Xna.Framework.Content;
using MultiplayerProject.Source.Helpers.Initialization;
using MultiplayerProject.Source;

namespace MultiplayerProject.Source.Helpers.Initialization
{
    /// <summary>
    /// Sealed initializer for LaserManager using Template Method pattern.
    /// </summary>
    public sealed class LaserManagerInitializer : GameObjectInitializer
    {
        private readonly LaserManager _laserManager;
        public LaserManagerInitializer(LaserManager laserManager)
        {
            _laserManager = laserManager;
        }
        protected override void LoadResources(ContentManager content)
        {
            Logger.Instance.Info("LaserManagerInitializer: Loading resources...");
            if (content != null)
                _laserManager.Initalise(content);
        }
        protected override void SetInitialState()
        {
            Logger.Instance.Info("LaserManagerInitializer: Setting initial state...");
            // Reset the laser list and fire rate
            _laserManager.Lasers.Clear();
            _laserManager.UpdateFireRate(0); // Reset fire rate to default
        }
        
    }
}