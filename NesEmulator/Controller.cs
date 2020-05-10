using System;
using System.Windows.Input;
using SharpDX.XInput;

namespace NesEmulator
{
    public abstract class Controller
    {
        public abstract byte PollKeysPressed();
    }

    public class NoController : Controller
    {
        public override byte PollKeysPressed()
        {
            return 0;
        }
    }

    public class XInputController : Controller
    {
        SharpDX.XInput.Controller controller;

        public XInputController()
        {
            this.controller = new SharpDX.XInput.Controller(UserIndex.One);
        }

        public override byte PollKeysPressed()
        {
            int state = (int)controller.GetState().Gamepad.Buttons;
            byte _A, _B, _Start, _Select, _Up, _Down, _Left, _Right;

            _A = (byte)(((state & (int)GamepadButtonFlags.A) > 0) ? 0b1000_0000 : 0);
            _B = (byte)(((state & ((int)GamepadButtonFlags.B | (int)GamepadButtonFlags.X)) > 0) ? 0b0100_0000 : 0);
            _Start = (byte)(((state & (int)GamepadButtonFlags.Start) > 0) ? 0b0010_0000 : 0);
            _Select = (byte)(((state & (int)GamepadButtonFlags.Back) > 0) ? 0b0001_0000 : 0);
            _Up = (byte)(((state & (int)GamepadButtonFlags.DPadUp) > 0) ? 0b0000_1000 : 0);
            _Down = (byte)(((state & (int)GamepadButtonFlags.DPadDown) > 0) ? 0b0000_0100 : 0);
            _Left = (byte)(((state & (int)GamepadButtonFlags.DPadLeft) > 0) ? 0b0000_0010 : 0);
            _Right = (byte)(((state & (int)GamepadButtonFlags.DPadRight) > 0) ? 0b0000_0001 : 0);

            return (byte)(_A | _B | _Start | _Select | _Up | _Down | _Left | _Right);
        }

    }

    public class KeyboardController : Controller
    {
        Key A, B, Start, Select, Up, Down, Left, Right;

        public override byte PollKeysPressed()
        {
            byte _A, _B, _Start, _Select, _Up, _Down, _Left, _Right;
            _A = (byte)(Keyboard.IsKeyDown(A) ? 0b1000_0000 : 0);
            _B = (byte)(Keyboard.IsKeyDown(B) ? 0b0100_0000 : 0);
            _Start = (byte)(Keyboard.IsKeyDown(Start) ? 0b0010_0000 : 0);
            _Select = (byte)(Keyboard.IsKeyDown(Select) ? 0b0001_0000 : 0);
            _Up = (byte)(Keyboard.IsKeyDown(Up) ? 0b0000_1000 : 0);
            _Down = (byte)(Keyboard.IsKeyDown(Down) ? 0b0000_0100 : 0);
            _Left = (byte)(Keyboard.IsKeyDown(Left) ? 0b0000_0010 : 0);
            _Right = (byte)(Keyboard.IsKeyDown(Right) ? 0b0000_0001 : 0);
            return (byte)(_A | _B | _Start | _Select | _Up | _Down | _Left | _Right);
        }

        public KeyboardController()
        {
            // Can be used to map keys to other keys later
            A = Key.Z;
            B = Key.X;
            Start = Key.A;
            Select = Key.S;
            Up = Key.Up;
            Down = Key.Down;
            Left = Key.Left;
            Right = Key.Right;
        }

    }
}
