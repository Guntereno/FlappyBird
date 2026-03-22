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
    private const int PIPE_GAP_VARIANCE = 700; // Variance in the gap between top and bottom pipes
    private const int PIPE_GAP_HEIGHT = 400;
    private const int PIPE_WIDTH = 200;

    // Fixed world height (constant gameplay space). Wider screens show more world horizontally.
    private const float WORLD_HEIGHT = 1080f;

    // Ground texture is a square, so this is both the width and height of a single tile.
    private const int GROUND_TILE_SIZE = 100;

    private const float LEVEL_SPEED = 800.0f;

    private static readonly FName ACTION_JUMP = new FName("Jump");
    private static readonly FName ACTION_PAUSE = new FName("Pause");


    private SpriteBatch _spriteBatch;

    private InputManager _inputManager;

    private Rectangle _bounds;
    private Matrix _cameraMatrix;

    private float _pipeSpawnTimer = 0f;
    private float _groundScrollOffset = 0f;

    private SlicedSprite _pipeSprite = null;
    private Texture2D _groundTexture = null;

    private List<Pipe> _pipes = new List<Pipe>();

    private Rectangle _groundBounds;

    private Bird _bird;

    private bool _isPaused = false;
    private bool _isGameOver = false;

    private Random _random = new Random(1234);

    private DebugRenderer _debugRenderer;


    public GameWorld(Game game) : base(game)
    {
        _pipeSpawnTimer = 0f;

        CalculateBounds(game.GraphicsDevice);

        _bird = new Bird(game);

        _debugRenderer = new DebugRenderer(128, 256, game.GraphicsDevice);

        _inputManager = new InputManager();
        _inputManager.Mapper.AddMapping(ACTION_JUMP, new InputKey(Keys.Space), new InputKey(true), new InputKey(Buttons.A), InputKey.Touch());
        _inputManager.Mapper.AddMapping(ACTION_PAUSE, new InputKey(Keys.P), new InputKey(Keys.Escape));
    }

    public void OnGraphicsDeviceReset(GraphicsDevice graphicsDevice)
    {
        CalculateBounds(graphicsDevice);
    }

    public override void Update(GameTime gameTime)
    {
        UpdateInput(); // Always check input (for pause handling)

        if (!_isPaused && !_isGameOver)
        {
            UpdatePipes(gameTime);
            UpdateGround(gameTime);
            _bird.Update(gameTime);

            if (CheckBirdCollision())
            {
                _isGameOver = true;
                OnPlayerDeath?.Invoke();
            }
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearWrap,
            null,
            null,
            null,
            _cameraMatrix);

        foreach (var pipe in _pipes)
        {
            DrawPipe(_spriteBatch, pipe);
        }

        DrawGround(_spriteBatch);

        _bird.Draw(_spriteBatch);

        _spriteBatch.End();

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
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        var content = Game.Services.GetService<ContentManager>();
        _pipeSprite = new SlicedSprite(
            content.Load<Texture2D>("GreenPipe"),
            new Rectangle(0, 0, 128, 128),
            new Rectangle(38, 49, 30, 28));

        _groundTexture = content.Load<Texture2D>("Ground");

        _bird.LoadContent(content);

        base.LoadContent();
    }


    private void CalculateBounds(GraphicsDevice graphicsDevice)
    {
        float screenWidth = graphicsDevice.Viewport.Width;
        float screenHeight = graphicsDevice.Viewport.Height;

        // Keep the vertical world size constant and expand horizontally based on aspect.
        float worldWidth = WORLD_HEIGHT * (screenWidth / screenHeight);
        _bounds = new Rectangle(0, 0, (int)MathF.Ceiling(worldWidth), (int)WORLD_HEIGHT);

        int groundY = _bounds.Height - GROUND_TILE_SIZE;
        _groundBounds = new Rectangle(0, groundY, _bounds.Width, GROUND_TILE_SIZE);

        // For SpriteBatch, use a scale matrix to map world units to screen pixels.
        float scale = screenHeight / WORLD_HEIGHT;
        _cameraMatrix = Matrix.CreateScale(scale, scale, 1f);
    }

    private void UpdateGround(GameTime gameTime)
    {
        _groundScrollOffset += LEVEL_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
        while (_groundScrollOffset >= GROUND_TILE_SIZE)
        {
            _groundScrollOffset -= GROUND_TILE_SIZE;
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
        _inputManager.Update();

        // Handle pause (always checkable)
        if (_inputManager.IsActionJustPressed(ACTION_PAUSE))
        {
            _isPaused = !_isPaused;
        }

        // Handle other inputs only if not paused
        if (!_isPaused)
        {
            if (_inputManager.IsActionJustPressed(ACTION_JUMP))
            {
                _bird.Jump();
            }
        }
    }


    private void RecalculatePipeRectangles(Pipe pipe)
    {
        int screenHeight = _bounds.Height;

        int gapCenter = pipe.Height;
        int pipeX = (int)(pipe.Position - (PIPE_WIDTH / 2));

        int topPipeHeight = gapCenter - (PIPE_GAP_HEIGHT / 2);
        Rectangle dest = new Rectangle(pipeX, 0, PIPE_WIDTH, topPipeHeight);
        pipe.TopPipe = dest;

        int bottomPipeHeight = screenHeight - (gapCenter + (PIPE_GAP_HEIGHT / 2));
        dest.Y = screenHeight - bottomPipeHeight;
        dest.Height = bottomPipeHeight;
        pipe.BottomPipe = dest;
    }

    private void SpawnPipe()
    {
        Pipe pipe = new Pipe()
        {
            Position = _bounds.Width + (PIPE_WIDTH / 2),
            Height = (_bounds.Height / 2) - (PIPE_GAP_VARIANCE / 2) + (int)(_random.NextDouble() * PIPE_GAP_VARIANCE)
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

        float numRepeats = (float)_bounds.Width / GROUND_TILE_SIZE;
        int sampleX = (int)(_groundScrollOffset / GROUND_TILE_SIZE * _groundTexture.Width);
        int sampleWidth = (int)(_groundTexture.Width * numRepeats);
        Rectangle sourceRect = new Rectangle(sampleX, 0, sampleWidth, _groundTexture.Height);

        spriteBatch.Draw(_groundTexture, destRect, sourceRect, Color.White);
    }

    private bool CheckBirdCollision()
    {
        Rectangle birdBounds = _bird.CollisionBounds;

        if (_pipes.Count > 0)
        {
            // Only check collision with the first pipe, since pipes are spaced out enough that
            // the bird can't collide with more than one at a time
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
