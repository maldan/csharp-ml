using MegaLib.Render.Buffer;

namespace MegaLib.Render.Texture
{
  public class Texture_Cube
  {
    public ImageGPU<(byte, byte, byte)> FRONT { get; set; }
    public ImageGPU<(byte, byte, byte)> BACK { get; set; }

    public ImageGPU<(byte, byte, byte)> TOP { get; set; }
    public ImageGPU<(byte, byte, byte)> BOTTOM { get; set; }

    public ImageGPU<(byte, byte, byte)> LEFT { get; set; }
    public ImageGPU<(byte, byte, byte)> RIGHT { get; set; }

    public TextureOptions Options;

    public Texture_Cube()
    {
      Options = new TextureOptions
      {
        Format = TextureFormat.RGB8,
        Width = 4,
        Height = 4,
        FiltrationMode = TextureFiltrationMode.Linear,
        UseMipMaps = true
      };

      TOP = new ImageGPU<(byte, byte, byte)>(4, 4);
      TOP.SetPixels(new (byte, byte, byte)[]
      {
        (128, 128, 128), (255, 255, 255), (128, 128, 128), (255, 255, 255),
        (255, 255, 255), (128, 128, 128), (255, 255, 255), (128, 128, 128),
        (128, 128, 128), (255, 255, 255), (128, 128, 128), (255, 255, 255),
        (255, 255, 255), (128, 128, 128), (255, 255, 255), (128, 128, 128),
      });
      FRONT = TOP;
      BACK = TOP;
      LEFT = TOP;
      RIGHT = TOP;
      BOTTOM = TOP;
    }
  }
}