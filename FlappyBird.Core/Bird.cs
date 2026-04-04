using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Momo.Audio;
using MonoGame.Extended.Graphics;

namespace FlappyBird.Core;

public class Bird
{
    public Rectangle CollisionBounds
    {
        get; private set;
    }

    public bool Hovering { get; set; }


    private static readonly string FLAPPING_ANIM_NAME = "flapping";

    private const float GRAVITY = 1500f; // Pixels per second squared
    private const float JUMP_STRENGTH = -400f; // Negative because Y goes down
    private const int BIRD_WIDTH = 256;
    private const int BIRD_HEIGHT = 172;


    // Position and size properties
    private Vector2 _position;
    private Vector2 _velocity;

    // Visual properties
    private AnimatedSprite? _sprite = null;

    private Rectangle _collisionBox;

    private Rectangle _spriteBounds;

    private SoundEffectPool _flapSounds = new SoundEffectPool();


    // Constructor
    public Bird(Game game)
    {
        _collisionBox = new Rectangle(44, 20, 147, 123); // Adjusted to fit the bird's body shape better

        Reset();
    }

    public void Reset()
    {
        SetPosition(new Vector2(300, 300));
        _velocity = Vector2.Zero;
    }

    public void LoadContent(ContentManager content)
    {
        Texture2D texture = content.Load<Texture2D>("FlappingBird");
        Texture2DAtlas atlas = Texture2DAtlas.Create("FlappingBird", texture, BIRD_WIDTH, BIRD_HEIGHT, 8, 0, 0);
        SpriteSheet spriteSheet = new SpriteSheet("FlappingBird", atlas);

        TimeSpan duration = TimeSpan.FromSeconds(0.1f);
        spriteSheet.DefineAnimation(FLAPPING_ANIM_NAME, builder =>
        {
            builder.IsLooping(false)
                .AddFrame("FlappingBird_0", duration)
                .AddFrame("FlappingBird_1", duration)
                .AddFrame("FlappingBird_2", duration)
                .AddFrame("FlappingBird_3", duration)
                .AddFrame("FlappingBird_4", duration)
                .AddFrame("FlappingBird_5", duration);
        });

        new SpriteSheet("SpriteSheet/adventurer", atlas);

        _sprite = new AnimatedSprite(spriteSheet, FLAPPING_ANIM_NAME);

        _flapSounds.Add(content.Load<SoundEffect>("Audio/SoundEffects/Flap01"));
        _flapSounds.Add(content.Load<SoundEffect>("Audio/SoundEffects/Flap02"));
        _flapSounds.Add(content.Load<SoundEffect>("Audio/SoundEffects/Flap03"));
        _flapSounds.Add(content.Load<SoundEffect>("Audio/SoundEffects/Flap04"));
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (!Hovering)
        {
            _velocity.Y += GRAVITY * deltaTime;
        }

        SetPosition(_position + _velocity * deltaTime);

        _sprite?.Update(gameTime);
    }

    public void Jump()
    {
        _velocity.Y = JUMP_STRENGTH;

        _flapSounds.Play();

        _sprite?.SetAnimation(FLAPPING_ANIM_NAME);
    }


    public void Draw(SpriteBatch spriteBatch)
    {
        if ((spriteBatch == null) || (_sprite == null))
        {
            return;
        }

        spriteBatch.Draw(_sprite, _spriteBounds.Location.ToVector2(), 0);
    }


    private void SetPosition(Vector2 position)
    {
        _position = position;
        CalculateBounds();
    }

    private void CalculateBounds()
    {
        // Set bounds of the bird based on its current position
        Rectangle bounds = _collisionBox;
        bounds.X += (int)(_position.X - BIRD_WIDTH / 2);
        bounds.Y += (int)(_position.Y - BIRD_HEIGHT / 2);
        CollisionBounds = bounds;

        _spriteBounds = new Rectangle(
                (int)_position.X - BIRD_WIDTH / 2,
                (int)_position.Y - BIRD_HEIGHT / 2,
                BIRD_WIDTH,
                BIRD_HEIGHT
            );
    }
}
