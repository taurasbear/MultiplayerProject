using MultiplayerProject.Source;

namespace MultiplayerProject.Source.PowerUps
{
    // Base class for all power-ups
    public abstract class PowerUpBase
    {
        // Template Method
        public void Activate(IPlayer player)
        {
            if (CanApply(player))
            {
                ApplyEffect(player);
                OnActivated(player);
            }
        }

        // Checks if the power-up can be applied
        protected abstract bool CanApply(IPlayer player);

        // Applies the actual effect
        protected abstract void ApplyEffect(IPlayer player);

        // Optional hook for extra behavior after activation
        protected virtual void OnActivated(IPlayer player)
        {
            // Default: do nothing
        }
    }
}
