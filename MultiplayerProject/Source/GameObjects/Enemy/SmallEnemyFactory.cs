namespace MultiplayerProject.Source
{
    class SmallEnemyFactory : EnemyFactory
    {
        public override Enemy CreateEnemy()
        {
            return new SmallEnemy();
        }

        public override Enemy CreateEnemy(string enemyId)
        {
            return new SmallEnemy(enemyId);
        }
    }
}