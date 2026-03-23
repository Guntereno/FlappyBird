using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace Momo.Ui;

public class TextBox : UiElement
{
    public string Text
    {
        get { return _text; }
        set
        {
            _text = value;
            _wrappedTextDirty = true;
        }
    }

    public SpriteFont Font
    {
        get { return _font; }
        set
        {
            _font = value;
            _wrappedTextDirty = true;
        }
    }

    public Color TextColor { get; set; }

    public bool Wrapped { get; set; }

    public float Scale { get; set; }

    public Vector2 WrappedTextDimensions { get; private set; }


    private string _text;
    private SpriteFont _font;

    private string _wrappedText = null;
    private bool _wrappedTextDirty = true;


    public TextBox()
    {
        Text = string.Empty;
        TextColor = Color.White;
        Wrapped = false;
        Scale = 1.0f;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        string text;
        if (Wrapped)
        {
            if (_wrappedTextDirty)
            {
                RecalculateWrapping();
            }
            text = _wrappedText;
        }
        else
        {
            text = Text;
        }


        base.Draw(spriteBatch);

        Vector2 pos = new Vector2();
        Vector2 stringDims = Font.MeasureString(text) * Scale;
        switch (HAlign)
        {
            case HorizontalAlign.Left:
                pos.X = DrawRect.Left;
                break;

            case HorizontalAlign.Center:
                pos.X = DrawRect.Center.X - (stringDims.X / 2.0f);
                break;

            case HorizontalAlign.Right:
                pos.X = DrawRect.Right - stringDims.X;
                break;

            default:
                throw new Exception("Invalid align type!");
        }

        switch (VAlign)
        {
            case VerticalAlign.Top:
                pos.Y = DrawRect.Top;
                break;

            case VerticalAlign.Center:
                pos.Y = DrawRect.Center.Y - (stringDims.Y / 2.0f);
                break;

            case VerticalAlign.Bottom:
                pos.Y = DrawRect.Bottom - stringDims.Y;
                break;

            default:
                throw new Exception("Invalid align type!");
        }

        spriteBatch.DrawString(
            Font,
            text,
            pos,
            TextColor,
            0.0f,
            Vector2.Zero,
            Scale,
            SpriteEffects.None,
            0.5f);
    }

    private void RecalculateWrapping()
    {
        _wrappedText = WrapText(Font, Text, DrawRect.Width);
        _wrappedTextDirty = false;

        WrappedTextDimensions = Font.MeasureString(_wrappedText) * Scale;
    }

    private string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
    {
        float scaledLineWidth = maxLineWidth * Scale;

        string[] words = text.Split(' ');

        StringBuilder sb = new StringBuilder();

        float lineWidth = 0f;

        float spaceWidth = spriteFont.MeasureString(" ").X;

        foreach (string word in words)
        {
            string currentWord = word;

            int lastIndexOfNewline = word.LastIndexOf("\n");
            if (lastIndexOfNewline != -1)
            {
                sb.Append(currentWord.Substring(0, lastIndexOfNewline + 1));
                currentWord = word.Substring(lastIndexOfNewline + 1);
                lineWidth = 0;
            }

            Vector2 size = spriteFont.MeasureString(currentWord);

            if (lineWidth + size.X < scaledLineWidth)
            {
                sb.Append(currentWord + " ");
                lineWidth += size.X + spaceWidth;
            }
            else
            {
                sb.Append("\n" + currentWord + " ");
                lineWidth = size.X + spaceWidth;
            }
        }

        return sb.ToString();
    }
}
