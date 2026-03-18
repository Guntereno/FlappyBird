using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlappyBird.Engine;

public class SlicedSprite
{
    private Texture2D _texture;
    private Rectangle _outside;
    private Rectangle _inside;

    public SlicedSprite(Texture2D texture, Rectangle outside, Rectangle inside)
    {
        _texture = texture;
        _outside = outside;
        _inside = inside;
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle dest, Color color)
    {
        int destLeft = dest.X - _outside.X;
        int destTop = dest.Y - _outside.Y;

        int[] srcRowHeights =
        {
            _inside.Y,
            _inside.Height,
            _texture.Height - _inside.Bottom
        };
        int[] srcColumnWidths =
        {
            _inside.X,
            _inside.Width,
            _texture.Width - _inside.Right
        };

        int[] margin = 
        {
            _inside.Top - _outside.Top,
            _outside.Right - _inside.Right,
            _outside.Bottom - _inside.Bottom,
            _inside.Left - _outside.Left
        };

        int destInsideHeight = dest.Height - (margin[0] + margin[2]);
        int destInsideWidth = dest.Width - (margin[1] + margin[3]);

        const int kNumRows = 3;
        const int kNumColumns = 3;

        Rectangle curSrc = new Rectangle(0, 0, 0, 0);
        Rectangle curDest = new Rectangle(destLeft, destTop, 0, 0);

        for(int x=0; x<kNumColumns; ++x)
        {
            curSrc.Width = srcColumnWidths[x];
            curDest.Width = (x == 1) ? destInsideWidth : curSrc.Width;

            for(int y=0; y<kNumRows; ++y)
            {
                curSrc.Height = srcRowHeights[y];
                curDest.Height = (y == 1) ? destInsideHeight : curSrc.Height;

                spriteBatch.Draw(_texture, curDest, curSrc, color);

                curSrc.Y += curSrc.Height;
                curDest.Y += curDest.Height;
            }

            curSrc.Y = 0;
            curDest.Y = destTop;

            curSrc.X += curSrc.Width;
            curDest.X += curDest.Width;
        }
    }
}