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
    private GraphicsDeviceManager _graphics;

    private GameWorld? _levelManager = null;
    private UserInterface? _userInterface = null;

    private int _highScore = 0;

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
            screenWidth = 1920;
            screenHeight = 1080;
            Window.AllowUserResizing = true;
        }
        else
        {
            screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
        }

        Window.ClientSizeChanged += OnViewportUpdated;

        _graphics.PreferredBackBufferWidth = screenWidth;
        _graphics.PreferredBackBufferHeight = screenHeight;

        _graphics.SynchronizeWithVerticalRetrace = true;

        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        _graphics.GraphicsDevice.DeviceReset += OnViewportUpdated;

        Momo.System.Resources.Initialize(_graphics.GraphicsDevice);

        Services.AddService(Content);

        CreateLevel();

        base.Initialize();
    }

    private void OnViewportUpdated(object? sender, EventArgs e)
    {
        // When the graphics device resets (e.g., window resize or entering/exiting
        // fullscreen), update any cached viewport-dependent values in the game world.
        _levelManager?.OnViewportUpdated();
        _userInterface?.OnViewportUpdated(GraphicsDevice);
    }


    private void CreateLevel()
    {
        Components.Add(_levelManager = new GameWorld(this));
        Components.Add(_userInterface = new UserInterface(this, _levelManager));

        _levelManager.OnStateChange += HandleGameWorldStateChanged;
        _levelManager.OnScoreChanged += HandleScoreChanged;
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

    public void HandleGameWorldStateChanged(GameWorld.State state)
    {
        if (_userInterface != null)
            _userInterface.HandleGameWorldStateChanged(state);
    }

    public void HandleScoreChanged(int score)
    {
        if(score > _highScore)
        {
            _highScore = score;
            _userInterface?.SetHighScore(_highScore);
        }

        _userInterface?.SetScore(score);
    }
}
