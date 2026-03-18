using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FlappyBird.Engine;

public class InputManager
{
    private InputMapper _mapper;

    private KeyboardState _currentKeyboard;
    private MouseState _currentMouse;
    private GamePadState _currentGamePad;

    private KeyboardState _previousKeyboard;
    private MouseState _previousMouse;
    private GamePadState _previousGamePad;

    public InputManager()
    {
        _mapper = new InputMapper();
    }

    public void Update()
    {
        _previousKeyboard = _currentKeyboard;
        _previousMouse = _currentMouse;
        _previousGamePad = _currentGamePad;

        _currentKeyboard = Keyboard.GetState();
        _currentMouse = Mouse.GetState();
        _currentGamePad = GamePad.GetState(PlayerIndex.One);
    }

    public bool IsActionPressed(string action)
    {
        return _mapper.IsActionPressed(action, _currentKeyboard, _currentMouse, _currentGamePad);
    }

    public bool IsActionJustPressed(string action)
    {
        return _mapper.IsActionJustPressed(action, _currentKeyboard, _currentMouse, _currentGamePad,
                                           _previousKeyboard, _previousMouse, _previousGamePad);
    }

    public InputMapper Mapper => _mapper;
}