using Microsoft.Xna.Framework;

namespace Momo.Graphics;

/// <summary>
/// Extension methods for the XNA Color type.
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Creates a new color with the specified alpha value (0-255).
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="alpha">The alpha value (0-255).</param>
    /// <returns>A new color with the modified alpha.</returns>
    public static Color WithAlpha(this Color color, byte alpha)
    {
        return new Color(color.R, color.G, color.B, alpha);
    }

    /// <summary>
    /// Creates a new color with the specified alpha value (0-1).
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="alpha">The alpha value as a normalized float (0-1).</param>
    /// <returns>A new color with the modified alpha.</returns>
    public static Color WithAlpha(this Color color, float alpha)
    {
        return new Color(color.R, color.G, color.B, (byte)(alpha * 255f));
    }
}
