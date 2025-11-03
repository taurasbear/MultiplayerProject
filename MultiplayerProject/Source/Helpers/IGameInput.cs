namespace MultiplayerProject.Source
{
    public interface IGameInput
    {
        KeyboardMovementInput GetMovementInput(InputInformation inputInfo);
    }
}
