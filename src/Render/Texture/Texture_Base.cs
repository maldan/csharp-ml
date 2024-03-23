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
    public int Id;
    public TextureOptions Options;
    public byte[] GPU_RAW { get; set; }

    public Texture_Base()
    {
      Options.Format = TextureFormat.RGBA8;
      Options.FiltrationMode = TextureFiltrationMode.Linear;
      Options.WrapMode = TextureWrapMode.Repeat;
      Options.UseMipMaps = true;

      GPU_RAW = new byte[] { 255, 0, 0, 255 };
      Options.Width = 1;
      Options.Height = 1;
    }

    /*public Texture_Base(byte[] data)
    {
      if (data == null) throw new Exception("Empty texture data");
      using var memoryStream = new MemoryStream(data);
      var bmp = new Bitmap(memoryStream);
      var channelCount = GetChannelsCount(bmp.PixelFormat);
      if (channelCount == 3) Options.Format = TextureFormat.BGR8;
      if (channelCount == 4) Options.Format = TextureFormat.RGBA8;

      var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      var bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

      var bytes = Math.Abs(bmpData.Stride) * bmp.Height;
      GPU_RAW = new byte[bytes];
      Marshal.Copy(bmpData.Scan0, GPU_RAW, 0, bytes);
      bmp.UnlockBits(bmpData);
      
      Options.Width = (ushort)bmp.Width;
      Options.Height = (ushort)bmp.Height;

      // Set default filtration mode
      Options.FiltrationMode = TextureFiltrationMode.Linear;
      Options.WrapMode = TextureWrapMode.Repeat;
    }*/

    /*private static int GetChannelsCount(PixelFormat pixelFormat)
    {
      Console.WriteLine(pixelFormat);
      switch (pixelFormat)
      {
        case PixelFormat.Format24bppRgb:
          return 3; // RGB
        case PixelFormat.Format32bppArgb:
        case PixelFormat.Format32bppPArgb:
          return 4; // RGBA
        // Другие форматы изображений можно добавить по необходимости
        default:
          throw new NotSupportedException("Не поддерживаемый формат изображения");
      }
    }*/
  }
}