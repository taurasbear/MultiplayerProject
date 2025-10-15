namespace MultiplayerProject.Source
{
    class BirdEnemy : Enemy
    {
        public BirdEnemy() : base()
        {
            Width = 94;
            Scale = 3f;
        }

        public BirdEnemy(string ID) : base(ID)
        {
            Width = 94;
            Scale = 3f;
        }
    }
}