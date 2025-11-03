using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using static MultiplayerProject.Source.GameScene;

namespace MultiplayerProject.Source
{
    public class KeyboardAdapter : IGameInput
    {
        private Dictionary<Keys, IInputCommand> _keyCommandMap;

        public KeyboardAdapter(Dictionary<Keys, IInputCommand> keyCommandMap)
        {
            _keyCommandMap = keyCommandMap;
        }

        public KeyboardMovementInput GetMovementInput(InputInformation inputInfo)
        {
            KeyboardMovementInput input = new KeyboardMovementInput();

            foreach (var kvp in _keyCommandMap)
            {
                if (inputInfo.CurrentKeyboardState.IsKeyDown(kvp.Key))
                {
                    kvp.Value.Execute(input);
                }
            }

            return input;
        }
    }
}
