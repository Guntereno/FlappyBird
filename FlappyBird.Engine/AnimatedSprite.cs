using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlappyBird.Engine;

public class AnimatedSprite
{
    private Texture2D _texture;
    private int _frameWidth;
    private int _frameHeight;
    private int _framesPerRow;
    private int _totalFrames;
    private float _animationSpeed; // frames per second
    private float _elapsedTime;
    private int _currentFrame;

    public AnimatedSprite(Texture2D texture, int frameWidth, int frameHeight, int framesPerRow, int totalFrames, float animationSpeed)
    {
        this._texture = texture;
        this._frameWidth = frameWidth;
        this._frameHeight = frameHeight;
        this._framesPerRow = framesPerRow;
        this._totalFrames = totalFrames;
        this._animationSpeed = animationSpeed;
        this._elapsedTime = 0f;
        this._currentFrame = 0;
    }

    public void Update(GameTime gameTime)
    {
        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        float frameTime = 1f / _animationSpeed;

        if (_elapsedTime >= frameTime)
        {
            _elapsedTime -= frameTime;
            _currentFrame++;

            if (_currentFrame >= _totalFrames)
            {
                _currentFrame = 0;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle destination)
    {
        int row = _currentFrame / _framesPerRow;
        int column = _currentFrame % _framesPerRow;

        Rectangle sourceRectangle = new Rectangle(
            column * _frameWidth,
            row * _frameHeight,
            _frameWidth,
            _frameHeight
        );

        spriteBatch.Draw(_texture, destination, sourceRectangle, Color.White);
    }

    public void SetAnimationSpeed(float speed)
    {
        _animationSpeed = speed;
    }

    public int GetCurrentFrame()
    {
        return _currentFrame;
    }

    public void Reset()
    {
        _currentFrame = 0;
        _elapsedTime = 0f;
    }
}
