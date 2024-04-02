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
}