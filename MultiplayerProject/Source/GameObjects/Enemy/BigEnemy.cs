namespace MultiplayerProject.Source
{
    class BigEnemy : Enemy
    {
        public BigEnemy() : base()
        {
            Width = 70;
        }

        public BigEnemy(string ID) : base(ID)
        {
            Width = 70;
        }
    }
}