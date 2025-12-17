namespace MultiplayerProject.Source
{
    public class SlowFireDecorator : PlayerDecorator
    {
        private readonly float _multiplier;

        public SlowFireDecorator(IPlayer wrappedPlayer, float multiplier)
            : base(wrappedPlayer)
        {
            _multiplier = multiplier;
        }

        public override float GetFireRateMultiplier()
        {
            return WrappedPlayer.GetFireRateMultiplier() * _multiplier;
        }
    }
}
