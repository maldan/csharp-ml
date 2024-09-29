using System.Collections.Generic;
using MegaLib.Render.Color;

namespace MegaLib.Render.Texture;

public static class TextureManager
{
  private static Dictionary<string, Texture_2D<RGBA<byte>>> _rgba = new();
  private static Dictionary<string, Texture_2D<RGB<byte>>> _rgb = new();
  private static Dictionary<string, Texture_2D<byte>> _r = new();

  public static void Add<T>(string name, Texture_2D<T> texture) where T : struct
  {
    switch (texture)
    {
      case Texture_2D<RGBA<byte>> trgba:
        _rgba[name] = trgba;
        break;
      case Texture_2D<RGB<byte>> trgb:
        _rgb[name] = trgb;
        break;
      case Texture_2D<byte> tr:
        _r[name] = tr;
        break;
    }
  }

  public static bool Has(string name)
  {
    return _rgba.ContainsKey(name) || _rgb.ContainsKey(name) || _r.ContainsKey(name);
  }

  public static Texture_2D<T> Get<T>(string name) where T : struct
  {
    if (typeof(T) == typeof(RGBA<byte>)) return (Texture_2D<T>)(object)_rgba[name];
    if (typeof(T) == typeof(RGB<byte>)) return (Texture_2D<T>)(object)_rgb[name];
    if (typeof(T) == typeof(byte)) return (Texture_2D<T>)(object)_r[name];
    return null;
  }
}