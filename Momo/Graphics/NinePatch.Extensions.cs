using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;

namespace Momo.Graphics;

public static class NinePatchExtensions
{
    public static NinePatch Create(Texture2D texture2D, Rectangle outer, Rectangle inner)
    {
        if (inner.Left < outer.Left || inner.Top < outer.Top ||
            inner.Right > outer.Right || inner.Bottom > outer.Bottom)
        {
            throw new ArgumentException("Inner rectangle must be strictly inside outer rectangle");
        }

        int topRowHeight = inner.Top - outer.Top;
        int bottomRowHeight = outer.Bottom - inner.Bottom;
        int leftColumnWidth = inner.Left - outer.Left;
        int rightColumnWidth = outer.Right - inner.Right;

        Texture2DRegion[] patches =
        [
            new(texture2D, new Rectangle(outer.Left, outer.Top, leftColumnWidth, topRowHeight)),            // Top Left
            new(texture2D, new Rectangle(inner.Left, outer.Top, inner.Width, topRowHeight)),                // Top Middle
            new(texture2D, new Rectangle(inner.Right, outer.Top, rightColumnWidth, topRowHeight)),          // Top Right
            new(texture2D, new Rectangle(outer.Left, inner.Top, leftColumnWidth, inner.Height)),            // Middle Left
            new(texture2D, inner),                                                                          // Middle
            new(texture2D, new Rectangle(inner.Right, inner.Top, rightColumnWidth, inner.Height)),          // Middle Right
            new(texture2D, new Rectangle(outer.Left, inner.Bottom, leftColumnWidth, bottomRowHeight)),      // Bottom Left
            new(texture2D, new Rectangle(inner.Left, inner.Bottom, inner.Width, bottomRowHeight)),          // Bottom Middle
            new(texture2D, new Rectangle(inner.Right, inner.Bottom, rightColumnWidth, bottomRowHeight)),    // Bottom Right
        ];

        return new NinePatch(patches);
    } 
}
