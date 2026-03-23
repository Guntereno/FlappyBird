using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Momo.System;

public static class Resources
{
    public static Texture2D WhiteTexture { get; private set; }

    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        // Create a 1x1 white texture
        WhiteTexture = new Texture2D(graphicsDevice, 1, 1);
        WhiteTexture.SetData(new[] { Color.White });
    }
}
