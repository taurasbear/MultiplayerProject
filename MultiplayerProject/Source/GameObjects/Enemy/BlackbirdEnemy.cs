using Microsoft.Xna.Framework;
using MultiplayerProject.Source.GameObjects.Enemy;

namespace MultiplayerProject.Source
{
    class BlackbirdEnemy : Enemy
    {
        public BlackbirdEnemy() : base()
        {
            Width = 30;
            Scale = 4f;
        }

        public BlackbirdEnemy(string ID) : base(ID)
        {
            Width = 30;
            Scale = 4f;
        }

        public override void UpdateOnEnemyEvent(EnemyEventType eventType)
        {
            if(eventType is EnemyEventType.GameCloseToFinishing)
            {
                Speed += 8f;
            }
            else if (eventType is EnemyEventType.PlayerShot)
            {
                Position.Y -= 25f;
            }
        }
    }
}