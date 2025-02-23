using System;
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
}