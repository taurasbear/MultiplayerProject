namespace MultiplayerProject.Source
{
    class EnemyFactory
    {
        public abstract Enemy CreateEnemy();

        public abstract Enemy CreateEnemy(string enemyId);
    }
}