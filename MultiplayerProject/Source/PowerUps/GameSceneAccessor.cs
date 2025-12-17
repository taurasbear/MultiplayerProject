namespace MultiplayerProject.Source.PowerUps
{
    public static class GameSceneAccessor
    {
        // Set this once in GameScene constructor
        public static GameScene Instance { get; set; }

        public static void ReplacePlayer(string networkId, IPlayer newPlayer)
        {
            Instance.ReplacePlayerInternal(networkId, newPlayer);
        }
    }
}
