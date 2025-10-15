namespace MultiplayerProject.Source
{
    abstract class EnemyFactory
    {
        public abstract Enemy CreateEnemy();

        public abstract Enemy CreateEnemy(string enemyId);
    }
}