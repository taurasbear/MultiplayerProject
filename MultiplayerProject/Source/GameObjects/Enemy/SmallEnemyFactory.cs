namespace MultiplayerProject.Source
{
    class SmallEnemyFactory : EnemyFactory
    {
        public Enemy CreateEnemy()
        {
            return new SmallEnemy();
        }

        public Enemy CreateEnemy(string enemyId)
        {
            return new SmallEnemy(enemyId);
        }
    }
}