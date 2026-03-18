using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlappyBird.Engine;

public class AnimatedSprite
{
    private Texture2D texture;
    private int frameWidth;
    private int frameHeight;
    private int framesPerRow;
    private int totalFrames;
    private float animationSpeed; // frames per second
    private float elapsedTime;
    private int currentFrame;

    public AnimatedSprite(Texture2D texture, int frameWidth, int frameHeight, int framesPerRow, int totalFrames, float animationSpeed)
    {
        this.texture = texture;
        this.frameWidth = frameWidth;
        this.frameHeight = frameHeight;
        this.framesPerRow = framesPerRow;
        this.totalFrames = totalFrames;
        this.animationSpeed = animationSpeed;
        this.elapsedTime = 0f;
        this.currentFrame = 0;
    }

    public void Update(GameTime gameTime)
    {
        elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        float frameTime = 1f / animationSpeed;

        if (elapsedTime >= frameTime)
        {
            elapsedTime -= frameTime;
            currentFrame++;

            if (currentFrame >= totalFrames)
            {
                currentFrame = 0;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle destination)
    {
        int row = currentFrame / framesPerRow;
        int column = currentFrame % framesPerRow;

        Rectangle sourceRectangle = new Rectangle(
            column * frameWidth,
            row * frameHeight,
            frameWidth,
            frameHeight
        );

        spriteBatch.Draw(texture, destination, sourceRectangle, Color.White);
    }

    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = speed;
    }

    public int GetCurrentFrame()
    {
        return currentFrame;
    }

    public void Reset()
    {
        currentFrame = 0;
        elapsedTime = 0f;
    }
}
