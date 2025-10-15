namespace MultiplayerProject.Source
{
    class BlackbirdEnemyFactory : EnemyFactory
    {
        public override Enemy CreateEnemy()
        {
            return new BlackbirdEnemy();
        }

        public override Enemy CreateEnemy(string enemyId)
        {
            return new BlackbirdEnemy(enemyId);
        }
    }
}