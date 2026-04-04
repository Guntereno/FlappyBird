using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Momo.Ui;
using MonoGame.Extended;

namespace FlappyBird.Core;

public class UserInterface : DrawableGameComponent
{
    private static readonly string GAME_OVER_FORMAT = "Game Over!\nScore: {0}";
    private static readonly string SCORE_LABEL = "Score";
    private static readonly string HIGH_SCORE_LABEL = "High";

    private SpriteBatch? _spriteBatch = null;

    private SpriteFont? _font = null;

    private OrthographicCamera? _camera;


    private TextBox _introText = new TextBox();
    private TextBox _scoreLabel = new TextBox();
    private TextBox _scoreValue = new TextBox();


    private TextBox _highScoreLabel = new TextBox();
    private TextBox _highScoreValue = new TextBox();

    private TextBox _gameOverText = new TextBox();

    private GameWorld.State _state = GameWorld.State.Intro;

    private int _score = 0;

    private Rectangle CameraBounds
    {
        get
        {
            if(_camera == null)
            {
                throw new Exception("Camera not initialised.");
            }

            return _camera.BoundingRectangle.ToRectangle();
        }
    }

    public override void Initialize()
    {
        OnViewportUpdated(Game.GraphicsDevice);

        base.Initialize();
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
                _scoreLabel.Draw(_spriteBatch);
                _scoreValue.Draw(_spriteBatch);
                _highScoreLabel.Draw(_spriteBatch);
                _highScoreValue.Draw(_spriteBatch);
                break;
            case GameWorld.State.GameOver:
                _gameOverText.Draw(_spriteBatch);
                break;
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public void OnViewportUpdated(GraphicsDevice graphicsDevice)
    {
        _camera = new OrthographicCamera(graphicsDevice);
        UpdateLayout();
    }

    internal UserInterface(Game game, GameWorld gameWorld) : base(game)
    {
    }

    internal void SetScore(int score)
    {
        _score =  score;
        _scoreValue.Text = score.ToString();
    }

    internal void SetHighScore(int highScore)
    {
        _highScoreValue.Text = highScore.ToString();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        var content = Game.Services.GetService<ContentManager>();

        _font = content.Load<SpriteFont>("Fonts/Nunito-Black");

        InitDefaultTextBox(_introText, _font);
        _introText.Text = "Tap to Begin";


        InitDefaultTextBox(_scoreLabel, _font);
        _scoreLabel.Scale = .5f;
        _scoreLabel.Text = SCORE_LABEL;

        InitDefaultTextBox(_scoreValue, _font);
        _scoreValue.Text = "0";


        InitDefaultTextBox(_highScoreLabel, _font);
        _highScoreLabel.Scale = .5f;
        _highScoreLabel.Text = HIGH_SCORE_LABEL;

        InitDefaultTextBox(_highScoreValue, _font);
        _highScoreValue.Text = "0";


        InitDefaultTextBox(_gameOverText, _font);
        _gameOverText.HAlign = UiElement.HorizontalAlign.Center;

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

    private void UpdateLayout()
    {
        Rectangle bounds = CameraBounds;

        _introText.DrawRect = bounds;

        const int BORDER = 20;
        const int ELEMENT_WIDTH = 300;
        const int LABEL_HEIGHT = 60;
        const int VALUE_HEIGHT = 100;

        int scoreLeft = bounds.Center.X - (ELEMENT_WIDTH / 2);
        _scoreLabel.DrawRect = new Rectangle(scoreLeft, BORDER, ELEMENT_WIDTH, LABEL_HEIGHT);
        _scoreValue.DrawRect = new Rectangle(scoreLeft, _scoreLabel.DrawRect.Bottom, ELEMENT_WIDTH, VALUE_HEIGHT);

        int highScoreLeft = bounds.Right - BORDER - ELEMENT_WIDTH;

        _highScoreLabel.DrawRect = new Rectangle(highScoreLeft, BORDER, ELEMENT_WIDTH, LABEL_HEIGHT);
        _highScoreValue.DrawRect = new Rectangle(highScoreLeft, _highScoreLabel.DrawRect.Bottom, ELEMENT_WIDTH, VALUE_HEIGHT);

        _gameOverText.DrawRect = bounds;
    }

    internal void HandleGameWorldStateChanged(GameWorld.State state)
    {
        switch (state)
        {
            case GameWorld.State.GameOver:
                {
                    _gameOverText.Text = string.Format(GAME_OVER_FORMAT, _score);
                }
                break;

            case GameWorld.State.Gameplay:
                {
                    _score = 0;
                    _scoreValue.Text = _score.ToString();
                }
                break;
        }
        _state = state;
    }
}
