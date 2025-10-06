namespace MultiplayerProject.Source
{
    class SmallEnemy : Enemy
    {
        public SmallEnemy()
        {
            EnemyID = Guid.NewGuid().ToString();
            Width = 30;
        }

        public SmallEnemy(string ID)
        {
            EnemyID = ID;
        }
    }
}