using MultiplayerProject.Source;

namespace MultiplayerProject.Source.PowerUps
{
    public class SlowFireRatePowerUp : PowerUpBase
    {
        private readonly float _multiplier;

        public SlowFireRatePowerUp(float multiplier = 0.8f)
        {
            _multiplier = multiplier;
        }

        protected override bool CanApply(IPlayer player)
        {
            return true; // Always applicable
        }

        protected override void ApplyEffect(IPlayer player)
        {
            player.FireRate *= _multiplier;
        }
    }
}
