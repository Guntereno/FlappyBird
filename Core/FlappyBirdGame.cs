using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FlappyBird.Core;

public class FlappyBirdGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private GameWorld _levelManager;

    public FlappyBirdGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

        _graphics.PreferredBackBufferWidth = screenWidth;
        _graphics.PreferredBackBufferHeight = screenHeight;

        _graphics.IsFullScreen = true;
        _graphics.SynchronizeWithVerticalRetrace = true;


        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        Services.AddService(Content);

        CreateLevel();

        base.Initialize();
    }

    private void CreateLevel()
    {
        Components.Add(_levelManager = new GameWorld(this));
        _levelManager.OnPlayerDeath += RestartGame;
    }


    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Services.AddService(_spriteBatch);
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
