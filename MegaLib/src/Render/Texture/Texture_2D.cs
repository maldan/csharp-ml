using MegaLib.Render.Buffer;

namespace MegaLib.Render.Texture
{
  public class Texture_2D<T>
  {
    public ImageGPU<T> RAW;
    public TextureOptions Options;

    public Texture_2D()
    {
    }
  }
}