using MegaLib.Render.Buffer;
using MegaLib.Render.Color;

namespace MegaLib.Render.Texture
{
  public class Texture_2D<T>
  {
    public ImageGPU<T> RAW { get; private set; }
    public TextureOptions Options;
    public ulong Id;

    public Texture_2D(ImageGPU<T> raw)
    {
      RAW = raw;
      Options.FiltrationMode = TextureFiltrationMode.Linear;
      Options.WrapMode = TextureWrapMode.Clamp;
      Options.UseMipMaps = true;
      Id = TextureId.NextId();

      AutoFormat();
    }

    public Texture_2D(int width, int height)
    {
      RAW = new ImageGPU<T>(width, height);
      Options.FiltrationMode = TextureFiltrationMode.Linear;
      Options.WrapMode = TextureWrapMode.Clamp;
      Options.UseMipMaps = true;
      Id = TextureId.NextId();

      AutoFormat();
    }

    private void AutoFormat()
    {
      Options.Format = RAW switch
      {
        ImageGPU<RGB<byte>> => TextureFormat.RGB8,
        ImageGPU<RGBA<byte>> => TextureFormat.RGBA8,
        ImageGPU<float> => TextureFormat.R32F,
        _ => Options.Format
      };
    }
  }
}