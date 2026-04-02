using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using Momo.Graphics;
using Momo.Audio;

namespace FlappyBird.Core;

public class Bird
{
    public Rectangle CollisionBounds
    {
        get; private set;
    }

    public bool Hovering {get; set;}

    // Position and size properties
    private Vector2 _position;
    private Vector2 _velocity;

    // Physics constants
    private const float GRAVITY = 1500f;      // Pixels per second squared
    private const float JUMP_STRENGTH = -400f; // Negative because Y goes down

    private const int BIRD_WIDTH = 256;
    private const int BIRD_HEIGHT = 172;


    // Visual properties
    private Texture2D? _birdTexture = null;
    private AnimatedSprite? _animatedSprite = null;

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
        _birdTexture = content.Load<Texture2D>("FlappingBird");

        _animatedSprite = new AnimatedSprite(_birdTexture, BIRD_WIDTH, BIRD_HEIGHT, 4, 8, 10f);

        _flapSounds.Add(content.Load<SoundEffect>("Audio/SoundEffects/Flap01"));
        _flapSounds.Add(content.Load<SoundEffect>("Audio/SoundEffects/Flap02"));
        _flapSounds.Add(content.Load<SoundEffect>("Audio/SoundEffects/Flap03"));
        _flapSounds.Add(content.Load<SoundEffect>("Audio/SoundEffects/Flap04"));
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (!Hovering)
            _velocity.Y += GRAVITY * deltaTime;

        SetPosition(_position + _velocity * deltaTime);

        if (_animatedSprite != null)
            _animatedSprite.Update(gameTime);
    }

    public void Jump()
    {
        _velocity.Y = JUMP_STRENGTH;

        _flapSounds.Play();
    }


    public void Draw(SpriteBatch spriteBatch)
    {
        if ((spriteBatch == null) || (_animatedSprite == null))
            return;

        _animatedSprite.Draw(spriteBatch, _spriteBounds);
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
