namespace MultiplayerProject.Source
{
    class BigEnemyFactory : EnemyFactory
    {
        public override Enemy CreateEnemy()
        {
            return new BigEnemy();
        }

        public override Enemy CreateEnemy(string enemyId)
        {
            return new BigEnemy(enemyId);
        }
    }
}