using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Momo.Maths;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Graphics;

namespace FlappyBird.Core;

public class CloudField
{
    private struct Cloud
    {
        public float Position = 0.0f;
        public float HeightNormalised = 0.0f;
        public int ParalaxLayer = -1;
        public Sprite? Sprite = null;

        public Cloud()
        {
        }

        public Cloud(float position, float heightNormalised, int paralaxLayer, Sprite sprite)
        {
            Position = position;
            HeightNormalised = heightNormalised;
            ParalaxLayer = paralaxLayer;
            Sprite = sprite;
        }
    }

    private const int CLOUD_COUNT = 12; // Number of clouds
    private const int CLOUD_LAYER_COUNT = 4; // Number of paralax layers
    private const float CLOUD_MOVE_SPEED_MAX = 400f; // Cloud movement speed at closest layer
    private const float CLOUD_MOVE_SPEED_MIN = 200f; // Cloud movement speed at furthest layer
    private const float CLOUD_HEIGHT_MAX = 1000.0f;
    private const float CLOUD_HEIGHT_MIN = 200.0f; // Allow some space to avoid them conflicting with the UI

    private Texture2DAtlas? _cloudsAtlas = null;
    private Sprite[]? _cloudSprites = null;

    private Cloud[] _clouds = new Cloud[CLOUD_COUNT];


    public void LoadContent(ContentManager content)
    {
        _cloudsAtlas = content.Load<Texture2DAtlas>("Clouds");
        int numClouds = _cloudsAtlas.RegionCount;
        _cloudSprites = new Sprite[numClouds];
        for (int i = 0; i < numClouds; ++i)
        {
            _cloudSprites[i] = _cloudsAtlas.CreateSprite(i);
        }
    }

    public void Initialise(Random random, Rectangle bounds)
    {
        for(int i=0; i<CLOUD_COUNT; ++i)
        {
            float xPosition = bounds.Left + ((float)random.NextDouble() * bounds.Width);
            SpawnCloud(ref _clouds[i], random, xPosition);
        }
    }

    public void Update(GameTime gameTime, Random random, Rectangle bounds)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        foreach (ref Cloud cloud in _clouds.AsSpan())
        {
            float speed = CLOUD_MOVE_SPEED_MIN + ((float)cloud.ParalaxLayer / CLOUD_LAYER_COUNT) * (CLOUD_MOVE_SPEED_MAX - CLOUD_MOVE_SPEED_MIN);
            cloud.Position -= speed * deltaTime;

            if (cloud.Sprite == null)
            {
                throw new Exception("Cloud sprite is null!");
            }

            int spriteWidth = cloud.Sprite.Size.X;
            if ((cloud.Position + spriteWidth) < bounds.Left)
            {
                float xPosition = bounds.Right + (spriteWidth / 2);
                SpawnCloud(ref cloud, random, xPosition);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle cameraBounds)
    {
        for (int i=0; i<CLOUD_LAYER_COUNT; ++i)
        {
            foreach (var cloud in _clouds)
            {
                if(cloud.ParalaxLayer != i)
                {
                    continue;
                }

                DrawCloud(spriteBatch, cameraBounds, cloud);
            }
        }
    }


    private void SpawnCloud(ref Cloud cloud, Random random, float xPosition)
    {
        if (_cloudSprites == null)
        {
            throw new Exception("Cloud sprites aren't loaded!");
        }

        float heightNormalised = (float)random.NextDouble();

        int randomIndex = random.Next(_cloudSprites.Length);
        Sprite sprite = _cloudSprites[randomIndex];

        int paralaxLayer = random.Next(CLOUD_LAYER_COUNT);
        cloud = new Cloud(xPosition, heightNormalised, paralaxLayer, sprite);
    }

     void DrawCloud(SpriteBatch spriteBatch, Rectangle cameraBounds, Cloud cloud)
    {
        if (_cloudSprites == null)
        {
            throw new Exception("Cloud sprites have not been loaded!");
        }

        float screenHeight = CLOUD_HEIGHT_MIN + (cloud.HeightNormalised * (CLOUD_HEIGHT_MAX - CLOUD_HEIGHT_MIN));
        Vector2 position = new Vector2(cloud.Position, screenHeight);
        spriteBatch.Draw(cloud.Sprite, position);
    }
}