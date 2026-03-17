using FlappyBird.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FlappyBird.Core;

public class GameWorld : DrawableGameComponent
{
    public event Action OnPlayerDeath;

    private class Pipe
    {
        public float Position;
        public int Height;

        public Rectangle TopPipe;
        public Rectangle BottomPipe;
    }


    private const float PIPE_SPAWN_INTERVAL = 1f;
    private const int PIPE_OPENING_VARIANCE = 700;
    private const int PIPE_GAP_HEIGHT = 400;
    private const int PIPE_WIDTH = 200;

    private const int GROUND_HEIGHT = 100;

    private const float LEVEL_SPEED = 800.0f;


    private Rectangle _bounds;

    private float _pipeSpawnTimer = 0f;
    private float _groundScrollOffset = 0f;

    private SlicedSprite _pipeSprite = null;
    private Texture2D _groundTexture = null;

    private List<Pipe> _pipes = new List<Pipe>();

    private Rectangle _groundBounds;

    private Bird _bird;

    KeyboardState _previousKeyboardState = Keyboard.GetState();
    MouseState _previousMouseState = Mouse.GetState();
    GamePadState _previousGamePadState = GamePad.GetState(PlayerIndex.One);

    private DebugRenderer _debugRenderer;


    public GameWorld(Game game) : base(game)
    {
        _pipeSpawnTimer = 0f;

        _bounds = game.GraphicsDevice.Viewport.Bounds;

        _bird = new Bird(game);

        _debugRenderer = new DebugRenderer(128, 256, game.GraphicsDevice);

        _groundBounds = BuildGroundBounds();
    }

    public override void Update(GameTime gameTime)
    {
        UpdateInput();
        UpdatePipes(gameTime);
        UpdateGround(gameTime);

        _bird.Update(gameTime);

        if (CheckBirdCollision())
        {
            OnPlayerDeath?.Invoke();
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = Game.Services.GetService<SpriteBatch>();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap);

        foreach (var pipe in _pipes)
        {
            DrawPipe(spriteBatch, pipe);
        }

        DrawGround(spriteBatch);

        _bird.Draw(spriteBatch);

        spriteBatch.End();

#if DEBUG
        _debugRenderer.Begin();

        DebugRenderCollisionBoxes();

        Matrix viewMatrix = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
        Matrix projMatrix = Matrix.CreateOrthographicOffCenter(0, _bounds.Width, _bounds.Height, 0, 0, 1);
        _debugRenderer.End(viewMatrix, projMatrix);
#endif
    }


    protected override void LoadContent()
    {
        var content = Game.Services.GetService<ContentManager>();
        _pipeSprite = new SlicedSprite(
            content.Load<Texture2D>("GreenPipe"),
            new Rectangle(0, 0, 128, 128),
            new Rectangle(38, 49, 30, 28));

        _groundTexture = content.Load<Texture2D>("Ground");

        _bird.LoadContent(content);

        base.LoadContent();
    }


    private void UpdateGround(GameTime gameTime)
    {
        _groundScrollOffset += LEVEL_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
        while (_groundScrollOffset >= GROUND_HEIGHT)
        {
            _groundScrollOffset -= GROUND_HEIGHT;
        }
    }

    private void UpdatePipes(GameTime gameTime)
    {
        _pipeSpawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_pipeSpawnTimer >= PIPE_SPAWN_INTERVAL)
        {
            SpawnPipe();
            _pipeSpawnTimer = 0f;
        }

