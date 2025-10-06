namespace MultiplayerProject.Source
{
    class BigEnemy : Enemy
    {
        public BigEnemy()
        {
            EnemyID = Guid.NewGuid().ToString();
            Width = 47;
        }

        public BigEnemy(string ID)
        {
            EnemyID = ID;
        }
    }
}