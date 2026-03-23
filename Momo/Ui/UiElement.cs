using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Momo.System;

namespace Momo.Ui;

public class UiElement
{
    public UiElement()
    {
        this.HAlign = HorizontalAlign.Center;
        this.VAlign = VerticalAlign.Center;
    }

    public enum HorizontalAlign
    {
        Left,
        Center,
        Right,
    }

    public enum VerticalAlign
    {
        Top,
        Center,
        Bottom
    }

    public virtual Rectangle DrawRect { get; set; }

    public HorizontalAlign HAlign { get; set; }

    public VerticalAlign VAlign { get; set; }

    protected Point GetAlignedOrigin(int width, int height)
    {
        Point origin = Point.Zero;

        origin.X = HAlign switch
        {
            HorizontalAlign.Left => DrawRect.Left,
            HorizontalAlign.Center => DrawRect.Left + ((DrawRect.Width - width) / 2),
            HorizontalAlign.Right => DrawRect.Right - width,
            _ => throw new Exception("Unhandled alignment!"),
        };

        origin.Y = VAlign switch
        {
            VerticalAlign.Top => DrawRect.Top,
            VerticalAlign.Center => DrawRect.Top + ((DrawRect.Height - height) / 2),
            VerticalAlign.Bottom => DrawRect.Bottom - height,
            _ => throw new Exception("Unhandled alignment!"),
        };

        return origin;
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
#if DEBUG
        Color color = Color.Violet;
        spriteBatch.Draw(Resources.WhiteTexture, DrawRect, color * 0.15f);
#endif
    }

    public static void CenterRectIn(ref Rectangle rect, Rectangle container)
    {
        rect.X = container.Left + ((container.Width - rect.Width) / 2);
        rect.Y = container.Top + ((container.Height - rect.Height) / 2);
    }
}
