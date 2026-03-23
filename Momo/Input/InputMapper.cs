using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using System.Linq;
using Momo.System;

namespace Momo.Input;

public class InputMapper
{
    private readonly Dictionary<FName, List<InputKey>> _mappings = new();

    public void AddMapping(FName action, params InputKey[] keys)
    {
        _mappings[action] = new List<InputKey>(keys);
    }

    public bool IsActionPressed(FName action, KeyboardState keyboard, MouseState mouse, GamePadState gamePad, TouchCollection touches)
    {
        if (!_mappings.TryGetValue(action, out var keys))
            return false;
        return keys.Any(key => key.IsPressed(keyboard, mouse, gamePad, touches));
    }

    public bool IsActionJustPressed(FName action, KeyboardState keyboard, MouseState mouse, GamePadState gamePad, TouchCollection touches,
                                    KeyboardState prevKeyboard, MouseState prevMouse, GamePadState prevGamePad, TouchCollection prevTouches
                                    )
    {
        if (!_mappings.TryGetValue(action, out var keys))
            return false;
        return keys.Any(key => key.IsJustPressed(keyboard, mouse, gamePad, touches, prevKeyboard, prevMouse, prevGamePad, prevTouches));
    }
}

public readonly struct InputKey
{
    public Keys? KeyboardKey { get; }
    public bool IsMouseLeft { get; }
    public bool IsMouseRight { get; }
    public Buttons? GamePadButton { get; }
    public bool IsScreenTouch { get; }

    private InputKey(Keys? key = null, bool mouseLeft = false, bool mouseRight = false,
                     Buttons? button = null, bool touch = false)
    {
        KeyboardKey = key;
        IsMouseLeft = mouseLeft;
        IsMouseRight = mouseRight;
        GamePadButton = button;
        IsScreenTouch = touch;
    }


    public static InputKey Touch() => new InputKey(touch: true);

    public InputKey(Keys key) => KeyboardKey = key;
    public InputKey(bool leftMouse) => IsMouseLeft = leftMouse;
    public InputKey(Buttons button) => GamePadButton = button;

    public bool IsPressed(KeyboardState keyboard, MouseState mouse, GamePadState gamepad, TouchCollection touches) =>
        (KeyboardKey.HasValue && keyboard.IsKeyDown(KeyboardKey.Value)) ||
        (IsMouseLeft && mouse.LeftButton == ButtonState.Pressed) ||
        (GamePadButton.HasValue && gamepad.IsButtonDown(GamePadButton.Value)) ||
        (IsScreenTouch && (touches.Count > 0));
    public bool IsJustPressed(KeyboardState keyboard, MouseState mouse, GamePadState gamepad, TouchCollection touches,
                              KeyboardState prevKeyboard, MouseState prevMouse, GamePadState prevGamepad, TouchCollection prevTouch) =>
        IsPressed(keyboard, mouse, gamepad, touches) && !IsPressed(prevKeyboard, prevMouse, prevGamepad, prevTouch);
}
