namespace MultiplayerProject.Source.GameObjects.Enemy
{
    public interface IPrototype<out T>
    {
        T ShallowClone();
        object DeepClone();
    }
}