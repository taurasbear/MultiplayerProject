using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source
{
    interface IMoveAlgorithm
    {
        void BehaveDifferently();
        void Move(ref Vector2 position, GameTime gameTime);
    }
}