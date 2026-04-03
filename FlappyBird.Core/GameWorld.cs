using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Momo.Audio;
using Momo.Graphics;
using Momo.Input;
using Momo.Maths;
using Momo.System;
using MonoGame.Extended.Graphics;

namespace FlappyBird.Core;

public class GameWorld : DrawableGameComponent
{
    public enum State
    {
        Intro,
        Gameplay,
        GameOver
    }

    public event Action<State>? OnStateChange;

    public event Action<int>? OnScoreChanged;

    private static readonly FName CATEGORY_GAME = new FName("GameWorld");

    private class Pipe
    {
        public int Id;

        public float Position;
        public int Height;

        public Rectangle TopPipe;
        public Rectangle BottomPipe;
    }


    private const float PIPE_SPAWN_INTERVAL = 1f; // Time between pipe spawns (seconds)
    private const int GAP_CENTER_VARIANCE = 300; // Variance range for vertical gap centering (±1/2 value in pixels)
    private const int PIPE_GAP_SIZE = 250; // Size of the gap between the pipes (pixels)
    private const int PIPE_WIDTH = 150; // Width of the pipe (pixels)
    private const float PIPE_GAP_CENTER_NOISE_FREQUENCY = 0.5f; // Frequency of the noise function used to generate the pipe height.

    // Fixed world height (constant gameplay space). Wider screens show more world horizontally.
    private const float WORLD_HEIGHT = 1080f;

    // Ground texture is a square, so this is both the width and height of a single tile.
    private const int GROUND_TILE_SIZE = 100;

    private const float LEVEL_SPEED = 800.0f;

    private static readonly FName ACTION_CONTINUE = new FName("Continue");
    private static readonly FName ACTION_JUMP = new FName("Jump");
    private static readonly FName ACTION_PAUSE = new FName("Pause");


    private State _state = State.Intro;

    private SpriteBatch? _spriteBatch = null;

    private InputManager _inputManager;

    private Rectangle _bounds;
    private Matrix _cameraMatrix;

    private float _pipeSpawnTimer = 0f;
    private float _groundScrollOffset = 0f;

    private NinePatch? _pipeNinePatch = null;
    private Texture2D? _groundTexture = null;


    private Song? _music = null;
    private SoundEffectInstance? _bellSound = null;
    private SoundEffectPool _hitSounds = new SoundEffectPool();

    private List<Pipe> _pipes = new List<Pipe>();

    private int _pipeCounter = 0;

    private int _pipesCrossed = 0;

    private Rectangle _groundBounds;

    private Bird _bird;

    private bool _isPaused = false;

    private DebugRenderer _debugRenderer;


    public GameWorld(Game game) : base(game)
    {
        _pipeSpawnTimer = 0f;

        CalculateBounds(game.GraphicsDevice);

        _bird = new Bird(game);

        _debugRenderer = new DebugRenderer(128, 256, game.GraphicsDevice);

        _inputManager = new InputManager();
        _inputManager.Mapper.AddMapping(ACTION_CONTINUE, new InputKey(Keys.Space), new InputKey(true), new InputKey(Buttons.A), InputKey.Touch());
        _inputManager.Mapper.AddMapping(ACTION_JUMP, new InputKey(Keys.Space), new InputKey(true), new InputKey(Buttons.A), InputKey.Touch());
        _inputManager.Mapper.AddMapping(ACTION_PAUSE, new InputKey(Keys.P), new InputKey(Keys.Escape));

        ChangeState(State.Intro);
    }

    public void OnGraphicsDeviceReset(GraphicsDevice graphicsDevice)
    {
        CalculateBounds(graphicsDevice);
    }

    public override void Update(GameTime gameTime)
    {
        _inputManager.Update();

        // Handle pause (always checkable)
        if (_inputManager.IsActionJustPressed(ACTION_PAUSE))
        {
            _isPaused = !_isPaused;
        }

        if (_isPaused)
        {
            return;
        }

        _bird.Update(gameTime);

        UpdatePipes(gameTime);
        UpdateGround(gameTime);

        switch (_state)
        {
            case State.Intro:
                CheckForContinue(gameTime, State.Gameplay);
                break;
            case State.Gameplay:
                UpdateGameplay(gameTime);
                break;
            case State.GameOver:
                {
                    // Only start checking for continue when the bird has fallen off the screen
                    if(!_bird.CollisionBounds.Intersects(_bounds))
                    {
                        _bird.Hovering = true;
                        CheckForContinue(gameTime, State.Intro);
                    }
                }
                break;
        }

        base.Update(gameTime);
    }

    private void CheckForContinue(GameTime gameTime, State nextState)
    {
        if (_inputManager.IsActionJustPressed(ACTION_CONTINUE))
            ChangeState(nextState);
    }

    private void UpdateGameplay(GameTime gameTime)
    {
        if (_inputManager.IsActionJustPressed(ACTION_JUMP))
        {
            _bird.Jump();
        }

        if (CheckBirdCollision())
        {
            _hitSounds.Play();
            ChangeState(State.GameOver);
        }

        CheckProgress();
    }

