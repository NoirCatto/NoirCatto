using System.Linq;
using UnityEngine;

namespace NoirCatto;

public partial class Extensions
{
    public static void RecolorTexture(this Texture2D texture, Color from, Color to)
    {
        var colors = texture.GetPixels32();
        for (var i = 0; i < colors.Length; i++)
        {
            if (colors[i] == from)
            {
                colors[i] = to;
            }
        }
        texture.SetPixels32(colors);
        texture.Apply();
    }
    public static void RecolorTexture(this Texture2D texture, Color[] from, Color to)
    {
        var colors = texture.GetPixels32();
        for (var i = 0; i < colors.Length; i++)
        {
            if (from.Contains(colors[i]))
            {
                colors[i] = to;
            }
        }
        texture.SetPixels32(colors);
        texture.Apply();
    }
    /// <remarks>Arrays <paramref name="to"/> and <paramref name="from"/> MUST be the same length.</remarks>
    public static void RecolorTexture(this Texture2D texture, Color[] from, Color[] to)
    {
        var colors = texture.GetPixels32();
        for (var i = 0; i < colors.Length; i++)
        {
            for (var j = 0; j < from.Length; j++)
            {
                if (colors[i] == from[j])
                {
                    colors[i] = to[j];
                    break;
                }
            }
        }
        texture.SetPixels32(colors);
        texture.Apply();
    }

    public static void RecolorTextureMagically(this Texture2D texture, Color[] from, Color to)
    {
        var colors = texture.GetPixels32();
        for (var i = 0; i < colors.Length; i++)
        {
            if (from.Contains(colors[i]))
                colors[i] = Extensions.RecolorMagically(colors[i], to);
        }
        texture.SetPixels32(colors);
        texture.Apply();
    }
    public static void RecolorTextureMagically(this Texture2D texture, Color[] from, Color[] to)
    {
        var colors = texture.GetPixels32();
        for (var i = 0; i < colors.Length; i++)
        {
            for (var j = 0; j < from.Length; j++)
            {
                if (colors[i] == from[j])
                {
                    colors[i] = Extensions.RecolorMagically(colors[i], to[j]);
                    break;
                }
            }
        }
        texture.SetPixels32(colors);
        texture.Apply();
    }

    //Noir specific
    public static void RecolorTextureNoSurvivors(this Texture2D texture, Color from, Color to, Color fromRemaining, Color toRemaining)
    {
        var colors = texture.GetPixels32();
        for (var i = 0; i < colors.Length; i++)
        {
            if (colors[i].a != 255) //'Where' doesn't work, for some reason...
                continue;

            if (colors[i] == from)
            {
                colors[i] = to;
            }
            else
            {
                var diff = fromRemaining.RGB2HSL() - RGB2HSL(colors[i]);
                var colAsHsl = toRemaining.RGB2HSL();
                colAsHsl -= diff;
                if (colAsHsl.x < 0) colAsHsl.x += 1; else if (colAsHsl.x > 1) colAsHsl.x -= 1; //Wrap-around negative values
                colAsHsl.y = Mathf.Clamp01(colAsHsl.y);
                colAsHsl.z = Mathf.Clamp01(colAsHsl.z);
                colors[i] = HSL2RGB(colAsHsl);
            }
        }
        texture.SetPixels32(colors);
        texture.Apply();
    }
    public static void RecolorTextureNoSurvivors(this Texture2D texture, Color[] from, Color[] to, Color fromRemaining, Color toRemaining)
    {
        var colors = texture.GetPixels32();
        for (var i = 0; i < colors.Length; i++)
        {
            if (colors[i].a != 255) //'Where' doesn't work, for some reason...
                continue;

            var handled = false;
            for (var j = 0; j < from.Length; j++)
            {
                if (colors[i] == from[j])
                {
                    colors[i] = to[j];
                    handled = true;
                    break;
                }
            }
            if (handled) continue;

            var diff = fromRemaining.RGB2HSL() - RGB2HSL(colors[i]);
            var colAsHsl = toRemaining.RGB2HSL();
            colAsHsl -= diff;
            if (colAsHsl.x < 0) colAsHsl.x += 1; else if (colAsHsl.x > 1) colAsHsl.x -= 1; //Wrap-around negative values
            colAsHsl.y = Mathf.Clamp01(colAsHsl.y);
            colAsHsl.z = Mathf.Clamp01(colAsHsl.z);
            colors[i] = HSL2RGB(colAsHsl);
        }
        texture.SetPixels32(colors);
        texture.Apply();
    }
    
    public static void RecolorTextureAndRemainingMagically(this Texture2D texture, Color[] from, Color[] to, Color toRemaining)
    {
        var colors = texture.GetPixels32();
        for (var i = 0; i < colors.Length; i++)
        {
            if (colors[i].a != 255) //'Where' doesn't work, for some reason...
                continue;

            var handled = false;
            for (var j = 0; j < from.Length; j++)
            {
                if (colors[i] == from[j])
                {
                    colors[i] = to[j];
                    handled = true;
                    break;
                }
            }
            if (handled) continue;

            colors[i] = Extensions.RecolorMagically(colors[i], toRemaining);
        }
        texture.SetPixels32(colors);
        texture.Apply();
    }
}