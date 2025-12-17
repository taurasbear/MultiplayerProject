using MultiplayerProject.Source;

namespace MultiplayerProject.Source.PowerUps
{
    public class ScoreBoostPowerUp : PowerUpBase
    {
        private readonly int _scoreIncrease;

        public ScoreBoostPowerUp(int scoreIncrease = 10)
        {
            _scoreIncrease = scoreIncrease;
        }

        protected override bool CanApply(IPlayer player)
        {
            return true; // Always applicable
        }

        protected override void ApplyEffect(IPlayer player)
        {
            player.Score += _scoreIncrease;
        }
    }
}
