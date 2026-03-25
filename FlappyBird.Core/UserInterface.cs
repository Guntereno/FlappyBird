using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Momo.Ui;
using Momo.Graphics;

namespace FlappyBird.Core;

public class UserInterface : DrawableGameComponent
{
    private static readonly string GAME_OVER_FORMAT = "Game Over!\nScore: {0}";

    private SpriteBatch? _spriteBatch = null;

    public SpriteFont? _font = null;

    private Rectangle _bounds;

    private TextBox _introText = new TextBox();
    private TextBox _scoreText = new TextBox();
    private TextBox _gameOverText = new TextBox();

    private SlicedSprite? _background = null;

    private GameWorld.State _state = GameWorld.State.Intro;

    private int _score = 0;

    public UserInterface(Game game, GameWorld gameWorld) : base(game)
    {
        CalculateBounds(game.GraphicsDevice);
    }

    public void SetScore(int score)
    {
        _score =  score;
        _scoreText.Text = score.ToString();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        var content = Game.Services.GetService<ContentManager>();

        _font = content.Load<SpriteFont>("Fonts/Nunito-Black");

        InitDefaultTextBox(_introText, _font);
        _introText.Text = "Tap to Begin";

        InitDefaultTextBox(_scoreText, _font);
        _scoreText.Text = "0";

        InitDefaultTextBox(_gameOverText, _font);
        _gameOverText.HAlign = UiElement.HorizontalAlign.Center;

        _background = new SlicedSprite(
            content.Load<Texture2D>("Sprites/UI/Background"),
            new Rectangle(0, 0, 256, 256),
            new Rectangle(30, 32, 197, 193)
        );

        UpdateLayout();

        base.LoadContent();
    }

    private static void InitDefaultTextBox(TextBox textBox, SpriteFont font)
    {
        textBox.Font = font;
        textBox.TextColor = Color.White;
        textBox.HAlign = UiElement.HorizontalAlign.Center;
        textBox.VAlign = UiElement.VerticalAlign.Center;
        textBox.Scale = 1.0f;
    }

    public override void Draw(GameTime gameTime)
    {
        if (_spriteBatch == null)
            throw new Exception("LoadContent must be called before drawing!");

        _spriteBatch.Begin();

        switch (_state)
        {
            case GameWorld.State.Intro:
                _introText.Draw(_spriteBatch);
                break;
            case GameWorld.State.Gameplay:
                _scoreText.Draw(_spriteBatch);
                break;
            case GameWorld.State.GameOver:
                _gameOverText.Draw(_spriteBatch);
                break;
        }

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
        _introText.DrawRect = _bounds;
        _scoreText.DrawRect = new Rectangle(20, 20, _bounds.Width - 40, 128);
        _gameOverText.DrawRect = _bounds;
    }

    internal void HandleGameWorldStateChanged(GameWorld.State state)
    {
        if(state == GameWorld.State.GameOver)
        {
            _gameOverText.Text = string.Format(GAME_OVER_FORMAT, _score);
        }

        _state = state;
    }
}
