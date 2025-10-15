namespace MultiplayerProject.Source
{
    class SmallEnemy : Enemy
    {
        public SmallEnemy() : base()
        {
            Width = 30;
            Scale = 4f;
        }

        public SmallEnemy(string ID) : base(ID)
        {
            Width = 30;
            Scale = 4f;
        }
    }
}