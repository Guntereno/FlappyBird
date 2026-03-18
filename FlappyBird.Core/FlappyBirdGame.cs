using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FlappyBird.Core;

public class FlappyBirdGame : Game
{
    private GraphicsDeviceManager _graphics;

    private GameWorld _levelManager;

    public FlappyBirdGame()
    {
        _graphics = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";

        IsMouseVisible = true;

#if DEBUG
        int screenWidth = 768;
        int screenHeight = 1024;
        Window.AllowUserResizing = true;
#else
        int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphics.IsFullScreen = true;
#endif

        _graphics.PreferredBackBufferWidth = screenWidth;
        _graphics.PreferredBackBufferHeight = screenHeight;

        _graphics.SynchronizeWithVerticalRetrace = true;

        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        _graphics.GraphicsDevice.DeviceReset += OnDeviceReset;

        Services.AddService(Content);

        CreateLevel();

        base.Initialize();
    }

    private void OnDeviceReset(object sender, EventArgs e)
    {
        // When the graphics device resets (e.g., window resize or entering/exiting
        // fullscreen), update any cached viewport-dependent values in the game world.
        _levelManager?.OnGraphicsDeviceReset(GraphicsDevice);
    }


    private void CreateLevel()
    {
        Components.Add(_levelManager = new GameWorld(this));
        _levelManager.OnPlayerDeath += RestartGame;
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
        CreateLevel();
    }
}
