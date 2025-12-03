// File: IVisitable.cs
// Location: MultiplayerProject/Source/Visitors/IVisitable.cs
using MultiplayerProject.Source.Helpers;
namespace MultiplayerProject.Source.Visitors
{
    /// <summary>
    /// Interface for objects that can accept visitors.
    /// Part of the Visitor pattern implementation.
    /// </summary>
    public interface IVisitable
    {
        void Accept(IGameObjectVisitor visitor);
    }
}