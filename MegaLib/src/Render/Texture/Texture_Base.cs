namespace MegaLib.Render.Texture
{
  public enum TextureFiltrationMode
  {
    Nearest,
    Linear,
  }

  public enum TextureWrapMode
  {
    Clamp,
    Repeat,
    Mirror
  }

  public enum TextureFormat
  {
    BGR8,
    RGB8,
    RGBA8,
    BGRA8,
    R32F,
  }

  public struct TextureOptions
  {
    public TextureFiltrationMode FiltrationMode;
    public TextureWrapMode WrapMode;
    public TextureFormat Format;
    public ushort Width;
    public ushort Height;
    public bool UseMipMaps;
  }

  public class Texture_Base
  {
    private static ulong _nextId = 1;

    public ulong Id;
    public TextureOptions Options;

    public bool IsChanged;

    public byte[] RAW_BYTE { get; private set; }
    public float[] RAW_FLOAT { get; private set; }

    public int NumberOfChannels
    {
      get
      {
        switch (Options.Format)
        {
          case TextureFormat.RGBA8:
          case TextureFormat.BGRA8:
            return 4;
          case TextureFormat.BGR8:
          case TextureFormat.RGB8:
            return 3;
          case TextureFormat.R32F:
            return 1;
          default:
            return 0;
        }
      }
    }

    public Texture_Base()
    {
      Options.Format = TextureFormat.RGBA8;
      Options.FiltrationMode = TextureFiltrationMode.Linear;
      Options.WrapMode = TextureWrapMode.Repeat;
      Options.UseMipMaps = true;

      RAW_BYTE = new byte[] { 255, 0, 0, 255 };
      Options.Width = 1;
      Options.Height = 1;

      Id = _nextId++;
    }

    public Texture_Base(TextureFormat format, int width, int height, byte[] data)
    {
      Options.Format = format;
      Options.FiltrationMode = TextureFiltrationMode.Linear;
      Options.WrapMode = TextureWrapMode.Repeat;
      Options.UseMipMaps = true;

      RAW_BYTE = data;
      Options.Width = (ushort)width;
      Options.Height = (ushort)height;

      Id = _nextId++;
    }

    public void SetPixels(float[] pixels)
    {
      RAW_FLOAT = pixels;
      IsChanged = true;
    }

    public void SetPixels(byte[] pixels)
    {
      RAW_BYTE = pixels;
      IsChanged = true;
    }

    public void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
    {
      var ch = NumberOfChannels;
      var index = (y * Options.Width + x) * ch;

      switch (ch)
      {
        case 1:
          RAW_BYTE[index] = r;
          break;
        case 2:
          RAW_BYTE[index] = r;
          RAW_BYTE[index + 1] = g;
          break;
        case 3:
          RAW_BYTE[index] = r;
          RAW_BYTE[index + 1] = g;
          RAW_BYTE[index + 2] = b;
          break;
        case 4:
          RAW_BYTE[index] = r;
          RAW_BYTE[index + 1] = g;
          RAW_BYTE[index + 2] = b;
          RAW_BYTE[index + 3] = a;
          break;
      }

      IsChanged = true;
    }

    public (byte, byte, byte, byte) GetPixel(int x, int y)
    {
      var ch = NumberOfChannels;
      var index = (y * Options.Width + x) * ch;
      return ch switch
      {
        1 => (RAW_BYTE[index], 0, 0, 0),
        2 => (RAW_BYTE[index], RAW_BYTE[index + 1], 0, 0),
        3 => (RAW_BYTE[index], RAW_BYTE[index + 1], RAW_BYTE[index + 2], 0),
        4 => (RAW_BYTE[index], RAW_BYTE[index + 1], RAW_BYTE[index + 2], RAW_BYTE[index + 3]),
        _ => (0, 0, 0, 0)
      };
    }
  }
}