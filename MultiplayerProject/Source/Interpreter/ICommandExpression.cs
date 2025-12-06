using System;

namespace MultiplayerProject.Source
{
    public interface ICommandExpression
    {
        string Interpret(GameCommandContext context);
    }
}