using MultiplayerProject.Source.Helpers;
namespace MultiplayerProject.Source.Visitors
{
    public interface IVisitable
    {
        void Accept(IGameObjectVisitor visitor);
    }
}