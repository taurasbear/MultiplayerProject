namespace MultiplayerProject.Source
{
    class BirdEnemyFactory : EnemyFactory
    {
        public override Enemy CreateEnemy()
        {
            return new BirdEnemy();
        }

        public override Enemy CreateEnemy(string enemyId)
        {
            return new BirdEnemy(enemyId);
        }
    }
}