using MultiplayerProject.Source.GameObjects.Enemy;
using MultiplayerProject.Source.Helpers;

namespace MultiplayerProject.Source.Visitors
{
    public sealed class PlayerScoreVisitor : IGameObjectVisitor
    {
        public int TotalScore { get; private set; }
        public int PlayerCount { get; private set; }
        public int HighestScore { get; private set; }
        public int LowestScore { get; private set; }

        public void Reset()
        {
            TotalScore = 0;
            PlayerCount = 0;
            HighestScore = int.MinValue;
            LowestScore = int.MaxValue;
        }

        public void Visit(Player player)
        {
            PlayerCount++;
        }

        public void Visit(Enemy enemy)
        {
            // Not tracking enemies
        }

        public void Visit(Laser laser)
        {
            // Not tracking lasers
        }

        public void Visit(Explosion explosion)
        {
            // Not tracking explosions
        }

        public void AddPlayerScore(string playerID, int score)
        {
            TotalScore += score;
            PlayerCount++; 

            if (score > HighestScore)
            {
                HighestScore = score;
            }

            if (score < LowestScore)
            {
                LowestScore = score;
            }
        }

        public int GetAverageScore()
        {
            return PlayerCount > 0 ? TotalScore / PlayerCount : 0;
        }

        public string LogScoreReport()
        {
            string result;            
            result = $"Average: {GetAverageScore()}, Highest: {HighestScore}, Lowest: {LowestScore}";
            return result;
        }
    }
}