// File: ICommandExpression.cs
// Location: MultiplayerProject/Source/Interpreter/ICommandExpression.cs

using System;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Abstract Expression interface for the Interpreter pattern.
    /// All command expressions must implement this interface.
    /// </summary>
    public interface ICommandExpression
    {
        /// <summary>
        /// Interpret and execute the command in the given context
        /// </summary>
        /// <param name="context">The game context containing server state and operations</param>
        /// <returns>Result message of the command execution</returns>
        string Interpret(GameCommandContext context);
    }
}