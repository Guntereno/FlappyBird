using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FlappyBird.Core;

public enum Platform
{
    Android,
    Desktop
}

public class FlappyBirdGame : Game
{
    private GraphicsDeviceManager _graphics = null;

    private GameWorld _levelManager = null;
    private UserInterface _userInterface = null;

    public FlappyBirdGame(Platform platform)
    {
        _graphics = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";

        IsMouseVisible = true;

        bool windowed = false;

#if DEBUG
        if (platform == Platform.Desktop)
            windowed = true;
#endif

        int screenWidth, screenHeight;

        if(windowed)
        {
            screenWidth = 1024;
            screenHeight = 768;
            Window.AllowUserResizing = true;
        }
        else
        {
            screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
        }


        _graphics.PreferredBackBufferWidth = screenWidth;
        _graphics.PreferredBackBufferHeight = screenHeight;

        _graphics.SynchronizeWithVerticalRetrace = true;

        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        _graphics.GraphicsDevice.DeviceReset += OnDeviceReset;

        Momo.System.Resources.Initialize(_graphics.GraphicsDevice);

        Services.AddService(Content);

        CreateLevel();

        base.Initialize();
    }

    private void OnDeviceReset(object sender, EventArgs e)
    {
        // When the graphics device resets (e.g., window resize or entering/exiting
        // fullscreen), update any cached viewport-dependent values in the game world.
        _levelManager?.OnGraphicsDeviceReset(GraphicsDevice);
        _userInterface?.OnGraphicsDeviceReset(GraphicsDevice);
    }


    private void CreateLevel()
    {
        Components.Add(_levelManager = new GameWorld(this));
        _levelManager.OnPlayerDeath += RestartGame;

        Components.Add(_userInterface = new UserInterface(this, _levelManager));

        _levelManager.OnScoreChanged += _userInterface.SetScore;
    }

    protected override void Update(GameTime gameTime)
    {
        GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
        KeyboardState keyboardState = Keyboard.GetState();

        if (gamePadState.Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);
    }

    public void RestartGame()
    {
        Components.Remove(_levelManager);
        Components.Remove(_userInterface);

        CreateLevel();
    }
}
