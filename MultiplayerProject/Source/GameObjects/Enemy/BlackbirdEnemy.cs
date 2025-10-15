namespace MultiplayerProject.Source
{
    class BlackbirdEnemy : Enemy
    {
        public BlackbirdEnemy() : base()
        {
            Width = 30;
            Scale = 4f;
        }

        public BlackbirdEnemy(string ID) : base(ID)
        {
            Width = 30;
            Scale = 4f;
        }
    }
}