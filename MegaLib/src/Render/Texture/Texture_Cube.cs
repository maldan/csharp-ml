using System;
using MegaLib.Render.Buffer;
using MegaLib.Render.Color;

namespace MegaLib.Render.Texture
{
  public class Texture_Cube
  {
    public ulong Id;

    public ImageGPU<RGB<byte>> FRONT { get; set; }
    public ImageGPU<RGB<byte>> BACK { get; set; }

    public ImageGPU<RGB<byte>> TOP { get; set; }
    public ImageGPU<RGB<byte>> BOTTOM { get; set; }

    public ImageGPU<RGB<byte>> LEFT { get; set; }
    public ImageGPU<RGB<byte>> RIGHT { get; set; }

    public TextureOptions Options;

    public event EventHandler<ulong> OnDestroy;

    public Texture_Cube()
    {
      Options = new TextureOptions
      {
        Format = TextureFormat.RGB8,
        FiltrationMode = TextureFiltrationMode.Linear,
        UseMipMaps = true
      };

      TOP = new ImageGPU<RGB<byte>>(4, 4);
      TOP.SetPixels(new RGB<byte>[]
      {
        new(128, 128, 128), new(255, 255, 255), new(128, 128, 128), new(255, 255, 255),
        new(255, 255, 255), new(128, 128, 128), new(255, 255, 255), new(128, 128, 128),
        new(128, 128, 128), new(255, 255, 255), new(128, 128, 128), new(255, 255, 255),
        new(255, 255, 255), new(128, 128, 128), new(255, 255, 255), new(128, 128, 128),
      });
      FRONT = TOP;
      BACK = TOP;
      LEFT = TOP;
      RIGHT = TOP;
      BOTTOM = TOP;
      Id = TextureId.NextId();
    }

    ~Texture_Cube()
    {
      OnDestroy?.Invoke(this, Id);
    }
  }
}