        // Update and remove off-screen pipes
        for (int i = _pipes.Count - 1; i >= 0; i--)
        {
            Pipe pipe = _pipes[i];

            pipe.Position -= LEVEL_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds; // Move pipe left
            RecalculatePipeRectangles(pipe);

            if (pipe.Position < -PIPE_WIDTH)
            {
                _pipes.RemoveAt(i);
            }
        }
    }

    private void UpdateInput()
    {
        KeyboardState keyboardState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();
        GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

        if ((keyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space)) ||
            (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released) ||
            (gamePadState.Buttons.A == ButtonState.Pressed && _previousGamePadState.Buttons.A == ButtonState.Released))
        {
            _bird.Jump();
        }

        _previousKeyboardState = keyboardState;
        _previousMouseState = mouseState;
        _previousGamePadState = gamePadState;
    }


    private void RecalculatePipeRectangles(Pipe pipe)
    {
        int screenHeight = _bounds.Height;

        int gapCenter = pipe.Height;
        int pipeX = (int)(pipe.Position - (PIPE_WIDTH / 2));

        int topPipeHeight = gapCenter - (PIPE_GAP_HEIGHT / 2);
        Point size = new Point(PIPE_WIDTH, topPipeHeight);
        Rectangle dest = new Rectangle(pipeX, 0, PIPE_WIDTH, topPipeHeight);
        pipe.TopPipe = dest;

        int bottomPipeHeight = screenHeight - (gapCenter + (PIPE_GAP_HEIGHT / 2));
        size.Y = bottomPipeHeight;
        dest.Y = screenHeight - bottomPipeHeight;
        dest.Height = bottomPipeHeight;
        pipe.BottomPipe = dest;
    }

    private void SpawnPipe()
    {
        Pipe pipe = new Pipe()
        {
            Position = _bounds.Width + (PIPE_WIDTH / 2),
            Height = (_bounds.Height / 2) - (PIPE_OPENING_VARIANCE / 2) + (int)(new Random().NextDouble() * PIPE_OPENING_VARIANCE)
        };
        _pipes.Add(pipe);
    }

    private void DrawPipe(SpriteBatch spriteBatch, Pipe pipe)
    {
        _pipeSprite.Draw(spriteBatch, pipe.TopPipe, Color.White);
        _pipeSprite.Draw(spriteBatch, pipe.BottomPipe, Color.White);
    }

    private void DrawGround(SpriteBatch spriteBatch)
    {
        Rectangle destRect = _groundBounds;

        float numRepeats = (float)_bounds.Width / GROUND_HEIGHT;
        int sampleX = (int)(_groundScrollOffset / GROUND_HEIGHT * _groundTexture.Width);
        int sampleWidth = (int)(_groundTexture.Width * numRepeats);
        Rectangle sourceRect = new Rectangle(sampleX, 0, sampleWidth, _groundTexture.Height);

        spriteBatch.Draw(_groundTexture, destRect, sourceRect, Color.White);
    }

    private Rectangle BuildGroundBounds()
    {
        int groundY = _bounds.Height - GROUND_HEIGHT;
        Rectangle destRect = new Rectangle(0, groundY, _bounds.Width, GROUND_HEIGHT);
        return destRect;
    }

    private bool CheckBirdCollision()
    {
        Rectangle birdBounds = _bird.CollisionBounds;

        if (_pipes.Count > 0)
        {
            Pipe pipe = _pipes[0];

            // Check collision with top pipe
            if (birdBounds.Intersects(pipe.TopPipe))
                return true;

            // Check collision with bottom pipe
            if (birdBounds.Intersects(pipe.BottomPipe))
                return true;
        }

        // Check collision with ground
        if (birdBounds.Intersects(_groundBounds))
            return true;

        return false;
    }

    private void DebugRenderCollisionBoxes()
    {
        Rectangle birdBounds = _bird.CollisionBounds;
        _debugRenderer.DrawRect(birdBounds, Color.Blue.WithAlpha(0.2f), Color.White, true, 1.0f);

        if (_pipes.Count > 0)
        {
            Pipe pipe = _pipes[0];
            _debugRenderer.DrawRect(pipe.TopPipe, Color.Green.WithAlpha(0.2f), Color.White, true, 1.0f);
            _debugRenderer.DrawRect(pipe.BottomPipe, Color.Green.WithAlpha(0.2f), Color.White, true, 1.0f);
        }

        _debugRenderer.DrawRect(_groundBounds, Color.Brown.WithAlpha(0.2f), Color.White, true, 1.0f);
    }
}