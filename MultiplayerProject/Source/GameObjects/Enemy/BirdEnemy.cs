using MultiplayerProject.Source.GameObjects.Enemy;

namespace MultiplayerProject.Source
{
    class BirdEnemy : Enemy
    {
        public BirdEnemy() : base()
        {
            Width = 94;
            Scale = 3f;
        }

        public BirdEnemy(string ID) : base(ID)
        {
            Width = 94;
            Scale = 3f;
        }

        public override void UpdateOnEnemyEvent(EnemyEventType eventType)
        {
            if (eventType is EnemyEventType.PlayerShot)
            {
                Position.Y += 25f;
            }
        }
    }
}