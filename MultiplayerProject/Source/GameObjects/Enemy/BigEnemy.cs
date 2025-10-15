namespace MultiplayerProject.Source
{
    class BigEnemy : Enemy
    {
        public BigEnemy() : base()
        {
            Width = 94;
            Scale = 3f;
        }

        public BigEnemy(string ID) : base(ID)
        {
            Width = 94;
            Scale = 3f;
        }
    }
}