using MultiplayerProject.Source;

namespace MultiplayerProject.Source.PowerUps
{
    public class SpeedPowerUp : PowerUpBase
    {
        private readonly float _speedIncrease;

        public SpeedPowerUp(float speedIncrease = 2f)
        {
            _speedIncrease = speedIncrease;
        }

        protected override bool CanApply(IPlayer player)
        {
            return true; // Always applicable
        }

        protected override void ApplyEffect(IPlayer player)
        {
            player.Speed += _speedIncrease;
        }
    }
}
