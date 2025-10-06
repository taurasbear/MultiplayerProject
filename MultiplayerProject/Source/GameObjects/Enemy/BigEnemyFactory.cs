namespace MultiplayerProject.Source
{
    class BigEnemyFactory : EnemyFactory
    {
        public Enemy CreateEnemy()
        {
            return new BigEnemy();
        }

        public Enemy CreateEnemy(string enemyId)
        {
            return new BigEnemy(enemyId);
        }
    }
}