    public override void Draw(GameTime gameTime)
    {
        if (_spriteBatch == null)
            throw new Exception("LoadContent must be called before drawing.");

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

        _pipeNinePatch = NinePatchExtensions.Create(
            content.Load<Texture2D>("GreenPipe"),
            new Rectangle(0, 0, 128, 128),
            new Rectangle(38, 50, 30, 26));

        _groundTexture = content.Load<Texture2D>("Ground");

        _music = content.Load<Song>("Audio/Music/GameMusic");
        PlayMusic();

        _hitSounds.Add(content.Load<SoundEffect>("Audio/SoundEffects/Hit01"));
        _hitSounds.Add(content.Load<SoundEffect>("Audio/SoundEffects/Hit02"));
        _hitSounds.Add(content.Load<SoundEffect>("Audio/SoundEffects/Hit03"));
        _hitSounds.Add(content.Load<SoundEffect>("Audio/SoundEffects/Hit04"));

        SoundEffect bellEffect = content.Load<SoundEffect>("Audio/SoundEffects/Bell01");
        _bellSound = bellEffect.CreateInstance();
        _bellSound.IsLooped = false;

        _bird.LoadContent(content);

        base.LoadContent();
    }

    private void Reset()
    {
        _pipesCrossed = 0;
        _pipeCounter = 0;

        _pipes.Clear();

        _bird.Reset();
        _bird.Hovering = true;

        PlayMusic();
    }

    private void PlayMusic()
    {
        if (_music != null)
        {
            if (MediaPlayer.State != MediaState.Stopped)
            {
                MediaPlayer.Stop();
            }
            MediaPlayer.Play(_music);
        }
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
        if (_state == State.Intro)
            return;

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


    private void RecalculatePipeRectangles(Pipe pipe)
    {
        int screenHeight = _bounds.Height;

        int gapCenter = pipe.Height;
        int pipeX = (int)(pipe.Position - (PIPE_WIDTH / 2));

        const int PIPE_TOP_OFFSET = 128; // Hides the top rim of the pipe
        // (Not needed for the bottom rim, which is hidden by the ground)

        int topPipeHeight = gapCenter - (PIPE_GAP_SIZE / 2) + PIPE_TOP_OFFSET;

        Rectangle dest = new Rectangle(pipeX, -PIPE_TOP_OFFSET, PIPE_WIDTH, topPipeHeight);
        pipe.TopPipe = dest;

        int bottomPipeHeight = screenHeight - (gapCenter + (PIPE_GAP_SIZE / 2));
        dest.Y = screenHeight - bottomPipeHeight;
        dest.Height = bottomPipeHeight;
        pipe.BottomPipe = dest;
    }

    private void SpawnPipe()
    {
        float noiseValue = SimplexNoise.Noise(_pipeCounter * PIPE_GAP_CENTER_NOISE_FREQUENCY);

        int screenCenter = (_bounds.Height / 2);
        int rangeMin = screenCenter - (GAP_CENTER_VARIANCE / 2);

        Pipe pipe = new Pipe()
        {
            Id = _pipeCounter,
            Position = _bounds.Width + (PIPE_WIDTH / 2),
            Height = rangeMin + (int)(noiseValue * GAP_CENTER_VARIANCE)
        };

        _pipes.Add(pipe);

        _pipeCounter++;
    }

    private void DrawPipe(SpriteBatch spriteBatch, Pipe pipe)
    {
        if (_pipeNinePatch == null)
            throw new Exception("LoadContent must be called before drawing!");

        spriteBatch.Draw(_pipeNinePatch, pipe.TopPipe, Color.White);
        spriteBatch.Draw(_pipeNinePatch, pipe.BottomPipe, Color.White);
    }

    private void DrawGround(SpriteBatch spriteBatch)
    {
        if (_groundTexture == null)
            throw new Exception("LoadContent must be called before drawing!");

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

    private void CheckProgress()
    {
        if (_pipes.Count <= 0)
        {
            return;
        }

        Pipe pipe = _pipes[0];

        if (pipe.Id < _pipesCrossed)
        {
            return; // This pipe has already been counted as crossed
        }

        // Check if we've just passed the pipe (crossed its center)
        if ((pipe.Position + (PIPE_WIDTH / 2)) < _bird.CollisionBounds.Left)
        {
            IncrementScore();

            _bellSound?.Stop();
            _bellSound?.Play();

            Logger.Info(CATEGORY_GAME, "Pipes Crossed: " + _pipesCrossed);
        }
    }

    private void IncrementScore()
    {
        _pipesCrossed++;
        OnScoreChanged?.Invoke(_pipesCrossed);
    }

    private void ChangeState(State state)
    {
        switch (state)
        {
            case State.Intro:
                Reset();
                break;

            case State.Gameplay:
                _bird.Hovering = false;
                break;
        }

        _state = state;
        OnStateChange?.Invoke(state);
        Logger.Info(CATEGORY_GAME, $"Entered state '{state}'.");
    }
}
