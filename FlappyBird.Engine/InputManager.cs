using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace FlappyBird.Engine;

public class InputManager
{
    private InputMapper _mapper;

    private KeyboardState _currentKeyboard;
    private MouseState _currentMouse;
    private GamePadState _currentGamePad;
    private TouchCollection _currentTouch;

    private KeyboardState _previousKeyboard;
    private MouseState _previousMouse;
    private GamePadState _previousGamePad;
    private TouchCollection _previousTouch;



    public InputManager()
    {
        _mapper = new InputMapper();
    }

    public void Update()
    {
        _previousKeyboard = _currentKeyboard;
        _previousMouse = _currentMouse;
        _previousGamePad = _currentGamePad;
        _previousTouch = _currentTouch;

        _currentKeyboard = Keyboard.GetState();
        _currentMouse = Mouse.GetState();
        _currentGamePad = GamePad.GetState(PlayerIndex.One);
        _currentTouch = TouchPanel.GetState();
    }

    public bool IsActionPressed(FName action)
    {
        return _mapper.IsActionPressed(action, _currentKeyboard, _currentMouse, _currentGamePad, _currentTouch);
    }

    public bool IsActionJustPressed(FName action)
    {
        return _mapper.IsActionJustPressed(action, _currentKeyboard, _currentMouse, _currentGamePad, _currentTouch,
                                           _previousKeyboard, _previousMouse, _previousGamePad, _previousTouch);
    }

    public InputMapper Mapper => _mapper;
}