using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Momo.Ui;

namespace FlappyBird.Core;

public class UserInterface : DrawableGameComponent
{
    private SpriteBatch? _spriteBatch = null;

    public SpriteFont? _font = null;

    private Rectangle _bounds;

    TextBox _scoreText = new TextBox();

    public UserInterface(Game game, GameWorld gameWorld) : base(game)
    {
        CalculateBounds(game.GraphicsDevice);
    }

    public void SetScore(int score)
    {
        _scoreText.Text = score.ToString();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        var content = Game.Services.GetService<ContentManager>();

        _font = content.Load<SpriteFont>("Fonts/Nunito-Black");

        _scoreText.Font = _font;
        _scoreText.TextColor = Color.White;
        _scoreText.HAlign = UiElement.HorizontalAlign.Center;
        _scoreText.VAlign = UiElement.VerticalAlign.Center;
        _scoreText.Text = "0";
        _scoreText.Scale = 1.0f;

        UpdateLayout();

        base.LoadContent();
    }

    public override void Draw(GameTime gameTime)
    {
        if (_spriteBatch == null)
            throw new Exception("LoadContent must be called before drawing!");

        _spriteBatch.Begin();

        _scoreText.Draw(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public void OnGraphicsDeviceReset(GraphicsDevice graphicsDevice)
    {
        CalculateBounds(graphicsDevice);
    }

    private void CalculateBounds(GraphicsDevice graphicsDevice)
    {
        int screenWidth = graphicsDevice.Viewport.Width;
        int screenHeight = graphicsDevice.Viewport.Height;

        _bounds = new Rectangle(0, 0, screenWidth, screenHeight);
    }

    private void UpdateLayout()
    {
        _scoreText.DrawRect = new Rectangle(20, 20, _bounds.Width - 40, 128);
    }
}
