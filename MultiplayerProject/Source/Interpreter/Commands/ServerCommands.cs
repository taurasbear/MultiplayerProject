using System.Text;
using System.Reflection;
using System.Linq;

namespace MultiplayerProject.Source.Commands
{
    public class ServerCommand : ICommandExpression
    {
        public enum ServerAction
        {
            Help
        }

        private readonly ServerAction _action;

        public ServerCommand(ServerAction action)
        {
            _action = action;
        }

        public string Interpret(GameCommandContext context)
        {
            switch (_action)
            {
                case ServerAction.Help:
                    return ShowHelp(context);
                default:
                    return "Error: Unknown server action.";
            }
        }

        private string ShowHelp(GameCommandContext context)
        {
            var sb = new StringBuilder();
            sb.Append("=== COMMAND REFERENCE ===|");
            sb.Append("|Available Commands:|");
            sb.Append("  /stats                      - Show game statistics|");
            sb.Append("  /help                       - Show this help|");
            sb.Append("  /list                       - Show connected players|");
            sb.Append("  /info <player>              - Show player details|");
            sb.Append("  /set_score <player> <score> - Set player score|");
            sb.Append("|");
            sb.Append("Controls: ~ or / (toggle), Enter (execute), Escape (cancel)|");
            return sb.ToString();
        }
    }

    public class HelpCommand : ICommandExpression
    {
        public string Interpret(GameCommandContext context)
        {
            return new ServerCommand(ServerCommand.ServerAction.Help).Interpret(context);
        }
    }
}