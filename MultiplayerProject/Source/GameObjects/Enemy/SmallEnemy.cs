namespace MultiplayerProject.Source
{
    class SmallEnemy : Enemy
    {
        public SmallEnemy() : base()
        {
            Width = 30;
        }

        public SmallEnemy(string ID) : base(ID)
        {
            Width = 30;
        }
    }
}