using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MultiplayerProject.Source
{
    public class GamepadAdapter : IGameInput
    {
        public KeyboardMovementInput GetMovementInput(InputInformation inputInfo)
        {
            KeyboardMovementInput input = new KeyboardMovementInput();

            if(inputInfo.CurrentGamePadState.ThumbSticks.Left.X < -0.2f || inputInfo.CurrentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                input.LeftPressed = true;
            }
            if(inputInfo.CurrentGamePadState.ThumbSticks.Left.X > 0.2f || inputInfo.CurrentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                input.RightPressed = true;
            }

            if(inputInfo.CurrentGamePadState.ThumbSticks.Left.Y > 0.2f || inputInfo.CurrentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                input.UpPressed = true;
            }

            if(inputInfo.CurrentGamePadState.ThumbSticks.Left.Y < -0.2f || inputInfo.CurrentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                input.DownPressed = true;
            }

            if(inputInfo.CurrentGamePadState.Buttons.A == ButtonState.Pressed)
            {
                input.FirePressed = true;
            }

            return input;
        }
    }
}
