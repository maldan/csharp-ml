using System.Threading;

namespace MegaLib.Render.Texture;

public enum TextureFiltrationMode
{
  Nearest,
  Linear
}

public enum TextureWrapMode
{
  Clamp,
  Repeat,
  Mirror,
  None
}

public enum TextureFormat
{
  BGR8,
  RGB8,
  RGBA8,
  BGRA8,
  R32F,
  R8
}

public struct TextureOptions
{
  public TextureFiltrationMode FiltrationMode;
  public TextureWrapMode WrapMode;

  public TextureFormat Format;

  //public ushort Width;
  //public ushort Height;
  public bool UseMipMaps;
}

internal static class TextureId
{
  private static ulong _nextId = 1;
  private static readonly Mutex Mutex = new();

  public static ulong NextId()
  {
    ulong id;
    Mutex.WaitOne();
    try
    {
      id = _nextId;
      _nextId += 1;
    }
    finally
    {
      Mutex.ReleaseMutex();
    }

    return id;
  }
}