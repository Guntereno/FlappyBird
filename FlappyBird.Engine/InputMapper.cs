using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace FlappyBird.Engine;

public class InputMapper
{
    private readonly Dictionary<FName, List<InputKey>> _mappings = new();

    public void AddMapping(FName action, params InputKey[] keys)
    {
        _mappings[action] = new List<InputKey>(keys);
    }

    public bool IsActionPressed(FName action, KeyboardState keyboard, MouseState mouse, GamePadState gamePad)
    {
        if (!_mappings.TryGetValue(action, out var keys))
            return false;
        return keys.Any(key => key.IsPressed(keyboard, mouse, gamePad));
    }

    public bool IsActionJustPressed(FName action, KeyboardState keyboard, MouseState mouse, GamePadState gamePad,
                                    KeyboardState prevKeyboard, MouseState prevMouse, GamePadState prevGamePad)
    {
        if (!_mappings.TryGetValue(action, out var keys))
            return false;
        return keys.Any(key => key.IsJustPressed(keyboard, mouse, gamePad, prevKeyboard, prevMouse, prevGamePad));
    }
}

public readonly struct InputKey
{
    public Keys? KeyboardKey { get; }
    public bool IsMouseLeft { get; }
    public bool IsMouseRight { get; }
    public Buttons? GamePadButton { get; }

    public InputKey(Keys key) => KeyboardKey = key;
    public InputKey(bool leftMouse) => IsMouseLeft = leftMouse;
    public InputKey(Buttons button) => GamePadButton = button;

    public bool IsPressed(KeyboardState k, MouseState m, GamePadState g) =>
        (KeyboardKey.HasValue && k.IsKeyDown(KeyboardKey.Value)) ||
        (IsMouseLeft && m.LeftButton == ButtonState.Pressed) ||
        (GamePadButton.HasValue && g.IsButtonDown(GamePadButton.Value));

    public bool IsJustPressed(KeyboardState k, MouseState m, GamePadState g,
                              KeyboardState pk, MouseState pm, GamePadState pg) =>
        IsPressed(k, m, g) && !IsPressed(pk, pm, pg);
}
