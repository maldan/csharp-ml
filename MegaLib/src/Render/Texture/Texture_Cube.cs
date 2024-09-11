using System;
using System.IO;
using System.Runtime.InteropServices;
using MegaLib.Render.Buffer;
using MegaLib.Render.Color;

namespace MegaLib.Render.Texture;

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
      WrapMode = TextureWrapMode.Clamp,
      UseMipMaps = false
    };

    TOP = new ImageGPU<RGB<byte>>(4, 4);
    BOTTOM = new ImageGPU<RGB<byte>>(4, 4);
    LEFT = new ImageGPU<RGB<byte>>(4, 4);
    RIGHT = new ImageGPU<RGB<byte>>(4, 4);
    BACK = new ImageGPU<RGB<byte>>(4, 4);
    FRONT = new ImageGPU<RGB<byte>>(4, 4);

    var l = new[] { RIGHT, LEFT, TOP, BOTTOM, BACK, FRONT };
    foreach (var ll in l)
    {
      ll.SetPixels(new RGB<byte>[]
      {
        new(128, 128, 128), new(255, 255, 255), new(128, 128, 128), new(255, 255, 255),
        new(255, 255, 255), new(128, 128, 128), new(255, 255, 255), new(128, 128, 128),
        new(128, 128, 128), new(255, 255, 255), new(128, 128, 128), new(255, 255, 255),
        new(255, 255, 255), new(128, 128, 128), new(255, 255, 255), new(128, 128, 128)
      });
    }

    Id = TextureId.NextId();
  }
}